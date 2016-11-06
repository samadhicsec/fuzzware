using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Schemer.AutoGenerated;
using Fuzzware.Schemer.Editors;
using Fuzzware.Common;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Fuzzers
{
    class DecimalFuzzer : FuzzerBase, ITypeFuzzer
    {
        protected DecimalValueFuzzer ValueConfig;

        public DecimalFuzzer(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData)
        {
            ConfigDefinedValues = null;
            ValueConfig = oConfigData.STFConfig.DecimalValueFuzzer;

            bool UsingCustomFuzzer = false;
            if (null != ValueConfig)
            {
                // Check if the user has assigned custom fuzzers for this node
                if (ValueConfig.CustomFuzzer != null)
                    for (int i = 0; i < ValueConfig.CustomFuzzer.Length; i++)
                    {
                        if (ValueConfig.CustomFuzzer[i].NodeNamespace.Equals(oSchemaElement.Name.Namespace, StringComparison.CurrentCultureIgnoreCase) &&
                            ValueConfig.CustomFuzzer[i].NodeName.Equals(oSchemaElement.Name.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AssignValueFuzzers(ValueConfig.CustomFuzzer[i]);
                            UsingCustomFuzzer = true;
                            Log.Write(MethodBase.GetCurrentMethod(), "Custom DecimalValue fuzzer being used", Log.LogType.LogOnlyInfo);
                            break;
                        }
                    }
                // Assign the default fuzzers if there is no custom one
                if (!UsingCustomFuzzer && (ValueConfig.DefaultFuzzers != null))
                    AssignValueFuzzers(ValueConfig.DefaultFuzzers);
            }
        }

        private void AssignValueFuzzers(DecimalValueFuzzersType FuzzersType)
        {
            if (FuzzersType.ReplaceDecimal != null)
            {
                Add("ReplaceDecimal", FuzzConfigDefinedValues);
                ConfigDefinedValues = GetValueGroup(FuzzersType.ReplaceDecimal.ValueGroupRef);
            }
        }

        private string[] GetValueGroup(String ID)
        {
            for (int i = 0; i < ValueConfig.DecimalGroup.Length; i++)
            {
                if (ID == ValueConfig.DecimalGroup[i].ID)
                    return ValueConfig.DecimalGroup[i].DecimalValue;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find a ValueGroup with ID='" + ID + "'.  The associated DecimalValueFuzzer will be skipped.", Log.LogType.Warning);
            return null;
        }
    }

    class IntegerFuzzer : FuzzerBase, ITypeFuzzer
    {
        protected IntegerValueFuzzer ValueConfig;

        public IntegerFuzzer(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData)
        {
            ConfigDefinedValues = null;
            ValueConfig = oConfigData.STFConfig.IntegerValueFuzzer;

            bool UsingCustomFuzzer = false;
            // Check if the user has assigned custom fuzzers for this node
            if (ValueConfig.CustomFuzzer != null)
                for (int i = 0; i < ValueConfig.CustomFuzzer.Length; i++)
                {
                    if (ValueConfig.CustomFuzzer[i].NodeNamespace.Equals(oSchemaElement.Name.Namespace, StringComparison.CurrentCultureIgnoreCase) &&
                        ValueConfig.CustomFuzzer[i].NodeName.Equals(oSchemaElement.Name.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        AssignValueFuzzers(ValueConfig.CustomFuzzer[i]);
                        UsingCustomFuzzer = true;
                        Log.Write(MethodBase.GetCurrentMethod(), "Custom IntegerValue fuzzer being used", Log.LogType.LogOnlyInfo);
                        break;
                    }
                }
            // Assign the default fuzzers if there is no custom one
            if (!UsingCustomFuzzer && (ValueConfig.DefaultFuzzers != null))
                AssignValueFuzzers(ValueConfig.DefaultFuzzers);
        }

        private void AssignValueFuzzers(IntegerValueFuzzersType FuzzersType)
        {
            if (FuzzersType.ReplaceInteger != null)
            {
                Add("ReplaceInteger", FuzzConfigDefinedValues);
                ConfigDefinedValues = GetValueGroup(FuzzersType.ReplaceInteger.ValueGroupRef);
            }
        }

        private string[] GetValueGroup(String ID)
        {
            for (int i = 0; i < ValueConfig.IntegerGroup.Length; i++)
            {
                if (ID == ValueConfig.IntegerGroup[i].ID)
                    return ValueConfig.IntegerGroup[i].IntegerValue;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find a ValueGroup with ID='" + ID + "'.  The associated IntegerValueFuzzer will be skipped.", Log.LogType.Warning);
            return null;
        }
    }
}
