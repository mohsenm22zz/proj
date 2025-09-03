#pragma once

#include "Circuit.h"
#include "export.h" 

// CS_EXTERN CS_API void dcAnalysis(Circuit& circuit);
// CS_EXTERN CS_API void transientAnalysis(Circuit& circuit, double t_step, double t_stop);
// CS_EXTERN CS_API void dcSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start, double end, double step);
// CS_EXTERN CS_API void acSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start_freq, double stop_freq, int num_points, const std::string& sweep_type);
// CS_EXTERN CS_API void phaseSweepAnalysis(Circuit& circuit, const std::string& sourceName, double base_freq, double start_phase, double stop_phase, int num_points);

CS_EXTERN CS_API bool dcAnalysis(Circuit& circuit);
CS_EXTERN CS_API bool transientAnalysis(Circuit& circuit, double t_step, double t_stop);
CS_EXTERN CS_API void dcSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start, double end, double step);
CS_EXTERN CS_API int acSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start_freq, double stop_freq, int num_points, const std::string& sweep_type);
CS_EXTERN CS_API int phaseSweepAnalysis(Circuit& circuit, const std::string& sourceName, double base_freq, double start_phase, double stop_phase, int num_points);
