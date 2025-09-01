#include "Inductor.h"
#include "Node.h"
#include <cmath>

using namespace std;

double Inductor::getCurrent() {
    return current;
}

double Inductor::getVoltage() {
    return 0.0;
}

void Inductor::update(double dt) {
    if (!node1 || !node2) return;
    prevCurrent = current;
}

void Inductor::setInductorCurrent(double c) {
    this->current = c;
}