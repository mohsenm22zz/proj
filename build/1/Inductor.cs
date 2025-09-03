using System;

namespace CircuitSimulator
{
    public class Inductor : Component
    {
        public double Inductance { get; set; }
        public double Current { get; set; }
        public double PrevCurrent { get; set; }

        public Inductor()
        {
            Inductance = 0.0;
            Current = 0.0;
            PrevCurrent = 0.0;
        }

        public override double GetCurrent()
        {
            return Current;
        }

        public override double GetVoltage()
        {
            return 0.0;
        }

        public void Update(double dt)
        {
            if (Node1 == null || Node2 == null) return;
            PrevCurrent = Current;
        }

        public void SetInductorCurrent(double current)
        {
            this.Current = current;
        }
    }
}