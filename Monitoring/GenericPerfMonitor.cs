using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Management;
using System.Threading;
using Fuzzware.Common;

namespace Fuzzware.Monitoring
{
    public class GenericPerfMonitor
    {
        // Delegate that will be called when the monitored event occurs
        public delegate void EventCallback(ManagementObjectCollection obj);

        protected Thread PolledSyncQueryThread;
        protected EventCallback CallbackFn;
        protected ManagementScope oMgmtScope;
        protected uint PollingInterval;
        protected ObjectQuery oQuery;

        bool Running = false;

        public GenericPerfMonitor(EventCallback delEventCallback, uint PollingInterval)
        {
            CallbackFn = delEventCallback;
            this.PollingInterval = PollingInterval;
            oMgmtScope = new ManagementScope(@"root\cimv2");
        }

        public virtual void SetRemoteComputer(String Hostname, String Username, String Password)
        {
            ConnectionOptions oConn = new ConnectionOptions();
            oConn.Username = Username;
            oConn.Password = Password;

            oMgmtScope = new ManagementScope(@"\\" + Hostname + @"\root\cimv2", oConn);
        }

        public ObjectQuery Query
        {
            set { oQuery = value; }
        }

        public ManagementObjectCollection RunQuery(ObjectQuery ObjQuery)
        {
            ManagementObjectSearcher MgmtSearcher = new ManagementObjectSearcher(oMgmtScope, ObjQuery);
            return MgmtSearcher.Get();
        }

        public virtual void Start()
        {
            if (null == oQuery)
                Log.Write(MethodBase.GetCurrentMethod(), "No ObjectQuery specified. Set the Query property..", Log.LogType.Error);

            if (Running)
                return;

            // Kick off a new thread that will sit and wait for events
            PolledSyncQueryThread = new Thread(new ThreadStart(this.PollForStatus));
            PolledSyncQueryThread.Priority = ThreadPriority.AboveNormal;
            PolledSyncQueryThread.Start();
        }

        /// <summary>
        /// Returns the return code of the command, 0 means success. Otherwise
        /// 2  - Access denied 
        /// 3  - Insufficient privilege 
        /// 8  - Unknown failure 
        /// 9  - Path not found 
        /// 21 - Invalid parameter 
        /// </summary>
        /// <param name="Commandline"></param>
        /// <returns></returns>
        public virtual uint RunCommand(String Commandline)
        {
            if (String.IsNullOrEmpty(Commandline))
                return 0;

            // Get the object on which the method will be invoked
            ManagementClass processClass = new ManagementClass(oMgmtScope, new ManagementPath("Win32_Process"), new ObjectGetOptions());

            // Get an input parameters object for this method
            ManagementBaseObject inParams = processClass.GetMethodParameters("Create");

            // Fill in input parameter values
            inParams["CommandLine"] = Commandline;

            // Method Options
            InvokeMethodOptions methodOptions = new InvokeMethodOptions(null, TimeSpan.MaxValue);

            // Execute the method
            ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, methodOptions);

            return (uint)outParams["returnValue"];
        }

        /// <summary>
        /// This function is executed in a seperate thread
        /// </summary>
        private void PollForStatus()
        {
            Running = true;
            while (true)
            {
                try
                {

                    ManagementObjectCollection moc = RunQuery(oQuery);

                    // Call user supplied Callback
                    CallbackFn(moc);

                    // Wait for next event
                    Thread.Sleep((int)PollingInterval*1000);
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        break;
                    Log.Write(e);
                }
            }
            Running = false;
        }

        public virtual void Stop()
        {
            if(Running)
                PolledSyncQueryThread.Abort();
        }
    }
}
