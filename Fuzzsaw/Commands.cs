using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Fuzzware.Fuzzsaw
{
    public class ErrorHelper
    {
        public object Source;
        public string ErrorMessage;
        public bool Recoverable;

        public ErrorHelper(object oSource, string sErrorMessage, bool bRecoverable)
        {
            Source = oSource;
            ErrorMessage = sErrorMessage;
            Recoverable = bRecoverable;
        }
    }

    public class HelperCommands
    {
        public static RoutedUICommand ShowError = new RoutedUICommand();
        public static RoutedUICommand AddToMainMenu = new RoutedUICommand();
        public static RoutedUICommand RemoveFromMainMenu = new RoutedUICommand();
        public static RoutedUICommand ShowProjectProperties = new RoutedUICommand();
        public static RoutedUICommand ShowProperties = new RoutedUICommand();
        public static RoutedUICommand ExitApplication = new RoutedUICommand();

        #region ExecuteFuzzer
        /// <summary>
        /// The command to execute the fuzzer
        /// </summary>
        public static readonly DependencyProperty ExecuteFuzzerProperty = DependencyProperty.RegisterAttached("ExecuteFuzzer", typeof(ICommand), typeof(HelperCommands));

        public static ICommand GetExecuteFuzzer(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ExecuteFuzzerProperty);
        }

        public static void SetExecuteFuzzer(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ExecuteFuzzerProperty, value);
        }
        #endregion
    }

    public class ProjectNavigationCommands
    {
        //public static RoutedUICommand ShowInputSelectionScreen = new RoutedUICommand();
        //public static RoutedUICommand ShowInputConfigScreen = new RoutedUICommand();

        //public static RoutedUICommand ShowDataTypesConfigScreen = new RoutedUICommand();
        //public static RoutedUICommand ShowDataStructuresConfigScreen = new RoutedUICommand();

        //public static RoutedUICommand ShowOutputSelectionScreen = new RoutedUICommand();
        //public static RoutedUICommand ShowOutputConfigScreen = new RoutedUICommand();

        //public static RoutedUICommand ShowMonitoringScreen = new RoutedUICommand();
        //public static RoutedUICommand ShowExecutionScreen = new RoutedUICommand();

        #region ShowInputSelection
        /// <summary>
        /// Shows the input selection control
        /// </summary>
        public static readonly DependencyProperty ShowInputSelectionProperty = DependencyProperty.RegisterAttached("ShowInputSelection", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowInputSelection(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowInputSelectionProperty);
        }

        public static void SetShowInputSelection(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowInputSelectionProperty, value);
        }
        #endregion

        #region ShowInputConfig
        /// <summary>
        /// Shows the input configuration control
        /// </summary>
        public static readonly DependencyProperty ShowInputConfigProperty = DependencyProperty.RegisterAttached("ShowInputConfig", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowInputConfig(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowInputConfigProperty);
        }

        public static void SetShowInputConfig(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowInputConfigProperty, value);
        }
        #endregion

        #region ShowDataTypesConfig
        /// <summary>
        /// Shows the data types configuration control
        /// </summary>
        public static readonly DependencyProperty ShowDataTypesConfigProperty = DependencyProperty.RegisterAttached("ShowDataTypesConfig", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowDataTypesConfig(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowDataTypesConfigProperty);
        }

        public static void SetShowDataTypesConfig(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowDataTypesConfigProperty, value);
        }
        #endregion

        #region ShowDataStructuresConfig
        /// <summary>
        /// Shows the data structures configuration control
        /// </summary>
        public static readonly DependencyProperty ShowDataStructuresConfigProperty = DependencyProperty.RegisterAttached("ShowDataStructuresConfig", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowDataStructuresConfig(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowDataStructuresConfigProperty);
        }

        public static void SetShowDataStructuresConfig(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowDataStructuresConfigProperty, value);
        }
        #endregion

        #region ShowOutputSelection
        /// <summary>
        /// Shows the output selection control
        /// </summary>
        public static readonly DependencyProperty ShowOutputSelectionProperty = DependencyProperty.RegisterAttached("ShowOutputSelection", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowOutputSelection(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowOutputSelectionProperty);
        }

        public static void SetShowOutputSelection(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowOutputSelectionProperty, value);
        }
        #endregion

        #region ShowOutputConfig
        /// <summary>
        /// Shows the output configuration control
        /// </summary>
        public static readonly DependencyProperty ShowOutputConfigProperty = DependencyProperty.RegisterAttached("ShowOutputConfig", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowOutputConfig(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowOutputConfigProperty);
        }

        public static void SetShowOutputConfig(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowOutputConfigProperty, value);
        }
        #endregion

        #region ShowMonitoring
        /// <summary>
        /// Shows the monitoring control
        /// </summary>
        public static readonly DependencyProperty ShowMonitoringProperty = DependencyProperty.RegisterAttached("ShowMonitoring", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowMonitoring(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowMonitoringProperty);
        }

        public static void SetShowMonitoring(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowMonitoringProperty, value);
        }
        #endregion

        #region ShowExecution
        /// <summary>
        /// Shows the execution control
        /// </summary>
        public static readonly DependencyProperty ShowExecutionProperty = DependencyProperty.RegisterAttached("ShowExecution", typeof(ICommand), typeof(ProjectNavigationCommands));

        public static ICommand GetShowExecution(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(ShowExecutionProperty);
        }

        public static void SetShowExecution(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(ShowExecutionProperty, value);
        }
        #endregion
    }

    public class HandlerCommands
    {
        // Input Commands
        //public static RoutedUICommand UseFileXMLInput = new RoutedUICommand();
        //public static RoutedUICommand UseFileC2XInput = new RoutedUICommand();

        //public static RoutedUICommand UseNetworkXMLInput = new RoutedUICommand();
        //public static RoutedUICommand UseNetworkC2XInput = new RoutedUICommand();
        //public static RoutedUICommand UseNetworkPDMLInput = new RoutedUICommand();

        //public static RoutedUICommand UseInterfaceWSDLInput = new RoutedUICommand();
        //public static RoutedUICommand UseInterfaceActiveXInput = new RoutedUICommand();

        //public static RoutedUICommand UseCustomInput = new RoutedUICommand();

        #region UseFileXMLInput
        /// <summary>
        /// Use XML File Input
        /// </summary>
        public static readonly DependencyProperty UseFileXMLInputProperty = DependencyProperty.RegisterAttached("UseFileXMLInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseFileXMLInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseFileXMLInputProperty);
        }

        public static void SetUseFileXMLInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseFileXMLInputProperty, value);
        }
        #endregion
        #region UseFileC2XInput
        /// <summary>
        /// Use C2X Input
        /// </summary>
        public static readonly DependencyProperty UseFileC2XInputProperty = DependencyProperty.RegisterAttached("UseFileC2XInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseFileC2XInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseFileC2XInputProperty);
        }

        public static void SetUseFileC2XInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseFileC2XInputProperty, value);
        }
        #endregion

        #region UseNetworkXMLInput
        /// <summary>
        /// Use Network XML File Input
        /// </summary>
        public static readonly DependencyProperty UseNetworkXMLInputProperty = DependencyProperty.RegisterAttached("UseNetworkXMLInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseNetworkXMLInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseNetworkXMLInputProperty);
        }

        public static void SetUseNetworkXMLInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseNetworkXMLInputProperty, value);
        }
        #endregion
        #region UseNetworkC2XInput
        /// <summary>
        /// Use Network C2X Input
        /// </summary>
        public static readonly DependencyProperty UseNetworkC2XInputProperty = DependencyProperty.RegisterAttached("UseNetworkC2XInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseNetworkC2XInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseNetworkC2XInputProperty);
        }

        public static void SetUseNetworkC2XInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseNetworkC2XInputProperty, value);
        }
        #endregion
        #region UseNetworkPDMLInput
        /// <summary>
        /// Use Network PDML Input
        /// </summary>
        public static readonly DependencyProperty UseNetworkPDMLInputProperty = DependencyProperty.RegisterAttached("UseNetworkPDMLInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseNetworkPDMLInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseNetworkPDMLInputProperty);
        }

        public static void SetUseNetworkPDMLInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseNetworkPDMLInputProperty, value);
        }
        #endregion

        #region UseInterfaceWSDLInput
        /// <summary>
        /// Use WSDL Input
        /// </summary>
        public static readonly DependencyProperty UseInterfaceWSDLInputProperty = DependencyProperty.RegisterAttached("UseInterfaceWSDLInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseInterfaceWSDLInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseInterfaceWSDLInputProperty);
        }

        public static void SetUseInterfaceWSDLInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseInterfaceWSDLInputProperty, value);
        }
        #endregion
        #region UseInterfaceActiveXInput
        /// <summary>
        /// Use ActiveX Input
        /// </summary>
        public static readonly DependencyProperty UseInterfaceActiveXInputProperty = DependencyProperty.RegisterAttached("UseInterfaceActiveXInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseInterfaceActiveXInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseInterfaceActiveXInputProperty);
        }

        public static void SetUseInterfaceActiveXInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseInterfaceActiveXInputProperty, value);
        }
        #endregion

        #region UseCustomInput
        /// <summary>
        /// Use Custom Input
        /// </summary>
        public static readonly DependencyProperty UseCustomInputProperty = DependencyProperty.RegisterAttached("UseCustomInput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseCustomInput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseCustomInputProperty);
        }

        public static void SetUseCustomInput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseCustomInputProperty, value);
        }
        #endregion

        // Output Commands
        //public static RoutedUICommand UseDirectoryOutput = new RoutedUICommand();
        //public static RoutedUICommand UseExeOutput = new RoutedUICommand();
        //public static RoutedUICommand UseNetworkOutput = new RoutedUICommand();
        //public static RoutedUICommand UseWSDLOutput = new RoutedUICommand();
        //public static RoutedUICommand UseActiveXOutput = new RoutedUICommand();
        //public static RoutedUICommand UseCustomOutput = new RoutedUICommand();

        #region UseDirectoryOutput
        /// <summary>
        /// Use Directory Output
        /// </summary>
        public static readonly DependencyProperty UseDirectoryOutputProperty = DependencyProperty.RegisterAttached("UseDirectoryOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseDirectoryOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseDirectoryOutputProperty);
        }

        public static void SetUseDirectoryOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseDirectoryOutputProperty, value);
        }
        #endregion

        #region UseExeOutput
        /// <summary>
        /// Use Exe Output
        /// </summary>
        public static readonly DependencyProperty UseExeOutputProperty = DependencyProperty.RegisterAttached("UseExeOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseExeOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseExeOutputProperty);
        }

        public static void SetUseExeOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseExeOutputProperty, value);
        }
        #endregion

        #region UseNetworkOutput
        /// <summary>
        /// Use Network Output
        /// </summary>
        public static readonly DependencyProperty UseNetworkOutputProperty = DependencyProperty.RegisterAttached("UseNetworkOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseNetworkOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseNetworkOutputProperty);
        }

        public static void SetUseNetworkOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseNetworkOutputProperty, value);
        }
        #endregion

        #region UseWSDLOutput
        /// <summary>
        /// Use WSDL Output
        /// </summary>
        public static readonly DependencyProperty UseWSDLOutputProperty = DependencyProperty.RegisterAttached("UseWSDLOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseWSDLOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseWSDLOutputProperty);
        }

        public static void SetUseWSDLOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseWSDLOutputProperty, value);
        }
        #endregion

        #region UseActiveXOutput
        /// <summary>
        /// Use ActiveX Output
        /// </summary>
        public static readonly DependencyProperty UseActiveXOutputProperty = DependencyProperty.RegisterAttached("UseActiveXOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseActiveXOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseActiveXOutputProperty);
        }

        public static void SetUseActiveXOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseActiveXOutputProperty, value);
        }
        #endregion

        #region UseCustomOutput
        /// <summary>
        /// Use ActiveX Output
        /// </summary>
        public static readonly DependencyProperty UseCustomOutputProperty = DependencyProperty.RegisterAttached("UseCustomOutput", typeof(ICommand), typeof(HandlerCommands));

        public static ICommand GetUseCustomOutput(DependencyObject target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(UseCustomOutputProperty);
        }

        public static void SetUseCustomOutput(DependencyObject target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(UseCustomOutputProperty, value);
        }
        #endregion
    }

    //public class NavPaneCommands
    //{
    //    public static RoutedUICommand SelectInput = new RoutedUICommand();
    //    public static RoutedUICommand SelectOutput = new RoutedUICommand();
    //}

    /// <summary>
    /// Commands for a list of data where we can add, delete, prmote and demote the data
    /// </summary>
    public class ListDataCommands
    {
        #region AddDataMethod
        /// <summary>
        /// Add Data DependencyProperty
        /// </summary>
        public static DependencyProperty AddDataMethodProperty =
                DependencyProperty.RegisterAttached(
                        "AddDataMethod",
                        typeof(ICommand),
                        typeof(ListDataCommands));

        public static ICommand GetAddDataMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(AddDataMethodProperty);
        }

        public static void SetAddDataMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(AddDataMethodProperty, value);
        }
        #endregion

        #region DeleteDataMethod
        /// <summary>
        /// Delete Data DependencyProperty
        /// </summary>
        public static DependencyProperty DeleteDataMethodProperty =
                DependencyProperty.RegisterAttached(
                        "DeleteDataMethod",
                        typeof(ICommand),
                        typeof(ListDataCommands));

        public static ICommand GetDeleteDataMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(DeleteDataMethodProperty);
        }

        public static void SetDeleteDataMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(DeleteDataMethodProperty, value);
        }
        #endregion

        #region PromoteDataMethod
        /// <summary>
        /// Promote Data DependencyProperty
        /// </summary>
        public static DependencyProperty PromoteDataMethodProperty =
                DependencyProperty.RegisterAttached(
                        "PromoteDataMethod",
                        typeof(ICommand),
                        typeof(ListDataCommands));

        public static ICommand GetPromoteDataMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(PromoteDataMethodProperty);
        }

        public static void SetPromoteDataMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(PromoteDataMethodProperty, value);
        }
        #endregion

        #region DemoteDataMethod
        /// <summary>
        /// Demote Data DependencyProperty
        /// </summary>
        public static DependencyProperty DemoteDataMethodProperty =
                DependencyProperty.RegisterAttached(
                        "DemoteDataMethod",
                        typeof(ICommand),
                        typeof(ListDataCommands));

        public static ICommand GetDemoteDataMethod(UIElement target)
        {
            if (null == target)
                return null;
            return (ICommand)target.GetValue(DemoteDataMethodProperty);
        }

        public static void SetDemoteDataMethod(UIElement target, ICommand value)
        {
            if (null == target)
                return;
            target.SetValue(DemoteDataMethodProperty, value);
        }
        #endregion
    }
}
