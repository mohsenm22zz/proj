const std::vector<double>& Inductor::getCurrentHistory() const {
    return currentHistory;
}
#pragma once

#include "Component.h"
#include <vector>

class Inductor : public Component {
public:
    double inductance;
    double current;
    double prevCurrent;
    std::vector<double> currentHistory;

    Inductor() : inductance(0.0), current(0.0), prevCurrent(0.0) {}

    double getCurrent() override;
    double getVoltage() override;
    void update(double dt);
    const std::vector<double>& getCurrentHistory() const;
    void setInductorCurrent(double c);
};