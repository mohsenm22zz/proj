using System;

namespace CircuitSimulator
{
    public enum DiodeType
    {
        NORMAL,
        ZENER
    }

    public enum DiodeState
    {
        STATE_OFF = 0,
        STATE_FORWARD_ON = 1,
        STATE_REVERSE_ON = 2
    }

    public class Diode : Component
    {
        public DiodeType DiodeType { get; private set; }
        public double ForwardVoltage { get; private set; }
        public double ZenerVoltage { get; private set; }
        
        private DiodeState currentState;
        private int branchIndex;
        private double current;

        public Diode(string name, Node n1, Node n2, DiodeType type, double vf, double vz = 0.0)
        {
            Name = name;
            Node1 = n1;
            Node2 = n2;
            DiodeType = type;
            currentState = DiodeState.STATE_OFF;
            ForwardVoltage = vf;
            ZenerVoltage = vz;
            branchIndex = -1;
            current = 0.0;
        }

        public DiodeType GetDiodeType()
        {
            return DiodeType;
        }

        public double GetForwardVoltage()
        {
            return ForwardVoltage;
        }

        public double GetZenerVoltage()
        {
            return ZenerVoltage;
        }

        public void SetState(DiodeState state)
        {
            currentState = state;
        }

        public DiodeState GetState()
        {
            return currentState;
        }

        public void SetBranchIndex(int index)
        {
            branchIndex = index;
        }

        public int GetBranchIndex()
        {
            return branchIndex;
        }

        public override void SetCurrent(double c)
        {
            current = c;
        }

        public override double GetCurrent()
        {
            if (currentState == DiodeState.STATE_OFF)
            {
                return 0.0;
            }
            return current;
        }

        public override double GetVoltage()
        {
            if (Node1 == null || Node2 == null) return 0.0;
            if (currentState == DiodeState.STATE_FORWARD_ON)
            {
                return ForwardVoltage;
            }
            else if (currentState == DiodeState.STATE_REVERSE_ON)
            {
                return -ZenerVoltage;
            }
            return Node1.GetVoltage() - Node2.GetVoltage();
        }
    }
}