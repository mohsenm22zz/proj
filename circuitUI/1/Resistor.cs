using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Resistor : Component
    {
        public double Resistance 
        { 
            get => Value; 
            set => Value = value; 
        }

        public Resistor(string name, Node node1, Node node2, double resistance)
            : base(name, node1, node2, resistance) { }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            if (Value == 0) return; // Avoid division by zero for ideal wires
            double conductance = 1.0 / Value;

            if (nodeMap.TryGetValue(Node1.Id, out int n1))
            {
                circuit.MnaA[n1][n1] += conductance;
            }
            if (nodeMap.TryGetValue(Node2.Id, out int n2))
            {
                circuit.MnaA[n2][n2] += conductance;
            }
            if (n1 != -1 && n2 != -1 && nodeMap.ContainsKey(Node1.Id) && nodeMap.ContainsKey(Node2.Id))
            {
                circuit.MnaA[n1][n2] -= conductance;
                circuit.MnaA[n2][n1] -= conductance;
            }
        }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap, double omega)
        {
            if (Value == 0) return;
            Complex admittance = 1.0 / Value;

            if (nodeMap.TryGetValue(Node1.Id, out int n1))
            {
                circuit.MnaAComplex[n1][n1] += admittance;
            }
            if (nodeMap.TryGetValue(Node2.Id, out int n2))
            {
                circuit.MnaAComplex[n2][n2] += admittance;
            }
            if (n1 != -1 && n2 != -1 && nodeMap.ContainsKey(Node1.Id) && nodeMap.ContainsKey(Node2.Id))
            {
                circuit.MnaAComplex[n1][n2] -= admittance;
                circuit.MnaAComplex[n2][n1] -= admittance;
            }
        }
        
        public double GetCurrent()
        {
            if (Value == 0) return 0; // Avoid division by zero
            double voltage = Node1.GetVoltage() - Node2.GetVoltage();
            return voltage / Value;
        }
    }
}