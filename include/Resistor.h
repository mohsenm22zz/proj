#pragma once

#include "Component.h"

class Resistor : public Component {
public:
    double resistance;

    Resistor() : resistance(0.0) {}

    // ComponentType getType() const override { return ResistorType; }
    double getResistance() const { return resistance; }
    void setCurrent(double c)  { /* Resistor current is determined by voltage */ }
    double getCurrent() override;
    double getVoltage() override;
};
// Similar implementations for Capacitor, Inductor, etc.
