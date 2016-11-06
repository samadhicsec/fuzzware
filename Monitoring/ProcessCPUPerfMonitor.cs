using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Threading;
using Fuzzware.Common;

namespace Fuzzware.Monitoring
{
    public class ProcessCPUPerfMonitor : GenericPerfMonitor
    {
        public delegate void ProcPerfCallback(PerfProc_Process Previous, PerfProc_Process Current);
        protected ProcPerfCallback CPUCallbackFn;

        String WhereClause;

        Dictionary<UInt32, PerfProc_Process> oPrevious;
        Dictionary<UInt32, PerfProc_Process> oCurrent;

        uint PollingIntervalSecs = UInt32.MaxValue;

        UInt32 ProcThresholdPer = 100;
        UInt32 LogicalProcThresholdPer;

        public uint ProcessorThreshold
        {
            get { return LogicalProcThresholdPer; }
        }

        private ProcessCPUPerfMonitor(uint PollingIntervalSecs, uint CPUThresholdPercentage, ProcPerfCallback CPUCallbackFn)
            : base(null, PollingIntervalSecs)
        {
            base.CallbackFn = this.ProcCPUMonitorWorker;
            this.CPUCallbackFn = CPUCallbackFn;
            this.PollingIntervalSecs = PollingIntervalSecs;
            ProcThresholdPer = CPUThresholdPercentage;
            oPrevious = new Dictionary<uint, PerfProc_Process>();
            oCurrent = new Dictionary<uint, PerfProc_Process>();
        }

        public ProcessCPUPerfMonitor(String ProcessName, uint PollingIntervalSecs, uint CPUThresholdPercentage, ProcPerfCallback CPUCallbackFn)
            : this(PollingIntervalSecs, CPUThresholdPercentage, CPUCallbackFn)
        {
            if (ProcessName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
                ProcessName = ProcessName.Remove(ProcessName.Length - 4, 4);
            WhereClause = "Name = '" + ProcessName + "'";
        }

        public ProcessCPUPerfMonitor(UInt32 ProcessID, uint PollingIntervalSecs, uint CPUThresholdPercentage, ProcPerfCallback CPUCallbackFn)
            : this(PollingIntervalSecs, CPUThresholdPercentage, CPUCallbackFn)
        {
            WhereClause = "IDProcess = '" + ProcessID + "'";
        }

        public override void Start()
        {
            // // Use Win32_OperatingSystem to determine OS
            bool IsVista = false;
            ManagementObjectCollection moc = RunQuery(new WqlObjectQuery("select * from Win32_OperatingSystem"));
            if ((null == moc) || (moc.Count != 1))
                Log.Write("ProcessPerfMonitor.Start", "Query for WMI Win32_ComputerSystem didn't return 1 object", Log.LogType.UnexpectedError);
            foreach (ManagementBaseObject mbo in moc)
            {
                int MajorVersion = Int32.Parse(((string)mbo.Properties["Version"].Value).Substring(0, 1));
                if (MajorVersion >= 6)
                    IsVista = true;
            }

            // Get the number of processes of the target computer
            UInt32 NumberOfProcessors = 1;
            moc = RunQuery(new WqlObjectQuery("select * from Win32_ComputerSystem"));
            if ((null == moc) || (moc.Count != 1))
                Log.Write("ProcessPerfMonitor.Start", "Query for WMI Win32_ComputerSystem didn't return 1 object", Log.LogType.UnexpectedError);
            foreach (ManagementBaseObject mbo in moc)
            {
                if(IsVista)
                    // For Vista use NumberOfLogicalProcessors to account for hyper-threading or multicore CPU's
                    NumberOfProcessors = (UInt32)mbo.Properties["NumberOfLogicalProcessors"].Value;
                else
                    // For non-Vista use NumberOfProcessors, but this ignores hyper-threading or multicore CPU's (limitation of non-Vista)
                    NumberOfProcessors = (UInt32)mbo.Properties["NumberOfProcessors"].Value;
            }
            // Calculate the actual percentage to check for
            LogicalProcThresholdPer = (UInt32)((100.0 / (Double)NumberOfProcessors) * ((Double)ProcThresholdPer / 100.0));

            base.Query = new ObjectQuery("select * from Win32_PerfFormattedData_PerfProc_Process WHERE " + WhereClause);
            base.Start();
        }

        private void ProcCPUMonitorWorker(ManagementObjectCollection moc)
        {
            //if((null == moc) || (moc.Count != 1))
            //    Log.Write("ProcessCPUPerfMonitor.ProcCPUMonitorWorker", "A single performace process object was not returned", Log.LogType.UnexpectedError);

            bool FirstTime = (oPrevious.Count == 0);

            oCurrent = new Dictionary<uint, PerfProc_Process>();
            foreach(ManagementBaseObject mbo in moc)
            {
                PerfProc_Process PPProcess = new PerfProc_Process(mbo);
                if (FirstTime)
                    oPrevious.Add(PPProcess.IDProcess, PPProcess);
                else
                    oCurrent.Add(PPProcess.IDProcess, PPProcess);
            }
            if (FirstTime)
                return;

            foreach(UInt32 Key in oPrevious.Keys)
            {
                PerfProc_Process oCurPPProcess;
                // Check for conditions
                if(oCurrent.TryGetValue(Key, out oCurPPProcess))
                    if (CheckCPUUsage(oPrevious[Key], oCurPPProcess))
                        CPUCallbackFn(oPrevious[Key], oCurPPProcess);
            }

            oPrevious = oCurrent;
        }

        private bool CheckCPUUsage(PerfProc_Process Previous, PerfProc_Process Current)
        {
            if ((Previous.PercentProcessorTime > LogicalProcThresholdPer) &&
                (Current.PercentProcessorTime > LogicalProcThresholdPer))
                return true;
            return false;
        }
    }
}
