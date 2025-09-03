#pragma once

#include "export.h"
#include "Circuit.h"
#include <vector>
#include <string>

// #ifdef _WIN32
//     #ifdef CIRCUITSIMULATOR_EXPORTS
//         #define CIRCUITSIMULATOR_API __declspec(dllexport)
//     #else
//         #define CIRCUITSIMULATOR_API __declspec(dllimport)
//     #endif
// #else
//     #define CIRCUITSIMULATOR_API
// #endif

extern "C" {
    CS_EXTERN CS_API Circuit* __cdecl CreateCircuit();
    CS_EXTERN CS_API void __cdecl DestroyCircuit(Circuit* circuit);
    CS_EXTERN CS_API void __cdecl AddNode(void* circuit, const char* name);

    // Component Addition
    CS_EXTERN CS_API void __cdecl AddResistor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value);
    CS_EXTERN CS_API void __cdecl AddCapacitor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value);
    CS_EXTERN CS_API void __cdecl AddInductor(Circuit* circuit, const char* name, const char* node1, const char* node2, double value);
    CS_EXTERN CS_API void __cdecl AddVoltageSource(Circuit* circuit, const char* name, const char* node1, const char* node2, double voltage);
    CS_EXTERN CS_API void __cdecl AddACVoltageSource(Circuit* circuit, const char* name, const char* node1, const char* node2, double magnitude, double phase);

    CS_EXTERN CS_API void __cdecl SetGroundNode(Circuit* circuit, const char* nodeName);

    // Analysis Functions
    CS_EXTERN CS_API bool __cdecl RunDCAnalysis(Circuit* circuit);
    CS_EXTERN CS_API bool __cdecl RunTransientAnalysis(Circuit* circuit, double stepTime, double stopTime);
    // CS_EXTERN CS_API bool RunACAnalysis(void* circuit, const char* sourceName, double startFreq, double stopFreq, int numPoints, const char* sweepType);
    // CS_EXTERN CS_API bool __cdecl RunPhaseSweepAnalysis(void* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);
    CS_EXTERN CS_API bool __cdecl RunPhaseSweepAnalysis(Circuit* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);

    // Result Retrieval
    CS_EXTERN CS_API double __cdecl GetNodeVoltage(void* circuit, const char* nodeName);
    CS_EXTERN CS_API int __cdecl GetNodeNames(void* circuit, char* nodeNamesBuffer, int bufferSize);
    CS_EXTERN CS_API int __cdecl GetNodeVoltageHistory(void* circuit, const char* nodeName, double* timePoints, double* voltages, int maxCount);
    CS_EXTERN CS_API int __cdecl GetNodeSweepHistory(void* circuit, const char* nodeName, double* frequencies, double* magnitudes, int maxCount);
    CS_EXTERN CS_API int __cdecl GetNodePhaseSweepHistory(void* circuit, const char* nodeName, double* phases, double* magnitudes, int maxCount);
    CS_EXTERN CS_API int __cdecl GetComponentCurrentHistory(void* circuit, const char* componentName, double* timePoints, double* currents, int maxCount);
    CS_EXTERN CS_API int __cdecl GetAllVoltageSourceNames(void* circuit, char* vsNamesBuffer, int bufferSize);
    CS_EXTERN CS_API double __cdecl GetVoltageSourceCurrent(void* circuit, const char* vsName);

    // New exports for comprehensive circuit analysis
    CS_EXTERN CS_API void __cdecl RunDCOperatingPoint(void* circuit);
    CS_EXTERN CS_API bool __cdecl RunACAnalysis(Circuit* circuit, const char* sourceName, double startFreq, double stopFreq, int numPoints, const char* sweepType);
    CS_EXTERN CS_API int __cdecl RunPhaseSweep(void* circuit, const char* sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);

    // Component property accessors with proper type handling
    CS_EXTERN CS_API const char* __cdecl GetComponentName(void* component);
    CS_EXTERN CS_API double __cdecl GetComponentResistance(void* component);
    CS_EXTERN CS_API void __cdecl SetComponentResistance(void* component, double resistance);
    CS_EXTERN CS_API double __cdecl GetComponentCapacitance(void* component);
    CS_EXTERN CS_API void __cdecl SetComponentCapacitance(void* component, double capacitance);
    CS_EXTERN CS_API double __cdecl GetComponentInductance(void* component);
    CS_EXTERN CS_API void __cdecl SetComponentInductance(void* component, double inductance);
    CS_EXTERN CS_API double __cdecl GetComponentVoltage(void* component);
    CS_EXTERN CS_API double __cdecl GetComponentCurrent(void* component);

    // Circuit state management
    CS_EXTERN CS_API void __cdecl SaveCircuitState(void* circuit, const char* filename);
    CS_EXTERN CS_API void __cdecl LoadCircuitState(void* circuit, const char* filename);

}
