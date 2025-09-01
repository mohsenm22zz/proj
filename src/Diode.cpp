#include "Diode.h"
#include "Node.h"
#include <cmath>

using namespace std;

Diode::Diode(const string &name, Node *n1, Node *n2, DiodeType type, double vf, double vz)
        :
          diodeType(type),
          currentState(STATE_OFF),
          forwardVoltage(vf),
          zenerVoltage(vz),
          branchIndex(-1),
          current(0.0) {
}

DiodeType Diode::getDiodeType() const {
    return diodeType;
}

double Diode::getForwardVoltage() const {
    return forwardVoltage;
}

double Diode::getZenerVoltage() const {
    return zenerVoltage;
}

void Diode::setState(DiodeState state) {
    currentState = state;
}

DiodeState Diode::getState() const {
    return currentState;
}

void Diode::setBranchIndex(int index) {
    branchIndex = index;
}

int Diode::getBranchIndex() const {
    return branchIndex;
}

void Diode::setCurrent(double c) {
    current = c;
}

double Diode::getCurrent() {
    if (currentState == STATE_OFF) {
        return 0.0;
    }
    return current;
}

double Diode::getVoltage() {
    if (!node1 || !node2) return 0.0;
    if (currentState == STATE_FORWARD_ON) {
        return forwardVoltage;
    } else if (currentState == STATE_REVERSE_ON) {
        return -zenerVoltage;
    }
    return node1->getVoltage() - node2->getVoltage();
}