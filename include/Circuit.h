#pragma once

#include <vector>
#include <string>
#include <list>
#include <map>
#include <memory>
#include <complex>

class Node;
class Component;

#include "Node.h"
#include "Resistor.h"
#include "Capacitor.h"
#include "Inductor.h"
#include "Diode.h"
#include "VoltageSource.h"
#include "CurrentSource.h"
#include "ACVoltageSource.h"
#include "Component.h"

using namespace std;

enum class AnalysisType {
    DC,
    TRANSIENT,
    AC_SWEEP
};


class Circuit {
public:
    vector<Node*> nodes;
    vector<Resistor> resistors;
    vector<Capacitor> capacitors;
    vector<Inductor> inductors;
    vector<Diode> diodes;
    vector<VoltageSource> voltageSources;
    vector<ACVoltageSource> acVoltageSources;
    vector<CurrentSource> currentSources;
    vector<string> groundNodeNames;

    double delta_t;

    vector<vector<double>> MNA_A;
    vector<double> MNA_RHS;

    vector<vector<complex<double>>> MNA_A_Complex;
    vector<complex<double>> MNA_RHS_Complex;
    
    vector<vector<double>> G();
    vector<vector<double>> B();
    vector<vector<double>> C();
    vector<vector<double>> D();
    vector<double> J();
    vector<double> E();

    void addNode(const string& name);
    Node* findNode(const string& name);
    Node* findOrCreateNode(const string& name);

    Resistor* findResistor(const string& name);
    Capacitor* findCapacitor(const string& name);
    Inductor* findInductor(const string& name);
    Diode* findDiode(const string& name);
    CurrentSource* findCurrentSource(const string& name);
    VoltageSource* findVoltageSource(const string& name);
    ACVoltageSource* findACVoltageSource(const string& name);

    bool deleteResistor(const string& name);
    void set_MNA_A(AnalysisType type, double frequency = 0);
    void set_MNA_RHS(AnalysisType type, double frequency = 0);

    void setDeltaT(double dt);
    void updateComponentStates();
    void clearComponentHistory();
    int getNodeMatrixIndex(const Node* target_node_ptr) const;
    int countNonGroundNodes() const;
    int countTotalExtraVariables();
    void assignDiodeBranchIndices();
};