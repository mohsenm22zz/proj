#include "Resistor.h"
#include "Node.h"
#include <cmath>
#include <limits>

using namespace std;

double Resistor::getCurrent() {
    if (resistance == 0) return numeric_limits<double>::infinity();
    if (!node1 || !node2) return 0.0;
    return (node1->getVoltage() - node2->getVoltage()) / resistance;
}

double Resistor::getVoltage() {
    if (!node1 || !node2) return 0.0;
    return fabs(node1->getVoltage() - node2->getVoltage());
}