﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    public class DecimalFuzzerView : BaseFuzzerView
    {
        #region Bindable Objects Setup

        // Decimal value fuzzers data
        public static DependencyProperty ReplaceDecimalsFuzzDataProperty;
        public static DependencyProperty RandomDecimalsFuzzDataProperty;
        public static DependencyProperty AddIntegersFuzzDataProperty;
        public static DependencyProperty SubtractIntegersFuzzDataProperty;
        public static DependencyProperty MultiplyIntegersFuzzDataProperty;
        public static DependencyProperty DivideIntegersFuzzDataProperty;

        static DecimalFuzzerView()
        {
            ReplaceDecimalsFuzzDataProperty = DependencyProperty.Register("ReplaceDecimalsFuzzData", typeof(ObservableCollection<ObservableString>), typeof(DecimalFuzzerView),
                new FrameworkPropertyMetadata(new ObservableCollection<ObservableString>()));
            RandomDecimalsFuzzDataProperty = DependencyProperty.Register("RandomDecimalsFuzzData", typeof(ObservableString), typeof(DecimalFuzzerView));
        }

        public ObservableCollection<ObservableString> ReplaceDecimalsFuzzData
        {
            get { return (ObservableCollection<ObservableString>)GetValue(ReplaceDecimalsFuzzDataProperty); }
            set { SetValue(ReplaceDecimalsFuzzDataProperty, value); }
        }

        public ObservableString RandomDecimalsFuzzData
        {
            get { return (ObservableString)GetValue(RandomDecimalsFuzzDataProperty); }
            set { SetValue(RandomDecimalsFuzzDataProperty, value); }
        }

        #endregion

        protected void PopulateFuzzData(SimpleTypeFuzzerConfig oSTFC, DecimalValueFuzzersType oDVFT)
        {
            // Always call populate so we add the final value to the collections that allows us to add new values
            PopulateValueFuzzData((null != oDVFT) ? oDVFT.ReplaceDecimal : null, oSTFC.DecimalValueFuzzer, ReplaceDecimalsFuzzData, GetRefName(ReplaceDecimalsFuzzDataProperty));
            if ((null != oDVFT) && (null != oDVFT.RandomDecimal))
                RandomDecimalsFuzzData = new ObservableString(oDVFT.RandomDecimal.Iterations.ToString());
        }

        /// <summary>
        /// The constructor for the default string fuzzer
        /// </summary>
        public DecimalFuzzerView(SimpleTypeFuzzerConfig oSTFC, DecimalValueFuzzersType oDVFT)
        {
            InitProperties();

            m_bIsDefault = true;
            IsConfigEnabled = true;

            // For the default fuzzer, if there are no fuzzers configured, then disable config
            if((null != oDVFT) &&
                (null == oDVFT.ReplaceDecimal) &&
                (null == oDVFT.RandomDecimal))
                IsConfigEnabled = false;

            PopulateFuzzData(oSTFC, oDVFT);
        }

        /// <summary>
        /// The constructor for the custom string fuzzer
        /// </summary>
        public DecimalFuzzerView(SimpleTypeFuzzerConfig oSTFC, DecimalValueFuzzerCustomFuzzer oDVFCF)
            : this(oSTFC, oDVFCF as DecimalValueFuzzersType)
        {
            InitProperties();

            IsConfigEnabled = true;

            PopulateFuzzData(oSTFC, oDVFCF as DecimalValueFuzzersType);

            if (null != oDVFCF)
            {
                CustomNodeNamespace = new ObservableString(oDVFCF.NodeNamespace);
                CustomNodeName = new ObservableString(oDVFCF.NodeName);
            }
        }

        /// <summary>
        /// I shouldn't need to do this, but when I don't I get weird behaviour, the properties contain the values
        /// of the last object of this type I populated.
        /// </summary>
        private void InitProperties()
        {
            ReplaceDecimalsFuzzData = new ObservableCollection<ObservableString>();
            RandomDecimalsFuzzData = new ObservableString("");
        }

        private void PopulateValueFuzzData(ValueFuzzerType oFuzzerType, DecimalValueFuzzer oDVF, ObservableCollection<ObservableString> BindableData, String DefaultGroupRef)
        {
            String ValueGroupRef = null;
            if ((null != oFuzzerType) && (null != oDVF))
            {
                ValueGroupRef = oFuzzerType.ValueGroupRef;    
            }
            else if(m_bIsDefault && !IsConfigEnabled && (null != oDVF))
            {
                ValueGroupRef = DefaultGroupRef;
            }
            if(!String.IsNullOrEmpty(ValueGroupRef))
            {
                DecimalValueFuzzerDecimalGroup oDVFDG = FindStringValueGroup(ValueGroupRef, oDVF.DecimalGroup);
                if (null != oDVFDG)
                    CopyFuzzData(oDVFDG.DecimalValue, BindableData);
            }
            // Always make sure the last item is the option to add a new value
            BindableData.Add(new ObservableString(ADD_A_VALUE));
        }

        /// <summary>
        /// Find the string value fuzz data
        /// </summary>
        private DecimalValueFuzzerDecimalGroup FindStringValueGroup(String Name, DecimalValueFuzzerDecimalGroup[] oDVFDG)
        {
            if (null != oDVFDG)
            {
                for (int i = 0; i < oDVFDG.Length; i++)
                {
                    if (oDVFDG[i].ID.Equals(Name))
                        return oDVFDG[i];
                }
            }
            return null;
        }

        /// <summary>
        /// The save the current Decimal types view to its original data source
        /// </summary>
        public void SaveDecimalTypeConfig(SimpleTypeFuzzerConfig oSTFC)
        {
            if (null == oSTFC.DecimalValueFuzzer)
                oSTFC.DecimalValueFuzzer = new DecimalValueFuzzer();

            // Save the default string fuzzers
            DecimalValueFuzzer oDVF = oSTFC.DecimalValueFuzzer;
            // Create the string length fuzzers
            DecimalValueFuzzersType oDVFT = CreateDecimalValueFuzzersType();
            oDVFT.ReplaceDecimal = CreateValueFuzzerType(ReplaceDecimalsFuzzData, GetRefName(ReplaceDecimalsFuzzDataProperty), null, null);
            oDVFT.RandomDecimal = CreateRandomFuzzerType(RandomDecimalsFuzzData);
            // Add the fuzzers as either the default or a custom
            if (m_bIsDefault)
                oDVF.DefaultFuzzers = oDVFT;
            else
            {
                List<DecimalValueFuzzerCustomFuzzer> oIVFCFList = new List<DecimalValueFuzzerCustomFuzzer>(oDVF.CustomFuzzer);
                oIVFCFList.Add(oDVFT as DecimalValueFuzzerCustomFuzzer);
                oDVF.CustomFuzzer = oIVFCFList.ToArray();
            }
            // Create the string value fuzzer value groups
            List<DecimalValueFuzzerDecimalGroup> oDecimalGroups = new List<DecimalValueFuzzerDecimalGroup>(oDVF.DecimalGroup);
            DecimalValueFuzzerDecimalGroup oIVFIG = new DecimalValueFuzzerDecimalGroup();
            // Add the ValueGroups, always add the default ones, even if no fuzzers were being used
            if ((null != oDVFT.ReplaceDecimal) || m_bIsDefault)
            {
                oIVFIG.ID = m_bIsDefault ? GetRefName(ReplaceDecimalsFuzzDataProperty) : oDVFT.ReplaceDecimal.ValueGroupRef;
                oIVFIG.DecimalValue = GetStringArray(ReplaceDecimalsFuzzData);
                oDecimalGroups.Add(oIVFIG);
            }
            oDVF.DecimalGroup = oDecimalGroups.ToArray();
        }

        /// <summary>
        /// If default return a DecimalValueFuzzersType, if not default return DecimalValueFuzzerCustomFuzzer and populate
        /// node name and namespace.  If not default and no name and namespace, return null.
        /// </summary>
        protected DecimalValueFuzzersType CreateDecimalValueFuzzersType()
        {
            if (m_bIsDefault)
                return new DecimalValueFuzzersType();
            else
            {
                if (String.IsNullOrEmpty(CustomNodeName.Value) && String.IsNullOrEmpty(CustomNodeNamespace.Value))
                    return null;

                DecimalValueFuzzerCustomFuzzer oDVFCF = new DecimalValueFuzzerCustomFuzzer();
                oDVFCF.NodeName = CustomNodeName.Value;
                oDVFCF.NodeNamespace = CustomNodeNamespace.Value;
                return oDVFCF as DecimalValueFuzzersType;
            }
        }
    }
}
