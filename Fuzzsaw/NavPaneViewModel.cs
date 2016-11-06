using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw
{
    public class NavPaneViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty EnabledProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not the NavPane is currently enabled
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        static readonly DependencyProperty InputSelectionSelectedProperty = DependencyProperty.Register("InputSelectionSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Input Selection is selected
        /// </summary>
        public bool InputSelectionSelected
        {
            get { return (bool)GetValue(InputSelectionSelectedProperty); }
            set { SetValue(InputSelectionSelectedProperty, value); }
        }

        static readonly DependencyProperty InputSourceSelectedProperty = DependencyProperty.Register("InputSourceSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Input Source is selected
        /// </summary>
        public bool InputSourceSelected
        {
            get { return (bool)GetValue(InputSourceSelectedProperty); }
            set { SetValue(InputSourceSelectedProperty, value); }
        }

        static readonly DependencyProperty DataTypesConfigSelectedProperty = DependencyProperty.Register("DataTypesConfigSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Data Types Config is selected
        /// </summary>
        public bool DataTypesConfigSelected
        {
            get { return (bool)GetValue(DataTypesConfigSelectedProperty); }
            set { SetValue(DataTypesConfigSelectedProperty, value); }
        }

        static readonly DependencyProperty DataStructuresConfigSelectedProperty = DependencyProperty.Register("DataStructuresConfigSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Data Structures Config is selected
        /// </summary>
        public bool DataStructuresConfigSelected
        {
            get { return (bool)GetValue(DataStructuresConfigSelectedProperty); }
            set { SetValue(DataStructuresConfigSelectedProperty, value); }
        }

        static readonly DependencyProperty OutputSelectionSelectedProperty = DependencyProperty.Register("OutputSelectionSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Output Selection is selected
        /// </summary>
        public bool OutputSelectionSelected
        {
            get { return (bool)GetValue(OutputSelectionSelectedProperty); }
            set { SetValue(OutputSelectionSelectedProperty, value); }
        }

        static readonly DependencyProperty OutputDestinationSelectedProperty = DependencyProperty.Register("OutputDestinationSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Output Destination is selected
        /// </summary>
        public bool OutputDestinationSelected
        {
            get { return (bool)GetValue(OutputDestinationSelectedProperty); }
            set { SetValue(OutputDestinationSelectedProperty, value); }
        }

        static readonly DependencyProperty MonitoringSelectedProperty = DependencyProperty.Register("MonitoringSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Monitoring is selected
        /// </summary>
        public bool MonitoringSelected
        {
            get { return (bool)GetValue(MonitoringSelectedProperty); }
            set { SetValue(MonitoringSelectedProperty, value); }
        }

        static readonly DependencyProperty ExecutionSelectedProperty = DependencyProperty.Register("ExecutionSelected", typeof(bool), typeof(NavPaneViewModel));
        /// <summary>
        /// Whether or not Execution is selected
        /// </summary>
        public bool ExecutionSelected
        {
            get { return (bool)GetValue(ExecutionSelectedProperty); }
            set { SetValue(ExecutionSelectedProperty, value); }
        }

        #endregion

        /// <summary>
        /// Deselect all the Nav Pane buttons
        /// </summary>
        public void DeselectAll()
        {
            InputSelectionSelected = false;
            InputSourceSelected = false;
            DataTypesConfigSelected = false;
            DataStructuresConfigSelected = false;
            OutputSelectionSelected = false;
            OutputDestinationSelected = false;
            MonitoringSelected = false;
            ExecutionSelected = false;
        }

    }
}
