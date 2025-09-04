#define _USE_MATH_DEFINES
#include <cmath>
#include "Circuit.h"
#include <algorithm>
#include <vector>
#include <string>
#include <complex>
#include <fstream>
#include <iostream>

void WriteToFile1(const std::string& content)
{
    const char* filePath = "output.txt";

    // Open file in append mode
    std::ofstream outFile(filePath, std::ios::out | std::ios::app);  // Using std::ios::app for append mode
    
    if (outFile.is_open())
    {
        outFile << content << std::endl;  // Write the content to the file, add a newline
        outFile.close();                  // Close the file after writing
        std::cout << "File written successfully!" << std::endl;
    }
    else
    {
        std::cerr << "Unable to open file!" << std::endl;
    }
}

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
    int num_non_gnd_nodes = countNonGroundNodes();
    vector<vector<double>> g_matrix(num_non_gnd_nodes, vector<double>(num_non_gnd_nodes, 0.0));

    for (const auto &res: resistors) {
        if (res.resistance == 0) continue;
        double conductance = 1.0 / res.resistance;
        int idx1 = getNodeMatrixIndex(res.node1);
        int idx2 = getNodeMatrixIndex(res.node2);
        if (idx1 != -1) g_matrix[idx1][idx1] += conductance;
        if (idx2 != -1) g_matrix[idx2][idx2] += conductance;
        if (idx1 != -1 && idx2 != -1) {
            g_matrix[idx1][idx2] -= conductance;
            g_matrix[idx2][idx1] -= conductance;
        }
    }

    if (delta_t > 0) {
        for (const auto &cap: capacitors) {
            double equiv_conductance = cap.capacitance / delta_t;
            int idx1 = getNodeMatrixIndex(cap.node1);
            int idx2 = getNodeMatrixIndex(cap.node2);
            if (idx1 != -1) g_matrix[idx1][idx1] += equiv_conductance;
            if (idx2 != -1) g_matrix[idx2][idx2] += equiv_conductance;
            if (idx1 != -1 && idx2 != -1) {
                g_matrix[idx1][idx2] -= equiv_conductance;
                g_matrix[idx2][idx1] -= equiv_conductance;
            }
        }
    }
    return g_matrix;
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
    int num_non_gnd_nodes = countNonGroundNodes();
    vector<double> j_vector(num_non_gnd_nodes, 0.0);

    for (const auto &cs: currentSources) {
        int p_node_idx = getNodeMatrixIndex(cs.node1);
        int n_node_idx = getNodeMatrixIndex(cs.node2);
        if (p_node_idx != -1) j_vector[p_node_idx] += cs.value;
        if (n_node_idx != -1) j_vector[n_node_idx] -= cs.value;
    }

    if (delta_t > 0) {
        for (const auto &cap: capacitors) {
            double cap_rhs_term = (cap.capacitance / delta_t) * cap.prevVoltage;
            int idx1 = getNodeMatrixIndex(cap.node1);
            int idx2 = getNodeMatrixIndex(cap.node2);
            if (idx1 != -1) j_vector[idx1] += cap_rhs_term;
            if (idx2 != -1) j_vector[idx2] -= cap_rhs_term;
        }
    }
    return j_vector;
}

vector<double> Circuit::E() {
    int m_vars = countTotalExtraVariables();
    if (m_vars == 0) return {};
    vector<double> e_vector(m_vars, 0.0);

    for (size_t j = 0; j < voltageSources.size(); ++j) {
        e_vector[j] = voltageSources[j].value;
    }

    if (delta_t > 0) {
        for (size_t k = 0; k < inductors.size(); ++k) {
            int inductor_row = voltageSources.size() + k;
            e_vector[inductor_row] = -(inductors[k].inductance / delta_t) * inductors[k].prevCurrent;
        }
    }

    for (const auto& d : diodes) {
        if (d.getState() == STATE_FORWARD_ON) {
            int diode_row = d.getBranchIndex();
            e_vector[diode_row] = d.getForwardVoltage();
        } else if (d.getState() == STATE_REVERSE_ON) {
            int diode_row = d.getBranchIndex();
            e_vector[diode_row] = -d.getZenerVoltage();
        }
    }
    return e_vector;
}
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
    assignDiodeBranchIndices();
    vector<vector<double>> g_mat = G();
    vector<vector<double>> b_mat = B();
    vector<vector<double>> c_mat = C();
    vector<vector<double>> d_mat = D();
    int n = g_mat.size();
    int m = countTotalExtraVariables();
    WriteToFile1("[set_MNA_A] n = " + std::to_string(n) + ", m = " + std::to_string(m) + ", total size = " + std::to_string(n + m));
    MNA_A.assign(n + m, vector<double>(n + m, 0.0));
    if (n > 0) {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; ++j) {
                MNA_A[i][j] = g_mat[i][j];
            }
        }
    }
    if (m > 0 && n > 0) {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < m; ++j) {
                MNA_A[i][n + j] = b_mat[i][j];
            }
        }
    }
    if (m > 0 && n > 0) {
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < n; ++j) {
                MNA_A[n + i][j] = c_mat[i][j];
            }
        }
    }
    if (m > 0) {
        for (int i = 0; i < m; i++) {
            for (int j = 0; j < m; ++j) {
                MNA_A[n + i][n + j] = d_mat[i][j];
            }
        }
    }
    int nk = countNonGroundNodes();
    int mk = countTotalExtraVariables();
    WriteToFile1("[set_MNA_A] n = " + std::to_string(nk) + ", m = " + std::to_string(mk) + ", total size = " + std::to_string(nk + mk));
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
        WriteToFile1("// Performing DC/Transient Analysis... 2 RHS");
       assignDiodeBranchIndices();
    vector<double> j_vec = J();
    vector<double> e_vec = E();
    WriteToFile1("[set_MNA_RHS] j_vec size = " + std::to_string(j_vec.size()) + ", e_vec size = " + std::to_string(e_vec.size()));
    int n = j_vec.size();
    int m = countTotalExtraVariables();
    MNA_RHS.assign(n + m, 0.0);
    for (int i = 0; i < n; i++) MNA_RHS[i] = j_vec[i];
    for (int i = 0; i < m; i++) MNA_RHS[n + i] = e_vec[i];
    
    for (auto it = j_vec.begin(); it != j_vec.end(); ++it) {
        WriteToFile1(to_string(*it)+ "\n");
    }
    WriteToFile1("\n");
    for (auto it = e_vec.begin(); it != e_vec.end(); ++it) {
        WriteToFile1(to_string(*it)+ "\n");
    }
    WriteToFile1("\n");
    WriteToFile1("\n");
    int nk = j_vec.size();
    int mk = countTotalExtraVariables();
    WriteToFile1("[set_MNA_RHS] n = " + std::to_string(nk) + ", m = " + std::to_string(mk) + ", total size = " + std::to_string(nk + mk));
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


