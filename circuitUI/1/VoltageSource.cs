using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class VoltageSource : Component
    {
        public double Voltage 
        { 
            get => Value; 
            set => Value = value; 
        }
        
        public int BranchIndex { get; set; }
        private double current;

        public VoltageSource(string name, Node node1, Node node2, double voltage)
            : base(name, node1, node2, voltage) { }

        public double GetCurrent() => current;
        public void SetCurrent(double value) => current = value;

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
        }
        
        public void AddRhsStamp(Circuit circuit, double time)
        {
            circuit.MnaRhs[BranchIndex] = Value;
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
        }
    }
}