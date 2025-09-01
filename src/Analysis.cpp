#include "Analysis.h"
#include "LinearSolver.h"
#include "Node.h"
#include <iostream>
#include <vector>
#include <iomanip>
#include <cmath>
#include <complex>

using namespace std;

void result_from_vec(Circuit& circuit, const vector<double>& solvedVoltages, const vector<Node*>& nonGroundNodes);

void dcAnalysis(Circuit& circuit) {
    cout << "// Performing DC Analysis..." << endl;
    circuit.setDeltaT(1e12);
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

    // Create a copy of the MNA_A matrix for reuse
    vector<vector<double>> a_matrix_copy;

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

        if (circuit.MNA_A.empty() || circuit.MNA_A[0].empty() || circuit.MNA_RHS.empty() || circuit.MNA_A.size() != circuit.MNA_RHS.size()) {
            cout << "// No solvable MNA system for the current circuit state." << endl;
            break;
        }

        // Save a copy of the A matrix for later reuse in sweeps
        if (a_matrix_copy.empty()) {
            a_matrix_copy = circuit.MNA_A;
        }

        vector<double> solved_solution;
        try {
            solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
        } catch (const exception& e) {
            cerr << "Error during Gaussian Elimination: " << e.what() << endl;
            converged = false;
            break;
        }

        result_from_vec(circuit, solved_solution, nonGroundNodes);

        for (size_t i = 0; i < circuit.diodes.size(); ++i) {
            Diode& current_diode = circuit.diodes[i];
            DiodeState old_state = previous_diode_states[i];

            double v_anode = current_diode.node1->getVoltage();
            double v_cathode = current_diode.node2->getVoltage();
            double v_diode_across = v_anode - v_cathode;
            DiodeState new_state = old_state;

            if (current_diode.getDiodeType() == NORMAL) {
                if (old_state == STATE_OFF) {
                    if (v_diode_across >= current_diode.getForwardVoltage() - EPSILON_CURRENT) {
                        new_state = STATE_FORWARD_ON;
                    }
                } else if (old_state == STATE_FORWARD_ON) {
                    if (current_diode.getCurrent() < -EPSILON_CURRENT) {
                        new_state = STATE_OFF;
                    }
                }
            } else if (current_diode.getDiodeType() == ZENER) {
                if (old_state == STATE_OFF) {
                    if (v_diode_across >= current_diode.getForwardVoltage() - EPSILON_CURRENT) {
                        new_state = STATE_FORWARD_ON;
                    } else if (v_diode_across <= -current_diode.getZenerVoltage() + EPSILON_CURRENT) {
                        new_state = STATE_REVERSE_ON;
                    }
                } else if (old_state == STATE_FORWARD_ON) {
                    if (current_diode.getCurrent() < -EPSILON_CURRENT) {
                        new_state = STATE_OFF;
                    }
                } else if (old_state == STATE_REVERSE_ON) {
                    if (current_diode.getCurrent() > EPSILON_CURRENT) {
                        new_state = STATE_OFF;
                    }
                }
            }

            if (new_state != old_state) {
                converged = false;
                current_diode.setState(new_state);
            }
        }

    } while (!converged && iteration_count < MAX_DIODE_ITERATIONS);

    if (!converged) {
        cerr << "Warning: DC Analysis did not converge after " << MAX_DIODE_ITERATIONS << " iterations for diodes." << endl;
    }

    cout << "// DC Analysis complete." << endl;
}


void transientAnalysis(Circuit& circuit, double t_step, double t_stop) {
    cout << "// Performing Transient Analysis..." << endl;
    circuit.clearComponentHistory();

    dcAnalysis(circuit);

    for (auto& cap : circuit.capacitors) {
        cap.prevVoltage = 0.0;
    }
    for (auto& ind : circuit.inductors) {
        ind.prevCurrent = 0.0;
    }

    circuit.setDeltaT(t_step);

    vector<Node*> nonGroundNodes;
    for (auto* node : circuit.nodes) {
        if (!node->isGround) {
            nonGroundNodes.push_back(node);
        }
    }

    // Add initial conditions at t=0
    for (auto* node : circuit.nodes) {
        if (!node->isGround) {
            node->addVoltageHistoryPoint(0.0, node->getVoltage());
        }
    }
    for (auto& vs : circuit.voltageSources) {
        vs.addCurrentHistoryPoint(0.0, vs.getCurrent());
    }

    for (double t = t_step; t <= t_stop; t += t_step) {
        const int MAX_DIODE_ITERATIONS = 100;
        const double EPSILON_CURRENT = 1e-9;
        bool converged = false;
        int iteration_count = 0;

        do {
            converged = true;
            iteration_count++;

            vector<DiodeState> previous_diode_states;
            for (const auto& diode : circuit.diodes) {
                previous_diode_states.push_back(diode.getState());
            }

            circuit.assignDiodeBranchIndices();
            circuit.set_MNA_A(AnalysisType::TRANSIENT);
            circuit.set_MNA_RHS(AnalysisType::TRANSIENT);

            vector<double> solved_solution;
            try {
                solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
            } catch (const exception& e) {
                cerr << "Error during Gaussian Elimination at t=" << t << ": " << e.what() << endl;
                break;
            }

            result_from_vec(circuit, solved_solution, nonGroundNodes);

            for (size_t i = 0; i < circuit.diodes.size(); ++i) {
                Diode& current_diode = circuit.diodes[i];
                DiodeState old_state = previous_diode_states[i];
                DiodeState new_state = old_state;

                double v_anode = current_diode.node1->getVoltage();
                double v_cathode = current_diode.node2->getVoltage();
                double v_diode_across = v_anode - v_cathode;

                if (current_diode.getDiodeType() == NORMAL) {
                    if (old_state == STATE_OFF) {
                        if (v_diode_across >= current_diode.getForwardVoltage() - EPSILON_CURRENT) {
                            new_state = STATE_FORWARD_ON;
                        }
                    } else if (old_state == STATE_FORWARD_ON) {
                        if (current_diode.getCurrent() < -EPSILON_CURRENT) {
                            new_state = STATE_OFF;
                        }
                    }
                } else if (current_diode.getDiodeType() == ZENER) {
                    if (old_state == STATE_OFF) {
                        if (v_diode_across >= current_diode.getForwardVoltage() - EPSILON_CURRENT) {
                            new_state = STATE_FORWARD_ON;
                        } else if (v_diode_across <= -current_diode.getZenerVoltage() + EPSILON_CURRENT) {
                            new_state = STATE_REVERSE_ON;
                        }
                    } else if (old_state == STATE_FORWARD_ON) {
                        if (current_diode.getCurrent() < -EPSILON_CURRENT) {
                            new_state = STATE_OFF;
                        }
                    } else if (old_state == STATE_REVERSE_ON) {
                        if (current_diode.getCurrent() > EPSILON_CURRENT) {
                            new_state = STATE_OFF;
                        }
                    }
                }

                if (new_state != old_state) {
                    converged = false;
                    current_diode.setState(new_state);
                }
            }
        } while (!converged && iteration_count < MAX_DIODE_ITERATIONS);

        if (!converged) {
            cerr << "Warning: Diode states did not converge at t=" << t << endl;
        }

        for (auto* node : circuit.nodes) {
            if (!node->isGround) {
                node->addVoltageHistoryPoint(t, node->getVoltage());
            }
        }
        for (auto& vs : circuit.voltageSources) {
            vs.addCurrentHistoryPoint(t, vs.getCurrent());
        }

        circuit.updateComponentStates();
    }
    cout << "// Transient Analysis complete." << endl;
}


void result_from_vec(Circuit& circuit, const vector<double>& solvedVoltages, const vector<Node*>& nonGroundNodes) {
    if (solvedVoltages.size() < nonGroundNodes.size()) {
        cerr << "Error: Solution vector size mismatch." << endl;
        return;
    }
    for (size_t i = 0; i < nonGroundNodes.size(); ++i) {
        nonGroundNodes[i]->setVoltage(solvedVoltages[i]);
    }

    int current_idx_offset = nonGroundNodes.size();

    for(size_t i = 0; i < circuit.voltageSources.size(); ++i) {
        circuit.voltageSources[i].setCurrent(solvedVoltages[current_idx_offset + i]);
    }
    current_idx_offset += circuit.voltageSources.size();

    for(size_t i = 0; i < circuit.inductors.size(); ++i) {
        circuit.inductors[i].setInductorCurrent(solvedVoltages[current_idx_offset + i]);
    }
    current_idx_offset += circuit.inductors.size();

    for (auto& diode : circuit.diodes) {
        if (diode.getState() == STATE_FORWARD_ON || diode.getState() == STATE_REVERSE_ON) {
            int diode_solution_idx = diode.getBranchIndex();
            if (diode_solution_idx != -1 && diode_solution_idx >= 0 && static_cast<size_t>(diode_solution_idx) < solvedVoltages.size()) {
                diode.setCurrent(solvedVoltages[diode_solution_idx]);
            } else {
                cerr << "Warning: Diode " << diode.name << " has invalid branch index or solution size mismatch. Cannot set current." << endl;
            }
        }
    }
}

void dcSweepAnalysis(Circuit& circuit, const string& sourceName, double start, double end, double step) {
    Component* sweepSource = nullptr;
    char sourceType = toupper(sourceName[0]);
    if (sourceType == 'V') sweepSource = circuit.findVoltageSource(sourceName);
    else if (sourceType == 'I') sweepSource = circuit.findCurrentSource(sourceName);

    if (!sweepSource) {
        cerr << "Error: Sweep source '" << sourceName << "' not found." << endl;
        return;
    }

    double originalValue = 0.0;
    if (sourceType == 'V') originalValue = static_cast<VoltageSource*>(sweepSource)->value;
    else if (sourceType == 'I') originalValue = static_cast<CurrentSource*>(sweepSource)->value;

    // Perform initial DC analysis to get the base MNA_A matrix
    dcAnalysis(circuit);
    
    // Save a copy of the A matrix for reuse in sweeps
    vector<vector<double>> a_matrix_copy = circuit.MNA_A;

    vector<Node*> nonGroundNodes;
    for (auto* node : circuit.nodes) {
        if (!node->isGround) {
            nonGroundNodes.push_back(node);
        }
    }

    for (double value = start; value <= end; value += step) {
        if (sourceType == 'V') static_cast<VoltageSource*>(sweepSource)->value = value;
        else if (sourceType == 'I') static_cast<CurrentSource*>(sweepSource)->value = value;
        
        circuit.set_MNA_RHS(AnalysisType::DC); 

        vector<double> solved_solution;
        try {
            solved_solution = gaussianElimination(a_matrix_copy, circuit.MNA_RHS);
        } catch (const exception& e) {
            cerr << "Error during Gaussian Elimination at sweep value " << value << ": " << e.what() << endl;
            continue;
        }

        result_from_vec(circuit, solved_solution, nonGroundNodes);

        for (auto* node : circuit.nodes) {
            if (!node->isGround) {
                node->dc_sweep_history.push_back({value, node->getVoltage()});
            }
        }
        for (auto& vs : circuit.voltageSources) {
            vs.dc_sweep_current_history.push_back({value, vs.getCurrent()});
        }
    }
    
    // Restore original value
    if (sourceType == 'V') static_cast<VoltageSource*>(sweepSource)->value = originalValue;
    else if (sourceType == 'I') static_cast<CurrentSource*>(sweepSource)->value = originalValue;
    cout << "// DC Sweep Analysis complete." << endl;
}

void acSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start_freq, double stop_freq, int num_points, const std::string& sweep_type) {
    cout << "// Performing AC Sweep Analysis..." << endl;
    circuit.clearComponentHistory();

    ACVoltageSource* acSource = circuit.findACVoltageSource(sourceName);
    if (!acSource) {
        cerr << "Error: AC source '" << sourceName << "' not found for sweep." << endl;
        return;
    }

    vector<Node*> nonGroundNodes;
    for (auto* node : circuit.nodes) {
        if (!node->isGround) {
            nonGroundNodes.push_back(node);
        }
    }
    
    if (num_points < 2) num_points = 2;

    for (int i = 0; i < num_points; ++i) {
        double current_freq;
        if (sweep_type == "Logarithmic") {
            current_freq = start_freq * pow(stop_freq / start_freq, (double)i / (double)(num_points - 1));
        } else { // Default to Linear
             current_freq = start_freq + i * (stop_freq - start_freq) / (double)(num_points - 1);
        }
       
        if (current_freq <= 1e-9) continue; 

        circuit.set_MNA_A(AnalysisType::AC_SWEEP, current_freq);
        circuit.set_MNA_RHS(AnalysisType::AC_SWEEP, current_freq);

        vector<complex<double>> solution;
        try {
            solution = gaussianElimination(circuit.MNA_A_Complex, circuit.MNA_RHS_Complex);
        } catch (const exception& e) {
            cerr << "Error during AC analysis at frequency " << current_freq << " Hz: " << e.what() << endl;
            continue; // Skip to the next frequency point
        }

        // 3. Store the results (magnitude of voltage/current)
        for (size_t j = 0; j < nonGroundNodes.size(); ++j) {
            if (j < solution.size()) {
                double magnitude = abs(solution[j]);
                nonGroundNodes[j]->ac_sweep_history.push_back({current_freq, magnitude});
            }
        }
        // You would also store current magnitudes for components here
    }

    cout << "// AC Sweep Analysis complete." << endl;
}

// --- NEW (Skeleton) ---
// Implementation for Phase Sweep would be similar
void phaseSweepAnalysis(Circuit& circuit, const std::string& sourceName, double base_freq, double start_phase, double stop_phase, int num_points) {
    cout << "// Performing Phase Sweep Analysis..." << endl;
    circuit.clearComponentHistory();
    ACVoltageSource* acSource = circuit.findACVoltageSource(sourceName);
    if (!acSource) {
        cerr << "Error: AC source '" << sourceName << "' not found for sweep." << endl;
        return;
    }
    
    vector<Node*> nonGroundNodes;
    for (auto* node : circuit.nodes) {
        if (!node->isGround) {
            nonGroundNodes.push_back(node);
        }
    }

    if (num_points < 2) num_points = 2;
    if (base_freq <= 1e-9) {
        cerr << "Error: Base frequency for phase sweep must be positive." << endl;
        return;
    }

    circuit.set_MNA_A(AnalysisType::AC_SWEEP, base_freq);

    double originalPhase = acSource->phase;

    for (int i = 0; i < num_points; ++i) {
        double current_phase = start_phase + i * (stop_phase - start_phase) / (double)(num_points - 1);
        acSource->phase = current_phase;

        circuit.set_MNA_RHS(AnalysisType::AC_SWEEP, base_freq);

        vector<complex<double>> solution;
        try {
            solution = gaussianElimination(circuit.MNA_A_Complex, circuit.MNA_RHS_Complex);
        } catch (const exception& e) {
            cerr << "Error during Phase analysis at phase " << current_phase << " deg: " << e.what() << endl;
            continue;
        }

        for (size_t j = 0; j < nonGroundNodes.size(); ++j) {
             if (j < solution.size()) {
                double magnitude = abs(solution[j]);
                nonGroundNodes[j]->phase_sweep_history.push_back({current_phase, magnitude});
             }
        }
    }
    
    acSource->phase = originalPhase; 
    cout << "// Phase Sweep Analysis complete." << endl;
}