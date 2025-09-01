#include "CurrentSource.h"
#include "Node.h"
#include <cmath>

using namespace std;

double CurrentSource::getCurrent() {
    return value;
}

double CurrentSource::getVoltage() {
    if (!node1 || !node2) return 0.0;
    return fabs(node1->getVoltage() - node2->getVoltage());
}