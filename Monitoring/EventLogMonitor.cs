using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Globalization;

namespace Fuzzware.Monitoring
{
    /// <summary>
    /// This mirrors the WMI Operating System class Win32_NTLogEvent
    /// </summary>
    public class Win32_NTLogEvent
    {
        public UInt16 Category;
        public string CategoryString;
        public string ComputerName;
        public byte[] Data;
        public UInt16 EventCode;
        public UInt32 EventIdentifier;
        public byte EventType;
        public string[] InsertionStrings;
        public string Logfile;
        public string Message;
        public UInt32 RecordNumber;
        public string SourceName;
        public DateTime TimeGenerated;    // Use ManagementDateTimeConverter.ToDateTime to convert to DataTime
        public DateTime TimeWritten;
        public string Type;
        public string User;

        private String SingleStringDesc;

        public Win32_NTLogEvent(ManagementBaseObject mbo)
        {
            if (mbo.Properties["Category"].Value != null)
                Category = (UInt16)mbo.Properties["Category"].Value;
            if (mbo.Properties["CategoryString"].Value != null)
                CategoryString = (String)mbo.Properties["CategoryString"].Value;
            if (mbo.Properties["ComputerName"].Value != null)
                ComputerName = (String)mbo.Properties["ComputerName"].Value;
            if (mbo.Properties["Data"].Value != null)
                Data = (byte[])mbo.Properties["Data"].Value;
            if (mbo.Properties["EventCode"].Value != null)
                EventCode = (UInt16)mbo.Properties["EventCode"].Value;
            if (mbo.Properties["EventIdentifier"].Value != null)
                EventIdentifier = (UInt32)(mbo.Properties["EventIdentifier"].Value);
            if (mbo.Properties["EventType"].Value != null)
                EventType = (byte)mbo.Properties["EventType"].Value;
            if (mbo.Properties["InsertionStrings"].Value != null)
                InsertionStrings = (string[])mbo.Properties["InsertionStrings"].Value;
            if (mbo.Properties["Logfile"].Value != null)
                Logfile = (string)mbo.Properties["Logfile"].Value;
            if (mbo.Properties["Message"].Value != null)
                Message = (String)mbo.Properties["Message"].Value;
            if (mbo.Properties["RecordNumber"].Value != null)
                RecordNumber = (UInt32)mbo.Properties["RecordNumber"].Value;
            if (mbo.Properties["SourceName"].Value != null)
                SourceName = (string)mbo.Properties["SourceName"].Value;
            if (mbo.Properties["TimeGenerated"].Value != null)
                TimeGenerated = ManagementDateTimeConverter.ToDateTime((string)mbo.Properties["TimeGenerated"].Value);
            if (mbo.Properties["TimeWritten"].Value != null)
                TimeWritten = ManagementDateTimeConverter.ToDateTime((string)mbo.Properties["TimeWritten"].Value);
            if (mbo.Properties["Type"].Value != null)
                Type = (string)mbo.Properties["Type"].Value;
            if (mbo.Properties["User"].Value != null)
                User = (string)mbo.Properties["User"].Value;

            // Create a string description of all of the properties of the object so searching is easy
            StringBuilder Desc = new StringBuilder();
            Desc.AppendLine(Category.ToString());
            Desc.AppendLine(CategoryString);
            Desc.AppendLine(ComputerName);
            if(null != Data)
                for(int i = 0; i < Data.Length; i++)
                    Desc.AppendLine(Data[i].ToString());
            Desc.AppendLine(EventCode.ToString());
            Desc.AppendLine(EventIdentifier.ToString());
            Desc.AppendLine(EventType.ToString());
            if(null != InsertionStrings)
                for(int i = 0; i < InsertionStrings.Length; i++)
                    Desc.AppendLine(InsertionStrings[i]);
            Desc.AppendLine(Logfile);
            Desc.AppendLine(Message);
            Desc.AppendLine(RecordNumber.ToString());
            Desc.AppendLine(SourceName);
            Desc.AppendLine(TimeGenerated.ToString());
            Desc.AppendLine(TimeWritten.ToString());
            Desc.AppendLine(Type);
            Desc.AppendLine(User);
            SingleStringDesc = Desc.ToString();
        }

        /// <summary>
        /// Returns true if ALL of the keywords in ANY of the lists appear in the object.  In other words a match is an OR on
        /// a match for any of the given lists, but an AND of each keyword in a given list.
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public bool Search(List<List<String>> KeywordsList)
        {
            for(int i = 0; i < KeywordsList.Count; i++)
            {
                List<String> keywords = KeywordsList[i];
                bool match = true;
                if(keywords.Count == 0)
                    match = false;
                // All of these keywords must match for a hit
                for(int j = 0; j < keywords.Count; j++)
                {
                    if(SingleStringDesc.IndexOf(keywords[j], 0, SingleStringDesc.Length, StringComparison.CurrentCultureIgnoreCase) == -1)
                    {
                        match = false;
                        break;
                    }   
                }
                if(match)
                    return true;
            }
            return false;
        }
    }

    //[System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    //[System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public class EventLogMonitor : GenericMonitor
    {
        // Delegate that will be called when the monitored event occurs
        public delegate void EventLogEventCallback(Win32_NTLogEvent EventLogEntry);

        EventLogEventCallback ELECallback;
        List<List<String>> SearchKeywords;

        public EventLogMonitor(EventLogEventCallback Callback, List<List<String>> Keywords)
            : base(null)
        {
            base.CallbackFn = this.GenCallback;
            ELECallback = Callback;
            SearchKeywords = Keywords;
        }

        public EventLogMonitor(EventLogEventCallback Callback, String[][] Keywords)
            : base(null)
        {
            SearchKeywords =  new List<List<string>>();
            for(int i = 0; i < Keywords.Length; i++)
            {
                List<String> temp = new List<string>();
                for(int j = 0; j < Keywords[i].Length; j++)
                    temp.Add(Keywords[i][j]);
                SearchKeywords.Add(temp);
            }
            base.CallbackFn = this.GenCallback;
            ELECallback = Callback;
        }

        private void GenCallback(object obj)
        {
            // Get the event log object
            ManagementBaseObject mo = (ManagementBaseObject)obj;
            PropertyData pd;
            if ((pd = mo.Properties["TargetInstance"]) == null)
                return;

            ManagementBaseObject mbo = pd.Value as ManagementBaseObject;
            // Copy the event into a local object
            Win32_NTLogEvent EventLogEntry = new Win32_NTLogEvent(mbo);

            // Search the event log entry
            if(EventLogEntry.Search(SearchKeywords))
                ELECallback(EventLogEntry);
        }

        public override void Start()
        {
            base.AsyncQuery(new EventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 10 WHERE TargetInstance ISA 'Win32_NTLogEvent'"));
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

        //public void ComputerSystem(ManagementScope oMs)
        //{
        //    WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_ComputerSystem");
        //    if (!oMs.IsConnected)
        //        return;

        //    ManagementObjectSearcher find = new ManagementObjectSearcher(oMs, query);

        //    foreach (ManagementObject mo in find.Get())
        //    {
        //        Console.WriteLine("Computer belongs to domain................" + mo["Domain"]);
        //        Console.WriteLine("Computer manufacturer....................." + mo["Manufacturer"]);
        //        Console.WriteLine("Model name given by manufacturer.........." + mo["Model"]);
        //    }

        //}
            
        //public void MemoryInfo(ManagementScope oMs)
        //{
        //    if (!oMs.IsConnected)
        //        return;

        //    WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_PerfFormattedData_PerfOS_Memory");

        //    ManagementObjectSearcher find = new ManagementObjectSearcher(oMs, query);

        //    foreach (ManagementObject mo in find.Get()) 
        //    {
        //        Console.WriteLine("Available bytes: " + mo["AvailableBytes"]);
        //        Console.WriteLine("Available KBs: " + mo["AvailableKBytes"]);
        //        Console.WriteLine("Available MBs: " + mo["AvailableMBytes"]);
        //        Console.WriteLine("Cache bytes: " + mo["CacheBytes"]);
        //        Console.WriteLine("Cache bytes peak: " + mo["CacheBytesPeak"]);
        //        Console.WriteLine("Cache bytes: " + mo["CacheBytes"]);
        //        Console.WriteLine("Commit limit: " + mo["CommitLimit"]);
        //        Console.WriteLine("Committed bytes: " + mo["CommittedBytes"]);
        //        Console.WriteLine("Free system page table entries: " + mo["FreeSystemPageTableEntries"]);
        //        Console.WriteLine("Pool paged bytes: " + mo["PoolPagedBytes"]);
        //        Console.WriteLine("System code total bytes: " + mo["SystemCodeTotalBytes"]);
        //        Console.WriteLine("System driver total bytes: " + mo["SystemDriverTotalBytes"]);
        //    }
        //}

        //public void OSInfo(ManagementScope oMs)
        //{
        //    if (!oMs.IsConnected)
        //        return;

        //    WqlObjectQuery query = new WqlObjectQuery("SELECT * FROM Win32_OperatingSystem");

        //    ManagementObjectSearcher find = new ManagementObjectSearcher(oMs, query);

        //    foreach (ManagementObject mo in find.Get())
        //    {
        //        Console.WriteLine("Boot device name........................." + mo["BootDevice"]);
        //        Console.WriteLine("Build number............................." + mo["BuildNumber"]);
        //        Console.WriteLine("Caption.................................." + mo["Caption"]);
        //        Console.WriteLine("Code page used by OS....................." + mo["CodeSet"]);
        //        Console.WriteLine("Country code............................." + mo["CountryCode"]);
        //        Console.WriteLine("Latest service pack installed............" + mo["CSDVersion"]);
        //        Console.WriteLine("Computer system name....................." + mo["CSName"]);
        //        Console.WriteLine("Time zone (minute offset from GMT........" + mo["CurrentTimeZone"]);
        //        Console.WriteLine("OS is debug build........................" + mo["Debug"]);
        //        Console.WriteLine("OS is distributed across several nodes..." + mo["Distributed"]);
        //        Console.WriteLine("Encryption level of transactions........." + mo["EncryptionLevel"] + " bits");
        //        //Console.WriteLine("Priority increase for foreground app....." + GetForeground(mo));
        //        Console.WriteLine("Available physical memory................" + mo["FreePhysicalMemory"] + " kilobytes");
        //        Console.WriteLine("Available virtual memory................." + mo["FreeVirtualMemory"] + " kilobytes");
        //        Console.WriteLine("Free paging-space withou swapping........" + mo["FreeSpaceInPagingFiles"]);
        //        Console.WriteLine("Installation date........................" + ManagementDateTimeConverter.ToDateTime(mo["InstallDate"].ToString()));
        //        Console.WriteLine("What type of memory optimization........." + (Convert.ToInt16(mo["LargeSystemCache"]) == 0 ? "for applications" : "for system performance"));
        //        Console.WriteLine("Time from last boot......................" + mo["LastBootUpTime"]);
        //        Console.WriteLine("Local date and time......................" + ManagementDateTimeConverter.ToDateTime(mo["LocalDateTime"].ToString()));
        //        Console.WriteLine("Language identifier (LANGID)............." + mo["Locale"]);
        //        Console.WriteLine("Local date and time......................" + ManagementDateTimeConverter.ToDateTime(mo["LocalDateTime"].ToString()));
        //        Console.WriteLine("Max# of processes supported by OS........" + mo["MaxNumberOfProcesses"]);
        //        Console.WriteLine("Max memory available for process........." + mo["MaxProcessMemorySize"] + " kilobytes");
        //        Console.WriteLine("Current number of processes.............." + mo["NumberOfProcesses"]);
        //        Console.WriteLine("Currently stored user sessions..........." + mo["NumberOfUsers"]);
        //        Console.WriteLine("OS language version......................" + mo["OSLanguage"]);
        //        //Console.WriteLine("OS product suite version................." + GetSuite(mo));
        //        //Console.WriteLine("OS type.................................." + GetOSType(mo));
        //        // this is extension to OS addressing space, not available to Windows XP, Windows 2000, and Windows NT 4.0 SP4 and later
        //        // Console.WriteLine("PAE enabled.............................." + mo["PAEEnabled "]);
        //        Console.WriteLine("Number of Windows Plus!.................." + mo["PlusProductID"]);
        //        Console.WriteLine("Version of Windows Plus!................." + mo["PlusVersionNumber"]);
        //        //Console.WriteLine("Type of installed OS....................." + GetProductType(mo));
        //        Console.WriteLine("Registered user of OS...................." + mo["RegisteredUser"]);
        //        Console.WriteLine("Serial number of product................." + mo["SerialNumber"]);
        //        Console.WriteLine("Serial number of product................." + mo["SerialNumber"]);
        //        Console.WriteLine("ServicePack major version................" + mo["ServicePackMajorVersion"]);
        //        Console.WriteLine("ServicePack minor version................" + mo["ServicePackMinorVersion"]);
        //        Console.WriteLine("Total number to store in paging files...." + mo["SizeStoredInPagingFiles"] + " kilobytes");
        //        Console.WriteLine("Status..................................." + mo["Status"]);
        //        Console.WriteLine("ServicePack minor version................" + mo["ServicePackMinorVersion"]);
        //        //Console.WriteLine("OS suite................................." + GetOSSuite(mo));
        //        Console.WriteLine("Physical disk partition with OS.........." + mo["SystemDevice"]);
        //        Console.WriteLine("System directory........................." + mo["SystemDirectory"]);
        //        Console.WriteLine("Total virtual memory....................." + mo["TotalVirtualMemorySize"] + " kilobytes");
        //        Console.WriteLine("Total physical memory...................." + mo["TotalVisibleMemorySize"] + " kilobytes");
        //        Console.WriteLine("Version number of OS....................." + mo["Version"]);
        //        Console.WriteLine("Windows directory........................" + mo["WindowsDirectory"]);
        //    }
    }
}
