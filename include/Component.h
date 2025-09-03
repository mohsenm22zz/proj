#pragma once

#include <string>

using namespace std;

class Node;

class Component {
public:
    string name;
    Node *node1;
    Node *node2;

    Component() : name("") {}

    virtual ComponentType getType() const = 0;
    virtual string getComponentName() const { return name; }
    virtual double getResistance() const { return 0.0; }
    virtual double getCapacitance() const { return 0.0; }
    virtual double getInductance() const { return 0.0; }
    virtual double getVoltage() const { return 0.0; }
    virtual double getCurrent() const { return 0.0; }
    virtual void setCurrent(double c) {}
    virtual ~Component() {};
};

// Forward declarations for all component types
class Resistor;
class Capacitor;
class Inductor;
class VoltageSource;
class ACVoltageSource;
class CurrentSource;
class Diode;

// Component type enum
enum ComponentType {
    ResistorType,
    CapacitorType,
    InductorType,
    VoltageSourceType,
    ACVoltageSourceType,
    CurrentSourceType,
    DiodeType
};
