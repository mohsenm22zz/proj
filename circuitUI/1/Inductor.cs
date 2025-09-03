using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Inductor : Component
    {
        public double Inductance 
        { 
            get => Value; 
            set => Value = value; 
        }
        
        public int BranchIndex { get; set; }
        public double Current { get; set; }
        public double PrevCurrent { get; set; }

        public Inductor(string name, Node node1, Node node2, double inductance)
            : base(name, node1, node2, inductance) { }

        /// <summary>
        /// Gets the current flowing through the inductor.
        /// </summary>
        /// <returns>The current value.</returns>
        public double GetCurrent()
        {
            return Current;
        }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            if (nodeMap.TryGetValue(Node1.Id, out int n1))
            {
                circuit.MnaA[BranchIndex][n1] = 1;
                circuit.MnaA[n1][BranchIndex] = 1;
            }
            if (nodeMap.TryGetValue(Node2.Id, out int n2))
            {
                circuit.MnaA[BranchIndex][n2] = -1;
                circuit.MnaA[n2][BranchIndex] = -1;
            }

            // For DC, large deltaT makes this a large conductance (short circuit)
            // For transient, this is part of the BE model
            circuit.MnaA[BranchIndex][BranchIndex] = -Value / circuit.DeltaT;
        }
        
        public void AddRhsStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            circuit.MnaRhs[BranchIndex] = -(Value / circuit.DeltaT) * PrevCurrent;
        }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap, double omega)
        {
            if (nodeMap.TryGetValue(Node1.Id, out int n1))
            {
                circuit.MnaAComplex[BranchIndex][n1] = 1;
                circuit.MnaAComplex[n1][BranchIndex] = 1;
            }
            if (nodeMap.TryGetValue(Node2.Id, out int n2))
            {
                circuit.MnaAComplex[BranchIndex][n2] = -1;
                circuit.MnaAComplex[n2][BranchIndex] = -1;
            }
            circuit.MnaAComplex[BranchIndex][BranchIndex] = new Complex(0, -omega * Value);
        }
    }
}