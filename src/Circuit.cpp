#include "Circuit.h"
#include <algorithm>
#include <vector>
#include <string>
#include <complex>

Circuit::Circuit() : delta_t(0), currentTime(0) {}

Circuit::~Circuit() {
    for (Node* node : nodes) {
        delete node;
    }
    nodes.clear();
}

void Circuit::addNode(const std::string& name) {
    if (!findNode(name)) {
        Node* newNode = new Node();
        newNode->name = name;
        nodes.push_back(newNode);
    }
}

Node* Circuit::findNode(const std::string& find_from_name) {
    for (Node* node : nodes) {
        if (node->name == find_from_name) {
            return node;
        }
    }
    return nullptr;
}


Node* Circuit::findOrCreateNode(const std::string& name) {
    Node* node = findNode(name);
    if (node) {
        return node;
    }
    addNode(name);
    return nodes.back();
}

Resistor* Circuit::findResistor(const std::string& find_from_name) {
    for (auto& res : resistors) { if (res.name == find_from_name) return &res; }
    return nullptr;
}

Capacitor* Circuit::findCapacitor(const std::string& find_from_name) {
    for (auto& cap : capacitors) { if (cap.name == find_from_name) return &cap; }
    return nullptr;
}

Inductor* Circuit::findInductor(const std::string& find_from_name) {
    for (auto& ind : inductors) { if (ind.name == find_from_name) return &ind; }
    return nullptr;
}

VoltageSource* Circuit::findVoltageSource(const std::string& find_from_name) {
    for (auto& vs : voltageSources) { if (vs.name == find_from_name) return &vs; }
    return nullptr;
}

ACVoltageSource* Circuit::findACVoltageSource(const std::string& find_from_name) {
    for (auto& ac_vs : acVoltageSources) { if (ac_vs.name == find_from_name) return &ac_vs; }
    return nullptr;
}

CurrentSource* Circuit::findCurrentSource(const std::string& find_from_name) {
    for (auto& cs : currentSources) { if (cs.name == find_from_name) return &cs; }
    return nullptr;
}

Diode* Circuit::findDiode(const std::string& find_from_name) {
    for (auto& d : diodes) { if (d.name == find_from_name) return &d; }
    return nullptr;
}


int Circuit::findNodeIndex(const std::string& name) {
    for (size_t i = 0; i < nodes.size(); ++i) {
        if (nodes[i]->name == name) {
            return static_cast<int>(i);
        }
    }
    return -1;
}

int Circuit::findVoltageSourceIndex(const std::string& name) {
    for (size_t i = 0; i < voltageSources.size(); ++i) {
        if (voltageSources[i].name == name) {
            return static_cast<int>(i);
        }
    }
    return -1;
}

// --- NEW HELPER FUNCTION ---
// Added a helper to find the index of an AC voltage source
int Circuit::findACVoltageSourceIndex(const std::string& name) {
    for (size_t i = 0; i < acVoltageSources.size(); ++i) {
        if (acVoltageSources[i].name == name) {
            return static_cast<int>(i);
        }
    }
    return -1;
}


void Circuit::setGroundNode(const std::string& node_name) {
    Node* node = findNode(node_name);
    if (node) {
        node->isGround = true;
    }
}

int Circuit::getNodeMatrixIndex(const Node* target_node_ptr) const {
    int non_gnd_idx_counter = 0;
    for (const auto& current_node_ptr : nodes) {
        if (current_node_ptr == target_node_ptr) {
            return non_gnd_idx_counter;
        }
        if (!current_node_ptr->isGround) {
            non_gnd_idx_counter++;
        }
    }
    return -1; // Should not happen if node exists
}

void Circuit::assignDiodeBranchIndices() {
    int n_nodes = 0;
    for (const auto& node : nodes) {
        if (!node->isGround) n_nodes++;
    }
    int branch_idx_offset = n_nodes + voltageSources.size() + inductors.size();
    for (auto& diode : diodes) {
        if (diode.getState() != STATE_OFF) {
            diode.setBranchIndex(branch_idx_offset++);
        }
        else {
            diode.setBranchIndex(-1);
        }
    }
}


void Circuit::set_MNA_A(AnalysisType type, double frequency) {
    int n = 0;
    for (const auto& node : nodes) {
        if (!node->isGround) n++;
    }

    // --- MODIFIED ---
    // AC Voltage Sources also need a branch current, so they are added to 'm'
    int m = voltageSources.size() + acVoltageSources.size() + inductors.size();
    int active_diodes = 0;
    if (type == AnalysisType::DC) {
        for (const auto& diode : diodes) {
            if (diode.getState() != STATE_OFF) {
                active_diodes++;
            }
        }
    }

    int matrix_size = n + m + active_diodes;
    MNA_A.assign(matrix_size, std::vector<double>(matrix_size, 0.0));
    MNA_A_Complex.assign(matrix_size, std::vector<std::complex<double>>(matrix_size, { 0.0, 0.0 }));

    if (type == AnalysisType::DC || type == AnalysisType::TRANSIENT) {
        // G Matrix (Resistors)
        for (const auto& res : resistors) {
            int n1_idx = getNodeMatrixIndex(res.node1);
            int n2_idx = getNodeMatrixIndex(res.node2);
            double g = 1.0 / res.resistance;
            if (n1_idx != -1) MNA_A[n1_idx][n1_idx] += g;
            if (n2_idx != -1) MNA_A[n2_idx][n2_idx] += g;
            if (n1_idx != -1 && n2_idx != -1) {
                MNA_A[n1_idx][n2_idx] -= g;
                MNA_A[n2_idx][n1_idx] -= g;
            }
        }
        if (type == AnalysisType::TRANSIENT) {
            // Add capacitor conductance for transient
            for (const auto& cap : capacitors) {
                int n1_idx = getNodeMatrixIndex(cap.node1);
                int n2_idx = getNodeMatrixIndex(cap.node2);
                double g_c = cap.capacitance / delta_t;
                if (n1_idx != -1) MNA_A[n1_idx][n1_idx] += g_c;
                if (n2_idx != -1) MNA_A[n2_idx][n2_idx] += g_c;
                if (n1_idx != -1 && n2_idx != -1) {
                    MNA_A[n1_idx][n2_idx] -= g_c;
                    MNA_A[n2_idx][n1_idx] -= g_c;
                }
            }
        }

        // B & C Matrix (Voltage Sources & Inductors)
        // DC Voltage Sources
        for (size_t i = 0; i < voltageSources.size(); ++i) {
            int n1_idx = getNodeMatrixIndex(voltageSources[i].node1);
            int n2_idx = getNodeMatrixIndex(voltageSources[i].node2);
            int branch_idx = n + i;
            if (n1_idx != -1) { MNA_A[n1_idx][branch_idx] += 1.0; MNA_A[branch_idx][n1_idx] += 1.0; }
            if (n2_idx != -1) { MNA_A[n2_idx][branch_idx] -= 1.0; MNA_A[branch_idx][n2_idx] -= 1.0; }
        }
        
        // --- NEW ---
        // AC Voltage Sources (for transient)
        int vs_offset = voltageSources.size();
        for (size_t i = 0; i < acVoltageSources.size(); ++i) {
            int n1_idx = getNodeMatrixIndex(acVoltageSources[i].node1);
            int n2_idx = getNodeMatrixIndex(acVoltageSources[i].node2);
            int branch_idx = n + vs_offset + i;
            if (n1_idx != -1) { MNA_A[n1_idx][branch_idx] += 1.0; MNA_A[branch_idx][n1_idx] += 1.0; }
            if (n2_idx != -1) { MNA_A[n2_idx][branch_idx] -= 1.0; MNA_A[branch_idx][n2_idx] -= 1.0; }
        }

        // Inductors
        int acvs_offset = vs_offset + acVoltageSources.size();
        for (size_t i = 0; i < inductors.size(); ++i) {
            int n1_idx = getNodeMatrixIndex(inductors[i].node1);
            int n2_idx = getNodeMatrixIndex(inductors[i].node2);
            int branch_idx = n + acvs_offset + i;
            if (n1_idx != -1) { MNA_A[n1_idx][branch_idx] += 1.0; MNA_A[branch_idx][n1_idx] += 1.0; }
            if (n2_idx != -1) { MNA_A[n2_idx][branch_idx] -= 1.0; MNA_A[branch_idx][n2_idx] -= 1.0; }
            if (type == AnalysisType::TRANSIENT) {
                 MNA_A[branch_idx][branch_idx] -= inductors[i].inductance / delta_t;
            }
        }
    }
    else if (type == AnalysisType::AC_SWEEP) {
        // Handle complex AC analysis...
    }
}

void Circuit::set_MNA_RHS(AnalysisType type, double frequency) {
    int n = 0;
    for (const auto& node : nodes) { if (!node->isGround) n++; }

    // --- MODIFIED ---
    // M must include ALL components with a branch current equation.
    int m = voltageSources.size() + acVoltageSources.size() + inductors.size();
    
    int active_diodes = 0;
    if (type == AnalysisType::DC) {
        for (const auto& diode : diodes) {
            if (diode.getState() != STATE_OFF) active_diodes++;
        }
    }

    int rhs_size = n + m + active_diodes;
    MNA_RHS.assign(rhs_size, 0.0);
    MNA_RHS_Complex.assign(rhs_size, { 0.0, 0.0 });
    
    // I vector (current sources)
    for (const auto& cs : currentSources) {
        int n1_idx = getNodeMatrixIndex(cs.node1);
        int n2_idx = getNodeMatrixIndex(cs.node2);
        if (n1_idx != -1) MNA_RHS[n1_idx] -= cs.value;
        if (n2_idx != -1) MNA_RHS[n2_idx] += cs.value;
    }
    if (type == AnalysisType::TRANSIENT) {
        // Capacitor current sources for transient
        for (const auto& cap : capacitors) {
            int n1_idx = getNodeMatrixIndex(cap.node1);
            int n2_idx = getNodeMatrixIndex(cap.node2);
            double i_c = (cap.capacitance / delta_t) * cap.prevVoltage;
            if (n1_idx != -1) MNA_RHS[n1_idx] += i_c;
            if (n2_idx != -1) MNA_RHS[n2_idx] -= i_c;
        }
        // Inductor voltage sources for transient
        int acvs_offset = voltageSources.size() + acVoltageSources.size();
        for (size_t i = 0; i < inductors.size(); ++i) {
            int branch_idx = n + acvs_offset + i;
            MNA_RHS[branch_idx] -= (inductors[i].inductance / delta_t) * inductors[i].prevCurrent;
        }
    }

    // E vector (voltage sources)
    // DC sources
    for (size_t i = 0; i < voltageSources.size(); ++i) {
        MNA_RHS[n + i] = voltageSources[i].value;
    }

    // --- NEW ---
    // AC sources in transient analysis
    if (type == AnalysisType::TRANSIENT) {
        int vs_offset = voltageSources.size();
        for (size_t i = 0; i < acVoltageSources.size(); ++i) {
            MNA_RHS[n + vs_offset + i] = acVoltageSources[i].getValue(currentTime);
        }
    }
}


void Circuit::MNA_sol_size() {
    MNA_solution.resize(MNA_A.size());
}

void Circuit::setDeltaT(double dt) {
    this->delta_t = dt;
}

void Circuit::updateComponentStates() {
    for (auto& cap : capacitors) {
        cap.update(delta_t);
    }
    for (auto& ind : inductors) {
        ind.update(delta_t);
    }
}

void Circuit::clearComponentHistory() {
    for (Node* node : nodes) {
        node->clearHistory();
    }
    // Also clear component histories
    for (auto& r : resistors) r.clearHistory();
    for (auto& c : capacitors) c.clearHistory();
    for (auto& l : inductors) l.clearHistory();
    for (auto& vs : voltageSources) vs.clearHistory();
    for (auto& ac_vs : acVoltageSources) ac_vs.clearHistory();
}

bool Circuit::isNodeNameGround(const std::string& node_name) const {
    for (const auto& gnd_name : groundNodeNames) {
        if (gnd_name == node_name) {
            return true;
        }
    }
    return false;
}

