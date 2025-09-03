#pragma once

#include "Component.h"
#include <string>
#include <vector>

using namespace std;

enum DiodeType {
    NORMAL,
    ZENER
};

enum DiodeState {
    STATE_OFF = 0,
    STATE_FORWARD_ON = 1,
    STATE_REVERSE_ON = 2
};

class Diode : public Component {
public:
    Diode(const string& name, Node* n1, Node* n2, DiodeType type, double vf, double vz = 0.0);

    DiodeType getDiodeType() const;
    double getForwardVoltage() const;
    double getZenerVoltage() const;

    void setState(DiodeState state);
    DiodeState getState() const;

    void setBranchIndex(int index);
    int getBranchIndex() const;

    double getCurrent()  override;
    void setCurrent(double c);
    double getVoltage()  override;


private:
    DiodeType diodeType;
    DiodeState currentState;
public:
    double forwardVoltage;
private:
    double zenerVoltage;
    int branchIndex;
    double current;
};