using System;

namespace CircuitSimulator
{
    public class Capacitor : Component
    {
        public double Capacitance { get; set; }
        public double PrevVoltage { get; set; }

        public Capacitor()
        {
            Capacitance = 0.0;
            PrevVoltage = 0.0;
        }

        public override double GetCurrent()
        {
            return 0.0;
        }

        public override double GetVoltage()
        {
            if (Node1 == null || Node2 == null) return 0.0;
            return Math.Abs(Node1.GetVoltage() - Node2.GetVoltage());
        }

        public void Update(double dt)
        {
            if (Node1 == null || Node2 == null) return;
            PrevVoltage = Node1.GetVoltage() - Node2.GetVoltage();
        }
    }
}