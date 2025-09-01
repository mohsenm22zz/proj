#pragma once

#include "export.h"
#include "Circuit.h"
#include <vector>
#include <string>

extern "C" {
    CIRCUITSIMULATOR_API void* CreateCircuit();
    CIRCUITSIMULATOR_API void DestroyCircuit(void* circuit);
    CIRCUITSIMULATOR_API void AddNode(void* circuit, const char* name);
    
    // Component Addition
    CIRCUITSIMULATOR_API void AddResistor(void* circuit, const char* name, const char* node1, const char* node2, double value);
    CIRCUITSIMULATOR_API void AddCapacitor(void* circuit, const char* name, const char* node1, const char* node2, double value);
    CIRCUITSIMULATOR_API void AddInductor(void* circuit, const char* name, const char* node1, const char* node2, double value);
    CIRCUITSIMULATOR_API void AddVoltageSource(void* circuit, const char* name, const char* node1, const char* node2, double voltage);
    CIRCUITSIMULATOR_API void AddACVoltageSource(void* circuit, const char* name, const char* node1, const char* node2, double magnitude, double phase);
    
    CIRCUITSIMULATOR_API void SetGroundNode(void* circuit, const char* nodeName);
    
    // Analysis Functions
    CIRCUITSIMULATOR_API bool RunDCAnalysis(void* circuit);
    CIRCUITSIMULATOR_API bool RunTransientAnalysis(void* circuit, double stepTime, double stopTime);
    CIRCUITSIMULATOR_API bool RunACAnalysis(void* circuit, const char* sourceName, double startFreq, double stopFreq, int numPoints, const char* sweepType);
    CIRCUITSIMULATOR_API bool RunPhaseSweepAnalysis(void* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);

    // Result Retrieval
    CIRCUITSIMULATOR_API double GetNodeVoltage(void* circuit, const char* nodeName);
    CIRCUITSIMULATOR_API int GetNodeNames(void* circuit, char* nodeNamesBuffer, int bufferSize);
    CIRCUITSIMULATOR_API int GetNodeVoltageHistory(void* circuit, const char* nodeName, double* timePoints, double* voltages, int maxCount);
    CIRCUITSIMULATOR_API int GetNodeSweepHistory(void* circuit, const char* nodeName, double* frequencies, double* magnitudes, int maxCount);
    CIRCUITSIMULATOR_API int GetNodePhaseSweepHistory(void* circuit, const char* nodeName, double* phases, double* magnitudes, int maxCount);
    CIRCUITSIMULATOR_API int GetComponentCurrentHistory(void* circuit, const char* componentName, double* timePoints, double* currents, int maxCount);
    CIRCUITSIMULATOR_API int GetAllVoltageSourceNames(void* circuit, char* vsNamesBuffer, int bufferSize);
    CIRCUITSIMULATOR_API double GetVoltageSourceCurrent(void* circuit, const char* vsName);
}
