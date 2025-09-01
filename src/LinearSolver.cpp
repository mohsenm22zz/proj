#include <iostream>
#include <vector>
#include <cmath>
#include <algorithm>
#include <complex>
#include "LinearSolver.h"

using namespace std;

vector<complex<double>> gaussianElimination(vector<vector<complex<double>>> A, vector<complex<double>> b) {
    int n = A.size();

    for (int i = 0; i < n; i++) {
        // Find pivot
        int max_row = i;
        for (int k = i + 1; k < n; k++) {
            if (abs(A[k][i]) > abs(A[max_row][i])) {
                max_row = k;
            }
        }
        swap(A[i], A[max_row]);
        swap(b[i], b[max_row]);

        // Make elements below pivot zero
        for (int k = i + 1; k < n; k++) {
            complex<double> factor = A[k][i] / A[i][i];
            for (int j = i; j < n; j++) {
                A[k][j] -= factor * A[i][j];
            }
            b[k] -= factor * b[i];
        }
    }

    // Back substitution
    vector<complex<double>> x(n);
    for (int i = n - 1; i >= 0; i--) {
        x[i] = b[i];
        for (int j = i + 1; j < n; j++) {
            x[i] -= A[i][j] * x[j];
        }
        x[i] /= A[i][i];
    }
    return x;
}

// Add a version for real numbers
vector<double> gaussianElimination(vector<vector<double>> A, vector<double> b) {
    int n = A.size();

    for (int i = 0; i < n; i++) {
        // Find pivot
        int max_row = i;
        for (int k = i + 1; k < n; k++) {
            if (abs(A[k][i]) > abs(A[max_row][i])) {
                max_row = k;
            }
        }
        swap(A[i], A[max_row]);
        swap(b[i], b[max_row]);

        // Make elements below pivot zero
        for (int k = i + 1; k < n; k++) {
            double factor = A[k][i] / A[i][i];
            for (int j = i; j < n; j++) {
                A[k][j] -= factor * A[i][j];
            }
            b[k] -= factor * b[i];
        }
    }

    // Back substitution
    vector<double> x(n);
    for (int i = n - 1; i >= 0; i--) {
        x[i] = b[i];
        for (int j = i + 1; j < n; j++) {
            x[i] -= A[i][j] * x[j];
        }
        x[i] /= A[i][i];
    }
    return x;
}

// Other functions (display_vec2D, display_vec, test_solver) remain the same...
void test_solver() {
    vector<vector<double>> a = {{1, 6, 3, 6},
                                {2, 3, 5, 6},
                                {4, 8, 1, 3},
                                {8, 3, 5, 7}};
    vector<double> b = {2, 7, 3, 2};
    display_vec2D(a);
    display_vec(gaussianElimination(a, b));
    display_vec(b);
}

void display_vec2D(vector<vector<double>> a) {
    for (auto it = a.begin(); it != a.end(); it++) {
        for (auto itr = it->begin(); itr != it->end(); itr++) {
            cout << *itr << " ";
        }
        cout << endl;
    }
    cout << endl << endl;
}

void display_vec(vector<double> a) {
    for (auto it = a.begin(); it != a.end(); it++) {
        cout << *it << endl;
    }
    cout << endl;
}