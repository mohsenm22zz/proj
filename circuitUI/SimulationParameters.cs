using System;

namespace wpfUI
{
    public class SimulationParameters
    {
        public enum AnalysisType { Transient, ACSweep, PhaseSweep, DCOperatingPoint }

        public AnalysisType CurrentAnalysis { get; set; }
        public double StopTime { get; set; }
        public double TimeToStartSavingData { get; set; }
        public double MaxTimestep { get; set; }
        public string SweepType { get; set; }
        public double StartFrequency { get; set; }
        public double StopFrequency { get; set; }
        public int NumberOfPoints { get; set; }
        public double BaseFrequency { get; set; }
        public double StartPhase { get; set; }
        public double StopPhase { get; set; }
    }
}
