using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    public class InputSelectionViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty UseFileXMLInputProperty = DependencyProperty.Register("UseFileXMLInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use File XML Input
        /// </summary>
        public bool UseFileXMLInput
        {
            get { return (bool)GetValue(UseFileXMLInputProperty); }
            set { SetValue(UseFileXMLInputProperty, value); }
        }

        static readonly DependencyProperty UseFileC2XInputProperty = DependencyProperty.Register("UseFileC2XInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use File C2X Input
        /// </summary>
        public bool UseFileC2XInput
        {
            get { return (bool)GetValue(UseFileC2XInputProperty); }
            set { SetValue(UseFileC2XInputProperty, value); }
        }

        static readonly DependencyProperty UseNetworkXMLInputProperty = DependencyProperty.Register("UseNetworkXMLInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public bool UseNetworkXMLInput
        {
            get { return (bool)GetValue(UseNetworkXMLInputProperty); }
            set { SetValue(UseNetworkXMLInputProperty, value); }
        }

        static readonly DependencyProperty UseNetworkC2XInputProperty = DependencyProperty.Register("UseNetworkC2XInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use Network C2X Input
        /// </summary>
        public bool UseNetworkC2XInput
        {
            get { return (bool)GetValue(UseNetworkC2XInputProperty); }
            set { SetValue(UseNetworkC2XInputProperty, value); }
        }

        static readonly DependencyProperty UseNetworkPDMLInputProperty = DependencyProperty.Register("UseNetworkPDMLInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use PDML Input
        /// </summary>
        public bool UseNetworkPDMLInput
        {
            get { return (bool)GetValue(UseNetworkPDMLInputProperty); }
            set { SetValue(UseNetworkPDMLInputProperty, value); }
        }

        static readonly DependencyProperty UseWSDLInputProperty = DependencyProperty.Register("UseWSDLInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use WSDL Input
        /// </summary>
        public bool UseWSDLInput
        {
            get { return (bool)GetValue(UseWSDLInputProperty); }
            set { SetValue(UseWSDLInputProperty, value); }
        }

        static readonly DependencyProperty UseActiveXInputProperty = DependencyProperty.Register("UseActiveXInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use ActiveX Input
        /// </summary>
        public bool UseActiveXInput
        {
            get { return (bool)GetValue(UseActiveXInputProperty); }
            set { SetValue(UseActiveXInputProperty, value); }
        }

        static readonly DependencyProperty UseCustomInputProperty = DependencyProperty.Register("UseCustomInput", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Use Custom Input
        /// </summary>
        public bool UseCustomInput
        {
            get { return (bool)GetValue(UseCustomInputProperty); }
            set { SetValue(UseCustomInputProperty, value); }
        }

        static readonly DependencyProperty ExpandFileFuzzProperty = DependencyProperty.Register("ExpandFileFuzz", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Expand the File Fuzz Section
        /// </summary>
        public bool ExpandFileFuzz
        {
            get { return (bool)GetValue(ExpandFileFuzzProperty); }
            set { SetValue(ExpandFileFuzzProperty, value); }
        }

        static readonly DependencyProperty ExpandNetworkFuzzProperty = DependencyProperty.Register("ExpandNetworkFuzz", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Expand the Network Fuzz Section
        /// </summary>
        public bool ExpandNetworkFuzz
        {
            get { return (bool)GetValue(ExpandNetworkFuzzProperty); }
            set { SetValue(ExpandNetworkFuzzProperty, value); }
        }

        static readonly DependencyProperty ExpandInterfaceFuzzProperty = DependencyProperty.Register("ExpandInterfaceFuzz", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Expand the Interface Fuzz Section
        /// </summary>
        public bool ExpandInterfaceFuzz
        {
            get { return (bool)GetValue(ExpandInterfaceFuzzProperty); }
            set { SetValue(ExpandInterfaceFuzzProperty, value); }
        }

        static readonly DependencyProperty ExpandCustomFuzzProperty = DependencyProperty.Register("ExpandCustomFuzz", typeof(bool), typeof(InputSelectionViewModel));
        /// <summary>
        /// Expand the Custom Fuzz Section
        /// </summary>
        public bool ExpandCustomFuzz
        {
            get { return (bool)GetValue(ExpandCustomFuzzProperty); }
            set { SetValue(ExpandCustomFuzzProperty, value); }
        }

        #endregion

        #region Handler Commands

        #region UseFileXMLInput
        RelayCommand m_oUseFileXMLInputCommand;

        /// <summary>
        /// Use XML File Input
        /// </summary>
        public ICommand UseFileXMLInputCommand
        {
            get
            {
                if (null == m_oUseFileXMLInputCommand)
                    m_oUseFileXMLInputCommand = new RelayCommand(UseFileXMLInputExecute);
                return m_oUseFileXMLInputCommand;
            }
        }

        public void UseFileXMLInputExecute()
        {
            DeSelectAll();
            ExpandFileFuzz = true;
            UseFileXMLInput = true;
        }
        #endregion

        #region UseFileC2XInput
        RelayCommand m_oUseFileC2XInputCommand;

        /// <summary>
        /// Use C2X File Input
        /// </summary>
        public ICommand UseFileC2XInputCommand
        {
            get
            {
                if (null == m_oUseFileC2XInputCommand)
                    m_oUseFileC2XInputCommand = new RelayCommand(UseFileC2XInputExecute);
                return m_oUseFileC2XInputCommand;
            }
        }

        public void UseFileC2XInputExecute()
        {
            DeSelectAll();
            ExpandFileFuzz = true;
            UseFileC2XInput = true;
        }
        #endregion

        #region UseNetworkXMLInput
        RelayCommand m_oUseNetworkXMLInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseNetworkXMLInputCommand
        {
            get
            {
                if (null == m_oUseNetworkXMLInputCommand)
                    m_oUseNetworkXMLInputCommand = new RelayCommand(UseNetworkXMLInputExecute);
                return m_oUseNetworkXMLInputCommand;
            }
        }

        public void UseNetworkXMLInputExecute()
        {
            DeSelectAll();
            ExpandNetworkFuzz = true;
            UseNetworkXMLInput = true;
        }
        #endregion

        #region UseNetworkC2XInput
        RelayCommand m_oUseNetworkC2XInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseNetworkC2XInputCommand
        {
            get
            {
                if (null == m_oUseNetworkC2XInputCommand)
                    m_oUseNetworkC2XInputCommand = new RelayCommand(UseNetworkC2XInputExecute);
                return m_oUseNetworkC2XInputCommand;
            }
        }

        public void UseNetworkC2XInputExecute()
        {
            DeSelectAll();
            ExpandNetworkFuzz = true;
            UseNetworkC2XInput = true;
        }
        #endregion

        #region UseNetworkPDMLInput
        RelayCommand m_oUseNetworkPDMLInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseNetworkPDMLInputCommand
        {
            get
            {
                if (null == m_oUseNetworkPDMLInputCommand)
                    m_oUseNetworkPDMLInputCommand = new RelayCommand(UseNetworkPDMLInputExecute);
                return m_oUseNetworkPDMLInputCommand;
            }
        }

        public void UseNetworkPDMLInputExecute()
        {
            DeSelectAll();
            ExpandNetworkFuzz = true;
            UseNetworkPDMLInput = true;
        }
        #endregion

        #region UseWSDLInput
        RelayCommand m_oUseWSDLInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseWSDLInputCommand
        {
            get
            {
                if (null == m_oUseWSDLInputCommand)
                    m_oUseWSDLInputCommand = new RelayCommand(UseWSDLInputExecute);
                return m_oUseWSDLInputCommand;
            }
        }

        public void UseWSDLInputExecute()
        {
            DeSelectAll();
            ExpandInterfaceFuzz = true;
            UseWSDLInput = true;
        }
        #endregion

        #region UseActiveXInput
        RelayCommand m_oUseActiveXInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseActiveXInputCommand
        {
            get
            {
                if (null == m_oUseActiveXInputCommand)
                    m_oUseActiveXInputCommand = new RelayCommand(UseActiveXInputExecute);
                return m_oUseActiveXInputCommand;
            }
        }

        public void UseActiveXInputExecute()
        {
            DeSelectAll();
            ExpandInterfaceFuzz = true;
            UseActiveXInput = true;
        }
        #endregion

        #region UseCustomInput
        RelayCommand m_oUseCustomInputCommand;

        /// <summary>
        /// Use Network XML Input
        /// </summary>
        public ICommand UseCustomInputCommand
        {
            get
            {
                if (null == m_oUseCustomInputCommand)
                    m_oUseCustomInputCommand = new RelayCommand(UseCustomInputExecute);
                return m_oUseCustomInputCommand;
            }
        }

        public void UseCustomInputExecute()
        {
            DeSelectAll();
            ExpandCustomFuzz = true;
            UseCustomInput = true;
        }
        #endregion

        #endregion

        private void DeSelectAll()
        {
            UseFileXMLInput = false;
            UseFileC2XInput = false;
            UseNetworkXMLInput = false;
            UseNetworkC2XInput = false;
            UseNetworkPDMLInput = false;
            UseWSDLInput = false;
            UseActiveXInput = false;
            UseCustomInput = false;

            ExpandFileFuzz = false;
            ExpandNetworkFuzz = false;
            ExpandInterfaceFuzz = false;
            ExpandCustomFuzz = false;
        }
    }
}
