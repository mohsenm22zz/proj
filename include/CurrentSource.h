#pragma once

#include "Component.h"

class CurrentSource : public Component {
public:
    double value;
    bool diode = false;
    CurrentSource() : value(0.0) {}

    double getCurrent() override;
    double getVoltage() override;
};