#include <iostream>
#include <iomanip>
#include <vector>
#include <sstream>
#include <string>
#include <algorithm>
#include <cstdlib>
#include "Circuit.h"
#include "Analysis.h"
#include "CircuitIO.h"
#include "LinearSolver.h"

using namespace std;

double stonum(const string &s);
void clrscrr();

int main() {
    /*//test_solver();
    while (true) {
        Circuit circuit;
        string line;
        vector<vector<string>> analysisCommands;

        cout << "---=== [mz-rjb Circuit Simulator] ===---" << endl;
        cout << "Enter circuit commands. Type '.run' to simulate or 'exit' to quit." << endl;

        while (getline(cin, line)) {
            if (line == "exit") break;

            istringstream iss(line);
            vector<string> cmds;
            string cmd_part;
            while (iss >> cmd_part) {
                cmds.push_back(cmd_part);
            }
            if (cmds[0] == ".print") {//TRAN <Tstep> <Tstop>
                string a_type = cmds[1];
                if (a_type == "TRAN") {
                    string Tstep = cmds[2];
                    string Tstop = cmds[3];
                    try {
                        double t_step = stonum(Tstep);
                        double t_stop = stonum(Tstop);
                        transientAnalysis(circuit, t_step, t_stop);
                        string name = cmds[4];
                        cout << fixed << setprecision(4);
                        if (name[0] == 'V') {/// V(name)
                            Node *node = circuit.findNode(name.substr(2, name.size() - 4));
                            if (node) {
                                for (const auto &point: node->voltage_history) {
                                    cout << "    Time: " << setw(8) << left << point.first
                                         << " Voltage: " << point.second << " V" << endl;
                                }
                            }
                        } else if (name[0] == 'I') {
                            string n = name.substr(2, name.size() - 4);
                            if (circuit.findCapacitor(n)) {
                                Capacitor *comp = circuit.findCapacitor(n);
                            } else if (circuit.findCurrentSource(n)) {
                                CurrentSource *comp = circuit.findCurrentSource(n);
                            } else if (circuit.findInductor(n)) {
                                Inductor *comp = circuit.findInductor(n);
                            } else if (circuit.findResistor(n)) {
                                Resistor *comp = circuit.findResistor(n);
                            } else if (circuit.findVoltageSource(n)) {
                                VoltageSource *comp = circuit.findVoltageSource(n);
                            }
                        }
                    }catch (const exception &e) {
                        cerr << "Error: " << e.what() << endl;
                    }
                }
            }
            if (!cmds.empty()) {
                if (!command_handling(circuit, cmds, analysisCommands)) break;
            }
        }

        if (line == "exit") break;

        clrscrr();
        cout << "---=== [Simulation Results] ===---" << endl;
        handleErrors(circuit);

        if (analysisCommands.empty()) {
            analysisCommands.push_back({".dc"});
        }

        for (const auto &cmd_parts: analysisCommands) {
            string analysisType = cmd_parts[0];
            transform(analysisType.begin(), analysisType.end(), analysisType.begin(), ::tolower);

            if (analysisType == ".dc") {
                if (cmd_parts.size() == 5) {
                    try {
                        string sourceName = cmd_parts[1];
                        transform(sourceName.begin(), sourceName.end(), sourceName.begin(), ::toupper);
                        double start = stonum(cmd_parts[2]);
                        double end = stonum(cmd_parts[3]);
                        double step = stonum(cmd_parts[4]);

                        if (step <= 0) {
                            cerr << "Error: Sweep step must be positive." << endl;
                            continue;
                        }

                        // Call the analysis function (it will populate the history vectors)
                        dcSweepAnalysis(circuit, sourceName, start, end, step);

                        // Now, print the results from the history vectors
                        cout << "\n// --- DC Sweep Analysis Results ---" << endl;
                        cout << fixed << setprecision(4);

                        // Print node voltage results
                        for (const auto* node : circuit.nodes) {
                            if (!node->isGround && !node->dc_sweep_history.empty()) {
                                cout << "  History for Node " << node->name << " (vs " << sourceName << "):" << endl;
                                for (const auto& point : node->dc_sweep_history) {
                                    cout << "    " << sourceName << " = " << setw(8) << left << point.first
                                         << " -> V(" << node->name << ") = " << point.second << " V" << endl;
                                }
                            }
                        }

                        for (const auto& vs : circuit.voltageSources) {
                            if (!vs.dc_sweep_current_history.empty()) {
                                cout << "  History for Current " << "I(" << vs.name << ") (vs " << sourceName << "):" << endl;
                                for (const auto& point : vs.dc_sweep_current_history) {
                                    cout << "    " << sourceName << " = " << setw(8) << left << point.first
                                         << " -> I(" << vs.name << ") = " << point.second << " A" << endl;
                                }
                            }
                        }
                        cout << "// --------------------------------" << endl;

                    } catch (const exception &e) {
                        cerr << "Error: Invalid parameters for .dc sweep. Format: .dc <source> <start> <end> <step>" << endl;
                    }
                }
                else {
                    dcAnalysis(circuit);
                    cout << fixed << setprecision(4);
                    cout << "\n// --- DC Analysis Results ---" << endl;
                    for (auto *node: circuit.nodes) {
                        if (!node->isGround) {
                            cout << "  V(" << node->name << ") = " << node->getVoltage() << " V" << endl;
                        }
                    }
                    for (auto &vs: circuit.voltageSources) {
                        cout << "  I(" << vs.name << ") = " << vs.getCurrent() << " A" << endl;
                    }
                    cout << "// --------------------------" << endl;
                }

            } else if (analysisType == ".tran") {
                if (cmd_parts.size() < 3) {
                    cerr << "Error: Format is '.tran <T_step> <T_stop>'" << endl;
                    continue;
                }
                try {
                    double t_step = stonum(cmd_parts[1]);
                    double t_stop = stonum(cmd_parts[2]);
                    transientAnalysis(circuit, t_step, t_stop);

                    cout << "\n// --- Stored History Example ---" << endl;
                    cout << fixed << setprecision(4);

                    for (const auto* node : circuit.nodes) {
                        if (!node->isGround) {
                            cout << "  History for Node " << node->name << ":" << endl;
                            for (const auto& point : node->voltage_history) {
                                cout << "    Time: " << setw(8) << left << point.first
                                     << " Voltage: " << point.second << " V" << endl;
                            }
                        }
                    }
                } catch (const exception &e) {
                    cerr << "Error: " << e.what() << endl;
                }
            }
        }
        cout << "\nSimulation finished. Circuit cleared." << endl;
    }*/
    return 0;
}