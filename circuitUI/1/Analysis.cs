using System;
using System.Collections.Generic;
using System.Numerics;

namespace CircuitSimulator
{
    public class Analysis
    {
        /// <summary>
        /// Performs DC operating point analysis on the circuit.
        /// Capacitors are treated as open circuits, and inductors as short circuits.
        /// Iteratively solves for diode states.
        /// </summary>
        /// <param name="circuit">The circuit to analyze.</param>
        /// <returns>True if the analysis was successful, false otherwise.</returns>
        public static bool DCAnalysis(Circuit circuit)
        {
            try
            {
                Console.WriteLine("// Performing DC Analysis...");
                circuit.DeltaT = 1e12; // A large deltaT treats capacitors as open and inductors as short

                const int MAX_DIODE_ITERATIONS = 100;
                const double EPSILON_CURRENT = 1e-9;
                bool converged = false;
                int iterationCount = 0;
                
                // Start with all diodes off
                foreach (Diode diode in circuit.Diodes)
                {
                    diode.SetState(DiodeState.STATE_OFF);
                }

                do
                {
                    converged = true;
                    iterationCount++;

                    // Store previous diode states to check for convergence
                    List<DiodeState> previousDiodeStates = new List<DiodeState>();
                    foreach (Diode diode in circuit.Diodes)
                    {
                        previousDiodeStates.Add(diode.GetState());
                    }

                    circuit.AssignDiodeBranchIndices();

                    // Set up the Modified Nodal Analysis (MNA) matrices
                    // NOTE: These methods must be implemented in the Circuit class.
                    circuit.SetMnaA(AnalysisType.DC);
                    circuit.SetMnaRhs(AnalysisType.DC);

                    // Solve the system of linear equations: Ax = b
                    List<double> x = LinearSolver.GaussianElimination(circuit.MnaA, circuit.MnaRhs);

                    if (x.Count == 0)
                    {
                        Console.Error.WriteLine("Critical error: Linear solver failed during DC Analysis.");
                        return false;
                    }

                    // Update node voltages and branch currents with the solution
                    // NOTE: This method must be implemented in the Circuit class.
                    circuit.UpdateNodeVoltagesAndBranchCurrents(x);

                    // Check if any diode has changed its state
                    for (int i = 0; i < circuit.Diodes.Count; ++i)
                    {
                        Diode currentDiode = circuit.Diodes[i];
                        DiodeState oldState = previousDiodeStates[i];
                        DiodeState newState = oldState;

                        double vDiodeAcross = currentDiode.Node1.GetVoltage() - currentDiode.Node2.GetVoltage();

                        if (oldState == DiodeState.STATE_OFF && vDiodeAcross >= currentDiode.GetForwardVoltage() - EPSILON_CURRENT)
                        {
                            newState = DiodeState.STATE_FORWARD_ON;
                        }
                        else if (oldState == DiodeState.STATE_FORWARD_ON && currentDiode.GetCurrent() < -EPSILON_CURRENT)
                        {
                            newState = DiodeState.STATE_OFF;
                        }

                        if (newState != oldState)
                        {
                            converged = false;
                            currentDiode.SetState(newState);
                        }
                    }

                } while (!converged && iterationCount < MAX_DIODE_ITERATIONS);

                if (!converged)
                {
                    Console.Error.WriteLine("Warning: DC analysis for diodes did not converge after " + MAX_DIODE_ITERATIONS + " iterations.");
                }

                Console.WriteLine("// DC Analysis complete.");
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Critical error during DC Analysis: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Performs transient analysis on the circuit over a specified time interval.
        /// </summary>
        /// <param name="circuit">The circuit to analyze.</param>
        /// <param name="tStep">The time step for the simulation.</param>
        /// <param name="tStop">The total duration of the simulation.</param>
        /// <returns>True if the analysis was successful, false otherwise.</returns>
        public static bool TransientAnalysis(Circuit circuit, double tStep, double tStop)
        {
            try
            {
                Console.WriteLine("// Performing Transient Analysis...");
                circuit.ClearComponentHistory();

                // Run DC analysis to get initial conditions at t=0
                if (!DCAnalysis(circuit))
                {
                    Console.Error.WriteLine("Initial DC analysis failed. Aborting transient analysis.");
                    return false;
                }

                // Save initial states for capacitors and inductors
                foreach (Capacitor cap in circuit.Capacitors)
                {
                    cap.PrevVoltage = cap.GetVoltage();
                }
                foreach (Inductor ind in circuit.Inductors)
                {
                    ind.PrevCurrent = ind.GetCurrent();
                }
                
                List<Node> nonGroundNodes = new List<Node>();
                foreach (Node node in circuit.Nodes)
                {
                    if (!node.IsGround)
                    {
                       nonGroundNodes.Add(node);
                       node.AddVoltageHistoryPoint(0.0, node.GetVoltage());
                    }
                }
                
                circuit.DeltaT = tStep;

                for (double t = tStep; t <= tStop; t += tStep)
                {
                    // Set up the MNA matrices for the current time step
                    circuit.SetMnaA(AnalysisType.TRANSIENT);
                    circuit.SetMnaRhs(AnalysisType.TRANSIENT, t);
                    
                    // Solve the system of linear equations
                    List<double> x = LinearSolver.GaussianElimination(circuit.MnaA, circuit.MnaRhs);
                     if (x.Count == 0)
                    {
                        Console.Error.WriteLine("Critical error: Linear solver failed during Transient Analysis at t=" + t);
                        return false;
                    }
                    
                    // Update the circuit state with the new solution
                    circuit.UpdateNodeVoltagesAndBranchCurrents(x);
                    
                    // Store the current state to be used as the "previous" state in the next iteration
                    circuit.UpdateComponentStates(); 

                    // Store results for plotting
                    foreach(var node in nonGroundNodes)
                    {
                        node.AddVoltageHistoryPoint(t, node.GetVoltage());
                    }
                }
                Console.WriteLine("// Transient Analysis complete.");
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Critical error during Transient Analysis: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Performs an AC frequency sweep analysis.
        /// </summary>
        /// <param name="circuit">The circuit to analyze.</param>
        /// <param name="sourceName">The name of the AC voltage source to sweep.</param>
        /// <param name="startFreq">The starting frequency in Hz.</param>
        /// <param name="stopFreq">The ending frequency in Hz.</param>
        /// <param name="numPoints">The number of points to simulate.</param>
        /// <param name="sweepType">The type of sweep ("Linear" or "Log").</param>
        /// <returns>The number of points successfully calculated.</returns>
        public static int ACSweepAnalysis(Circuit circuit, string sourceName, double startFreq, double stopFreq, int numPoints, string sweepType)
        {
            try
            {
                Console.WriteLine("// Performing AC Sweep Analysis...");
                circuit.ClearComponentHistory();
                ACVoltageSource acSource = circuit.FindACVoltageSource(sourceName);
                if (acSource == null)
                {
                    Console.Error.WriteLine("Error: AC source '" + sourceName + "' not found.");
                    return 0;
                }

                int pointsCalculated = 0;
                for (int i = 0; i < numPoints; ++i)
                {
                    double currentFreq;
                    if (numPoints == 1)
                    {
                        currentFreq = startFreq;
                    }
                    else if (sweepType == "Linear")
                    {
                        currentFreq = startFreq + i * (stopFreq - startFreq) / (numPoints - 1);
                    }
                    else // Logarithmic (Decade)
                    {
                        if (numPoints <= 1)
                        {
                            currentFreq = startFreq;
                        }
                        else
                        {
                             currentFreq = startFreq * Math.Pow(10.0, i / (double)(numPoints - 1) * Math.Log10(stopFreq / startFreq));
                        }
                    }

                    if (currentFreq <= 0) continue;

                    // Set up the complex MNA matrices for the current frequency
                    circuit.SetMnaA(AnalysisType.AC_SWEEP, currentFreq);
                    circuit.SetMnaRhs(AnalysisType.AC_SWEEP, currentFreq);
                    
                    // Solve the complex system of equations
                    List<Complex> x = LinearSolver.GaussianElimination(circuit.MnaAComplex, circuit.MnaRhsComplex);
                     if (x.Count == 0)
                    {
                        Console.Error.WriteLine("Critical error: Linear solver failed during AC Sweep at f=" + currentFreq);
                        continue; // Try the next point
                    }

                    // Update the circuit with the complex solution (phasors)
                    // NOTE: This method must be implemented in the Circuit class.
                    circuit.UpdateNodeVoltagesAndBranchCurrentsAC(x, currentFreq);
                    
                    pointsCalculated++;
                }
                Console.WriteLine("// AC Sweep Analysis complete.");
                return pointsCalculated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Critical error during AC Sweep: " + e.Message);
                return 0;
            }
        }

        // PhaseSweepAnalysis is less common. For now, it's stubbed out similarly to ACSweep.
        // A full implementation would require careful handling of how results are stored and interpreted.
        public static int PhaseSweepAnalysis(Circuit circuit, string sourceName, double baseFreq, double startPhase, double stopPhase, int numPoints)
        {
            try
            {
                Console.WriteLine("// Performing Phase Sweep Analysis...");
                circuit.ClearComponentHistory();
                ACVoltageSource acSource = circuit.FindACVoltageSource(sourceName);
                if (acSource == null)
                {
                    Console.Error.WriteLine("Error: AC source '" + sourceName + "' not found.");
                    return 0;
                }

                double originalPhase = acSource.Phase; // Save original phase
                int pointsCalculated = 0;

                for (int i = 0; i < numPoints; ++i)
                {
                    double currentPhase = (numPoints == 1) ? startPhase : startPhase + i * (stopPhase - startPhase) / (numPoints - 1);
                    acSource.Phase = currentPhase;

                    // Each step of a phase sweep is an AC analysis at a fixed frequency
                    circuit.SetMnaA(AnalysisType.AC_SWEEP, baseFreq);
                    circuit.SetMnaRhs(AnalysisType.AC_SWEEP, baseFreq);

                    List<Complex> x = LinearSolver.GaussianElimination(circuit.MnaAComplex, circuit.MnaRhsComplex);
                     if (x.Count == 0)
                    {
                        Console.Error.WriteLine("Critical error: Linear solver failed during Phase Sweep at phase=" + currentPhase);
                        continue; 
                    }
                    
                    // NOTE: You might want a different update method to store results by phase instead of frequency.
                    circuit.UpdateNodeVoltagesAndBranchCurrentsAC(x, baseFreq);

                    pointsCalculated++;
                }

                acSource.Phase = originalPhase; // Restore original phase
                Console.WriteLine("// Phase Sweep Analysis complete.");
                return pointsCalculated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Critical error during Phase Sweep: " + e.Message);
                return 0;
            }
        }
    }
}
