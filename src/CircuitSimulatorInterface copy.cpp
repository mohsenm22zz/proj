#include "CircuitSimulatorInterface.h"
#include <iostream>
#include "Analysis.h"
#include <vector>
#include <string>
#include <cstring> // For strncpy

// Helper function to concatenate strings for C# marshalling
static void concatenate_strings(const std::vector<std::string>& names, char* buffer, int bufferSize) {
    if (!buffer || bufferSize <= 0) return;
    
    std::string result = "";
    for (size_t i = 0; i < names.size(); ++i) {
        result += names[i];
        if (i < names.size() - 1) {
            result += ",";
        }
    }

    strncpy(buffer, result.c_str(), bufferSize - 1);
    buffer[bufferSize - 1] = '\0'; // Ensure null termination
}

extern "C" {

Circuit* __cdecl CreateCircuit() {
    return new Circuit();
}

void __cdecl DestroyCircuit(Circuit* circuit) {
    delete circuit;
}

void __cdecl AddResistor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value) {
    Resistor r;
    r.name = name;
    r.node1 = circuit->findOrCreateNode(node1);
    r.node2 = circuit->findOrCreateNode(node2);
    r.resistance = value;
    circuit->resistors.push_back(r);
}

void __cdecl AddCapacitor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value) {
   Capacitor c;
    c.name = name;
    c.node1 = circuit->findOrCreateNode(node1);
    c.node2 = circuit->findOrCreateNode(node2);
    c.capacitance = value;
    circuit->capacitors.push_back(c);
}

void __cdecl AddInductor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value) {
   Inductor l;
    l.name = name;
    l.node1 = circuit->findOrCreateNode(node1);
    l.node2 = circuit->findOrCreateNode(node2);
    l.inductance = value;
    circuit->inductors.push_back(l);
}

void __cdecl AddVoltageSource(Circuit* circuit, const char* name, const char* node1, const char* node2, double voltage) {
  VoltageSource v;
    v.name = name;
    v.node1 = circuit->findOrCreateNode(node1);
    v.node2 = circuit->findOrCreateNode(node2);
    v.value = voltage;
    circuit->voltageSources.push_back(v);
}

void __cdecl AddACVoltageSource(Circuit* circuit, const char* name, const char* node1, const char* node2, double magnitude, double phase) {
    ACVoltageSource ac;
    ac.name = name;
    ac.node1 = circuit->findOrCreateNode(node1);
    ac.node2 = circuit->findOrCreateNode(node2);
    ac.magnitude = magnitude;
    ac.phase = phase;
    circuit->acVoltageSources.push_back(ac);
}

void __cdecl SetGroundNode(Circuit* circuit, const char* nodeName) {
    Node *gndNode = circuit->findOrCreateNode(nodeName);
    gndNode->setGround(true);
    if (find(circuit->groundNodeNames.begin(), circuit->groundNodeNames.end(), nodeName) ==
        circuit->groundNodeNames.end()) {
        circuit->groundNodeNames.push_back(nodeName);
    }
}

bool __cdecl RunDCAnalysis(Circuit* circuit) {
    if (circuit) {
        return dcAnalysis(*circuit);
    }
    return false;
}

bool __cdecl RunTransientAnalysis(Circuit* circuit, double stepTime, double stopTime) {
    if (circuit) {
        return transientAnalysis(*circuit, stepTime, stopTime);
    }
    return false;
}

// int __cdecl RunACAnalysis(Circuit* circuit, const char* sourceName, double startFreq, double stopFreq, int numPoints, const char* sweepType) {
//     if (circuit) {
//         return acAnalysis(*circuit, sourceName, startFreq, stopFreq, numPoints, sweepType);
//     }
//     return 0;
// }

bool __cdecl RunPhaseSweepAnalysis(Circuit* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints) {
    if (circuit) {
        return phaseSweepAnalysis(*circuit, sourceName, baseFreq, startPhase, stopPhase, numPoints);
    }
    return false;
}


double __cdecl GetNodeVoltage(void* circuit, const char* nodeName) {
    if (circuit) return static_cast<Circuit*>(circuit)->findOrCreateNode(nodeName)->getVoltage();
    return 0.0;///todo
}

double __cdecl GetVoltageSourceCurrent(void* circuit, const char* vsName) {
    if (circuit) {
        Component* comp = static_cast<Circuit*>(circuit)->findVoltageSource(vsName);
        if (comp && dynamic_cast<VoltageSource*>(comp)) {
            return static_cast<VoltageSource*>(comp)->getCurrent();
        }
    }
    return 0.0;
}


int __cdecl GetNodeNames(void* circuit, char* buffer, int bufferSize) {
    if (!circuit || !buffer) return 0;
    auto nodeNames = static_cast<Circuit*>(circuit)->();
    concatenate_strings(nodeNames, buffer, bufferSize);
    return static_cast<int>(strlen(buffer));
}

CS_API int __cdecl GetAllVoltageSourceNames(void* circuit, char* buffer, int bufferSize) {
    if (!circuit || !buffer) return 0;
    auto vsNames = static_cast<Circuit*>(circuit)->getVoltageSourceNames();
    concatenate_strings(vsNames, buffer, bufferSize);
    return static_cast<int>(strlen(buffer));
}


int __cdecl GetNodeVoltageHistory(Circuit* circuit, const char* nodeName, double* timePoints, double* voltages, int maxCount) {
    if (!circuit) return 0;
    const auto& history = circuit->getNodeVoltageHistory(nodeName);
    int count = std::min((int)history.size(), maxCount);
    for (int i = 0; i < count; ++i) {
        timePoints[i] = history[i].first;
        voltages[i] = history[i].second;
    }
    return count;
}

int __cdecl GetComponentCurrentHistory(Circuit* circuit, const char* componentName, double* timePoints, double* currents, int maxCount) {
    if (!circuit) return 0;
    const auto& history = circuit->getComponentCurrentHistory(componentName);
    int count = std::min((int)history.size(), maxCount);
    for (int i = 0; i < count; ++i) {
        timePoints[i] = history[i].first;
        currents[i] = history[i].second;
    }
    return count;
}

int __cdecl GetNodeSweepHistory(Circuit* circuit, const char* nodeName, double* frequencies, double* magnitudes, int maxCount) {
    if (!circuit) return 0;
    const auto& history = circuit->getNodeSweepHistory(nodeName);
    int count = std::min((int)history.size(), maxCount);
    for (int i = 0; i < count; ++i) {
        frequencies[i] = history[i].first;
        magnitudes[i] = history[i].second;
    }
    return count;
}

int __cdecl GetNodePhaseSweepHistory(Circuit* circuit, const char* nodeName, double* phases, double* magnitudes, int maxCount) {
    if (!circuit) return 0;
    const auto& history = circuit->getNodePhaseSweepHistory(nodeName);
    int count = std::min((int)history.size(), maxCount);
    for (int i = 0; i < count; ++i) {
        phases[i] = history[i].first;
        magnitudes[i] = history[i].second;
    }
    return count;
}

} // extern "C"
