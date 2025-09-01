#pragma once

#include "Circuit.h"
#include "export.h" 

CIRCUITSIMULATOR_API void dcAnalysis(Circuit& circuit);
CIRCUITSIMULATOR_API void transientAnalysis(Circuit& circuit, double t_step, double t_stop);
CIRCUITSIMULATOR_API void dcSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start, double end, double step);
CIRCUITSIMULATOR_API void acSweepAnalysis(Circuit& circuit, const std::string& sourceName, double start_freq, double stop_freq, int num_points, const std::string& sweep_type);
CIRCUITSIMULATOR_API void phaseSweepAnalysis(Circuit& circuit, const std::string& sourceName, double base_freq, double start_phase, double stop_phase, int num_points);
