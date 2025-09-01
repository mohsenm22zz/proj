#ifndef CIRCUITIO_H
#define CIRCUITIO_H

#include "Circuit.h"
#include <string>
#include <vector>
#include <memory>

using namespace std;

struct GraphicalCircuit {
    vector<shared_ptr<Component>> components;
    vector<pair<pair<double, double>, pair<double, double>>> wires;
};

class CircuitIO {
public:
    static string generateNetlist(const GraphicalCircuit& gCircuit);
    static Circuit createCircuitFromNetlist(const string& netlist);
};

#endif