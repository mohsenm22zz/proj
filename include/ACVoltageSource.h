#ifndef ACVOLTAGESOURCE_H
#define ACVOLTAGESOURCE_H

#include "Component.h"
#include "Node.h"
#include <complex>

class ACVoltageSource : public Component {
public:
    double magnitude = 0.0;
    double frequency = 0.0; 
    double phase = 0.0;     
    Node* node1 = nullptr;
    Node* node2 = nullptr;

    ACVoltageSource() = default;
    ACVoltageSource(const std::string& name, Node* n1, Node* n2, double mag, double ph);

    double getValue(double time) const;
    std::complex<double> getPhasor() const;
    
    double getCurrent() override;  
    double getVoltage() override;  
};

#endif