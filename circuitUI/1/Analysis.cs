using System;
using System.Collections.Generic;

namespace CircuitSimulator
{
    public class Analysis
    {
        public static bool DCAnalysis(Circuit circuit)
        {
            try
            {
                Console.WriteLine("// Performing DC Analysis...");
                circuit.DeltaT = 1e12; // Treat capacitors as open, inductors as short
                
                List<Node> nonGroundNodes = new List<Node>();
                foreach (Node node in circuit.Nodes)
                {
                    if (!node.IsGround)
                    {
                        nonGroundNodes.Add(node);
                    }
                }

                const int MAX_DIODE_ITERATIONS = 100;
                bool converged = false;
                int iterationCount = 0;
                const double EPSILON_CURRENT = 1e-9;

                foreach (Diode diode in circuit.Diodes)
                {
                    diode.SetState(DiodeState.STATE_OFF);
                }

                do
                {
                    converged = true;
                    iterationCount++;

                    List<DiodeState> previousDiodeStates = new List<DiodeState>();
                    foreach (Diode diode in circuit.Diodes)
                    {
                        previousDiodeStates.Add(diode.GetState());
                    }

                    circuit.AssignDiodeBranchIndices();
                    // TODO: Implement SetMnaA and SetMnaRHS methods
                    // circuit.Set_MNA_A(AnalysisType.DC);
                    // circuit.Set_MNA_RHS(AnalysisType.DC);

                    // Placeholder for matrix solving
                    // This would require implementing the matrix operations in C#

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
                    }
                }

                foreach (Node node in circuit.Nodes)
                {
                    if (!node.IsGround) node.AddVoltageHistoryPoint(0.0, node.GetVoltage());
                }

                circuit.DeltaT = tStep;

                for (double t = tStep; t <= tStop; t += tStep)
                {
                    // TODO: Implement SetMnaA and SetMnaRHS methods
                    // circuit.Set_MNA_A(AnalysisType.TRANSIENT);
                    // circuit.Set_MNA_RHS(AnalysisType.TRANSIENT);

                    // Placeholder for matrix solving
                    // This would require implementing the matrix operations in C#

                    circuit.UpdateComponentStates(); // Update prevVoltage/prevCurrent for next step
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

                List<Node> nonGroundNodes = new List<Node>();
                foreach (Node node in circuit.Nodes)
                {
                    if (!node.IsGround) nonGroundNodes.Add(node);
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
                    else
                    { // Logarithmic (Decade)
                        currentFreq = startFreq * Math.Pow(10.0, i / (double)(numPoints - 1) * Math.Log10(stopFreq / startFreq));
                    }

                    if (currentFreq <= 0) continue;

                    // TODO: Implement SetMnaA and SetMnaRHS methods for AC
                    // circuit.Set_MNA_A(AnalysisType.AC_SWEEP, currentFreq);
                    // circuit.Set_MNA_RHS(AnalysisType.AC_SWEEP, currentFreq);

                    // Placeholder for matrix solving
                    // This would require implementing the matrix operations in C#

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

                List<Node> nonGroundNodes = new List<Node>();
                foreach (Node node in circuit.Nodes)
                {
                    if (!node.IsGround) nonGroundNodes.Add(node);
                }

                double originalPhase = acSource.Phase; // Save original phase
                int pointsCalculated = 0;

                for (int i = 0; i < numPoints; ++i)
                {
                    double currentPhase = (numPoints == 1) ? startPhase : startPhase + i * (stopPhase - startPhase) / (numPoints - 1);
                    acSource.Phase = currentPhase;

                    // TODO: Implement SetMnaA and SetMnaRHS methods for AC
                    // circuit.Set_MNA_A(AnalysisType.AC_SWEEP, baseFreq);
                    // circuit.Set_MNA_RHS(AnalysisType.AC_SWEEP, baseFreq);

                    // Placeholder for matrix solving
                    // This would require implementing the matrix operations in C#

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