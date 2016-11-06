using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Management;
using System.Threading;
using Fuzzware.Common;

namespace Fuzzware.Monitoring
{
    public class PerfProc_Process
    {
        public readonly string Caption;
        public UInt32 CreatingProcessID;
        public string Description;
        public UInt64 ElapsedTime;
        //public UInt64 Frequency_Object;
        //public UInt64 Frequency_PerfTime;
        //public UInt64 Frequency_Sys100NS;
        public UInt32 HandleCount;
        public UInt32 IDProcess;
        //public UInt64 IODataBytesPerSec;
        //public UInt64 IODataOperationsPerSec;
        //public UInt64 IOOtherBytesPerSec;
        //public UInt64 IOOtherOperationsPerSec;
        //public UInt64 IOReadBytesPerSec;
        //public UInt64 IOReadOperationsPerSec;
        //public UInt64 IOWriteBytesPerSec;
        //public UInt64 IOWriteOperationsPerSec;
        public string Name;
        public UInt32 PageFaultsPerSec;
        public UInt64 PageFileBytes;
        public UInt64 PageFileBytesPeak;
        public UInt64 PercentPrivilegedTime;
        public UInt64 PercentProcessorTime;
        public UInt64 PercentUserTime;
        public UInt32 PoolNonpagedBytes;
        public UInt32 PoolPagedBytes;
        public UInt32 PriorityBase;
        public UInt64 PrivateBytes;
        public UInt32 ThreadCount;
        //public UInt64 Timestamp_Object;
        //public UInt64 Timestamp_PerfTime;
        //public UInt64 Timestamp_Sys100NS;
        public UInt64 VirtualBytes;
        public UInt64 VirtualBytesPeak;
        public UInt64 WorkingSet;
        public UInt64 WorkingSetPeak;

        ManagementBaseObject mbo;
        DateTime oTime;
        public PerfProc_Process(ManagementBaseObject mbo)
        {
            this.mbo = mbo;
            oTime = DateTime.Now;
            Caption = (string)mbo.Properties["Caption"].Value;
            CreatingProcessID = (UInt32)mbo.Properties["CreatingProcessID"].Value;
            Description = (string)mbo.Properties["Description"].Value;
            ElapsedTime = (UInt64)mbo.Properties["ElapsedTime"].Value;
            //Frequency_Object = (UInt64)mbo.Properties["Frequency_Object"].Value;
            //Frequency_PerfTime = (UInt64)mbo.Properties["Frequency_PerfTime"].Value;
            //Frequency_Sys100NS = (UInt64)mbo.Properties["Frequency_Sys100NS"].Value;
            HandleCount = (UInt32)mbo.Properties["HandleCount"].Value;
            IDProcess = (UInt32)mbo.Properties["IDProcess"].Value;
            //IODataBytesPerSec = (UInt64)mbo.Properties["IODataBytesPerSec"].Value;
            //IODataOperationsPerSec = (UInt64)mbo.Properties["IODataOperationsPerSec"].Value;
            //IOOtherBytesPerSec = (UInt64)mbo.Properties["IOOtherBytesPerSec"].Value;
            //IOOtherOperationsPerSec = (UInt64)mbo.Properties["IOOtherOperationsPerSec"].Value;
            //IOReadBytesPerSec = (UInt64)mbo.Properties["IOReadBytesPerSec"].Value;
            //IOReadOperationsPerSec = (UInt64)mbo.Properties["IOReadOperationsPerSec"].Value;
            //IOWriteBytesPerSec = (UInt64)mbo.Properties["IOWriteBytesPerSec"].Value;
            //IOWriteOperationsPerSec = (UInt64)mbo.Properties["IOWriteOperationsPerSec"].Value;
            Name = (string)mbo.Properties["Name"].Value;
            PageFaultsPerSec = (UInt32)mbo.Properties["PageFaultsPerSec"].Value;
            PageFileBytes = (UInt64)mbo.Properties["PageFileBytes"].Value;
            PageFileBytesPeak = (UInt64)mbo.Properties["PageFileBytesPeak"].Value;
            PercentPrivilegedTime = (UInt64)mbo.Properties["PercentPrivilegedTime"].Value;
            PercentProcessorTime = (UInt64)mbo.Properties["PercentProcessorTime"].Value;
            PercentUserTime = (UInt64)mbo.Properties["PercentUserTime"].Value;
            PoolNonpagedBytes = (UInt32)mbo.Properties["PoolNonpagedBytes"].Value;
            PoolPagedBytes = (UInt32)mbo.Properties["PoolPagedBytes"].Value;
            PriorityBase = (UInt32)mbo.Properties["PriorityBase"].Value;
            PrivateBytes = (UInt64)mbo.Properties["PrivateBytes"].Value;
            ThreadCount = (UInt32)mbo.Properties["ThreadCount"].Value;
            //Timestamp_Object = (UInt64)mbo.Properties["Timestamp_Object"].Value;
            //Timestamp_PerfTime = (UInt64)mbo.Properties["Timestamp_PerfTime"].Value;
            //Timestamp_Sys100NS = (UInt64)mbo.Properties["Timestamp_Sys100NS"].Value;
            VirtualBytes = (UInt64)mbo.Properties["VirtualBytes"].Value;
            VirtualBytesPeak = (UInt64)mbo.Properties["VirtualBytesPeak"].Value;
            WorkingSet = (UInt64)mbo.Properties["WorkingSet"].Value;
            WorkingSetPeak = (UInt64)mbo.Properties["WorkingSetPeak"].Value;
        }

        public UInt64 Properties(string Property)
        {
            UInt64 Value = 0;
            if(mbo.Properties[Property].Type == CimType.UInt32)
                Value = (UInt64)((UInt32)mbo.Properties[Property].Value);
            else
                Value = (UInt64)mbo.Properties[Property].Value;
            return Value;
        }

        public DateTime Time
        {
            get { return oTime; }
        }
    }

    /// <summary>
    /// For performance reasons this is basically 3 classes in one, as each of the types of process checking; max, previous multiply and
    /// base multiply require information about the same process, if they were split up then it would be 3 queries which introduces a
    /// lot of overhead (probably).
    /// </summary>

    public class ProcessPerfMonitor : GenericPerfMonitor
    {
        public delegate void ProcessPerfCallback(String Property, UInt64 Value, PerfProc_Process Current, PerfProc_Process Previous, object UserDefined, ProcessPerfMonitor Monitor);
        // The callback for max value property checks
        protected ProcessPerfCallback ProcessMaxPerfCallbackFn;
        // The callback for base multiplier property checks
        protected ProcessPerfCallback ProcessBaseMulPerfCallbackFn;
        // The callback for previous poll multiplier property checks
        protected ProcessPerfCallback ProcessPrevMulPerfCallbackFn;

        object UserDefined;
        String WhereClause;

        // Stores the property name and the max value that will be checked for
        Dictionary<String, UInt64> MaxPropertyValueDict;
        // Stores the property name the previous value multiplier that will be checked for
        Dictionary<String, UInt16> PrevMultiplierPropertyValueDict;
        // Stores the property name the base value multiplier that will be checked for
        Dictionary<String, UInt16> BaseMultiplierPropertyValueDict;

        // Stores the previous process/es property values
        Dictionary<UInt32, PerfProc_Process> oPrevious;
        // Holds the current process/es property values
        Dictionary<UInt32, PerfProc_Process> oCurrent;
        // Holds the base case process/es property values
        Dictionary<UInt32, PerfProc_Process> oBase;

        uint PollingIntervalSecs = UInt32.MaxValue;

        //UInt32 ProcThresholdPer = 100;
        //UInt32 LogicalProcThresholdPer;

        #region Contructors
        private ProcessPerfMonitor(uint PollingIntervalSecs)
            : base(null, PollingIntervalSecs)
        {
            base.CallbackFn = this.ProcMonitor;
            this.PollingIntervalSecs = PollingIntervalSecs;
            
            // Initialise dictionaries
            MaxPropertyValueDict = new Dictionary<string, ulong>();
            PrevMultiplierPropertyValueDict = new Dictionary<string, ushort>();
            BaseMultiplierPropertyValueDict = new Dictionary<string, ushort>();
            
            oBase = new Dictionary<uint, PerfProc_Process>();
            oPrevious = new Dictionary<uint, PerfProc_Process>();
            oCurrent = new Dictionary<uint, PerfProc_Process>();
        }

        public ProcessPerfMonitor(String ProcessName, uint PollingIntervalSecs)
            : this(PollingIntervalSecs)
        {
            if (ProcessName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
                ProcessName = ProcessName.Remove(ProcessName.Length - 4, 4);
            WhereClause = "Name = '" + ProcessName + "'";
        }

        public ProcessPerfMonitor(UInt32 ProcessID, uint PollingIntervalSecs)
            : this(PollingIntervalSecs)
        {
            WhereClause = "IDProcess = '" + ProcessID + "'";
        }
        #endregion

        #region Properties

        public object CallbackUserObject
        {
            set { UserDefined = value; }
        }
        #endregion

        #region Process Property Adders
        public void AddMaxPropertyCheck(String Property, UInt64 MaxValue)
        {
            MaxPropertyValueDict.Add(Property, MaxValue);
        }

        public void AddPrevMultplierPropertyCheck(String Property, UInt16 MaxValue)
        {
            PrevMultiplierPropertyValueDict.Add(Property, MaxValue);
        }

        public void AddBaseMultplierPropertyCheck(String Property, UInt16 MaxValue)
        {
            BaseMultiplierPropertyValueDict.Add(Property, MaxValue);
        }
        #endregion

        #region Set Callbacks
        public void SetMaxCallback(ProcessPerfCallback Callback)
        {
            ProcessMaxPerfCallbackFn = Callback;
        }

        public void SetPrevMultiplierCallback(ProcessPerfCallback Callback)
        {
            ProcessPrevMulPerfCallbackFn = Callback;
        }

        public void SetBaseMultiplierCallback(ProcessPerfCallback Callback)
        {
            ProcessBaseMulPerfCallbackFn = Callback;
        }
        #endregion

        public UInt32 GetNumberOfProcessors()
        {
            // Use Win32_OperatingSystem to determine OS
            bool IsVista = false;
            ManagementObjectCollection moc = RunQuery(new WqlObjectQuery("select * from Win32_OperatingSystem"));
            if ((null == moc) || (moc.Count != 1))
                Log.Write(MethodBase.GetCurrentMethod(), "Query for WMI Win32_ComputerSystem didn't return 1 object", Log.LogType.Error);
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
                Log.Write(MethodBase.GetCurrentMethod(), "Query for WMI Win32_ComputerSystem didn't return 1 object", Log.LogType.Error);
            foreach (ManagementBaseObject mbo in moc)
            {
                if (IsVista)
                    // For Vista use NumberOfLogicalProcessors to account for hyper-threading or multicore CPU's
                    NumberOfProcessors = (UInt32)mbo.Properties["NumberOfLogicalProcessors"].Value;
                else
                    // For non-Vista use NumberOfProcessors, but this ignores hyper-threading or multicore CPU's (limitation of non-Vista)
                    NumberOfProcessors = (UInt32)mbo.Properties["NumberOfProcessors"].Value;
            }
            return NumberOfProcessors;
        }
        
        public override void Start()
        {
            // Set the query
            base.Query = new ObjectQuery("select * from Win32_PerfFormattedData_PerfProc_Process WHERE " + WhereClause);
            
            // Set the base query
            ManagementObjectCollection moc = base.RunQuery(oQuery);
            AssignProcessData(moc, ref oBase);

            // Start monitoring
            base.Start();
        }

        private void AssignProcessData(ManagementObjectCollection moc, ref Dictionary<uint, PerfProc_Process> DataDict)
        {
            DataDict = new Dictionary<uint, PerfProc_Process>();
            foreach (ManagementBaseObject mbo in moc)
            {
                PerfProc_Process PPProcess = new PerfProc_Process(mbo);
                DataDict.Add(PPProcess.IDProcess, PPProcess);
            }
        }

        /// <summary>
        /// Local callback function.  Checks whether the process/es meet the values set
        /// </summary>
        /// <param name="moc"></param>
        private void ProcMonitor(ManagementObjectCollection moc)
        {
            if (oPrevious.Count == 0)
            {
                // We have no previous data to compare, so assign some and leave
                AssignProcessData(moc, ref oPrevious);
                return;
            }
            else
                AssignProcessData(moc, ref oCurrent);

            // Check previous poll value each metric that was added
            foreach (UInt32 ProcessId in oPrevious.Keys)
            {
                if (!oCurrent.ContainsKey(ProcessId))
                    continue;

                // Check each max value
                if(null != ProcessMaxPerfCallbackFn)
                    foreach (String Property in MaxPropertyValueDict.Keys)
                    {
                        if (CheckMaxPropertyValue(oPrevious[ProcessId], oCurrent[ProcessId], Property, MaxPropertyValueDict[Property]))
                            ProcessMaxPerfCallbackFn(Property, MaxPropertyValueDict[Property], oCurrent[ProcessId], oPrevious[ProcessId], UserDefined, this);
                    }

                // Check against previous case
                if (null != ProcessPrevMulPerfCallbackFn)
                    foreach (String Property in PrevMultiplierPropertyValueDict.Keys)
                    {
                        if (CheckPrevPollPropertyValue(oPrevious[ProcessId], oCurrent[ProcessId], Property, PrevMultiplierPropertyValueDict[Property]))
                            ProcessPrevMulPerfCallbackFn(Property, PrevMultiplierPropertyValueDict[Property], oCurrent[ProcessId], oPrevious[ProcessId], UserDefined, this);
                    }
            }

            if (oBase.Count > 0)
            {
                // Check base case
                foreach (UInt32 ProcessId in oBase.Keys)
                {
                    if (!oCurrent.ContainsKey(ProcessId))
                        continue;

                    // Check against base case
                    if (null != ProcessBaseMulPerfCallbackFn)
                        foreach (String Property in BaseMultiplierPropertyValueDict.Keys)
                        {
                            if (CheckPrevPollPropertyValue(oBase[ProcessId], oCurrent[ProcessId], Property, BaseMultiplierPropertyValueDict[Property]))
                                ProcessBaseMulPerfCallbackFn(Property, BaseMultiplierPropertyValueDict[Property], oCurrent[ProcessId], oBase[ProcessId], UserDefined, this);
                        }
                }
            }

            // Make the current polling value the previous
            oPrevious = oCurrent;
        }

        private bool CheckMaxPropertyValue(PerfProc_Process Previous, PerfProc_Process Current, string Property, UInt64 Value)
        {
            //Log.Write(MethodBase.GetCurrentMethod(), "Previous '" + Property + "' = " + Previous.Properties(Property) + ", Current '" + Property + "' = " + Current.Properties(Property), Log.LogType.Info);
            if ((Previous.Properties(Property) >= Value) && (Current.Properties(Property) >= Value))
                return true;
            return false;
        }

        private bool CheckPrevPollPropertyValue(PerfProc_Process Previous, PerfProc_Process Current, string Property, UInt16 Value)
        {
            if (Current.Properties(Property) >= Previous.Properties(Property) * Value)
                return true;
            return false;
        }

    }
}
