using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class CurrentSource : Component
    {
        public CurrentSource(string name, Node node1, Node node2, double current)
            : base(name, node1, node2, current) { }

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            // Current sources only affect the RHS vector
        }

        public void AddRhsStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            if (nodeMap.TryGetValue(Node1.Id, out int n1)) circuit.MnaRhs[n1] -= Value;
            if (nodeMap.TryGetValue(Node2.Id, out int n2)) circuit.MnaRhs[n2] += Value;
        }
        
        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap, double omega)
        {
            // Ideal current sources do not contribute to the A matrix in AC analysis
        }
    }
}