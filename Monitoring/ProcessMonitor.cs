using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace Fuzzware.Monitoring
{
    public class Win32_Process
    {
        public string Caption { get { return (string)mbo.Properties["Caption"].Value; } }
        public string CommandLine { get { return (string)mbo.Properties["CommandLine"].Value; } }
        public string CreationClassName { get { return (string)mbo.Properties["CreationClassName"].Value; } }
        public string CreationDate { get { return (string)mbo.Properties["CreationDate"].Value; } }
        public string CSCreationClassName { get { return (string)mbo.Properties["CSCreationClassName"].Value; } }
        public string CSName { get { return (string)mbo.Properties["CSName"].Value; } }
        public string Description { get { return (string)mbo.Properties["Description"].Value; } }
        public string ExecutablePath { get { return (string)mbo.Properties["ExecutablePath"].Value; } }
        //public UInt16 ExecutionState { get { return (UInt16)mbo.Properties["ExecutionState"].Value; } }
        public string Handle { get { return (string)mbo.Properties["Handle"].Value; } }
        public UInt32 HandleCount { get { return (UInt32)mbo.Properties["HandleCount"].Value; } }
        public string InstallDate { get { return (string)mbo.Properties["InstallDate"].Value; } }
        public UInt64 KernelModeTime { get { return (UInt64)mbo.Properties["KernelModeTime"].Value; } }
        public UInt32 MaximumWorkingSetSize { get { return (UInt32)mbo.Properties["MaximumWorkingSetSize"].Value; } }
        public UInt32 MinimumWorkingSetSize { get { return (UInt32)mbo.Properties["MinimumWorkingSetSize"].Value; } }
        public string Name { get { return (string)mbo.Properties["Name"].Value; } }
        public string OSCreationClassName { get { return (string)mbo.Properties["OSCreationClassName"].Value; } }
        public string OSName { get { return (string)mbo.Properties["OSName"].Value; } }
        public UInt64 OtherOperationCount { get { return (UInt64)mbo.Properties["OtherOperationCount"].Value; } }
        public UInt64 OtherTransferCount { get { return (UInt64)mbo.Properties["OtherTransferCount"].Value; } }
        public UInt32 PageFaults { get { return (UInt32)mbo.Properties["PageFaults"].Value; } }
        public UInt32 PageFileUsage { get { return (UInt32)mbo.Properties["PageFileUsage"].Value; } }
        public UInt32 ParentProcessId { get { return (UInt32)mbo.Properties["ParentProcessId"].Value; } }
        public UInt32 PeakPageFileUsage { get { return (UInt32)mbo.Properties["PeakPageFileUsage"].Value; } }
        public UInt64 PeakVirtualSize { get { return (UInt64)mbo.Properties["PeakVirtualSize"].Value; } }
        public UInt32 PeakWorkingSetSize { get { return (UInt32)mbo.Properties["PeakWorkingSetSize"].Value; } }
        public UInt32 Priority { get { return (UInt32)mbo.Properties["Priority"].Value; } }
        public UInt64 PrivatePageCount { get { return (UInt64)mbo.Properties["PrivatePageCount"].Value; } }
        public UInt32 ProcessId { get { return (UInt32)mbo.Properties["ProcessId"].Value; } }
        public UInt32 QuotaNonPagedPoolUsage { get { return (UInt32)mbo.Properties["QuotaNonPagedPoolUsage"].Value; } }
        public UInt32 QuotaPagedPoolUsage { get { return (UInt32)mbo.Properties["QuotaPagedPoolUsage"].Value; } }
        public UInt32 QuotaPeakNonPagedPoolUsage { get { return (UInt32)mbo.Properties["QuotaPeakNonPagedPoolUsage"].Value; } }
        public UInt32 QuotaPeakPagedPoolUsage { get { return (UInt32)mbo.Properties["QuotaPeakPagedPoolUsage"].Value; } }
        public UInt64 ReadOperationCount { get { return (UInt64)mbo.Properties["ReadOperationCount"].Value; } }
        public UInt64 ReadTransferCount { get { return (UInt64)mbo.Properties["ReadTransferCount"].Value; } }
        public UInt32 SessionId { get { return (UInt32)mbo.Properties["SessionId"].Value; } }
        public string Status { get { return (string)mbo.Properties["Status"].Value; } }
        public string TerminationDate { get { return (string)mbo.Properties["TerminationDate"].Value; } }
        public UInt32 ThreadCount { get { return (UInt32)mbo.Properties["ThreadCount"].Value; } }
        public UInt64 UserModeTime { get { return (UInt64)mbo.Properties["UserModeTime"].Value; } }
        public UInt64 VirtualSize { get { return (UInt64)mbo.Properties["VirtualSize"].Value; } }
        public string WindowsVersion { get { return (string)mbo.Properties["WindowsVersion"].Value; } }
        public UInt64 WorkingSetSize { get { return (UInt64)mbo.Properties["WorkingSetSize"].Value; } }
        public UInt64 WriteOperationCount { get { return (UInt64)mbo.Properties["WriteOperationCount"].Value; } }
        public UInt64 WriteTransferCount { get { return (UInt64)mbo.Properties["WriteTransferCount"].Value; } }

        ManagementBaseObject mbo;
        DateTime EventTime;
        public Win32_Process(ManagementBaseObject mbo)
        {
            this.mbo = mbo;
            EventTime = DateTime.Now;
        }

        public DateTime Time
        {
            get { return EventTime; }
        }
    };

    /// <summary>
    /// Monitors for the a process terminating.
    /// </summary>
    public class ProcessMonitor : GenericMonitor
    {
        public delegate void ProcessEventCallback(Win32_Process TerminatedProcess, object UserDefined, ProcessMonitor Monitor);

        ProcessEventCallback PECallback;
        object UserDefined;
        String WhereClause = "";

        public ProcessMonitor(String ProcessName, ProcessEventCallback ProcessCallback)
            : base(null)
        {
            base.CallbackFn = GenCallback;
            PECallback = ProcessCallback;
            WhereClause = "TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = '" + ProcessName + "'";
        }

        public ProcessMonitor(UInt32 ProcessID, ProcessEventCallback ProcessCallback)
            : base(null)
        {
            base.CallbackFn = GenCallback;
            PECallback = ProcessCallback;
            WhereClause = "TargetInstance ISA 'Win32_Process' AND TargetInstance.ProcessId = '" + ProcessID + "'";
        }

        public object CallbackUserObject
        {
            set { UserDefined = value; }
        }

        private void GenCallback(object obj)
        {
            // Get the process object
            ManagementBaseObject mo = (ManagementBaseObject)obj;
            PropertyData pd;
            if ((pd = mo.Properties["TargetInstance"]) == null)
                return;

            ManagementBaseObject mbo = pd.Value as ManagementBaseObject;
            
            Win32_Process Proc = new Win32_Process(mbo);
            PECallback(Proc, UserDefined, this);
        }

        public override void Start()
        {
            base.AsyncQuery(new EventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE " + WhereClause));
            base.Start();
        }

        public override void AsyncQuery(EventQuery Query)
        {
            return;
        }

        public override ManagementObjectCollection SyncQuery(ObjectQuery Query)
        {
            return null;
        }
    }
}
