﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Common.Encoding;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    /// <summary>
    /// The base fuzzer config, contains the custom fuzzer node name and some common functions.
    /// </summary>
    public class BaseFuzzerView : DependencyObject
    {
        protected const string ADD_A_VALUE = "Add a value";
        protected bool m_bIsDefault = false;

        public static DependencyProperty IsConfigEnabledProperty;
        public static DependencyProperty CustomNodeNamespaceProperty;
        public static DependencyProperty CustomNodeNameProperty;

        //static protected void RegisterBaseFuzzerConfigProperties(Type oChildType)
        //{
        //    // For some reason I get an odd threading exception when I try to set the Dependency Property with a default value?
        //    //try
        //    //{
        //        //CustomNodeNamespaceProperty = DependencyProperty.Register("CustomNodeNamespace", typeof(ObservableString), oChildType,
        //        //    new FrameworkPropertyMetadata(new ObservableString("")));
        //        CustomNodeNamespaceProperty = DependencyProperty.Register("CustomNodeNamespace", typeof(ObservableString), oChildType);
        //        //CustomNodeNameProperty = DependencyProperty.Register("CustomNodeName", typeof(ObservableString), oChildType,
        //        //    new FrameworkPropertyMetadata(new ObservableString("")));
        //        CustomNodeNameProperty = DependencyProperty.Register("CustomNodeName", typeof(ObservableString), oChildType);
        //    //}
        //    //catch (Exception e) { }
        //}

        static BaseFuzzerView()
        {
            // No matter how many classes use this class as a base class, we only need to register the dependency properties once
            // This is fine since when we register we define the property rather create an instance of it, so all we are doing
            // is making sure we define it only once.
            if (null == IsConfigEnabledProperty)
                IsConfigEnabledProperty = DependencyProperty.Register("IsConfigEnabled", typeof(bool), typeof(BaseFuzzerView), 
                    new FrameworkPropertyMetadata(true));
            if(null == CustomNodeNamespaceProperty)
                CustomNodeNamespaceProperty = DependencyProperty.Register("CustomNodeNamespace", typeof(ObservableString), typeof(BaseFuzzerView));
            if(null == CustomNodeNameProperty)
                CustomNodeNameProperty = DependencyProperty.Register("CustomNodeName", typeof(ObservableString), typeof(BaseFuzzerView));
        }

        public bool IsConfigEnabled
        {
            get { return (bool)GetValue(IsConfigEnabledProperty); }
            set { SetValue(IsConfigEnabledProperty, value); }
        }

        public ObservableString CustomNodeNamespace
        {
            get { return (ObservableString)GetValue(CustomNodeNamespaceProperty); }
            set { SetValue(CustomNodeNamespaceProperty, value); }
        }

        public ObservableString CustomNodeName
        {
            get { return (ObservableString)GetValue(CustomNodeNameProperty); }
            set { SetValue(CustomNodeNameProperty, value); }
        }

        protected BaseFuzzerView() { }

        /// <summary>
        /// Find the range data
        /// </summary>
        protected ValueRange FindRange(String Name, ValueRange[] oValueRanges)
        {
            if (null != oValueRanges)
            {
                for (int i = 0; i < oValueRanges.Length; i++)
                {
                    if (oValueRanges[i].ID.Equals(Name))
                        return oValueRanges[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Copy array data into an ObservableCollection we can bind to
        /// </summary>
        protected void CopyFuzzData(byte[][] Data, ObservableCollection<ObservableString> BindableData)
        {
            HexCoder oHexCoder = new HexCoder();
            if (null != Data)
                for (int i = 0; i < Data.Length; i++)
                {
                    // Covert byte array into string
                    BindableData.Add(new ObservableString(oHexCoder.Encode(Data[i])));
                }
        }

        /// <summary>
        /// Copy array data into an ObservableCollection we can bind to
        /// </summary>
        protected void CopyFuzzData(uint[] Data, ObservableCollection<ObservableString> BindableData)
        {
            if (null != Data)
                for (int i = 0; i < Data.Length; i++)
                    BindableData.Add(new ObservableString(Data[i].ToString()));
        }

        /// <summary>
        /// Copy array data into an ObservableCollection we can bind to
        /// </summary>
        protected void CopyFuzzData(string[] Data, ObservableCollection<ObservableString> BindableData)
        {
            if (null != Data)
                for (int i = 0; i < Data.Length; i++)
                    BindableData.Add(new ObservableString(Data[i]));
        }

        /// <summary>
        /// Add the fuzzing technique to the fuzzer
        /// </summary>
        protected ValueFuzzerType CreateValueFuzzerType(ObservableCollection<ObservableString> oValues,
            String oValueGroupRef,
            RangeDataView oRangeDataView,
            String oValuesRangeRef)
        {
            if (IsConfigEnabled && (oValues.Count > 1))
            {
                ValueFuzzerType oValueFuzzerType = new ValueFuzzerType();
                oValueFuzzerType.ValueGroupRef = oValueGroupRef;
                if ((null != oRangeDataView) && (!oRangeDataView.IsEmpty))
                {
                    oValueFuzzerType.ValueRangeRef = oValuesRangeRef;
                }
                return oValueFuzzerType;
            }
            return null;
        }

        /// <summary>
        /// Add the fuzzing technique to the fuzzer
        /// </summary>
        protected RandomFuzzerType CreateRandomFuzzerType(ObservableString oValue)
        {
            if (IsConfigEnabled && !String.IsNullOrEmpty(oValue.Value))
            {
                uint val = 0;
                if (UInt32.TryParse(oValue.Value, out val))
                {
                    RandomFuzzerType oRandomFuzzerType = new RandomFuzzerType();
                    oRandomFuzzerType.Iterations = val;
                    return oRandomFuzzerType;
                }
            }
            return null;
        }

        /// <summary>
        /// Get a ref name for a default fuzzer
        /// </summary>
        protected String GetRefName(DependencyProperty oProperty)
        {
            if (m_bIsDefault)
                return "default-" + oProperty.Name;
            else
                return "ref-" + Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Convert a ObservableCollection&lt;ObservableString> to a uint[]
        /// </summary>
        protected uint[] GetUIntArray(ObservableCollection<ObservableString> oValues)
        {
            List<uint> oValueList = new List<uint>();
            for (int i = 0; i < oValues.Count - 1; i++)
            {
                uint val = 0;
                if (UInt32.TryParse(oValues[i].Value, out val))
                    oValueList.Add(val);
                else
                    System.Diagnostics.Debug.Write("GetUIntArray: Could not convert '" + oValues[i].Value + "' to uint");
            }
            return oValueList.ToArray();
        }

        /// <summary>
        /// Convert a ObservableCollection&lt;ObservableString> to a string[]
        /// </summary>
        protected string[] GetStringArray(ObservableCollection<ObservableString> oValues)
        {
            List<string> oValueList = new List<string>();
            for (int i = 0; i < oValues.Count - 1; i++)
            {
                oValueList.Add(oValues[i].Value);
            }
            return oValueList.ToArray();
        }

        /// <summary>
        /// Convert a ObservableCollection&lt;ObservableString> to a byte[][]
        /// </summary>
        protected byte[][] GetByteArray(ObservableCollection<ObservableString> oValues)
        {
            HexCoder oHexCoder = new HexCoder();
            List<byte[]> oValueList = new List<byte[]>();
            for (int i = 0; i < oValues.Count - 1; i++)
            {
                byte[] converted = oHexCoder.DecodeToBytes(oValues[i].Value);
                if((null != converted) && converted.Length > 0)
                    oValueList.Add(converted);
            }
            return oValueList.ToArray();
        }

        /// <summary>
        /// Adds a ValueRange to the list
        /// </summary>
        protected void AddValueRange(List<ValueRange> oValueRangeList, ValueFuzzerType oValueFuzzerType, RangeDataView oRangeDataView, String DefaultValueRangeRef)
        {
            ValueRange oRefValueRange = new ValueRange();
            if (((null != oValueFuzzerType) && (String.IsNullOrEmpty(oValueFuzzerType.ValueRangeRef))) || m_bIsDefault)
            {
                ValueRange oValueRange = new ValueRange();
                uint val = 0;
                oValueRange.ID = m_bIsDefault ? DefaultValueRangeRef : oValueFuzzerType.ValueRangeRef;
                if (!String.IsNullOrEmpty(oRangeDataView.StartPosition.Value))
                    if (UInt32.TryParse(oRangeDataView.StartPosition.Value, out val))
                        oValueRange.StartIndex = val;
                if (!String.IsNullOrEmpty(oRangeDataView.StepSize.Value))
                    if (UInt32.TryParse(oRangeDataView.StepSize.Value, out val))
                        oValueRange.StepSize = val;
                if (!String.IsNullOrEmpty(oRangeDataView.Length.Value))
                    if (UInt32.TryParse(oRangeDataView.Length.Value, out val))
                        oValueRange.Length = val;
                // Don't bother adding it if it's the default value
                if ((oValueRange.StartIndex == oRefValueRange.StartIndex) &&
                    (oValueRange.StepSize == oRefValueRange.StepSize) &&
                    (oValueRange.Length == oRefValueRange.Length))
                    return;

                oValueRangeList.Add(oValueRange);
            }
        }

        /// <summary>
        /// Populate a RangeDataView
        /// </summary>
        protected void PopulateRangeData(ValueFuzzerType oFuzzerType, ValueRange[] oValueRanges, RangeDataView oRangeDataView, String DefaultRangeRef)
        {
            String ValueRangeRef = null;
            if (null != oFuzzerType)
            {
                ValueRangeRef = oFuzzerType.ValueRangeRef;
            }
            else if (m_bIsDefault && !IsConfigEnabled)
            {
                ValueRangeRef = DefaultRangeRef;
            }
            ValueRange oValueRange = FindRange(ValueRangeRef, oValueRanges);
            if (null != oValueRange)
            {
                oRangeDataView.StartPosition = new ObservableString(oValueRange.StartIndex.ToString());
                oRangeDataView.StepSize = new ObservableString(oValueRange.StepSize.ToString());
                oRangeDataView.Length = new ObservableString(oValueRange.Length.ToString());
            }
        }
    }
}
