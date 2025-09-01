#pragma once
#include <string>
#include <vector>
#include <utility>

using namespace std;
class Node {
public:
    string name;
    int num;
    static int nextNum;
    double voltage;
    bool isGround;

    vector<pair<double, double>> voltage_history;
    vector<pair<double, double>> dc_sweep_history;
    vector<pair<double, double>> ac_sweep_history;
    vector<pair<double, double>> phase_sweep_history;

    Node();
    double getVoltage() const;
    void setVoltage(double v);
    void setGround(bool ground_status);

    void addVoltageHistoryPoint(double time, double vol);
    void clearHistory();
};

