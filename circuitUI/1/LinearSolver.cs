using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class LinearSolver
    {
        public static List<Complex> GaussianElimination(List<List<Complex>> A, List<Complex> b)
        {
            int n = A.Count;

            for (int i = 0; i < n; i++)
            {
                // Find pivot
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Complex.Abs(A[k][i]) > Complex.Abs(A[maxRow][i]))
                    {
                        maxRow = k;
                    }
                }
                
                // Swap rows
                List<Complex> tempRow = A[i];
                A[i] = A[maxRow];
                A[maxRow] = tempRow;
                
                Complex tempVal = b[i];
                b[i] = b[maxRow];
                b[maxRow] = tempVal;

                // Make elements below pivot zero
                for (int k = i + 1; k < n; k++)
                {
                    Complex factor = A[k][i] / A[i][i];
                    for (int j = i; j < n; j++)
                    {
                        A[k][j] -= factor * A[i][j];
                    }
                    b[k] -= factor * b[i];
                }
            }

            // Back substitution
            List<Complex> x = new List<Complex>(new Complex[n]);
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= A[i][j] * x[j];
                }
                x[i] /= A[i][i];
            }
            return x;
        }

        // Add a version for real numbers
        public static List<double> GaussianElimination(List<List<double>> A, List<double> b)
        {
            int n = A.Count;

            for (int i = 0; i < n; i++)
            {
                // Find pivot
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(A[k][i]) > Math.Abs(A[maxRow][i]))
                    {
                        maxRow = k;
                    }
                }
                
                // Swap rows
                List<double> tempRow = A[i];
                A[i] = A[maxRow];
                A[maxRow] = tempRow;
                
                double tempVal = b[i];
                b[i] = b[maxRow];
                b[maxRow] = tempVal;

                // Make elements below pivot zero
                for (int k = i + 1; k < n; k++)
                {
                    double factor = A[k][i] / A[i][i];
                    for (int j = i; j < n; j++)
                    {
                        A[k][j] -= factor * A[i][j];
                    }
                    b[k] -= factor * b[i];
                }
            }

            // Back substitution
            List<double> x = new List<double>(new double[n]);
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= A[i][j] * x[j];
                }
                x[i] /= A[i][i];
            }
            return x;
        }
    }
}