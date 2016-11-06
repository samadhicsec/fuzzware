﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Fuzzware.Common;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Evaluate.FuzzwareDBG;

namespace Fuzzware.Evaluate
{
    public class DebugProcessProxy : ProcessProxy
    {
        protected IFuzzwareDBG oIFuzzwareDBG;
        protected IntPtr hMainWndHandle;

        /// <summary>
        /// Delegate for the EnumChildWindows method
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="parameter">Caller-defined variable; we use it for a pointer to our list</param>
        /// <returns>True to continue enumerating, false to bail.</returns>
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr parameter);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowLong(HandleRef hWnd, int nIndex);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);


        public DebugProcessProxy(OutputToExe oOutputToExe) : base(oOutputToExe)
        {
            // Register the COM object
            RegisterCOMObjects();

            if (oOutputToExe.ExePathAndName.UseDebugger)
            {
                // CoCreate the COM object
                FuzzwareDBG.FuzzwareDBG oFuzzwareDBG = new FuzzwareDBG.FuzzwareDBG();
                oIFuzzwareDBG = (IFuzzwareDBG)oFuzzwareDBG;
            }
        }

        protected void RegisterCOMObjects()
        {
            // Register the stub, this populates a registry entry from the COM Interface
            RegisterFuzzwareDBGStub();

            // Register the FuzzwareDBG as a local-server COM object, this populates a registry entry with the CLSID 
            RegisterFuzzwareDBG();
        }

        protected void RegisterFuzzwareDBGStub()
        {
#if DEBUG
            String FuzzwareDBGStubPath = @"..\..\..\FuzzwareDBG\Debug\FuzzwareDBGStub.dll";
#else
            String FuzzwareDBGStubPath = @"FuzzwareDBGStub.dll";
#endif
            FuzzwareDBGStubPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), FuzzwareDBGStubPath));

            if (!File.Exists(FuzzwareDBGStubPath))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find '" + FuzzwareDBGStubPath + "'.", Log.LogType.Warning);
                return;
            }

            Process oRegSvr32 = new Process();
            oRegSvr32.StartInfo.FileName = @"RegSvr32.exe";
            oRegSvr32.StartInfo.Arguments = "/s \"" + FuzzwareDBGStubPath + "\"";
            oRegSvr32.StartInfo.UseShellExecute = true;
            oRegSvr32.Start();

            while (!oRegSvr32.HasExited)
                Thread.Sleep(50);
        }

        protected void RegisterFuzzwareDBG()
        {
#if DEBUG
            String FuzzwareDBGPath = @"..\..\..\FuzzwareDBG\Debug\FuzzwareDBG.exe";
#else
            String FuzzwareDBGPath = @"FuzzwareDBG.exe";
#endif
            FuzzwareDBGPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), FuzzwareDBGPath));

            if (!File.Exists(FuzzwareDBGPath))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find '" + FuzzwareDBGPath + "'.", Log.LogType.Warning);
                return;
            }

            Process oFuzzwareDBGExe = new Process();
            oFuzzwareDBGExe.StartInfo.FileName = "\"" + FuzzwareDBGPath + "\"";
            oFuzzwareDBGExe.StartInfo.Arguments = @"/RegServer";
            oFuzzwareDBGExe.StartInfo.UseShellExecute = true;
            oFuzzwareDBGExe.Start();

            while (!oFuzzwareDBGExe.HasExited)
                Thread.Sleep(50);
        }

        protected void UnregisterFuzzwareDBG()
        {
#if DEBUG
            String FuzzwareDBGPath = @"FuzzwareDBG\Debug\FuzzwareDBG.exe";
#else
            String FuzzwareDBGPath = @"FuzzwareDBG.exe";
#endif
            if (!File.Exists(FuzzwareDBGPath))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find '" + FuzzwareDBGPath + "' in '" + Environment.CurrentDirectory + "'.", Log.LogType.Warning);
                return;
            }

            Process oFuzzwareDBGExe = new Process();
            oFuzzwareDBGExe.StartInfo.FileName = FuzzwareDBGPath;
            oFuzzwareDBGExe.StartInfo.Arguments = @"/UnregServer";
            oFuzzwareDBGExe.StartInfo.UseShellExecute = true;
            oFuzzwareDBGExe.Start();

            while (!oFuzzwareDBGExe.HasExited)
                Thread.Sleep(50);
        }

        protected override void ProcessStart(String CommandLine, String StateDesc)
        {
            // Use FuzzwareDBG
            string ProcessStr = oOutputToExe.ExePathAndName.Value + " " + CommandLine;

            oIFuzzwareDBG.SetOutputDir(Path.GetFullPath(oOutputToExe.UniqueOutputs.Directory));
            oIFuzzwareDBG.SetCrashComment(StateDesc);
            oIFuzzwareDBG.CreateProcess(ProcessStr);

            PID = oIFuzzwareDBG.RunProcess();
        }

        private bool IsMainWindow(IntPtr handle)
        {
            // 4 = GW_OWNER
            return (!(GetWindow(new HandleRef(this, handle), 4) != IntPtr.Zero) && IsWindowVisible(new HandleRef(this, handle)));
        }

        /// <summary>
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private bool EnumWindowsProcedure(IntPtr hWnd, IntPtr parameter)
        {
            int dwProcessId = 0;
            GetWindowThreadProcessId(new HandleRef(this, hWnd), out dwProcessId);

            if ((0 != PID) && ((int)PID == dwProcessId))
            {
                // We found a process ID match, so this is a candidate.  We may end up using this even 
                // if it is not a main window, as sometimes open windows need actions sent to them
                hMainWndHandle = hWnd;
                if (IsMainWindow(hWnd))
                {
                    // We found a main window, so stop enumeration
                    return false;
                }
            }

            // Continue enumeration
            //hMainWndHandle = IntPtr.Zero;
            return true;
        }

        protected override IntPtr GetMainWindowHandle()
        {
            if (IntPtr.Zero != hMainWndHandle)
            {
                // Try to determine if the handle we have is still valid
                if (IsWindow(hMainWndHandle) && IsMainWindow(hMainWndHandle))
                    return hMainWndHandle;

                // For now, if the process hasn't exited, then return the handle we already got.
                //if (!HasExited())
                //    return hMainWndHandle;
            }
            hMainWndHandle = IntPtr.Zero;

            // Search for top level window with a thread ID the same as the process ID
            EnumWindows(EnumWindowsProcedure, IntPtr.Zero);

            if (IntPtr.Zero != hMainWndHandle)
                return hMainWndHandle;

            Log.Write(MethodBase.GetCurrentMethod(), "Unable to get main window handle.", Log.LogType.Warning);
            return IntPtr.Zero;
        }

        protected override void BeginReadOutput()
        {
            // We ignore the output when we run in a debugger
        }

        protected override String GetProcessOutput()
        {
            return "";
        }

        protected override void WaitForExit(int Delay)
        {
            int iSleepTime = 100;
            // Poll for process exit for the duration of the delay
            while (!HasExited() && (Delay > 0))
            {
                if (Delay <= iSleepTime)
                    iSleepTime = Delay;

                Thread.Sleep(iSleepTime);

                Delay -= iSleepTime;
            }
        }

        protected override bool HasExited()
        {
            return (0 == oIFuzzwareDBG.HasProcessExited())?false:true;
        }

        protected override void ExitApp()
        {
            // Stolen from System.Diagnostics.Process.CloseMainWindow
            IntPtr mainWindowHandle = GetMainWindowHandle();
            if (mainWindowHandle == IntPtr.Zero)
            {
                return;
            }
            // -16 = GWL_STYLE
            if ((GetWindowLong(new HandleRef(this, mainWindowHandle), -16) & 0x8000000) != 0)
            {
                return;
            }
            // 0x10 = WM_CLOSE
            PostMessage(new HandleRef(this, mainWindowHandle), 0x10, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void KillApp()
        {
            oIFuzzwareDBG.KillProcess();
        }

        protected override void Cleanup()
        {
            
        }
    }
}
