using System;
using System.Numerics;

namespace CircuitSimulator
{
    public class ACVoltageSource : Component
    {
        public double Magnitude { get; set; }
        public double Frequency { get; set; }
        public double Phase { get; set; }
        // Remove the duplicate Node1 and Node2 declarations since they're inherited from Component

        public ACVoltageSource()
        {
            Magnitude = 0.0;
            Frequency = 0.0;
            Phase = 0.0;
            Name = "";
            Node1 = null;
            Node2 = null;
        }

        public ACVoltageSource(string name, Node n1, Node n2, double mag, double ph)
        {
            Name = name;
            Magnitude = mag;
            Phase = ph;
            Node1 = n1;
            Node2 = n2;
        }

        public double GetValue(double time)
        {
            double phaseRad = Phase * Math.PI / 180.0;
            return Magnitude * Math.Sin(2 * Math.PI * Frequency * time + phaseRad);
        }

        public Complex GetPhasor()
        {
            // Convert from magnitude and phase (in degrees) to complex phasor
            double phaseRad = Phase * Math.PI / 180.0;
            return new Complex(Magnitude * Math.Cos(phaseRad), Magnitude * Math.Sin(phaseRad));
        }

        // Implement pure virtual functions from Component
        public override double GetCurrent()
        {
            return 0.0; // AC sources don't have a simple current value
        }

        public override double GetVoltage()
        {
            return Magnitude; // Return magnitude as voltage
        }
    }
}