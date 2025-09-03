using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Diode : Component
    {
        private const double G_ON = 1e3;  // High conductance when ON
        private const double G_OFF = 1e-9; // Low conductance when OFF
        private const double V_F = 0.7;    // Forward voltage drop

        public int BranchIndex { get; set; }
        private DiodeState state;
        private double current;

        public Diode(string name, Node node1, Node node2)
            : base(name, node1, node2, 0) // Value is not used
        {
            state = DiodeState.STATE_OFF;
        }

        public DiodeState GetState() => state;
        public void SetState(DiodeState newState) => state = newState;
        public double GetCurrent() => current;
        public void SetCurrent(double value) => current = value;
        public double GetForwardVoltage() => V_F;

        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            if (state == DiodeState.STATE_OFF)
            {
                if (nodeMap.TryGetValue(Node1.Id, out int n1)) circuit.MnaA[n1][n1] += G_OFF;
                if (nodeMap.TryGetValue(Node2.Id, out int n2)) circuit.MnaA[n2][n2] += G_OFF;
                if (n1 != -1 && n2 != -1 && nodeMap.ContainsKey(Node1.Id) && nodeMap.ContainsKey(Node2.Id))
                {
                    circuit.MnaA[n1][n2] -= G_OFF;
                    circuit.MnaA[n2][n1] -= G_OFF;
                }
            }
            else // STATE_FORWARD_ON
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
                circuit.MnaA[BranchIndex][BranchIndex] = -1.0 / G_ON;
            }
        }
        
        public void AddRhsStamp(Circuit circuit, Dictionary<int, int> nodeMap)
        {
            if (state == DiodeState.STATE_FORWARD_ON)
            {
                 circuit.MnaRhs[BranchIndex] = V_F;
            }
        }
        
        public void AddStamp(Circuit circuit, Dictionary<int, int> nodeMap, double omega)
        {
            // For AC analysis, we use the DC operating point to linearize the diode.
            // A simple model is to use its dynamic resistance at the Q-point.
            // For now, we'll use the same DC model.
            AddStamp(circuit, nodeMap);
        }
    }
}
