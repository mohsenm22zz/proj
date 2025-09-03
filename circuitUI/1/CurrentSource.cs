using System;

namespace CircuitSimulator
{
    public class CurrentSource : Component
    {
        public double Value { get; set; }
        public bool Diode { get; set; }

        public CurrentSource()
        {
            Value = 0.0;
            Diode = false;
        }

        public override double GetCurrent()
        {
            return Value;
        }

        public override double GetVoltage()
        {
            if (Node1 == null || Node2 == null) return 0.0;
            return Math.Abs(Node1.GetVoltage() - Node2.GetVoltage());
        }
    }
}