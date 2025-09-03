using System;

namespace CircuitSimulator
{
    public class Resistor : Component
    {
        public double Resistance { get; set; }

        public Resistor()
        {
            Resistance = 0.0;
        }

        public override double GetCurrent()
        {
            if (Resistance == 0) return double.PositiveInfinity;
            if (Node1 == null || Node2 == null) return 0.0;
            return (Node1.GetVoltage() - Node2.GetVoltage()) / Resistance;
        }

        public override double GetVoltage()
        {
            if (Node1 == null || Node2 == null) return 0.0;
            return Math.Abs(Node1.GetVoltage() - Node2.GetVoltage());
        }
    }
}