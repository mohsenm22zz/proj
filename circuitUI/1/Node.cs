using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Node
    {
        public int Id { get; }
        public string Name { get; set; }
        public bool IsGround { get; set; }
        public double Voltage { get; set; }

        // For storing analysis results
        public SortedDictionary<double, double> VoltageHistory { get; } = new SortedDictionary<double, double>();
        public SortedDictionary<double, Complex> PhasorHistory { get; } = new SortedDictionary<double, Complex>();

        public Node(int id)
        {
            Id = id;
            IsGround = false;
            Voltage = 0.0;
        }

        public double GetVoltage() => IsGround ? 0.0 : Voltage;

        public void AddVoltageHistoryPoint(double time, double voltage)
        {
            VoltageHistory[time] = voltage;
        }
        
        public void AddPhasorHistoryPoint(double freq, Complex phasor)
        {
            PhasorHistory[freq] = phasor;
        }

        public void ClearVoltageHistory()
        {
            VoltageHistory.Clear();
            PhasorHistory.Clear();
        }
        
        public void SetGround(bool isGround)
        {
            IsGround = isGround;
        }
    }
}