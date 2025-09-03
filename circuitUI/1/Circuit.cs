using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public enum AnalysisType
    {
        DC,
        TRANSIENT,
        AC_SWEEP
    }

    public class Circuit
    {
        public List<Node> Nodes { get; private set; }
        public List<Resistor> Resistors { get; private set; }
        public List<Capacitor> Capacitors { get; private set; }
        public List<Inductor> Inductors { get; private set; }
        public List<Diode> Diodes { get; private set; }
        public List<VoltageSource> VoltageSources { get; private set; }
        public List<ACVoltageSource> AcVoltageSources { get; private set; }
        public List<CurrentSource> CurrentSources { get; private set; }
        public List<string> GroundNodeNames { get; private set; }

        public double DeltaT { get; set; }

        public List<List<double>> MNA_A { get; private set; }
        public List<double> MNA_RHS { get; private set; }

        public List<List<Complex>> MNA_A_Complex { get; private set; }
        public List<Complex> MNA_RHS_Complex { get; private set; }

        public Circuit()
        {
            Nodes = new List<Node>();
            Resistors = new List<Resistor>();
            Capacitors = new List<Capacitor>();
            Inductors = new List<Inductor>();
            Diodes = new List<Diode>();
            VoltageSources = new List<VoltageSource>();
            AcVoltageSources = new List<ACVoltageSource>();
            CurrentSources = new List<CurrentSource>();
            GroundNodeNames = new List<string>();

            DeltaT = 0;

            MNA_A = new List<List<double>>();
            MNA_RHS = new List<double>();

            MNA_A_Complex = new List<List<Complex>>();
            MNA_RHS_Complex = new List<Complex>();
        }

        public void AddNode(string name)
        {
            if (FindNode(name) == null)
            {
                Node newNode = new Node();
                newNode.Name = name;
                Nodes.Add(newNode);
            }
        }

        public Node FindNode(string name)
        {
            foreach (Node node in Nodes)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            return null;
        }

        public Node FindOrCreateNode(string name)
        {
            Node node = FindNode(name);
            if (node != null)
            {
                return node;
            }
            AddNode(name);
            return Nodes[Nodes.Count - 1];
        }

        public Resistor FindResistor(string name)
        {
            foreach (Resistor res in Resistors)
            {
                if (res.Name == name)
                    return res;
            }
            return null;
        }

        public Capacitor FindCapacitor(string name)
        {
            foreach (Capacitor cap in Capacitors)
            {
                if (cap.Name == name)
                    return cap;
            }
            return null;
        }

        public Inductor FindInductor(string name)
        {
            foreach (Inductor ind in Inductors)
            {
                if (ind.Name == name)
                    return ind;
            }
            return null;
        }

        public Diode FindDiode(string name)
        {
            foreach (Diode d in Diodes)
            {
                if (d.Name == name)
                    return d;
            }
            return null;
        }

        public CurrentSource FindCurrentSource(string name)
        {
            foreach (CurrentSource cs in CurrentSources)
            {
                if (cs.Name == name)
                    return cs;
            }
            return null;
        }

        public VoltageSource FindVoltageSource(string name)
        {
            foreach (VoltageSource vs in VoltageSources)
            {
                if (vs.Name == name)
                    return vs;
            }
            return null;
        }

        public ACVoltageSource FindACVoltageSource(string name)
        {
            foreach (ACVoltageSource acVs in AcVoltageSources)
            {
                if (acVs.Name == name)
                    return acVs;
            }
            return null;
        }

        public bool DeleteResistor(string name)
        {
            Resistor res = FindResistor(name);
            if (res != null)
            {
                Resistors.Remove(res);
                return true;
            }
            return false;
        }

        public bool DeleteCapacitor(string name)
        {
            Capacitor cap = FindCapacitor(name);
            if (cap != null)
            {
                Capacitors.Remove(cap);
                return true;
            }
            return false;
        }

        public bool DeleteInductor(string name)
        {
            Inductor ind = FindInductor(name);
            if (ind != null)
            {
                Inductors.Remove(ind);
                return true;
            }
            return false;
        }

        public bool DeleteDiode(string name)
        {
            Diode diode = FindDiode(name);
            if (diode != null)
            {
                Diodes.Remove(diode);
                return true;
            }
            return false;
        }

        public bool DeleteVoltageSource(string name)
        {
            VoltageSource vs = FindVoltageSource(name);
            if (vs != null)
            {
                VoltageSources.Remove(vs);
                return true;
            }
            return false;
        }

        public bool DeleteCurrentSource(string name)
        {
            CurrentSource cs = FindCurrentSource(name);
            if (cs != null)
            {
                CurrentSources.Remove(cs);
                return true;
            }
            return false;
        }

        public int CountTotalExtraVariables()
        {
            int mVars = VoltageSources.Count + Inductors.Count;
            foreach (Diode diode in Diodes)
            {
                if (diode.GetState() == DiodeState.STATE_FORWARD_ON || diode.GetState() == DiodeState.STATE_REVERSE_ON)
                {
                    mVars++;
                }
            }
            return mVars;
        }

        public void AssignDiodeBranchIndices()
        {
            int currentBranchIdx = VoltageSources.Count + Inductors.Count;
            foreach (Diode diode in Diodes)
            {
                if (diode.GetState() == DiodeState.STATE_FORWARD_ON || diode.GetState() == DiodeState.STATE_REVERSE_ON)
                {
                    diode.SetBranchIndex(currentBranchIdx++);
                }
                else
                {
                    diode.SetBranchIndex(-1);
                }
            }
        }

        public void SetDeltaT(double dt)
        {
            this.DeltaT = dt;
        }

        public void UpdateComponentStates()
        {
            foreach (Capacitor cap in Capacitors)
            {
                cap.Update(DeltaT);
            }
            foreach (Inductor ind in Inductors)
            {
                ind.Update(DeltaT);
            }
        }

        public void ClearComponentHistory()
        {
            foreach (Node node in Nodes)
            {
                node.ClearHistory();
            }
            foreach (VoltageSource vs in VoltageSources)
            {
                vs.ClearHistory();
            }
        }

        public int GetNodeMatrixIndex(Node targetNodePtr)
        {
            if (targetNodePtr == null || targetNodePtr.IsGround)
            {
                return -1;
            }
            int matrixIdx = 0;
            foreach (Node nInList in Nodes)
            {
                if (!nInList.IsGround)
                {
                    if (nInList.Num == targetNodePtr.Num)
                    {
                        return matrixIdx;
                    }
                    matrixIdx++;
                }
            }
            return -1;
        }

        public int CountNonGroundNodes()
        {
            int count = 0;
            foreach (Node node in Nodes)
            {
                if (!node.IsGround)
                {
                    count++;
                }
            }
            return count;
        }
    }
}