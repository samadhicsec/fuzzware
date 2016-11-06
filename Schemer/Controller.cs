using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using Fuzzware.Common;
using Fuzzware.Schemer.Statistics;
using Fuzzware.Evaluate.Statistics;

namespace Fuzzware.Schemer
{
    /// <summary>
    /// This class is used to capture and respond to keyboard commands that allow the fuzzer to be paused, resumed, skipped or stopped.
    /// It is run in a seperate thread.
    /// </summary>
    class Controller
    {
        EngineState oState;
        bool Paused = false;
        int ThreadId;

        public void PrintControllerCommands()
        {
            StringBuilder ControllerCommands = new StringBuilder();
            ControllerCommands.AppendLine("Fuzzing Commands:");
            ControllerCommands.AppendLine("\t\tCTRL+P - Pause fuzzing");
            ControllerCommands.AppendLine("\t\tCTRL+R - Resume fuzzing");
            ControllerCommands.AppendLine("\t\tCTRL+K - Skip fuzzing current node");
            ControllerCommands.AppendLine("\t\tCTRL+T - Prints the current state");
            ControllerCommands.Append("\t\tCTRL+C - Cancel fuzzing");
            Log.Write(MethodBase.GetCurrentMethod(), ControllerCommands.ToString(), Log.LogType.Info);
        }

        public void KeyboardHandler()
        {
            Console.TreatControlCAsInput = true;
            WatchForCommands();
        }

        public Controller(EngineState oState)
        {
            this.oState = oState;
        }

        void ShowOutputStats()
        {
            if (Paused)
            {
                FuzzerStats.LogFuzzerStats();
                OutputStats.LogOutputStats();
            }
        }

        /// <summary>
        /// Tries to pause control flow.  If this fails initially, it tells the user that it is waiting for control.
        /// </summary>
        private void GetStateControlMutex()
        {
            if (!oState.ControlMutex.WaitOne(500))
            {
                // Let the user know if they are going to be waiting a while
                Log.Write(MethodBase.GetCurrentMethod(), "Command is waiting for current testcase to finish evaluating...", Log.LogType.Status);
                oState.ControlMutex.WaitOne();
            }
        }

        /// <summary>
        /// Releases the control flow back to the fuzzer.
        /// </summary>
        private void ReleaseStateControlMutex()
        {
            oState.ControlMutex.ReleaseMutex();
        }

        /// <summary>
        /// Pause can be called from different threads, but only the original thread that paused it can Resume
        /// </summary>
        public void Pause()
        {
            if (Paused)
                return;

            // Since Pause is a public function it can be called from a different thread than the Controller thread.
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            // Get control of the State.ControlMutex.  While Controller has the Mutex the Dispatcher cannot continue fuzzing
            GetStateControlMutex();
            StringBuilder PauseCommands = new StringBuilder();
            PauseCommands.AppendLine("Fuzzing paused at state - " + oState.ToString() + " (CTRL+R to Resume)");
            PauseCommands.AppendLine("\t\tCTRL+A - Show current statistics");

            Log.Write(MethodBase.GetCurrentMethod(), PauseCommands.ToString(), Log.LogType.Info);
            Paused = true;
        }

        /// <summary>
        /// Resume can be called from different threads, but only the thread that called Pause can resume.
        /// </summary>
        public void Resume()
        {
            if (!Paused)
                return;
            // We cannot release the Mutex from a different thread than the one that acquired it.
            if (Thread.CurrentThread.ManagedThreadId != ThreadId)
                return;
            Log.Write(MethodBase.GetCurrentMethod(), "Resuming fuzzing", Log.LogType.Info);
            ReleaseStateControlMutex();
            Paused = false;
        }

        private void SkipNode()
        {
            GetStateControlMutex();
            Log.Write(MethodBase.GetCurrentMethod(), "Skipping current node.", Log.LogType.Info);
            while ((null != oState.CurrentFuzzerState) && 
                (!oState.CurrentFuzzerState.IsFinished))
            {
                oState.CurrentFuzzerState.Next();
                while (oState.CurrentFuzzerState.NextFuzzType()) ;
            }
            ReleaseStateControlMutex();
        }

        private void PrintState()
        {
            if (Paused)
                return;
            GetStateControlMutex();
            Log.Write(MethodBase.GetCurrentMethod(), "Current state is - " + oState.ToString(), Log.LogType.Info);
            ReleaseStateControlMutex();
        }

        private void Stop()
        {
            if (Paused)
                ReleaseStateControlMutex();
            GetStateControlMutex();
            Log.Write(MethodBase.GetCurrentMethod(), "Fuzzing ended by user at state - " + oState.ToString(), Log.LogType.Info);
            // Finish this fuzzer
            while ((null != oState.CurrentFuzzerState) && 
                (!oState.CurrentFuzzerState.IsFinished))
            {
                oState.CurrentFuzzerState.Next();
                while (oState.CurrentFuzzerState.NextFuzzType()) ;
            }
            // Move through all the schema elements
            while (oState.NextState()) ;
            ReleaseStateControlMutex();
        }

        private void WatchForCommands()
        {
            // Infinite loop, but the Console.ReadKey function will block the thread until a key is pressed
            while (true)
            {
                try
                {
                    // Wait for input from the command console
                    while (!Console.KeyAvailable)
                        Thread.Sleep(200);
                    ConsoleKeyInfo KeyInfo = Console.ReadKey(true);

                    // If we aren't currently fuzzing there is nothing to control.
                    if (!oState.IsFuzzing)
                        continue;

                    if ((KeyInfo.Modifiers & ConsoleModifiers.Control) == 0)
                        continue;

                    switch (KeyInfo.Key)
                    {
                        case ConsoleKey.A:
                            ShowOutputStats();
                            break;
                        case ConsoleKey.P:   // Pause
                            Pause();
                            break;
                        case ConsoleKey.R:   // Resume
                            Resume();
                            break;
                        case ConsoleKey.K:   // Skip current node
                            SkipNode();
                            break;
                        case ConsoleKey.T:   // Print current state
                            PrintState();
                            break;
                        case ConsoleKey.C:   // Stop fuzzing
                            Stop();
                            // Kill this thread
                            Thread.CurrentThread.Abort();
                            break;
                        default:    // Ignore key

                            break;
                    }
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                        Log.Write(MethodBase.GetCurrentMethod(), "The thread controlling fuzzing commands has encountered an error, the commands will be unavailable" +
                            Environment.NewLine + e.Message, Log.LogType.Warning);
                    else
                        return;
                }
            }
        }
    }
}
