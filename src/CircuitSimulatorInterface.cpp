#include "CircuitSimulatorInterface.h"
#include "Analysis.h"
#include <cstring>
#include <string>
#include <sstream>
#include <iostream>

// --- HELPER FUNCTION ---
// Safely copies a C++ string to a C-style char buffer provided by C#
void safeStringCopy(char* buffer, int bufferSize, const std::string& source) {
    if (buffer && bufferSize > 0) {
        if (source.length() < static_cast<size_t>(bufferSize)) {
            strcpy_s(buffer, bufferSize, source.c_str());
        } else {
            buffer[0] = '\0'; // Indicate error or insufficient buffer
        }
    }
}

extern "C" {
    void* CreateCircuit() {
        try {
            return new Circuit();
        } catch (...) {
            return nullptr;
        }
    }

    void DestroyCircuit(void* circuit) {
        try {
            if (circuit) {
                delete static_cast<Circuit*>(circuit);
            }
        } catch (...) {
            // Fail silently
        }
    }

    void AddNode(void* circuit, const char* name) {
        if (!circuit || !name) return;
        try {
            static_cast<Circuit*>(circuit)->addNode(name);
        } catch (...) {}
    }

    void AddResistor(void* circuit, const char* name, const char* node1, const char* node2, double value) {
        if (!circuit || !name || !node1 || !node2) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* n1 = c->findOrCreateNode(node1);
            Node* n2 = c->findOrCreateNode(node2);
            c->resistors.emplace_back();
            Resistor& resistor = c->resistors.back();
            resistor.name = name;
            resistor.node1 = n1;
            resistor.node2 = n2;
            resistor.resistance = value;
        } catch (...) {}
    }

    void AddCapacitor(void* circuit, const char* name, const char* node1, const char* node2, double value) {
        if (!circuit || !name || !node1 || !node2) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* n1 = c->findOrCreateNode(node1);
            Node* n2 = c->findOrCreateNode(node2);
            c->capacitors.emplace_back();
            Capacitor& capacitor = c->capacitors.back();
            capacitor.name = name;
            capacitor.node1 = n1;
            capacitor.node2 = n2;
            capacitor.capacitance = value;
        } catch (...) {}
    }

    void AddInductor(void* circuit, const char* name, const char* node1, const char* node2, double value) {
        if (!circuit || !name || !node1 || !node2) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* n1 = c->findOrCreateNode(node1);
            Node* n2 = c->findOrCreateNode(node2);
            c->inductors.emplace_back();
            Inductor& inductor = c->inductors.back();
            inductor.name = name;
            inductor.node1 = n1;
            inductor.node2 = n2;
            inductor.inductance = value;
        } catch (...) {}
    }

    void AddVoltageSource(void* circuit, const char* name, const char* node1, const char* node2, double voltage) {
        if (!circuit || !name || !node1 || !node2) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* n1 = c->findOrCreateNode(node1);
            Node* n2 = c->findOrCreateNode(node2);
            c->voltageSources.emplace_back();
            VoltageSource& vs = c->voltageSources.back();
            vs.name = name;
            vs.node1 = n1;
            vs.node2 = n2;
            vs.value = voltage;
        } catch (...) {}
    }
    
    void AddACVoltageSource(void* circuit, const char* name, const char* node1, const char* node2, double magnitude, double phase) {
        if (!circuit || !name || !node1 || !node2) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* n1 = c->findOrCreateNode(node1);
            Node* n2 = c->findOrCreateNode(node2);
            c->acVoltageSources.emplace_back();
            ACVoltageSource& ac_vs = c->acVoltageSources.back();
            ac_vs.name = name;
            ac_vs.node1 = n1;
            ac_vs.node2 = n2;
            ac_vs.magnitude = magnitude;
            ac_vs.phase = phase;
        } catch (...) {}
    }

    void SetGroundNode(void* circuit, const char* nodeName) {
        if (!circuit || !nodeName) return;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            Node* node = c->findOrCreateNode(nodeName);
            if (node) {
                node->setGround(true);
            }
        } catch (...) {}
    }

    bool RunDCAnalysis(void* circuit) {
        if (!circuit) return false;
        try {
            dcAnalysis(*static_cast<Circuit*>(circuit));
            return true;
        } catch (const std::exception& e) {
            std::cerr << "DC Analysis Exception: " << e.what() << std::endl;
            return false;
        } catch (...) {
            std::cerr << "Unknown exception in DC Analysis." << std::endl;
            return false;
        }
    }

    bool RunTransientAnalysis(void* circuit, double stepTime, double stopTime) {
        if (!circuit) return false;
        try {
            transientAnalysis(*static_cast<Circuit*>(circuit), stepTime, stopTime);
            return true;
        } catch (const std::exception& e) {
            std::cerr << "Transient Analysis Exception: " << e.what() << std::endl;
            return false;
        } catch (...) {
            std::cerr << "Unknown exception in Transient Analysis." << std::endl;
            return false;
        }
    }

    bool RunACAnalysis(void* circuit, const char* sourceName, double startFreq, double stopFreq, int numPoints, const char* sweepType) {
        if (!circuit || !sourceName || !sweepType) return false;
        try {
            acSweepAnalysis(*static_cast<Circuit*>(circuit), sourceName, startFreq, stopFreq, numPoints, sweepType);
            return true;
        } catch (const std::exception& e) {
            std::cerr << "AC Analysis Exception: " << e.what() << std::endl;
            return false;
        } catch (...) {
            std::cerr << "Unknown exception in AC Analysis." << std::endl;
            return false;
        }
    }

    bool RunPhaseSweepAnalysis(void* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints) {
        if (!circuit || !sourceName) return false;
        try {
            phaseSweepAnalysis(*static_cast<Circuit*>(circuit), sourceName, baseFreq, startPhase, stopPhase, numPoints);
            return true;
        } catch (const std::exception& e) {
            std::cerr << "Phase Sweep Exception: " << e.what() << std::endl;
            return false;
        } catch (...) {
            std::cerr << "Unknown exception in Phase Sweep." << std::endl;
            return false;
        }
    }

    double GetNodeVoltage(void* circuit, const char* nodeName) {
        if (!circuit || !nodeName) return 0.0;
        try {
            Node* node = static_cast<Circuit*>(circuit)->findNode(nodeName);
            return node ? node->getVoltage() : 0.0;
        } catch (...) {
            return 0.0;
        }
    }

    int GetNodeNames(void* circuit, char* nodeNamesBuffer, int bufferSize) {
        if (!circuit || !nodeNamesBuffer || bufferSize <= 0) return 0;
        try {
            auto* c = static_cast<Circuit*>(circuit);
            std::stringstream ss;
            bool first = true;
            for (const auto& node : c->nodes) {
                if (node && !node->isGround) {
                    if (!first) ss << ",";
                    ss << node->name;
                    first = false;
                }
            }
            std::string allNames = ss.str();
            safeStringCopy(nodeNamesBuffer, bufferSize, allNames);
            return static_cast<int>(allNames.length());
        } catch (...) {
            return 0;
        }
    }

    int GetNodeVoltageHistory(void* circuit, const char* nodeName, double* timePoints, double* voltages, int maxCount) {
        if (!circuit || !nodeName || !timePoints || !voltages || maxCount <= 0) return 0;
        try {
            Node* node = static_cast<Circuit*>(circuit)->findNode(nodeName);
            if (node) {
                int count = 0;
                for (const auto& point : node->voltage_history) {
                    if (count >= maxCount) break;
                    timePoints[count] = point.first;
                    voltages[count] = point.second;
                    count++;
                }
                return count;
            }
            return 0;
        } catch (...) {
            return 0;
        }
    }

    int GetNodeSweepHistory(void* circuit, const char* nodeName, double* frequencies, double* magnitudes, int maxCount) {
        if (!circuit || !nodeName || !frequencies || !magnitudes || maxCount <= 0) return 0;
        try {
            Node* node = static_cast<Circuit*>(circuit)->findNode(nodeName);
            if (node) {
                int count = 0;
                for (const auto& point : node->ac_sweep_history) {
                    if (count >= maxCount) break;
                    frequencies[count] = point.first;
                    magnitudes[count] = point.second;
                    count++;
                }
                return count;
            }
            return 0;
        } catch (...) {
            return 0;
        }
    }

    int GetNodePhaseSweepHistory(void* circuit, const char* nodeName, double* phases, double* magnitudes, int maxCount) {
        if (!circuit || !nodeName || !phases || !magnitudes || maxCount <= 0) return 0;
        try {
            Node* node = static_cast<Circuit*>(circuit)->findNode(nodeName);
            if (node) {
                int count = 0;
                for (const auto& point : node->phase_sweep_history) {
                    if (count >= maxCount) break;
                    phases[count] = point.first;
                    magnitudes[count] = point.second;
                    count++;
                }
                return count;
            }
            return 0;
        } catch (...) {
            return 0;
        }
    }

    int GetComponentCurrentHistory(void* circuit, const char* componentName, double* timePoints, double* currents, int maxCount) {
        if (!circuit || !componentName || !timePoints || !currents || maxCount <= 0) return 0;
        try {
            Circuit* c = static_cast<Circuit*>(circuit);
            VoltageSource* vs = c->findVoltageSource(componentName);
            if (vs) {
                int count = 0;
                for (const auto& point : vs->current_history) {
                    if (count >= maxCount) break;
                    timePoints[count] = point.first;
                    currents[count] = point.second;
                    count++;
                }
                return count;
            }
            return 0;
        } catch (...) {
            return 0;
        }
    }

    int GetAllVoltageSourceNames(void* circuit, char* vsNamesBuffer, int bufferSize) {
        if (!circuit || !vsNamesBuffer || bufferSize <= 0) return 0;
        try {
            auto* c = static_cast<Circuit*>(circuit);
            std::stringstream ss;
            bool first = true;
            for (const auto& vs : c->voltageSources) {
                if (!first) ss << ",";
                ss << vs.name;
                first = false;
            }
            std::string allNames = ss.str();
            safeStringCopy(vsNamesBuffer, bufferSize, allNames);
            return static_cast<int>(allNames.length());
        } catch (...) {
            return 0;
        }
    }

    double GetVoltageSourceCurrent(void* circuit, const char* vsName) {
        if (!circuit || !vsName) return 0.0;
        try {
            VoltageSource* vs = static_cast<Circuit*>(circuit)->findVoltageSource(vsName);
            return vs ? vs->getCurrent() : 0.0;
        } catch (...) {
            return 0.0;
        }
    }
}

