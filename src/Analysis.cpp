#include "Analysis.h"
#include "LinearSolver.h"
#include "Node.h"
#include <iostream>
#include <vector>
#include <iomanip>
#include <cmath>
#include <complex>
#include <stdexcept>

using namespace std;

void result_from_vec(Circuit& circuit, const vector<double>& solvedVoltages, const vector<Node*>& nonGroundNodes);

bool dcAnalysis(Circuit& circuit) {
    try {
        cout << "// Performing DC Analysis..." << endl;
        circuit.setDeltaT(1e12); // Treat capacitors as open, inductors as short
        vector<Node*> nonGroundNodes;
        for (auto* node : circuit.nodes) {
            if (!node->isGround) {
                nonGroundNodes.push_back(node);
            }
        }

        const int MAX_DIODE_ITERATIONS = 100;
        bool converged = false;
        int iteration_count = 0;
        const double EPSILON_CURRENT = 1e-9;

        for (auto& diode : circuit.diodes) {
            diode.setState(STATE_OFF);
        }

        do {
            converged = true;
            iteration_count++;

            vector<DiodeState> previous_diode_states;
            for (const auto& diode : circuit.diodes) {
                previous_diode_states.push_back(diode.getState());
            }

            circuit.assignDiodeBranchIndices();
            circuit.set_MNA_A(AnalysisType::DC);
            circuit.set_MNA_RHS(AnalysisType::DC);

            if (circuit.MNA_A.empty() || circuit.MNA_RHS.empty() || circuit.MNA_A.size() != circuit.MNA_RHS.size()) {
                cerr << "Error: MNA matrix is singular or malformed." << endl;
                return false;
            }

            // Solve the system
            vector<double> solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
            result_from_vec(circuit, solved_solution, nonGroundNodes);

            // Check if any diode has changed its state
            for (size_t i = 0; i < circuit.diodes.size(); ++i) {
                Diode& current_diode = circuit.diodes[i];
                DiodeState old_state = previous_diode_states[i];
                DiodeState new_state = old_state;

                double v_diode_across = current_diode.node1->getVoltage() - current_diode.node2->getVoltage();

                if (old_state == STATE_OFF && v_diode_across >= current_diode.getForwardVoltage() - EPSILON_CURRENT) {
                    new_state = STATE_FORWARD_ON;
                } else if (old_state == STATE_FORWARD_ON && current_diode.getCurrent() < -EPSILON_CURRENT) {
                    new_state = STATE_OFF;
                }
                
                if (new_state != old_state) {
                    converged = false;
                    current_diode.setState(new_state);
                }
            }

        } while (!converged && iteration_count < MAX_DIODE_ITERATIONS);

        if (!converged) {
            cerr << "Warning: DC analysis for diodes did not converge after " << MAX_DIODE_ITERATIONS << " iterations." << endl;
        }

        cout << "// DC Analysis complete." << endl;
        return true;

    } catch (const std::exception& e) {
        cerr << "Critical error during DC Analysis: " << e.what() << endl;
        return false;
    }
}

bool transientAnalysis(Circuit& circuit, double t_step, double t_stop) {
    try {
        cout << "// Performing Transient Analysis..." << endl;
        circuit.clearComponentHistory();

        // Run DC analysis to get initial conditions at t=0
        if (!dcAnalysis(circuit)) {
            cerr << "Initial DC analysis failed. Aborting transient analysis." << endl;
            return false;
        }

        for (auto& cap : circuit.capacitors) {
            cap.prevVoltage = cap.getVoltage();
        }
        for (auto& ind : circuit.inductors) {
            ind.prevCurrent = ind.getCurrent();
        }
        
        vector<Node*> nonGroundNodes;
        for (auto* node : circuit.nodes) {
            if (!node->isGround) {
                nonGroundNodes.push_back(node);
            }
        }

        for (auto* node : circuit.nodes) {
            if (!node->isGround) node->addVoltageHistoryPoint(0.0, node->getVoltage());
        }

        circuit.setDeltaT(t_step);

        for (double t = t_step; t <= t_stop; t += t_step) {
            circuit.set_MNA_A(AnalysisType::TRANSIENT);
            circuit.set_MNA_RHS(AnalysisType::TRANSIENT);

            vector<double> solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
            result_from_vec(circuit, solved_solution, nonGroundNodes);

            for (auto* node : circuit.nodes) {
                if (!node->isGround) node->addVoltageHistoryPoint(t, node->getVoltage());
            }

            circuit.updateComponentStates(); // Update prevVoltage/prevCurrent for next step
        }
        cout << "// Transient Analysis complete." << endl;
        return true;
    } catch (const std::exception& e) {
        cerr << "Critical error during Transient Analysis: " << e.what() << endl;
        return false;
    }
}


// --- Wrapped in a safety block ---
int acSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start_freq, double stop_freq, int num_points, const std::string& sweep_type) {
    try {
        cout << "// Performing AC Sweep Analysis..." << endl;
        circuit.clearComponentHistory();
        ACVoltageSource* acSource = circuit.findACVoltageSource(sourceName);
        if (!acSource) {
            cerr << "Error: AC source '" << sourceName << "' not found." << endl;
            return 0;
        }

        vector<Node*> nonGroundNodes;
        for (auto* node : circuit.nodes) {
            if (!node->isGround) nonGroundNodes.push_back(node);
        }
        
        int pointsCalculated = 0;
        for (int i = 0; i < num_points; ++i) {
            double current_freq;
            if (num_points == 1) {
                current_freq = start_freq;
            } else if (sweep_type == "Linear") {
                 current_freq = start_freq + i * (stop_freq - start_freq) / (num_points - 1);
            } else { // Logarithmic (Decade)
                current_freq = start_freq * pow(10.0, i / (double)(num_points - 1) * log10(stop_freq / start_freq));
            }
            
            if (current_freq <= 0) continue;

            circuit.set_MNA_A(AnalysisType::AC_SWEEP, current_freq);
            circuit.set_MNA_RHS(AnalysisType::AC_SWEEP, current_freq);

            vector<complex<double>> solution = gaussianElimination(circuit.MNA_A_Complex, circuit.MNA_RHS_Complex);

            for (size_t j = 0; j < nonGroundNodes.size(); ++j) {
                if(j < solution.size()) {
                    nonGroundNodes[j]->ac_sweep_history.push_back({current_freq, abs(solution[j])});
                }
            }
            pointsCalculated++;
        }
        cout << "// AC Sweep Analysis complete." << endl;
        return pointsCalculated;
    } catch(const std::exception& e) {
        cerr << "Critical error during AC Sweep: " << e.what() << endl;
        return 0;
    }
}

int phaseSweepAnalysis(Circuit& circuit, const std::string& sourceName, double base_freq, double start_phase, double stop_phase, int num_points) {
    try {
        cout << "// Performing Phase Sweep Analysis..." << endl;
        circuit.clearComponentHistory();
        ACVoltageSource* acSource = circuit.findACVoltageSource(sourceName);
        if (!acSource) {
            cerr << "Error: AC source '" << sourceName << "' not found." << endl;
            return 0;
        }

        vector<Node*> nonGroundNodes;
        for (auto* node : circuit.nodes) {
            if (!node->isGround) nonGroundNodes.push_back(node);
        }

        double originalPhase = acSource->phase; // Save original phase
        int pointsCalculated = 0;

        for (int i = 0; i < num_points; ++i) {
            double current_phase = (num_points == 1) ? start_phase : start_phase + i * (stop_phase - start_phase) / (num_points - 1);
            acSource->phase = current_phase;

            circuit.set_MNA_A(AnalysisType::AC_SWEEP, base_freq);
            circuit.set_MNA_RHS(AnalysisType::AC_SWEEP, base_freq);

            vector<complex<double>> solution = gaussianElimination(circuit.MNA_A_Complex, circuit.MNA_RHS_Complex);

            for (size_t j = 0; j < nonGroundNodes.size(); ++j) {
                 if(j < solution.size()) {
                    nonGroundNodes[j]->phase_sweep_history.push_back({current_phase, abs(solution[j])});
                }
            }
            pointsCalculated++;
        }
        
        acSource->phase = originalPhase; // Restore original phase
        cout << "// Phase Sweep Analysis complete." << endl;
        return pointsCalculated;
    } catch (const std::exception& e) {
        cerr << "Critical error during Phase Sweep: " << e.what() << endl;
        return 0;
    }
}

void result_from_vec(Circuit& circuit, const vector<double>& solvedVoltages, const vector<Node*>& nonGroundNodes) {
    if (solvedVoltages.empty()) {
        throw std::runtime_error("Solver returned an empty solution vector.");
    }
    if (solvedVoltages.size() < nonGroundNodes.size()) {
        throw std::runtime_error("Solution vector size is smaller than the number of non-ground nodes.");
    }
    for (size_t i = 0; i < nonGroundNodes.size(); ++i) {
        nonGroundNodes[i]->setVoltage(solvedVoltages[i]);
    }

    int current_idx_offset = nonGroundNodes.size();

    for(size_t i = 0; i < circuit.voltageSources.size(); ++i) {
        if (current_idx_offset + i < solvedVoltages.size()) {
            circuit.voltageSources[i].setCurrent(solvedVoltages[current_idx_offset + i]);
        }
    }
    current_idx_offset += circuit.voltageSources.size();

    for(size_t i = 0; i < circuit.inductors.size(); ++i) {
        if (current_idx_offset + i < solvedVoltages.size()) {
            circuit.inductors[i].setInductorCurrent(solvedVoltages[current_idx_offset + i]);
        }
    }
    current_idx_offset += circuit.inductors.size();

    for (auto& diode : circuit.diodes) {
        if (diode.getState() != STATE_OFF) {
            int idx = diode.getBranchIndex();
             if (idx != -1 && static_cast<size_t>(idx) < solvedVoltages.size()) {
                diode.setCurrent(solvedVoltages[idx]);
            }
        } else {
            diode.setCurrent(0.0);
        }
    }
}

