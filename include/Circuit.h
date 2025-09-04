#pragma once

#include "Node.h"
#include "Resistor.h"
#include "Capacitor.h"
#include "Inductor.h"
#include "VoltageSource.h"
#include "ACVoltageSource.h"
#include "CurrentSource.h"
#include "Diode.h"
#include <vector>
#include <complex>
#include <string>

enum class AnalysisType {
    DC,
    TRANSIENT,
    AC_SWEEP
};

class Circuit {
public:
    std::vector<Node*> nodes;
    std::vector<Resistor> resistors;
    std::vector<Capacitor> capacitors;
    std::vector<Inductor> inductors;
    std::vector<VoltageSource> voltageSources;
    std::vector<ACVoltageSource> acVoltageSources;
    std::vector<CurrentSource> currentSources;
    std::vector<Diode> diodes;
    
    double delta_t;
    double currentTime;

    // --- MNA System Matrices ---
    std::vector<std::vector<double>> MNA_A;
    std::vector<double> MNA_RHS;
    std::vector<double> MNA_solution;
    std::vector<std::vector<std::complex<double>>> MNA_A_Complex;
    std::vector<std::complex<double>> MNA_RHS_Complex;
    
    
    std::vector<std::string> groundNodeNames;

    // --- Constructor & Destructor ---
    Circuit();
    ~Circuit();

    void addNode(const std::string& name);
    Node* findNode(const std::string& find_from_name);
    Node* findOrCreateNode(const std::string& name);
    void setGroundNode(const std::string& node_name);

    // --- Find Component Methods ---
    Resistor* findResistor(const std::string& find_from_name);
    Capacitor* findCapacitor(const std::string& find_from_name);
    Inductor* findInductor(const std::string& find_from_name);
    VoltageSource* findVoltageSource(const std::string& find_from_name);
    ACVoltageSource* findACVoltageSource(const std::string& find_from_name);
    CurrentSource* findCurrentSource(const std::string& find_from_name);
    Diode* findDiode(const std::string& find_from_name);

    // --- MNA Matrix Methods ---
    void set_MNA_A(AnalysisType type, double frequency = 0);
    void set_MNA_RHS(AnalysisType type, double frequency = 0);
    void MNA_sol_size();
    
    // --- Analysis Helpers ---
    void setDeltaT(double dt);
    void updateComponentStates();
    void clearComponentHistory();
    void assignDiodeBranchIndices();

    // --- Indexing and Utility ---
    int findNodeIndex(const std::string& name);
    int findVoltageSourceIndex(const std::string& name);
    int findACVoltageSourceIndex(const std::string& name);
    int getNodeMatrixIndex(const Node* target_node_ptr) const;
    bool isNodeNameGround(const std::string& node_name) const;
};

