#pragma once

#include "Component.h"

class Resistor : public Component {
public:
    double resistance;

    Resistor() : resistance(0.0) {}

    double getCurrent() override;
    double getVoltage() override;
};