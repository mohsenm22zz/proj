#pragma once

#include "Component.h"

class Capacitor : public Component {
public:
    double capacitance;
    double prevVoltage;

    Capacitor() : capacitance(0.0), prevVoltage(0.0) {}

    double getCurrent() override;
    double getVoltage() override;
    void update(double dt);
};