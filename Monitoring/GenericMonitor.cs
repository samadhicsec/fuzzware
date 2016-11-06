using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Management;
using System.Threading;
using Fuzzware.Common;

namespace Fuzzware.Monitoring
{
    //[System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name = "FullTrust")]
    //[System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public class GenericMonitor
    {
        // Delegate that will be called when the monitored event occurs
        public delegate void EventCallback(object obj);

        protected EventCallback CallbackFn;
        protected ManagementScope oMgmtScope;

        // We need a class reference to Asynchronous events so we can cancel them
        protected ManagementEventWatcher MEWatcher;
        bool CancelQuery = false;

        public GenericMonitor(EventCallback delEventCallback)
        {
            MEWatcher = null;
            CallbackFn = delEventCallback;

            oMgmtScope = new ManagementScope(@"root\cimv2");
        }

        public virtual void SetRemoteComputer(String Hostname, String Username, String Password)
        {
            ConnectionOptions oConn = new ConnectionOptions();
            oConn.Username = Username;
            oConn.Password = Password;

            oMgmtScope = new ManagementScope(@"\\" + Hostname + @"\root\cimv2", oConn);
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

        public virtual ManagementObjectCollection SyncQuery(ObjectQuery Query)
        {
            ManagementObjectSearcher MgmtSearcher = new ManagementObjectSearcher(oMgmtScope, Query);
            return MgmtSearcher.Get();
        }

        public virtual void AsyncQuery(EventQuery Query)
        {
            if (null != MEWatcher)
                Stop();

            MEWatcher = new ManagementEventWatcher(oMgmtScope, Query);
            //MEWatcher.EventArrived += new EventArrivedEventHandler(HandleEvent);
            //MEWatcher.Options.Timeout = new TimeSpan(0, 0, 15);
            //MEWatcher.Scope.Options.EnablePrivileges = true;
            //MEWatcher.Scope.Options.Authentication = AuthenticationLevel.Unchanged;
            //MEWatcher.Scope.Options.Authority = "NTLMDOMAIN:redmond.corp.microsoft.com.LOCAL";
            //MEWatcher.Scope.Options.Locale = "MS_409";
            //MEWatcher.Scope.Options.Impersonation = ImpersonationLevel.Delegate;
            //MEWatcher.Start();
        }

        public virtual void Start()
        {
            if (null == MEWatcher)
                Log.Write(MethodBase.GetCurrentMethod(), "No EventQuery specified. Call AsyncQuery to specify.", Log.LogType.Error);

            Stop();
            Thread.Sleep(500);

            // Kick off a new thread that will sit and wait for events
            Thread AsyncQueryThread = new Thread(new ThreadStart(this.WaitForEvents));
            AsyncQueryThread.Priority = ThreadPriority.AboveNormal;
            AsyncQueryThread.Start();
        }

        //private void HandleEvent(object sender,
        //EventArrivedEventArgs e)
        //{
        //    Console.WriteLine("Event arrived !");
        //}


        /// <summary>
        /// This function is executed in s seperate thread
        /// </summary>
        private void WaitForEvents()
        {
            CancelQuery = false;
            while (true)
            {
                ManagementBaseObject mo =  null;
                // Wait for next event
                MEWatcher.Options.Timeout = new TimeSpan(0, 0, 0, 0, 250);
                while (true)
                {
                    try  // If a timeout occurs an exception is thrown
                    {
                        mo = MEWatcher.WaitForNextEvent();
                    }
                    catch (Exception) { }
                    // If we received an event or we are cancelling, break
                    if ((null != mo) || CancelQuery)
                        break;
                }

                if (CancelQuery)
                    break;

                // Call user supplied Callback
                CallbackFn(mo);
            }
            MEWatcher.Stop();
        }

        public virtual void Stop()
        {
            CancelQuery = true;
        }
    }
}
