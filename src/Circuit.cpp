#define _USE_MATH_DEFINES
#include <cmath>
#include "Circuit.h"
#include <algorithm>
#include <vector>
#include <string>
#include <complex>

Circuit::Circuit() : delta_t(0) {}

Circuit::~Circuit() {
    for (Node *node: nodes) {
        delete node;
    }
    nodes.clear();
}

void Circuit::addNode(const string &name) {
    if (!findNode(name)) {
        Node *newNode = new Node();
        newNode->name = name;
        nodes.push_back(newNode);
    }
}

Node *Circuit::findNode(const string &find_from_name) {
    for (Node *node: nodes) {
        if (node->name == find_from_name) {
            return node;
        }
    }
    return nullptr;
}


Node *Circuit::findOrCreateNode(const string &name) {
    Node *node = findNode(name);
    if (node) {
        return node;
    }
    addNode(name);
    return nodes.back();
}

Resistor *Circuit::findResistor(const string &find_from_name) {
    for (auto &res: resistors) { if (res.name == find_from_name) return &res; }
    return nullptr;
}

Capacitor *Circuit::findCapacitor(const string &find_from_name) {
    for (auto &cap: capacitors) { if (cap.name == find_from_name) return &cap; }
    return nullptr;
}

Inductor *Circuit::findInductor(const string &find_from_name) {
    for (auto &ind: inductors) { if (ind.name == find_from_name) return &ind; }
    return nullptr;
}

Diode *Circuit::findDiode(const string &find_from_name) {
    for (auto &d: diodes) { if (d.name == find_from_name) return &d; }
    return nullptr;
}

CurrentSource *Circuit::findCurrentSource(const string &find_from_name) {
    for (auto &cs: currentSources) { if (cs.name == find_from_name) return &cs; }
    return nullptr;
}

VoltageSource *Circuit::findVoltageSource(const string &find_from_name) {
    for (auto &vs: voltageSources) { if (vs.name == find_from_name) return &vs; }
    return nullptr;
}

ACVoltageSource *Circuit::findACVoltageSource(const string &find_from_name) {
    for (auto &vs: acVoltageSources) { if (vs.name == find_from_name) return &vs; }
    return nullptr;
}

bool Circuit::deleteResistor(const string &name) {
    auto it = remove_if(resistors.begin(), resistors.end(), [&](const Resistor &r) { return r.name == name; });
    if (it != resistors.end()) {
        resistors.erase(it, resistors.end());
        return true;
    }
    return false;
}

bool Circuit::deleteCapacitor(const string &name) {
    auto it = remove_if(capacitors.begin(), capacitors.end(), [&](const Capacitor &c) { return c.name == name; });
    if (it != capacitors.end()) {
        capacitors.erase(it, capacitors.end());
        return true;
    }
    return false;
}

bool Circuit::deleteInductor(const string &name) {
    auto it = remove_if(inductors.begin(), inductors.end(), [&](const Inductor &i) { return i.name == name; });
    if (it != inductors.end()) {
        inductors.erase(it, inductors.end());
        return true;
    }
    return false;
}

bool Circuit::deleteDiode(const string &name) {
    auto it = remove_if(diodes.begin(), diodes.end(), [&](const Diode &d) { return d.name == name; });
    if (it != diodes.end()) {
        diodes.erase(it, diodes.end());
        return true;
    }
    return false;
}

bool Circuit::deleteVoltageSource(const string &name) {
    auto it = remove_if(voltageSources.begin(), voltageSources.end(),
                        [&](const VoltageSource &vs) { return vs.name == name; });
    if (it != voltageSources.end()) {
        voltageSources.erase(it, voltageSources.end());
        return true;
    }
    return false;
}

bool Circuit::deleteCurrentSource(const string &name) {
    auto it = remove_if(currentSources.begin(), currentSources.end(),
                        [&](const CurrentSource &cs) { return cs.name == name; });
    if (it != currentSources.end()) {
        currentSources.erase(it, currentSources.end());
        return true;
    }
    return false;
}

int Circuit::countTotalExtraVariables() {
    int m_vars = voltageSources.size() + inductors.size();
    for (const auto& diode : diodes) {
        if (diode.getState() == STATE_FORWARD_ON || diode.getState() == STATE_REVERSE_ON) {
            m_vars++;
        }
    }
    return m_vars;
}

void Circuit::assignDiodeBranchIndices() {
    int current_branch_idx = voltageSources.size() + inductors.size();
    for (auto& diode : diodes) {
        if (diode.getState() == STATE_FORWARD_ON || diode.getState() == STATE_REVERSE_ON) {
            diode.setBranchIndex(current_branch_idx++);
        } else {
            diode.setBranchIndex(-1);
        }
    }
}

vector<vector<double>> Circuit::G() {
    int n = countNonGroundNodes();
    vector<vector<double>> result(n, vector<double>(n, 0.0));
    
    // Resistors contribute to G matrix
    for (const auto& res : resistors) {
        int n1_index = getNodeMatrixIndex(res.node1);
        int n2_index = getNodeMatrixIndex(res.node2);
        
        if (n1_index != -1 && n2_index != -1) {
            double g = 1.0 / res.resistance;
            if (n1_index == n2_index) continue; // Skip if both terminals on same node
            
            if (n1_index != -1) {
                result[n1_index][n1_index] += g;
            }
            if (n2_index != -1) {
                result[n2_index][n2_index] += g;
            }
            if (n1_index != -1 && n2_index != -1) {
                result[n1_index][n2_index] -= g;
                result[n2_index][n1_index] -= g;
            }
        }
    }
    
    return result;
}

vector<vector<double>> Circuit::B() {
    int n = countNonGroundNodes();
    int extra_vars = countTotalExtraVariables();
    vector<vector<double>> result(n, vector<double>(extra_vars, 0.0));
    
    // Voltage sources contribute to B matrix
    for (size_t i = 0; i < voltageSources.size(); ++i) {
        const auto& vs = voltageSources[i];
        int n1_index = getNodeMatrixIndex(vs.node1);
        int n2_index = getNodeMatrixIndex(vs.node2);
        int vs_index = i;
        
        if (n1_index != -1) {
            result[n1_index][vs_index] = 1.0;
        }
        if (n2_index != -1) {
            result[n2_index][vs_index] = -1.0;
        }
    }
    
    // Inductors contribute to B matrix
    for (size_t i = 0; i < inductors.size(); ++i) {
        const auto& ind = inductors[i];
        int n1_index = getNodeMatrixIndex(ind.node1);
        int n2_index = getNodeMatrixIndex(ind.node2);
        int ind_index = voltageSources.size() + i;
        
        if (n1_index != -1) {
            result[n1_index][ind_index] = 1.0;
        }
        if (n2_index != -1) {
            result[n2_index][ind_index] = -1.0;
        }
    }
    
    return result;
}

vector<vector<double>> Circuit::C() {
    int n = countNonGroundNodes();
    int extra_vars = countTotalExtraVariables();
    vector<vector<double>> result(extra_vars, vector<double>(n, 0.0));
    
    // Voltage sources contribute to C matrix (transpose of B)
    for (size_t i = 0; i < voltageSources.size(); ++i) {
        const auto& vs = voltageSources[i];
        int n1_index = getNodeMatrixIndex(vs.node1);
        int n2_index = getNodeMatrixIndex(vs.node2);
        int vs_index = i;
        
        if (n1_index != -1) {
            result[vs_index][n1_index] = 1.0;
        }
        if (n2_index != -1) {
            result[vs_index][n2_index] = -1.0;
        }
    }
    
    // Inductors contribute to C matrix (transpose of B)
    for (size_t i = 0; i < inductors.size(); ++i) {
        const auto& ind = inductors[i];
        int n1_index = getNodeMatrixIndex(ind.node1);
        int n2_index = getNodeMatrixIndex(ind.node2);
        int ind_index = voltageSources.size() + i;
        
        if (n1_index != -1) {
            result[ind_index][n1_index] = 1.0;
        }
        if (n2_index != -1) {
            result[ind_index][n2_index] = -1.0;
        }
    }
    
    return result;
}

vector<vector<double>> Circuit::D() {
    int extra_vars = countTotalExtraVariables();
    vector<vector<double>> result(extra_vars, vector<double>(extra_vars, 0.0));
    
    // Diodes in forward or reverse conducting state contribute to D matrix
    for (const auto& d : diodes) {
        if (d.getState() == DiodeState::STATE_FORWARD_ON || d.getState() == DiodeState::STATE_REVERSE_ON) {
            int branch_index = d.getBranchIndex();
            if (branch_index >= 0 && branch_index < extra_vars) {
                result[branch_index][branch_index] = 1.0; // Diode current unknown, so placeholder
            }
        }
    }
    
    return result;
}

vector<double> Circuit::J() {
    int extra_vars = countTotalExtraVariables();
    vector<double> result(extra_vars, 0.0);
    
    // Voltage sources contribute to J vector
    for (size_t i = 0; i < voltageSources.size(); ++i) {
        result[i] = voltageSources[i].value;
    }
    
    // Inductors contribute to J vector
    for (size_t i = 0; i < inductors.size(); ++i) {
        int ind_index = voltageSources.size() + i;
        if (ind_index < extra_vars) {
            // For backward Euler: v_L(n+1) = L/dt * (i_L(n+1) - i_L(n))
            // Rearranging: -L/dt * i_L(n+1) = -L/dt * i_L(n) - v_L(n+1)
            // So the term added to RHS is -L/dt * i_L(n)
            result[ind_index] = -inductors[i].inductance / delta_t * inductors[i].prevCurrent;
        }
    }
    
    return result;
}

vector<double> Circuit::E() {
    int n = countNonGroundNodes();
    vector<double> result(n, 0.0);
    
    // Current sources contribute to E vector
    for (const auto& cs : currentSources) {
        int n1_index = getNodeMatrixIndex(cs.node1);
        int n2_index = getNodeMatrixIndex(cs.node2);
        
        if (n1_index != -1) {
            result[n1_index] += cs.value;
        }
        if (n2_index != -1) {
            result[n2_index] -= cs.value;
        }
    }
    
    // Capacitors contribute to E vector
    for (const auto& cap : capacitors) {
        int n1_index = getNodeMatrixIndex(cap.node1);
        int n2_index = getNodeMatrixIndex(cap.node2);
        double i_cap = cap.capacitance / delta_t * (cap.node1->getVoltage() - cap.node2->getVoltage() - cap.prevVoltage);
        
        if (n1_index != -1) {
            result[n1_index] += i_cap;
        }
        if (n2_index != -1) {
            result[n2_index] -= i_cap;
        }
    }
    
    return result;
}

// --- MODIFIED ---
// This function is now a dispatcher. It builds the correct MNA matrix
// based on the analysis type.
void Circuit::set_MNA_A(AnalysisType type, double frequency) {
    if (type == AnalysisType::AC_SWEEP) {
        // --- NEW LOGIC FOR AC ANALYSIS ---
        int n = countNonGroundNodes();
        // For simplicity, this example assumes only voltage sources add extra variables in AC
        int m = acVoltageSources.size();
        MNA_A_Complex.assign(n + m, vector<complex<double>>(n + m, {0.0, 0.0}));

        // G Matrix (Resistors)
        for (const auto &res : resistors) {
            double conductance = 1.0 / res.resistance;
            int idx1 = getNodeMatrixIndex(res.node1);
            int idx2 = getNodeMatrixIndex(res.node2);
            if (idx1 != -1) MNA_A_Complex[idx1][idx1] += conductance;
            if (idx2 != -1) MNA_A_Complex[idx2][idx2] += conductance;
            if (idx1 != -1 && idx2 != -1) {
                MNA_A_Complex[idx1][idx2] -= conductance;
                MNA_A_Complex[idx2][idx1] -= conductance;
            }
        }

        // Impedances for L and C
        const complex<double> j(0.0, 1.0);
        for (const auto &cap : capacitors) {
            complex<double> impedance = 1.0 / (j * 2.0 * M_PI * frequency * cap.capacitance);
            complex<double> admittance = 1.0 / impedance;
            int idx1 = getNodeMatrixIndex(cap.node1);
            int idx2 = getNodeMatrixIndex(cap.node2);
            if (idx1 != -1) MNA_A_Complex[idx1][idx1] += admittance;
            if (idx2 != -1) MNA_A_Complex[idx2][idx2] += admittance;
            if (idx1 != -1 && idx2 != -1) {
                MNA_A_Complex[idx1][idx2] -= admittance;
                MNA_A_Complex[idx2][idx1] -= admittance;
            }
        }

        for (const auto &ind : inductors) {
            complex<double> impedance = j * 2.0 * M_PI * frequency * ind.inductance;
            complex<double> admittance = 1.0 / impedance;
            int idx1 = getNodeMatrixIndex(ind.node1);
            int idx2 = getNodeMatrixIndex(ind.node2);
            if (idx1 != -1) MNA_A_Complex[idx1][idx1] += admittance;
            if (idx2 != -1) MNA_A_Complex[idx2][idx2] += admittance;
            if (idx1 != -1 && idx2 != -1) {
                MNA_A_Complex[idx1][idx2] -= admittance;
                MNA_A_Complex[idx2][idx1] -= admittance;
            }
        }

        // B, C, D matrices for AC sources
        for (size_t i = 0; i < acVoltageSources.size(); ++i) {
            int idx1 = getNodeMatrixIndex(acVoltageSources[i].node1);
            int idx2 = getNodeMatrixIndex(acVoltageSources[i].node2);
            int var_idx = n + i;
            if (idx1 != -1) {
                MNA_A_Complex[idx1][var_idx] += 1.0;
                MNA_A_Complex[var_idx][idx1] += 1.0;
            }
            if (idx2 != -1) {
                MNA_A_Complex[idx2][var_idx] -= 1.0;
                MNA_A_Complex[var_idx][idx2] -= 1.0;
            }
        }

    } else {
        // --- EXISTING LOGIC FOR DC/TRANSIENT ---
        // (This is the original implementation using real numbers)
        vector<vector<double>> g_mat = G();
        vector<vector<double>> b_mat = B();
        vector<vector<double>> c_mat = C();
        vector<vector<double>> d_mat = D();
        int n = g_mat.size();
        int m = countTotalExtraVariables();

        MNA_A.assign(n + m, vector<double>(n + m, 0.0));
        
        // Fill G matrix
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                MNA_A[i][j] = g_mat[i][j];
            }
        }
        
        // Fill B matrix
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < m; j++) {
                if (!b_mat.empty() && i < b_mat.size() && j < b_mat[0].size()) {
                    MNA_A[i][n + j] = b_mat[i][j];
                }
            }
        }
        
        // Fill C matrix
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < n; j++) {
                if (!c_mat.empty() && i < c_mat.size() && j < c_mat[0].size()) {
                    MNA_A[n + i][j] = c_mat[i][j];
                }
            }
        }
        
        // Fill D matrix
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < m; j++) {
                if (!d_mat.empty() && i < d_mat.size() && j < d_mat[0].size()) {
                    MNA_A[n + i][n + j] = d_mat[i][j];
                }
            }
        }
    }
}

// The set_MNA_RHS function would be similarly modified to handle complex values for AC sources.
void Circuit::set_MNA_RHS(AnalysisType type, double frequency) {
    if (type == AnalysisType::AC_SWEEP) {
        int n = countNonGroundNodes();
        int m = acVoltageSources.size();
        MNA_RHS_Complex.assign(n + m, {0.0, 0.0});

        // E vector for AC sources
        for (size_t i = 0; i < acVoltageSources.size(); ++i) {
            MNA_RHS_Complex[n + i] = acVoltageSources[i].getPhasor();
        }
        // Note: AC current sources would contribute to the 'J' part of the vector
    } else {
        // Original implementation for DC/Transient
        vector<double> j_vec = J();
        vector<double> e_vec = E();
        int n = j_vec.size();
        int m = countTotalExtraVariables();
        MNA_RHS.assign(n + m, 0.0);
        for (int i = 0; i < n; i++) MNA_RHS[i] = j_vec[i];
        for (int i = 0; i < m; i++) MNA_RHS[n + i] = e_vec[i];
    }
}


void Circuit::MNA_sol_size() {
    MNA_solution.resize(MNA_A.size());
}

void Circuit::setDeltaT(double dt) {
    this->delta_t = dt;
}

void Circuit::updateComponentStates() {
    for (auto &cap: capacitors) {
        cap.update(delta_t);
    }
    for (auto &ind: inductors) {
        ind.update(delta_t);
    }
}

void Circuit::clearComponentHistory() {
    for (Node *node: nodes) {
        node->clearHistory();
    }
    for (auto &vs: voltageSources) {
        vs.clearHistory();
    }
}

bool Circuit::isNodeNameGround(const string &node_name) const {
    for (const auto &gnd_name: groundNodeNames) {
        if (gnd_name == node_name) {
            return true;
        }
    }
    return false;
}

int Circuit::getNodeMatrixIndex(const Node *target_node_ptr) const {
    if (!target_node_ptr || target_node_ptr->isGround) {
        return -1;
    }
    int matrix_idx = 0;
    for (const Node *n_in_list: nodes) {
        if (!n_in_list->isGround) {
            if (n_in_list->num == target_node_ptr->num) {
                return matrix_idx;
            }
            matrix_idx++;
        }
    }
    return -1;
}

int Circuit::countNonGroundNodes() const {
    int count = 0;
    for (const Node *node: nodes) {
        if (!node->isGround) {
            count++;
        }
    }
    return count;
}


