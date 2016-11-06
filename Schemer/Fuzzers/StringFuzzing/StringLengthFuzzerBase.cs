﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Schemer.Fuzzers.StringFuzzing
{
    abstract class StringLengthFuzzerBase : StringFuzzerBase
    {
        protected StringLengthFuzzer LengthConfig;

        // Length Fuzzing
        protected string LengthRepetitionString = "";
        protected uint[] Lengths;
        protected ValueRange oValueRange;
        static protected StringBuilder LongString;
        
        public StringLengthFuzzerBase(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {
            bool UsingCustomFuzzer = false;
            LengthConfig = oConfigData.STFConfig.StringLengthFuzzer;

            if (null != LengthConfig)
            {
                UsingCustomFuzzer = false;
                // Check if the user has assigned custom fuzzers for this node
                if (LengthConfig.CustomFuzzer != null)
                    for (int i = 0; i < LengthConfig.CustomFuzzer.Length; i++)
                    {
                        if (LengthConfig.CustomFuzzer[i].NodeNamespace.Equals(oSchemaElement.Name.Namespace, StringComparison.CurrentCultureIgnoreCase) &&
                            LengthConfig.CustomFuzzer[i].NodeName.Equals(oSchemaElement.Name.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AssignLengthFuzzers(LengthConfig.CustomFuzzer[i]);
                            UsingCustomFuzzer = true;
                            Log.Write(MethodBase.GetCurrentMethod(), "Custom StringLength fuzzer being used", Log.LogType.LogOnlyInfo);
                            break;
                        }
                    }
                // Assign the default fuzzers if there is no custom one
                if (!UsingCustomFuzzer && (LengthConfig.DefaultFuzzers != null))
                    AssignLengthFuzzers(LengthConfig.DefaultFuzzers);

                LengthRepetitionString = LengthConfig.LengthRepetitionString;
            }
            ConfigDefinedValues = null;

            // Find the maximum string length
            uint maxlength = 0;
            if(Lengths != null)
                for (int i = 0; i < Lengths.Length; i++)
                    if (maxlength < Lengths[i])
                        maxlength = Lengths[i];
            // Make the string builder allocate enough memory, this avoids lots of memory allocations when creating long strings
            if(null == LongString)
                LongString = new StringBuilder();
            LongString.EnsureCapacity((int)(maxlength * LengthRepetitionString.Length));
        }

        /// <summary>
        /// Implement to assign the fuzzer function
        /// </summary>
        protected abstract void AssignLengthFuzzers(StringLengthFuzzersType FuzzersType);

        protected uint[] GetLengthGroup(String ID)
        {
            for (int i = 0; i < LengthConfig.LengthGroup.Length; i++)
            {
                if (ID == LengthConfig.LengthGroup[i].ID)
                    return LengthConfig.LengthGroup[i].StringLength;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find a LengthGroup with ID='" + ID + "'.  The associated StringValueFuzzer will be skipped.", Log.LogType.Warning);
            return null;
        }

        /// <summary>
        /// Creates a long string.
        /// </summary>
        /// <param name="OriginalValue">Not used</param>
        /// <param name="Source">Array of StringLengths</param>
        /// <param name="Index">Index into array of StringLengths</param>
        /// <returns>A long string</returns>
        protected override String StringCreateFn(String OriginalValue, Array Source, int Index)
        {
            uint[] StringLengths = (uint[])Source;

            int size = (int)StringLengths[Index];

            // Check if we already have the correct length
            if (LongString.Length == (size * LengthRepetitionString.Length))
                return LongString.ToString();

            // This should be fast if XPathNavs read in are in order
            //  - Append as long a string as possible, get it from the current long string
            LongString.EnsureCapacity(size * LengthRepetitionString.Length);
            if (LongString.Length < size * LengthRepetitionString.Length)
            {
                // Make sure LongString has at least 1 char
                if (0 == LongString.Length)
                    LongString.Append(LengthRepetitionString);

                // Double up to quickly add length
                while (LongString.Length < ((size * LengthRepetitionString.Length) / 2) + 1)
                    LongString.Append(LongString.ToString());

                // If we are within some threshold, then just sequentially add, otherwise double and remove.
                //if (((2 * LongString.Length) - (size * LengthRepetitionString.Length)) > ((size * LengthRepetitionString.Length) - LongString.Length))
                if (((size * LengthRepetitionString.Length) - LongString.Length) < 20)
                {
                    // Fill up to the required length
                    while (LongString.Length < (size * LengthRepetitionString.Length))
                        LongString.Append(LengthRepetitionString);
                }
                else
                {
                    // Double and remove
                    LongString.Append(LongString.ToString());
                    LongString.Remove(size * LengthRepetitionString.Length, LongString.Length - (size * LengthRepetitionString.Length));
                }
            }
            else
            {
                LongString.Remove(size * LengthRepetitionString.Length, LongString.Length - (size * LengthRepetitionString.Length));
            }
            return LongString.ToString();
        }

    }
}
