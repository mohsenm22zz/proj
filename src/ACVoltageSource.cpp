#include "ACVoltageSource.h"
#include <cmath>
#include <complex>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

ACVoltageSource::ACVoltageSource(const std::string& name, Node* n1, Node* n2, double mag, double ph) 
    : magnitude(mag), phase(ph), node1(n1), node2(n2) {
    this->name = name;
}

double ACVoltageSource::getValue(double time) const {
    double phase_rad = phase * M_PI / 180.0;
    return magnitude * sin(2 * M_PI * frequency * time + phase_rad);
}

std::complex<double> ACVoltageSource::getPhasor() const {
    // Convert from magnitude and phase (in degrees) to complex phasor
    double phase_rad = phase * M_PI / 180.0;
    return std::complex<double>(magnitude * cos(phase_rad), magnitude * sin(phase_rad));
}


double ACVoltageSource::getCurrent() {
    // Voltage sources typically don't have a simple current value
    return 0.0;  // Placeholder value, depends on how the circuit is modeled
}

double ACVoltageSource::getVoltage() {
    return magnitude;  // Return the magnitude as the AC voltage
}