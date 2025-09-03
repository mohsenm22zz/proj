using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace wpfUI
{
    public enum ComponentType
    {
        Resistor,
        Capacitor,
        Inductor,
        VoltageSource,
        ACVoltageSource,
        CurrentSource,
        Diode
    }



    public class CircuitSimulatorService : IDisposable
    {
        private const string DllName = "CircuitSimulator.dll";

        static CircuitSimulatorService()
        {
            string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
            Console.WriteLine($"Looking for CircuitSimulator.dll at: {dllPath}");
            Console.WriteLine($"File exists: {System.IO.File.Exists(dllPath)}");
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateCircuit();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyCircuit(IntPtr circuit);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void AddNode(IntPtr circuit, string name);


    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void AddResistor(IntPtr circuit, string name, string node1, string node2, double value);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void AddCapacitor(IntPtr circuit, string name, string node1, string node2, double value);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void AddInductor(IntPtr circuit, string name, string node1, string node2, double value);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void AddVoltageSource(IntPtr circuit, string name, string node1, string node2, double voltage);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void AddACVoltageSource(IntPtr circuit, string name, string node1, string node2, double magnitude, double phase);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SetGroundNode(IntPtr circuit, string nodeName);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RunDCAnalysis(IntPtr circuit);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RunTransientAnalysis(IntPtr circuit, double stepTime, double stopTime);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void RunDCOperatingPoint(IntPtr circuit);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int RunACAnalysis(IntPtr circuit, string sourceName, double startFreq, double stopFreq, int numPoints, string sweepType);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int RunPhaseSweep(IntPtr circuit, string sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);

        // Component property accessors
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetComponentName(IntPtr component);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern double GetComponentResistance(IntPtr component);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetComponentResistance(IntPtr component, double resistance);

        // ... similar imports for capacitance, inductance, etc. ...

        // Circuit state management
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void SaveCircuitState(IntPtr circuit, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void LoadCircuitState(IntPtr circuit, string filename);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int RunPhaseSweepAnalysis(IntPtr circuit, string sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern double GetNodeVoltage(IntPtr circuit, string nodeName);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetNodeNames(IntPtr circuit, StringBuilder nodeNamesBuffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetNodeVoltageHistory(IntPtr circuit, string nodeName, [Out] double[] timePoints, [Out] double[] voltages, int maxCount);
            
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetNodeSweepHistory(IntPtr circuit, string nodeName, [Out] double[] frequencies, [Out] double[] magnitudes, int maxCount);
        
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetNodePhaseSweepHistory(IntPtr circuit, string nodeName, [Out] double[] phases, [Out] double[] magnitudes, int maxCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetComponentCurrentHistory(IntPtr circuit, string componentName, [Out] double[] timePoints, [Out] double[] currents, int maxCount);

        // --- NEW: P/Invoke signatures for getting DC results ---
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int GetAllVoltageSourceNames(IntPtr circuit, StringBuilder vsNamesBuffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern double GetVoltageSourceCurrent(IntPtr circuit, string vsName);


        private IntPtr circuitHandle;
        private bool disposed = false;

        public CircuitSimulatorService()
        {
            circuitHandle = CreateCircuit();
        }
        
        public void AddNode(string name) => AddNode(circuitHandle, name);
    public void AddResistor(string name, string node1, string node2, double value) => AddResistor(circuitHandle, name, node1, node2, value);
    public void AddCapacitor(string name, string node1, string node2, double value) => AddCapacitor(circuitHandle, name, node1, node2, value);
    public void AddInductor(string name, string node1, string node2, double value) => AddInductor(circuitHandle, name, node1, node2, value);
        public void AddVoltageSource(string name, string node1, string node2, double voltage) => AddVoltageSource(circuitHandle, name, node1, node2, voltage);
        public void AddACVoltageSource(string name, string node1, string node2, double magnitude, double phase) => AddACVoltageSource(circuitHandle, name, node1, node2, magnitude, phase);
        public void SetGroundNode(string nodeName) => SetGroundNode(circuitHandle, nodeName);
        public bool RunDCAnalysis() => RunDCAnalysis(circuitHandle);
        public bool RunTransientAnalysis(double stepTime, double stopTime) => RunTransientAnalysis(circuitHandle, stepTime, stopTime);
        public int RunPhaseSweepAnalysis(string sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints) => RunPhaseSweepAnalysis(circuitHandle, sourceName, baseFreq, startPhase, stopPhase, numPoints);
        public double GetNodeVoltage(string nodeName) => GetNodeVoltage(circuitHandle, nodeName);

        public string[] GetNodeNames()
        {
            const int maxBufferSize = 2048;
            StringBuilder buffer = new StringBuilder(maxBufferSize);
            int length = GetNodeNames(circuitHandle, buffer, buffer.Capacity);

            if (length > 0)
            {
                return buffer.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }

        /// <summary>
        /// Gets the voltage history for a node
        /// Returns a tuple with Item1 = time points array, Item2 = voltage values array
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <returns>Tuple with time points and voltage values</returns>
        public Tuple<double[], double[]> GetNodeVoltageHistory(string nodeName)
        {
            const int maxPoints = 2000;
            double[] timePoints = new double[maxPoints];
            double[] voltages = new double[maxPoints];
            int count = GetNodeVoltageHistory(circuitHandle, nodeName, timePoints, voltages, maxPoints);
            Array.Resize(ref timePoints, count);
            Array.Resize(ref voltages, count);
            return Tuple.Create(timePoints, voltages);
        }

        /// <summary>
        /// Gets the current history for a component
        /// Returns a tuple with Item1 = time points array, Item2 = current values array
        /// </summary>
        /// <param name="componentName">Name of the component</param>
        /// <returns>Tuple with time points and current values</returns>
        public Tuple<double[], double[]> GetComponentCurrentHistory(string componentName)
        {
            const int maxPoints = 2000;
            double[] timePoints = new double[maxPoints];
            double[] currents = new double[maxPoints];
            int count = GetComponentCurrentHistory(circuitHandle, componentName, timePoints, currents, maxPoints);
            Array.Resize(ref timePoints, count);
            Array.Resize(ref currents, count);
            return Tuple.Create(timePoints, currents);
        }

        public Tuple<double[], double[]> GetNodeSweepHistory(string nodeName)
        {
            const int maxPoints = 2000;
            double[] frequencies = new double[maxPoints];
            double[] magnitudes = new double[maxPoints];
            int count = GetNodeSweepHistory(circuitHandle, nodeName, frequencies, magnitudes, maxPoints);
            Array.Resize(ref frequencies, count);
            Array.Resize(ref magnitudes, count);
            return Tuple.Create(frequencies, magnitudes);
        }
        
        public Tuple<double[], double[]> GetNodePhaseSweepHistory(string nodeName)
        {
            const int maxPoints = 2000;
            double[] phases = new double[maxPoints];
            double[] magnitudes = new double[maxPoints];
            int count = GetNodePhaseSweepHistory(circuitHandle, nodeName, phases, magnitudes, maxPoints);
            Array.Resize(ref phases, count);
            Array.Resize(ref magnitudes, count);
            return Tuple.Create(phases, magnitudes);
        }
        
        // --- NEW: Public method to get all DC results for the results window ---
        public Dictionary<string, double> GetAllDCResults()
        {
            var results = new Dictionary<string, double>();
            
            // Get all node voltages
            var nodeNames = GetNodeNames();
            foreach (var nodeName in nodeNames)
            {
                results[$"V({nodeName})"] = GetNodeVoltage(nodeName);
            }

            // Get all voltage source currents
            const int maxBufferSize = 2048;
            StringBuilder buffer = new StringBuilder(maxBufferSize);
            int length = GetAllVoltageSourceNames(circuitHandle, buffer, buffer.Capacity);
            if (length > 0)
            {
                var vsNames = buffer.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var vsName in vsNames)
                {
                    results[$"I({vsName})"] = GetVoltageSourceCurrent(circuitHandle, vsName);
                }
            }
            
            return results;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (circuitHandle != IntPtr.Zero)
                {
                    DestroyCircuit(circuitHandle);
                    circuitHandle = IntPtr.Zero;
                }
                disposed = true;
            }
        }

        ~CircuitSimulatorService()
        {
            Dispose(false);
        }

        public void RunDCOperatingPoint()
        {
            RunDCOperatingPoint(circuitHandle);
        }

        public Tuple<double[], double[]> RunACAnalysis(string sourceName, double startFreq, double stopFreq, int numPoints, string sweepType)
        {
            const int maxPoints = 2000;
            double[] frequencies = new double[maxPoints];
            double[] magnitudes = new double[maxPoints];
            int count = RunACAnalysis(circuitHandle, sourceName, startFreq, stopFreq, numPoints, sweepType);
            if (count > 0)
            {
                Array.Resize(ref frequencies, count);
                Array.Resize(ref magnitudes, count);
            }
            return Tuple.Create(frequencies, magnitudes);
        }

        public void SaveCircuitState(string filename)
        {
            SaveCircuitState(circuitHandle, filename);
        }

        public void LoadCircuitState(string filename)
        {
            LoadCircuitState(circuitHandle, filename);
        }
    }
}