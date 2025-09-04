#include "VoltageSource.h"
#include "Node.h"
#include <cmath>

using namespace std;

double VoltageSource::getCurrent() {
    return current;
}

double VoltageSource::getVoltage() {
    if (!node1 || !node2) return 0.0;
    return value;
}

void VoltageSource::setCurrent(double c) {
    this->current = c;
}