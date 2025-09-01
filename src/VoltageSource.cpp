#include "VoltageSource.h"

using namespace std;

double VoltageSource::getCurrent() {
    return current;
}

void VoltageSource::setCurrent(double c) {
    current = c;
}

double VoltageSource::getVoltage() {
    return value;
}

void VoltageSource::addCurrentHistoryPoint(double time, double cur) {
    current_history.push_back({time, cur});
}

void VoltageSource::clearHistory() {
    current_history.clear();
    dc_sweep_current_history.clear();
}