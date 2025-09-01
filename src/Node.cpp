#include "Node.h"

using namespace std;

int Node::nextNum = 0;
Node::Node() : name(""), num(nextNum++), voltage(0.0), isGround(false) {}

double Node::getVoltage() const {
    if (isGround) return 0.0;
    return voltage;
}

void Node::setVoltage(double v) {
    if (isGround) {
        voltage = 0.0;
    } else {
        voltage = v;
    }
}

void Node::setGround(bool ground_status) {
    isGround = ground_status;
    if (isGround) {
        voltage = 0.0;
    }
}

void Node::addVoltageHistoryPoint(double time, double vol) {
    voltage_history.push_back({time, vol});
}

void Node::clearHistory() {
    voltage_history.clear();
    dc_sweep_history.clear();
    ac_sweep_history.clear();
    phase_sweep_history.clear();
}

