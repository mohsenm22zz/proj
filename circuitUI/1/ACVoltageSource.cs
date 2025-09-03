using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class ACVoltageSource : Component
    {
        public double Magnitude 
        { 
            get => Value; 
            set => Value = value; 
        }
        
        public int BranchIndex { get; set; }
        public double DcOffset { get; set; }
        public double Amplitude { get; set; }
        public double Frequency { get; set; }
        public double Phase { get; set; } // in degrees
        private double current;

        public ACVoltageSource(string name, Node node1, Node node2, double dcOffset, double amplitude, double frequency, double phase)
            : base(name, node1, node2, 0) // base value not used
        {
            DcOffset = dcOffset;
            Amplitude = amplitude;
            Frequency = frequency;
            Phase = phase;
        }
        
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

        public void AddRhsStamp(Circuit circuit, double time, bool isTransient = false)
        {
            if (isTransient)
            {
                double phaseRad = Phase * Math.PI / 180.0;
                circuit.MnaRhs[BranchIndex] = DcOffset + Amplitude * Math.Sin(2 * Math.PI * Frequency * time + phaseRad);
            }
            else
            {
                circuit.MnaRhs[BranchIndex] = DcOffset; // DC value
            }
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

         public void AddRhsStamp(Circuit circuit, double freq)
        {
            double phaseRad = Phase * Math.PI / 180.0;
            circuit.MnaRhsComplex[BranchIndex] = Complex.FromPolarCoordinates(Amplitude, phaseRad);
        }
    }
}