#include "Component.h"

// Component is an abstract base class, so we don't need to implement the pure virtual functions here.
// The implementations of getCurrent() and getVoltage() are provided in the derived classes.
// However, we can provide implementations for the non-pure virtual functions if needed.

Component::Component() : name(""), node1(nullptr), node2(nullptr) {}

Component::~Component() {}

void Component::setCurrent(double c) {
    // Default implementation does nothing
    // Derived classes can override this if needed
}