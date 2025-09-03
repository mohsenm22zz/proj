using System;
using System.Collections.Generic;

namespace CircuitSimulator
{
    public class Node
    {
        private static int nextNum = 0;
        public string Name { get; set; }
        public int Num { get; private set; }
        public double Voltage { get; set; }
        public bool IsGround { get; set; }

        public List<Tuple<double, double>> VoltageHistory { get; private set; }
        public List<Tuple<double, double>> DcSweepHistory { get; private set; }
        public List<Tuple<double, double>> AcSweepHistory { get; private set; }
        public List<Tuple<double, double>> PhaseSweepHistory { get; private set; }

        public Node()
        {
            Name = "";
            Num = nextNum++;
            Voltage = 0.0;
            IsGround = false;
            VoltageHistory = new List<Tuple<double, double>>();
            DcSweepHistory = new List<Tuple<double, double>>();
            AcSweepHistory = new List<Tuple<double, double>>();
            PhaseSweepHistory = new List<Tuple<double, double>>();
        }

        public double GetVoltage()
        {
            if (IsGround) return 0.0;
            return Voltage;
        }

        public void SetVoltage(double voltage)
        {
            if (IsGround)
            {
                Voltage = 0.0;
            }
            else
            {
                Voltage = voltage;
            }
        }

        public void SetGround(bool groundStatus)
        {
            IsGround = groundStatus;
            if (IsGround)
            {
                Voltage = 0.0;
            }
        }

        public void AddVoltageHistoryPoint(double time, double voltage)
        {
            VoltageHistory.Add(new Tuple<double, double>(time, voltage));
        }

        public void ClearHistory()
        {
            VoltageHistory.Clear();
            DcSweepHistory.Clear();
            AcSweepHistory.Clear();
            PhaseSweepHistory.Clear();
        }
    }
}