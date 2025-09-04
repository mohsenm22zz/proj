#include "Analysis.h"
#include "LinearSolver.h"
#include "Node.h"
#include <iostream>
#include <vector>
#include <iomanip>
#include <cmath>
#include <complex>
#include <stdexcept>
#include <fstream>

using namespace std;

void result_from_vec(Circuit& circuit, const vector<double>& solvedVoltages, const vector<Node*>& nonGroundNodes);

void WriteToFile(const std::string& content)
{
    const char* filePath = "output.txt";

    // Open file in append mode
    std::ofstream outFile(filePath, std::ios::out | std::ios::app);  // Using std::ios::app for append mode
    
    if (outFile.is_open())
    {
        outFile << content << std::endl;  // Write the content to the file, add a newline
        outFile.close();                  // Close the file after writing
        std::cout << "File written successfully!" << std::endl;
    }
    else
    {
        std::cerr << "Unable to open file!" << std::endl;
    }
}
// void WriteToFile(const string content)
// {
//     const char* filePath = "output.txt";
//     std::ofstream outFile(filePath, std::ios::out);
//     if (outFile.is_open())
//     {
//         outFile << content;  // Write the content to the file
//         outFile.close();     // Close the file after writing
//         std::cout << "File written successfully!" << std::endl;
//     }
//     else
//     {
//         std::cerr << "Unable to open file!" << std::endl;
//     }
// }
bool dcAnalysis(Circuit& circuit) {
    WriteToFile("// Performing DC Analysis...");
    if (circuit.resistors.empty()) {
        WriteToFile("// No resistors found.");
    }
    for (auto it = circuit.resistors.begin(); it != circuit.resistors.end(); ++it) {
        WriteToFile("Analyzing resistor: "  + it->name + " with value: " + std::to_string(it->resistance));
    }
    for (auto it = circuit.voltageSources.begin(); it != circuit.voltageSources.end(); ++it) {
        WriteToFile("Analyzing voltage source: " + it->name + " with value: " + std::to_string(it->value));
    }
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
            WriteToFile(circuit.MNA_A.empty() ? "// MNA_A is empty." : "// MNA_A is not empty.");
            WriteToFile(circuit.MNA_RHS.empty() ? "// MNA_RHS is empty." : "// MNA_RHS is not empty.");
            WriteToFile("MNA_A size: " + std::to_string(circuit.MNA_A.size()));
            WriteToFile("MNA_RHS size: " + std::to_string(circuit.MNA_RHS.size()));
            WriteToFile("// No solvable MNA system for the current circuit state.");
            cout << "// No solvable MNA system for the current circuit state." << endl;
            return false;
        }

        for (auto it = circuit.MNA_A.begin(); it != circuit.MNA_A.end(); ++it) {
            for (auto jt = it->begin(); jt != it->end(); ++jt) {
                    (to_string(*jt) + " ");
            }
            WriteToFile("\n");
        }
        for (auto it = circuit.MNA_RHS.begin(); it != circuit.MNA_RHS.end(); ++it) {
            WriteToFile(to_string(*it));
            WriteToFile("\n");
        }
        vector<double> solved_solution;
        try {
            solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
        } catch (const exception& e) {
            WriteToFile("Error during Gaussian Elimination: " + std::string(e.what()));
            cerr << "Error during Gaussian Elimination: " << e.what() << endl;
            converged = false;
            return false;
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
        WriteToFile("Warning: DC Analysis did not converge after " + std::to_string(MAX_DIODE_ITERATIONS) + " iterations for diodes.");
        cerr << "Warning: DC Analysis did not converge after " << MAX_DIODE_ITERATIONS << " iterations for diodes." << endl;
        return false;
    }

    cout << "// DC Analysis complete." << endl;
    WriteToFile("// DC Analysis complete.");
    return true;
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
            circuit.currentTime = t;
            circuit.set_MNA_A(AnalysisType::TRANSIENT);
            circuit.set_MNA_RHS(AnalysisType::TRANSIENT);

            vector<double> solved_solution = gaussianElimination(circuit.MNA_A, circuit.MNA_RHS);
            
            result_from_vec(circuit, solved_solution, nonGroundNodes);

            for (auto* node : circuit.nodes) {
                if (!node->isGround) node->addVoltageHistoryPoint(t, node->getVoltage());
            }
             for (auto& r : circuit.resistors) {
          double r_current = (r.node1->getVoltage() - r.node2->getVoltage()) / r.resistance;
          r.setCurrent(r_current);
          r.addCurrentHistoryPoint(t, r_current);
        }
        for (auto& c : circuit.capacitors) {
            double c_current = c.capacitance * ((c.node1->getVoltage() - c.node2->getVoltage()) - c.prevVoltage) / t_step;
            c.setCurrent(c_current);
            c.addCurrentHistoryPoint(t, c_current);
        }
        for (auto& l : circuit.inductors) {
            l.addCurrentHistoryPoint(t, l.getCurrent()); // Current is already solved by MNA
        }
        for (auto& vs : circuit.voltageSources) {
            vs.addCurrentHistoryPoint(t, vs.getCurrent()); // Current is already solved by MNA
        }
        for (auto& ac_vs : circuit.acVoltageSources) {
            // NOTE: AC source current in transient is not being solved by MNA.
            // This would require modification to the MNA builder to treat it as a standard voltage source.
            // For now, we'll add a placeholder.
            ac_vs.addCurrentHistoryPoint(t, 0.0);
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

            for (int j = 0; j < nonGroundNodes.size(); ++j) {
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

            for (int j = 0; j < nonGroundNodes.size(); ++j) {
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
    for (int i = 0; i < nonGroundNodes.size(); ++i) {
        nonGroundNodes[i]->setVoltage(solvedVoltages[i]);
    }

    int current_idx_offset = nonGroundNodes.size();

    for(int i = 0; i < circuit.voltageSources.size(); ++i) {
        if (current_idx_offset + i < solvedVoltages.size()) {
            circuit.voltageSources[i].setCurrent(solvedVoltages[current_idx_offset + i]);
        }
    }
    current_idx_offset += circuit.voltageSources.size();

    for(int i = 0; i < circuit.inductors.size(); ++i) {
        if (current_idx_offset + i < solvedVoltages.size()) {
            circuit.inductors[i].setInductorCurrent(solvedVoltages[current_idx_offset + i]);
        }
    }
    current_idx_offset += circuit.inductors.size();

    for (auto& diode : circuit.diodes) {
        if (diode.getState() != STATE_OFF) {
            int idx = diode.getBranchIndex();
             if (idx != -1 && static_cast<int>(idx) < solvedVoltages.size()) {
                diode.setCurrent(solvedVoltages[idx]);
            }
        } else {
            diode.setCurrent(0.0);
        }
    }
}

