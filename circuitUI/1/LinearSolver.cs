using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    /// <summary>
    /// Solves systems of linear equations using Gaussian elimination.
    /// This class provides methods for both real (double) and complex-valued systems.
    /// </summary>
    public class LinearSolver
    {
        /// <summary>
        /// Solves a system of complex linear equations Ax = b using Gaussian elimination with partial pivoting.
        /// </summary>
        /// <param name="A">The matrix of coefficients.</param>
        /// <param name="b">The right-hand side vector.</param>
        /// <returns>The solution vector x.</returns>
        public static List<Complex> GaussianElimination(List<List<Complex>> A, List<Complex> b)
        {
            int n = A.Count;

            // Forward Elimination with Partial Pivoting
            for (int i = 0; i < n; i++)
            {
                // Find pivot row
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Complex.Abs(A[k][i]) > Complex.Abs(A[maxRow][i]))
                    {
                        maxRow = k;
                    }
                }
                
                // Swap rows in matrix A
                (A[i], A[maxRow]) = (A[maxRow], A[i]);
                
                // Swap corresponding elements in vector b
                (b[i], b[maxRow]) = (b[maxRow], b[i]);

                // Make elements below the pivot zero
                for (int k = i + 1; k < n; k++)
                {
                    if (A[i][i] == Complex.Zero)
                    {
                        // This indicates a singular or near-singular matrix.
                        // For a simple implementation, we can continue, but a robust solver
                        // would throw an exception or handle this case more gracefully.
                        Console.Error.WriteLine("Warning: Matrix is singular or near-singular. Solution may be inaccurate.");
                        continue;
                    }
                    Complex factor = A[k][i] / A[i][i];
                    for (int j = i; j < n; j++)
                    {
                        A[k][j] -= factor * A[i][j];
                    }
                    b[k] -= factor * b[i];
                }
            }

            // Back Substitution
            List<Complex> x = new List<Complex>(new Complex[n]);
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= A[i][j] * x[j];
                }
                 if (A[i][i] == Complex.Zero)
                {
                     Console.Error.WriteLine("Warning: Division by zero during back substitution. Matrix is singular.");
                     // Returning an empty list or throwing an exception would be appropriate here.
                     return new List<Complex>();
                }
                x[i] /= A[i][i];
            }

            return x;
        }

        /// <summary>
        /// Solves a system of real linear equations Ax = b using Gaussian elimination with partial pivoting.
        /// </summary>
        /// <param name="A">The matrix of coefficients.</param>
        /// <param name="b">The right-hand side vector.</param>
        /// <returns>The solution vector x.</returns>
        public static List<double> GaussianElimination(List<List<double>> A, List<double> b)
        {
            int n = A.Count;

            // Forward Elimination with Partial Pivoting
            for (int i = 0; i < n; i++)
            {
                // Find pivot row
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(A[k][i]) > Math.Abs(A[maxRow][i]))
                    {
                        maxRow = k;
                    }
                }
                
                // Swap rows in matrix A
                (A[i], A[maxRow]) = (A[maxRow], A[i]);
                
                // Swap corresponding elements in vector b
                (b[i], b[maxRow]) = (b[maxRow], b[i]);


                // Make elements below the pivot zero
                for (int k = i + 1; k < n; k++)
                {
                     if (A[i][i] == 0)
                    {
                        Console.Error.WriteLine("Warning: Matrix is singular or near-singular. Solution may be inaccurate.");
                        continue;
                    }
                    double factor = A[k][i] / A[i][i];
                    for (int j = i; j < n; j++)
                    {
                        A[k][j] -= factor * A[i][j];
                    }
                    b[k] -= factor * b[i];
                }
            }

            // Back Substitution
            List<double> x = new List<double>(new double[n]);
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= A[i][j] * x[j];
                }
                if (A[i][i] == 0)
                {
                     Console.Error.WriteLine("Warning: Division by zero during back substitution. Matrix is singular.");
                     return new List<double>();
                }
                x[i] /= A[i][i];
            }

            return x;
        }
    }
}
