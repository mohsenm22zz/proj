#pragma once

#include <string>
#include <vector>
#include <utility>

using namespace std;

class Component {
public:
    virtual ~Component() = default;
    virtual double getCurrent() { return 0.0; }
    virtual double getVoltage() { return 0.0; }
    
    // Add current history for plotting
    vector<pair<double, double>> current_history;
    
    void addCurrentHistoryPoint(double time, double current) {
        current_history.push_back({time, current});
    }
    
    void clearHistory() {
        current_history.clear();
    }
};

class Node;

class Component {
public:
    string name;
    Node *node1;
    Node *node2;

    Component() : name(""), node1(nullptr), node2(nullptr) {}

    virtual double getCurrent() = 0;
    virtual double getVoltage() = 0;
    virtual ~Component() {};
};