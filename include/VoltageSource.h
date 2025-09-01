#pragma once

#include "Component.h"
#include <vector>
#include <utility>

class VoltageSource : public Component {
public:
    double value;
    double current;
    bool diode = false;

    vector<pair<double, double>> current_history;
    vector<pair<double, double>> dc_sweep_current_history;

    VoltageSource() : value(0.0), current(0.0) {}

    double getCurrent() override;

    void setCurrent(double c) override;

    double getVoltage() override;

    void addCurrentHistoryPoint(double time, double cur);

    void clearHistory();
};