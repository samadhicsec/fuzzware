using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Common;
using Fuzzware.Convert2XML.C2X;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    public class TestC2XWindowViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty InitialiseEnabledProperty = DependencyProperty.Register("InitialiseEnabled", typeof(bool), typeof(TestC2XWindowViewModel));
        public bool InitialiseEnabled
        {
            get { return (bool)GetValue(InitialiseEnabledProperty); }
            set { SetValue(InitialiseEnabledProperty, value); }
        }

        static readonly DependencyProperty StartEnabledProperty = DependencyProperty.Register("StartEnabled", typeof(bool), typeof(TestC2XWindowViewModel));
        public bool StartEnabled
        {
            get { return (bool)GetValue(StartEnabledProperty); }
            set { SetValue(StartEnabledProperty, value); }
        }

        static readonly DependencyProperty PauseEnabledProperty = DependencyProperty.Register("PauseEnabled", typeof(bool), typeof(TestC2XWindowViewModel));
        public bool PauseEnabled
        {
            get { return (bool)GetValue(PauseEnabledProperty); }
            set { SetValue(PauseEnabledProperty, value); }
        }

        static readonly DependencyProperty ResumeEnabledProperty = DependencyProperty.Register("ResumeEnabled", typeof(bool), typeof(TestC2XWindowViewModel));
        public bool ResumeEnabled
        {
            get { return (bool)GetValue(ResumeEnabledProperty); }
            set { SetValue(ResumeEnabledProperty, value); }
        }

        static readonly DependencyProperty EndEnabledProperty = DependencyProperty.Register("EndEnabled", typeof(bool), typeof(TestC2XWindowViewModel));
        public bool EndEnabled
        {
            get { return (bool)GetValue(EndEnabledProperty); }
            set { SetValue(EndEnabledProperty, value); }
        }

        static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(double), typeof(TestC2XWindowViewModel));
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        static readonly DependencyProperty ConversionOutputProperty = DependencyProperty.Register("ConversionOutput", typeof(String), typeof(TestC2XWindowViewModel));
        public String ConversionOutput
        {
            get { return (String)GetValue(ConversionOutputProperty); }
            set { SetValue(ConversionOutputProperty, value); }
        }

        static readonly DependencyProperty LogOutputProperty = DependencyProperty.Register("LogOutput", typeof(String), typeof(TestC2XWindowViewModel));
        public String LogOutput
        {
            get { return (String)GetValue(LogOutputProperty); }
            set { SetValue(LogOutputProperty, value); }
        }

        #endregion

        #region Commands

        #region Initialise
        RelayCommand m_oInitialiseCommand;

        public ICommand InitialiseCommand
        {
            get
            {
                if (null == m_oInitialiseCommand)
                    m_oInitialiseCommand = new RelayCommand(InitialiseExecute);
                return m_oInitialiseCommand;
            }
        }

        /// <summary>
        /// Initialise the test before starting
        /// </summary>
        public void InitialiseExecute()
        {
            try
            {
                m_C2XInputHandler = new Fuzzware.Convert2XML.Convert2XMLInput();
                m_C2XInputHandler.Initialise(m_C2XSettings, m_oOutputEncoding);
                // Read it so it gets generated
                object o = m_C2XInputHandler.ObjectDB;
            }
            catch
            {
                return;
            }
            StartEnabled = true;
        }
        #endregion

        #region Start
        RelayCommand m_oStartCommand;

        public ICommand StartCommand
        {
            get
            {
                if (null == m_oStartCommand)
                    m_oStartCommand = new RelayCommand(StartExecute);
                return m_oStartCommand;
            }
        }

        /// <summary>
        /// Start the test in a new thread
        /// </summary>
        public void StartExecute()
        {
            PauseEnabled = false;
            //tbiC2XOutput.Focus();
            // May need to kick this off in its own thread
            m_C2XThread = new Thread(new ThreadStart(DoConversion));
            m_C2XThread.IsBackground = true;
            m_C2XThread.SetApartmentState(ApartmentState.STA);
            m_C2XThread.Start();
            InitialiseEnabled = false;
            StartEnabled = false;
            PauseEnabled = true;
            ResumeEnabled = false;
            EndEnabled = true;
        }
        #endregion

        #region Pause
        RelayCommand m_oPauseCommand;

        public ICommand PauseCommand
        {
            get
            {
                if (null == m_oPauseCommand)
                    m_oPauseCommand = new RelayCommand(PauseExecute);
                return m_oPauseCommand;
            }
        }

        /// <summary>
        /// Pause the test
        /// </summary>
        public void PauseExecute()
        {
            m_bPause = true;
            PauseEnabled = false;
            ResumeEnabled = true;
        }
        #endregion

        #region Resume
        RelayCommand m_oResumeCommand;

        public ICommand ResumeCommand
        {
            get
            {
                if (null == m_oResumeCommand)
                    m_oResumeCommand = new RelayCommand(ResumeExecute);
                return m_oResumeCommand;
            }
        }

        /// <summary>
        /// Resume the test
        /// </summary>
        public void ResumeExecute()
        {
            m_bPause = false;
            PauseEnabled = true;
            ResumeEnabled = false;
        }
        #endregion

        #region End
        RelayCommand m_oEndCommand;

        public ICommand EndCommand
        {
            get
            {
                if (null == m_oEndCommand)
                    m_oEndCommand = new RelayCommand(EndExecute);
                return m_oEndCommand;
            }
        }

        /// <summary>
        /// End the test
        /// </summary>
        public void EndExecute()
        {
            if ((null != m_C2XThread) && (m_C2XThread.IsAlive))
                m_C2XThread.Abort();
            InitialiseEnabled = true;
            StartEnabled = true;
            PauseEnabled = false;
            ResumeEnabled = false;
            EndEnabled = false;
        }
        #endregion

        #region FinishTest
        RelayCommand m_oFinishTestCommand;

        public ICommand FinishTestCommand
        {
            get
            {
                if (null == m_oFinishTestCommand)
                    m_oFinishTestCommand = new RelayCommand(FinishTestExecute);
                return m_oFinishTestCommand;
            }
        }

        /// <summary>
        /// Allows cleanup to occur when exiting the test
        /// </summary>
        public void FinishTestExecute()
        {
            // In case the user closes the TestC2X functionality without ending it first
            EndExecute();

            StopCapturingLog();
        }
        #endregion

        #endregion

        Encoding m_oOutputEncoding;
        Fuzzware.Convert2XML.Convert2XMLInput m_C2XInputHandler;
        protected Fuzzware.Schemas.AutoGenerated.Convert2XMLInput m_C2XSettings;
        Thread m_C2XThread;
        bool m_bPause;

        public TestC2XWindowViewModel(Fuzzware.Schemas.AutoGenerated.Convert2XMLInput C2XInput, Encoding OutputEncoding)
        {
            CaptureLog(Log.LogType.Info);

            m_C2XSettings = C2XInput;
            m_oOutputEncoding = OutputEncoding;

            InitialiseEnabled = true;
            StartEnabled = false;
            PauseEnabled = false;
            ResumeEnabled = false;
            EndEnabled = false;
        }

        private void DoConversion()
        {
            try
            {
                Fuzzware.Convert2XML.C2X.Convert2XML C2X = new Fuzzware.Convert2XML.C2X.Convert2XML(m_C2XInputHandler.SchemaSet, m_C2XInputHandler.ObjectDB, m_oOutputEncoding);
                C2X.ProgressEventHandler += ProgressCallback;
                if (C2X.Convert(m_C2XSettings.Convert2XML))
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Conversion process was successful", Log.LogType.Info);
                }
                else
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Conversion process was unsuccessful", Log.LogType.Info);
                }
            }
            catch (Exception e)
            {
                if (e is System.Threading.ThreadAbortException)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Conversion process was ended", Log.LogType.Info);
                }
                else
                {
                    System.Windows.MessageBox.Show("An error occurred, see Log window for details", "Error", MessageBoxButton.OK);
                }
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate()
            {
                EndExecute();
            }));
        }

        void ProgressCallback(object sender, C2XProgressEventArgs a)
        {
            double dDelay = 0;
            // Show the current XML
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate()
            {
                ConversionOutput = a.XML;
                //tbC2XOutput.ScrollToEnd();
                dDelay = Delay;
            }));

            // If paused, then loop;
            while (m_bPause)
                System.Threading.Thread.Sleep(100);

            // Pause for some amount of time
            System.Threading.Thread.Sleep((int)dDelay);
        }

        /// <summary>
        /// When the log gets updated, update the corresponding DP
        /// </summary>
        protected override void LogUpdated(object sender, LogEventArgs a)
        {
            base.LogUpdated(sender, a);
            
            if(this.Dispatcher.CheckAccess())
                LogOutput = m_oLogOutput.ToString();
            else
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate()
                {
                    LogOutput = m_oLogOutput.ToString();
                }));
        }
    }
}
