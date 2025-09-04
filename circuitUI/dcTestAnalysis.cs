using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;


public static class DCSolver
{
    public static string result;
    // Abstract base class for any circuit component.
    private abstract class Component
    {
        public string Name { get; }
        public string Node1 { get; }
        public string Node2 { get; }

        protected Component(string name, string node1, string node2)
        {
            Name = name;
            Node1 = node1;
            Node2 = node2;
        }
    }

    // Represents a Resistor in the circuit.
    private class Resistor : Component
    {
        public double Resistance { get; }
        public Resistor(string name, string node1, string node2, double resistance)
            : base(name, node1, node2)
        {
            if (resistance <= 0) throw new ArgumentException("Resistance must be positive.", nameof(resistance));
            Resistance = resistance;
        }
    }

    // Represents an ideal DC Voltage Source.
    private class VoltageSource : Component
    {
        public double Voltage { get; }
        public VoltageSource(string name, string node1, string node2, double voltage)
            : base(name, node1, node2)
        {
            // By SPICE convention, Node1 is the positive terminal.
            Voltage = voltage;
        }
    }

    /// <summary>
    /// Performs a DC analysis on a circuit defined by a netlist using Modified Nodal Analysis.
    /// </summary>
    /// <param name="netlistLines">A list of strings representing the circuit definition line by line.</param>
    /// <returns>A formatted string containing the calculated node voltages and component currents.</returns>
    public static string AnalyzeDCCircuit(List<string> netlistLines)
    {
        // 1. --- PARSE THE NETLIST ---
        var components = new List<Component>();
        var nodes = new HashSet<string>();
        
        foreach (var line in netlistLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1) continue;

            var type = parts[0][0];
            
            // The 'GND' line is just for definition, skip processing it as a component.
            if (type == 'G') continue; 

            // Basic validation for component line format
            if (parts.Length < 4) throw new FormatException($"Invalid component definition: '{line}'. Expected at least 4 parts.");
            
            var name = parts[0];
            var node1 = parts[1];
            var node2 = parts[2];
            if (!double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                throw new FormatException($"Invalid value '{parts[3]}' in line: '{line}'.");
            }

            nodes.Add(node1);
            nodes.Add(node2);

            switch (char.ToUpper(type))
            {
                case 'R':
                    components.Add(new Resistor(name, node1, node2, value));
                    break;
                case 'V':
                    components.Add(new VoltageSource(name, node1, node2, value));
                    break;
                default:
                     throw new NotSupportedException($"Component type '{type}' is not supported.");
            }
        }

        var voltageSources = components.OfType<VoltageSource>().ToList();
        
        // Exclude the ground node "0" from our system of equations.
        var nonGroundNodes = nodes.Where(n => n != "0").ToList();
        nonGroundNodes.Sort(); // Sort for consistent matrix indexing.
        
        // Create a map from node name to its index in the matrix.
        var nodeMap = nonGroundNodes.Select((name, index) => (name, index))
                                    .ToDictionary(p => p.name, p => p.index);
        
        int numNodes = nonGroundNodes.Count;
        int numVSources = voltageSources.Count;
        int matrixSize = numNodes + numVSources;

        // 2. --- BUILD THE MNA MATRICES (A * x = b) ---
        var A = Matrix<double>.Build.Dense(matrixSize, matrixSize);
        var b = Vector<double>.Build.Dense(matrixSize);

        // "Stamp" each component's properties onto the matrices.
        foreach (var comp in components)
        {
            if (comp is Resistor r)
            {
                double conductance = 1.0 / r.Resistance;
                int n1 = r.Node1 != "0" ? nodeMap[r.Node1] : -1;
                int n2 = r.Node2 != "0" ? nodeMap[r.Node2] : -1;

                if (n1 != -1) A[n1, n1] += conductance;
                if (n2 != -1) A[n2, n2] += conductance;
                if (n1 != -1 && n2 != -1)
                {
                    A[n1, n2] -= conductance;
                    A[n2, n1] -= conductance;
                }
            }
        }
        
        for (int i = 0; i < voltageSources.Count; i++)
        {
            var v = voltageSources[i];
            int n_plus = v.Node1 != "0" ? nodeMap[v.Node1] : -1;
            int n_minus = v.Node2 != "0" ? nodeMap[v.Node2] : -1;
            int k = numNodes + i; // This is the index for the voltage source's current variable.

            if (n_plus != -1) { A[n_plus, k] = 1; A[k, n_plus] = 1; }
            if (n_minus != -1) { A[n_minus, k] = -1; A[k, n_minus] = -1; }
            b[k] = v.Voltage;
        }

        // 3. --- SOLVE THE SYSTEM ---
        Vector<double> x;
        try
        {
            x = A.Solve(b);
        }
        catch (Exception)   
        {
            // This is a critical exception to handle. It means the circuit is not solvable.
            throw new InvalidOperationException("Circuit analysis failed. The circuit may be ill-defined (e.g., floating nodes, voltage source loops), which results in a singular matrix.");
        }
        
        // 4. --- FORMAT THE OUTPUT ---
        var resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("--- Node Voltages ---");

        var nodeVoltages = new Dictionary<string, double> { { "0", 0.0 } };
        foreach (var pair in nodeMap)
        {
            nodeVoltages[pair.Key] = x[pair.Value];
            resultBuilder.AppendLine($"V({pair.Key}): {x[pair.Value]:F4} V");
        }
        resultBuilder.AppendLine($"V(0): 0.0000 V"); // Explicitly show ground voltage.

        resultBuilder.AppendLine("\n--- Component Currents ---");
        
        // Calculate resistor currents using Ohm's Law and the solved node voltages.
        foreach (var comp in components.OfType<Resistor>())
        {
            double v1 = nodeVoltages[comp.Node1];
            double v2 = nodeVoltages[comp.Node2];
            double current = (v1 - v2) / comp.Resistance;
            resultBuilder.AppendLine($"I({comp.Name}): {current * 1000:F4} mA");
        }

        // Get voltage source currents directly from the solution vector 'x'.
        for (int i = 0; i < voltageSources.Count; i++)
        {
            var v = voltageSources[i];
            // The current from a source is negative of the variable in the solution vector,
            // based on the passive sign convention.
            double current = -x[numNodes + i]; 
            resultBuilder.AppendLine($"I({v.Name}): {current * 1000:F4} mA");
        }

        return resultBuilder.ToString();
    }
}

