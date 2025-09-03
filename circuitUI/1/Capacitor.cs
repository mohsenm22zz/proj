using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Capacitor : Component
    {
        public double Capacitance 
        { 
            get => Value; 
            set => Value = value; 
        }
        
        public double PrevVoltage { get; set; }

        public Capacitor(string name, Node node1, Node node2, double capacitance)
            : base(name, node1, node2, capacitance) { }

        /// <summary>
        /// Gets the voltage across the capacitor.
        /// </summary>
        /// <returns>The voltage difference between Node1 and Node2.</returns>
        public double GetVoltage()
        {
            return Node1.GetVoltage() - Node2.GetVoltage();
        }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            // For DC, large deltaT makes conductance near zero (open circuit)
            // For Transient, this is the BE model conductance
            double conductance = Value / circuit.DeltaT;

            if (nodeMap.TryGetValue(Node1.Id, out int n1)) circuit.MnaA[n1][n1] += conductance;
            if (nodeMap.TryGetValue(Node2.Id, out int n2)) circuit.MnaA[n2][n2] += conductance;
            if (n1 != -1 && n2 != -1 && nodeMap.ContainsKey(Node1.Id) && nodeMap.ContainsKey(Node2.Id))
            {
                circuit.MnaA[n1][n2] -= conductance;
                circuit.MnaA[n2][n1] -= conductance;
            }
        }

        public void AddRhsStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            double conductance = Value / circuit.DeltaT;
            double Ieq = conductance * PrevVoltage;

            if (nodeMap.TryGetValue(Node1.Id, out int n1)) circuit.MnaRhs[n1] -= Ieq;
            if (nodeMap.TryGetValue(Node2.Id, out int n2)) circuit.MnaRhs[n2] += Ieq;
        }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap, double omega)
        {
            Complex admittance = new Complex(0, omega * Value);

            if (nodeMap.TryGetValue(Node1.Id, out int n1)) circuit.MnaAComplex[n1][n1] += admittance;
            if (nodeMap.TryGetValue(Node2.Id, out int n2)) circuit.MnaAComplex[n2][n2] += admittance;
            if (n1 != -1 && n2 != -1 && nodeMap.ContainsKey(Node1.Id) && nodeMap.ContainsKey(Node2.Id))
            {
                circuit.MnaAComplex[n1][n2] -= admittance;
                circuit.MnaAComplex[n2][n1] -= admittance;
            }
        }
    }
}