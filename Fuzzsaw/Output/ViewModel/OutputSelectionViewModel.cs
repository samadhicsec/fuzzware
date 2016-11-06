using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Output.ViewModel
{
    public class OutputSelectionViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty UseDirectoryOutputProperty = DependencyProperty.Register("UseDirectoryOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use Directory Output
        /// </summary>
        public bool UseDirectoryOutput
        {
            get { return (bool)GetValue(UseDirectoryOutputProperty); }
            set { SetValue(UseDirectoryOutputProperty, value); }
        }

        static readonly DependencyProperty UseExeOutputProperty = DependencyProperty.Register("UseExeOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use Exe Output
        /// </summary>
        public bool UseExeOutput
        {
            get { return (bool)GetValue(UseExeOutputProperty); }
            set { SetValue(UseExeOutputProperty, value); }
        }

        static readonly DependencyProperty UseNetworkOutputProperty = DependencyProperty.Register("UseNetworkOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use Network Output
        /// </summary>
        public bool UseNetworkOutput
        {
            get { return (bool)GetValue(UseNetworkOutputProperty); }
            set { SetValue(UseNetworkOutputProperty, value); }
        }

        static readonly DependencyProperty UseWSDLOutputProperty = DependencyProperty.Register("UseWSDLOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use WSDL Output
        /// </summary>
        public bool UseWSDLOutput
        {
            get { return (bool)GetValue(UseWSDLOutputProperty); }
            set { SetValue(UseWSDLOutputProperty, value); }
        }

        static readonly DependencyProperty UseActiveXOutputProperty = DependencyProperty.Register("UseActiveXOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use ActiveX Output
        /// </summary>
        public bool UseActiveXOutput
        {
            get { return (bool)GetValue(UseActiveXOutputProperty); }
            set { SetValue(UseActiveXOutputProperty, value); }
        }

        static readonly DependencyProperty UseCustomOutputProperty = DependencyProperty.Register("UseCustomOutput", typeof(bool), typeof(OutputSelectionViewModel));
        /// <summary>
        /// Use Custom Output
        /// </summary>
        public bool UseCustomOutput
        {
            get { return (bool)GetValue(UseCustomOutputProperty); }
            set { SetValue(UseCustomOutputProperty, value); }
        }

        #endregion

        #region Handler Commands

        #region UseDirectoryOutput
        RelayCommand m_oUseDirectoryOutputCommand;

        /// <summary>
        /// Use Directory Output
        /// </summary>
        public ICommand UseDirectoryOutputCommand
        {
            get
            {
                if (null == m_oUseDirectoryOutputCommand)
                    m_oUseDirectoryOutputCommand = new RelayCommand(UseDirectoryOutputExecute);
                return m_oUseDirectoryOutputCommand;
            }
        }

        public void UseDirectoryOutputExecute()
        {
            DeSelectAll();
            UseDirectoryOutput = true;
        }
        #endregion

        #region UseExeOutput
        RelayCommand m_oUseExeOutputCommand;

        /// <summary>
        /// Use Exe Output
        /// </summary>
        public ICommand UseExeOutputCommand
        {
            get
            {
                if (null == m_oUseExeOutputCommand)
                    m_oUseExeOutputCommand = new RelayCommand(UseExeOutputExecute);
                return m_oUseExeOutputCommand;
            }
        }

        public void UseExeOutputExecute()
        {
            DeSelectAll();
            UseExeOutput = true;
        }
        #endregion

        #region UseNetworkOutput
        RelayCommand m_oUseNetworkOutputCommand;

        /// <summary>
        /// Use Network Output
        /// </summary>
        public ICommand UseNetworkOutputCommand
        {
            get
            {
                if (null == m_oUseNetworkOutputCommand)
                    m_oUseNetworkOutputCommand = new RelayCommand(UseNetworkOutputExecute);
                return m_oUseNetworkOutputCommand;
            }
        }

        public void UseNetworkOutputExecute()
        {
            DeSelectAll();
            UseNetworkOutput = true;
        }
        #endregion

        #region UseWSDLOutput
        RelayCommand m_oUseWSDLOutputCommand;

        /// <summary>
        /// Use WSDL Output
        /// </summary>
        public ICommand UseWSDLOutputCommand
        {
            get
            {
                if (null == m_oUseWSDLOutputCommand)
                    m_oUseWSDLOutputCommand = new RelayCommand(UseWSDLOutputExecute);
                return m_oUseWSDLOutputCommand;
            }
        }

        public void UseWSDLOutputExecute()
        {
            DeSelectAll();
            UseWSDLOutput = true;
        }
        #endregion

        #region UseActiveXOutput
        RelayCommand m_oUseActiveXOutputCommand;

        /// <summary>
        /// Use ActiveX Output
        /// </summary>
        public ICommand UseActiveXOutputCommand
        {
            get
            {
                if (null == m_oUseActiveXOutputCommand)
                    m_oUseActiveXOutputCommand = new RelayCommand(UseActiveXOutputExecute);
                return m_oUseActiveXOutputCommand;
            }
        }

        public void UseActiveXOutputExecute()
        {
            DeSelectAll();
            UseActiveXOutput = true;
        }
        #endregion

        #region UseCustomOutput
        RelayCommand m_oUseCustomOutputCommand;

        /// <summary>
        /// Use Custom Output
        /// </summary>
        public ICommand UseCustomOutputCommand
        {
            get
            {
                if (null == m_oUseCustomOutputCommand)
                    m_oUseCustomOutputCommand = new RelayCommand(UseCustomOutputExecute);
                return m_oUseCustomOutputCommand;
            }
        }

        public void UseCustomOutputExecute()
        {
            DeSelectAll();
            UseCustomOutput = true;
        }
        #endregion

        #endregion

        private void DeSelectAll()
        {
            UseDirectoryOutput = false;
            UseExeOutput = false;
            UseNetworkOutput = false;
            UseWSDLOutput = false;
            UseActiveXOutput = false;
            UseCustomOutput = false;
        }
    }
}
