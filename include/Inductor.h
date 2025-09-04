#pragma once

#include "Component.h"

class Inductor : public Component {
public:
    double inductance;
    double current;
    double prevCurrent;

    Inductor() : inductance(0.0), current(0.0), prevCurrent(0.0) {}

    double getCurrent() override;
    double getVoltage() override;
    void update(double dt);
    void setInductorCurrent(double c);
};