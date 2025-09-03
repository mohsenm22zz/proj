using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CircuitSimulator
{
    // Enums required by the Analysis and Circuit classes
    public enum AnalysisType { DC, TRANSIENT, AC_SWEEP }
    public enum DiodeState { STATE_OFF, STATE_FORWARD_ON }

    public class Circuit
    {
        // Component lists
        public List<Node> Nodes { get; } = new List<Node>();
        public List<Resistor> Resistors { get; } = new List<Resistor>();
        public List<Capacitor> Capacitors { get; } = new List<Capacitor>();
        public List<Inductor> Inductors { get; } = new List<Inductor>();
        public List<VoltageSource> VoltageSources { get; } = new List<VoltageSource>();
        public List<CurrentSource> CurrentSources { get; } = new List<CurrentSource>();
        public List<ACVoltageSource> ACVoltageSources { get; } = new List<ACVoltageSource>();
        public List<Diode> Diodes { get; } = new List<Diode>();
        
        // MNA matrices and vectors
        public List<List<double>> MnaA { get; private set; }
        public List<double> MnaRhs { get; private set; }
        public List<List<Complex>> MnaAComplex { get; private set; }
        public List<Complex> MnaRhsComplex { get; private set; }

        // Simulation parameters
        public double DeltaT { get; set; }

        private Dictionary<int, int> nodeMap = new Dictionary<int, int>();
        private int branchCount;
        private int nextNodeId = 1; // Start from 1 since 0 is reserved for ground

        public Circuit()
        {
            // Ensure there is always a ground node
            AddNode(0, true);
        }

        public Node AddNode(int id, bool isGround = false)
        {
            var existingNode = FindNode(id);
            if (existingNode != null) return existingNode;

            var newNode = new Node(id) { IsGround = isGround };
            Nodes.Add(newNode);
            return newNode;
        }

        public Node FindNode(int id) => Nodes.FirstOrDefault(n => n.Id == id);
        public ACVoltageSource FindACVoltageSource(string name) => ACVoltageSources.FirstOrDefault(s => s.Name == name);

        // Adding the missing FindOrCreateNode method
        public Node FindOrCreateNode(string nodeName)
        {
            // Try to find existing node with this name
            foreach (var node in Nodes)
            {
                if (node.Name == nodeName)
                    return node;
            }

            // If not found, create a new node
            var newNode = new Node(nextNodeId++) { Name = nodeName };
            Nodes.Add(newNode);
            return newNode;
        }

        private void AssignNodeAndBranchMaps()
        {
            nodeMap.Clear();
            int nonGroundNodeCount = 0;
            // Ensure consistent ordering for the matrix
            foreach (var node in Nodes.Where(n => !n.IsGround).OrderBy(n => n.Id))
            {
                nodeMap[node.Id] = nonGroundNodeCount++;
            }

            int currentBranch = nonGroundNodeCount;
            foreach (var vs in VoltageSources) { vs.BranchIndex = currentBranch++; }
            foreach (var acVs in ACVoltageSources) { acVs.BranchIndex = currentBranch++; }
            foreach (var ind in Inductors) { ind.BranchIndex = currentBranch++; }
            foreach (var diode in Diodes.Where(d => d.GetState() == DiodeState.STATE_FORWARD_ON)) 
            { 
                diode.BranchIndex = currentBranch++; 
            }
            
            branchCount = currentBranch - nonGroundNodeCount;
        }
        
        public void AssignDiodeBranchIndices()
        {
             AssignNodeAndBranchMaps();
        }

        private void InitializeMatrix(int size)
        {
            MnaA = new List<List<double>>(size);
            for (int i = 0; i < size; i++)
            {
                MnaA.Add(new List<double>(new double[size]));
            }
            MnaRhs = new List<double>(new double[size]);
        }
        
        private void InitializeComplexMatrix(int size)
        {
            MnaAComplex = new List<List<Complex>>(size);
            for (int i = 0; i < size; i++)
            {
                MnaAComplex.Add(new List<Complex>(new Complex[size]));
            }
            MnaRhsComplex = new List<Complex>(new Complex[size]);
        }

        public void SetMnaA(AnalysisType type, double freq = 0)
        {
            AssignNodeAndBranchMaps();
            int size = nodeMap.Count + branchCount;

            if (type == AnalysisType.AC_SWEEP)
            {
                InitializeComplexMatrix(size);
                double omega = 2 * Math.PI * freq;
                // Add component stamps for AC analysis
                foreach (var res in Resistors) res.AddStamp(this, nodeMap, omega);
                foreach (var cap in Capacitors) cap.AddStamp(this, nodeMap, omega);
                foreach (var ind in Inductors) ind.AddStamp(this, nodeMap, omega);
                foreach (var vs in VoltageSources) vs.AddStamp(this, nodeMap, omega);
                foreach (var acVs in ACVoltageSources) acVs.AddStamp(this, nodeMap, omega);
                foreach (var d in Diodes) d.AddStamp(this, nodeMap, omega);
            }
            else
            {
                InitializeMatrix(size);
                 // Add component stamps for DC/Transient analysis
                foreach (var res in Resistors) res.AddStamp(this, nodeMap);
                foreach (var cap in Capacitors) cap.AddStamp(this, nodeMap);
                foreach (var ind in Inductors) ind.AddStamp(this, nodeMap);
                foreach (var vs in VoltageSources) vs.AddStamp(this, nodeMap);
                foreach (var acVs in ACVoltageSources) acVs.AddStamp(this, nodeMap);
                foreach (var d in Diodes) d.AddStamp(this, nodeMap);
            }
        }

        public void SetMnaRhs(AnalysisType type, double timeOrFreq = 0)
        {
            int size = nodeMap.Count + branchCount;
            var emptyRhs = new double[size].ToList();
             var emptyRhsComplex = new Complex[size].ToList();


            if (type == AnalysisType.AC_SWEEP)
            {
                 MnaRhsComplex = emptyRhsComplex;
                // Add RHS stamps for AC analysis
                foreach (var acVs in ACVoltageSources) acVs.AddRhsStamp(this, timeOrFreq);
            }
            else
            {
                MnaRhs = emptyRhs;
                // Add RHS stamps for DC/Transient
                foreach (var cs in CurrentSources) cs.AddRhsStamp(this, nodeMap);
                foreach (var vs in VoltageSources) vs.AddRhsStamp(this, timeOrFreq);
                foreach (var acVs in ACVoltageSources) acVs.AddRhsStamp(this, timeOrFreq, type == AnalysisType.TRANSIENT);
                foreach (var cap in Capacitors) cap.AddRhsStamp(this, nodeMap);
                foreach (var ind in Inductors) ind.AddRhsStamp(this, nodeMap);
                foreach (var d in Diodes) d.AddRhsStamp(this, nodeMap);
            }
        }
        
        public void UpdateNodeVoltagesAndBranchCurrents(List<double> x)
        {
            foreach (var entry in nodeMap)
            {
                FindNode(entry.Key).Voltage = x[entry.Value];
            }
            foreach (var vs in VoltageSources) { vs.SetCurrent(x[vs.BranchIndex]); }
            foreach (var acVs in ACVoltageSources) { acVs.SetCurrent(x[acVs.BranchIndex]); }
            foreach (var ind in Inductors) { ind.Current = x[ind.BranchIndex]; }
            foreach (var d in Diodes.Where(d => d.GetState() == DiodeState.STATE_FORWARD_ON))
            { 
                d.SetCurrent(x[d.BranchIndex]); 
            }
        }

        public void UpdateNodeVoltagesAndBranchCurrentsAC(List<Complex> x, double freq)
        {
             foreach (var entry in nodeMap)
            {
                FindNode(entry.Key).AddPhasorHistoryPoint(freq, x[entry.Value]);
            }
        }

        public void UpdateComponentStates()
        {
            foreach (var cap in Capacitors)
            {
                cap.PrevVoltage = cap.Node1.GetVoltage() - cap.Node2.GetVoltage();
            }
            foreach (var ind in Inductors)
            {
                ind.PrevCurrent = ind.Current;
            }
        }

        public void ClearComponentHistory()
        {
             foreach (var node in Nodes) node.ClearVoltageHistory();
        }
    }
}