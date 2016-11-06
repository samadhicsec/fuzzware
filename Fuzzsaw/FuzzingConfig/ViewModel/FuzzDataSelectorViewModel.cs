using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw.FuzzingConfig.ViewModel
{
    public class FuzzDataSelectorViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty DataCollectionProperty = DependencyProperty.Register("DataCollection", typeof(ObservableCollection<ObservableString>), typeof(FuzzDataSelectorViewModel));
        /// <summary>
        /// The data in this selector
        /// </summary>
        public ObservableCollection<ObservableString> DataCollection
        {
            get { return (ObservableCollection<ObservableString>)GetValue(DataCollectionProperty); }
            set { SetValue(DataCollectionProperty, value); }
        }

        static readonly DependencyProperty ValidateValuesMethodProperty = DependencyProperty.Register("ValidateValuesMethod", typeof(Func<object, string>), typeof(FuzzDataSelectorViewModel));
        /// <summary>
        /// The method to validate values
        /// </summary>
        public Func<object, string> ValidateValuesMethod
        {
            get { return (Func<object, string>)GetValue(ValidateValuesMethodProperty); }
            set { SetValue(ValidateValuesMethodProperty, value); }
        }

        #endregion

        #region Commands

        #region PreEditData
        RelayCommand<ObservableString> m_oPreEditDataCommand;

        /// <summary>
        /// Add data
        /// </summary>
        public ICommand PreEditDataCommand
        {
            get
            {
                if (null == m_oPreEditDataCommand)
                    m_oPreEditDataCommand = new RelayCommand<ObservableString>(PreEditDataExecute);
                return m_oPreEditDataCommand;
            }
        }

        public void PreEditDataExecute(ObservableString oData)
        {
            // If we are editting the last data value, clear out its value (it will be the 'Add a Value' text)
            if ((null != DataCollection) && (DataCollection.Count > 0) && (DataCollection[DataCollection.Count - 1] == oData))
                oData.Value = "";
        }
        #endregion

        #region AddData
        RelayCommand<ObservableString> m_oAddDataCommand;

        /// <summary>
        /// Add data
        /// </summary>
        public ICommand AddDataCommand
        {
            get
            {
                if (null == m_oAddDataCommand)
                    m_oAddDataCommand = new RelayCommand<ObservableString>(AddDataExecute);
                return m_oAddDataCommand;
            }
        }

        public void AddDataExecute(ObservableString oData)
        {
            // If the last Data value is not what we expect add a new one
            if ((null != DataCollection) && (DataCollection.Count > 0) && !(DataCollection[DataCollection.Count - 1].Value.Equals(ADD_A_VALUE)))
                DataCollection.Add(new ObservableString(ADD_A_VALUE));
        }
        #endregion

        #region DeleteData
        RelayCommand<ObservableString> m_oDeleteDataCommand;

        /// <summary>
        /// Delete data
        /// </summary>
        public ICommand DeleteDataCommand
        {
            get
            {
                if (null == m_oDeleteDataCommand)
                    m_oDeleteDataCommand = new RelayCommand<ObservableString>(DeleteDataExecute);
                return m_oDeleteDataCommand;
            }
        }

        public void DeleteDataExecute(ObservableString oData)
        {
            if ((null != DataCollection) && (null != oData) && !oData.Value.Equals(ADD_A_VALUE))
                DataCollection.Remove(oData);
        }
        #endregion

        #region PromoteData
        RelayCommand<ObservableString> m_oPromoteDataCommand;

        /// <summary>
        /// Promote data
        /// </summary>
        public ICommand PromoteDataCommand
        {
            get
            {
                if (null == m_oPromoteDataCommand)
                    m_oPromoteDataCommand = new RelayCommand<ObservableString>(PromoteDataExecute);
                return m_oPromoteDataCommand;
            }
        }

        public void PromoteDataExecute(ObservableString oData)
        {
            int index = DataCollection.IndexOf(oData);
            // Can promote any Data except for the first and the last
            if ((index != -1) && (index > 0) && (index != DataCollection.Count - 1))
            {
                DataCollection.Move(index, index - 1);
            }
        }
        #endregion

        #region DemoteData
        RelayCommand<ObservableString> m_oDemoteDataCommand;

        /// <summary>
        /// Demote data
        /// </summary>
        public ICommand DemoteDataCommand
        {
            get
            {
                if (null == m_oDemoteDataCommand)
                    m_oDemoteDataCommand = new RelayCommand<ObservableString>(DemoteDataExecute);
                return m_oDemoteDataCommand;
            }
        }

        public void DemoteDataExecute(ObservableString oData)
        {
            int index = DataCollection.IndexOf(oData);
            // Can demote any data except for the last and second to last
            if ((index != -1) && (index < DataCollection.Count - 2))
            {
                DataCollection.Move(index, index + 1);
            }
        }
        #endregion

        #endregion

        protected const string ADD_A_VALUE = "Add a value";

        public FuzzDataSelectorViewModel()
        {
            DataCollection = new ObservableCollection<ObservableString>();
        }
    }
}
