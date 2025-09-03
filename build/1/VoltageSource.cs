using System;
using System.Collections.Generic;

namespace CircuitSimulator
{
    public class VoltageSource : Component
    {
        public double Value { get; set; }
        public double Current { get; set; }
        public bool Diode { get; set; }

        public List<Tuple<double, double>> CurrentHistory { get; private set; }
        public List<Tuple<double, double>> DcSweepCurrentHistory { get; private set; }

        public VoltageSource()
        {
            Value = 0.0;
            Current = 0.0;
            Diode = false;
            CurrentHistory = new List<Tuple<double, double>>();
            DcSweepCurrentHistory = new List<Tuple<double, double>>();
        }

        public override double GetCurrent()
        {
            return Current;
        }

        public override void SetCurrent(double current)
        {
            Current = current;
        }

        public override double GetVoltage()
        {
            return Value;
        }

        public void AddCurrentHistoryPoint(double time, double current)
        {
            CurrentHistory.Add(new Tuple<double, double>(time, current));
        }

        public void ClearHistory()
        {
            CurrentHistory.Clear();
            DcSweepCurrentHistory.Clear();
        }
    }
}