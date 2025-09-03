#pragma once

#include <string>

using namespace std;

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