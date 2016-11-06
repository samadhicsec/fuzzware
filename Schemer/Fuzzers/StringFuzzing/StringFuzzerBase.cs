﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Schemer.AutoGenerated;


namespace Fuzzware.Schemer.Fuzzers.StringFuzzing
{
    abstract class StringFuzzerBase : FuzzerBase, ITypeFuzzer
    {
        protected ObjectDBEntry oSchemaElement;
        protected int MaxFuzzIndex;

        public StringFuzzerBase(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData)
        {
            this.oSchemaElement = oSchemaElement;
        }

        /// <summary>
        /// Implement to create the required string
        /// </summary>
        protected abstract String StringCreateFn(String OriginalValue, Array Source, int Index);

        #region ITypeFuzzer Members
        public override void Initialise()
        {
            base.Initialise();
            MaxFuzzIndex = CountOfCurrentNodes;
        }
        #endregion

        protected bool ReplaceStringValue(int FuzzIndex, int NodeIndex, Array ReplacementValues)
        {
            if (FuzzIndex >= ReplacementValues.Length)
                return false;

            String temp = StringCreateFn(null, ReplacementValues, FuzzIndex);
            ChangeToValue(temp, NodeIndex);
            return true;
        }

        protected bool InsertStringValue(int FuzzIndex, int NodeIndex, Array InsertValues, ValueRange Range)
        {
            if (null == InsertValues)
                return false;
            if (0 == InsertValues.Length)
                return false;

            // We avoid changing all the values, as they will be of different lengths, we can't insert in some of them
            if (NodeIndex >= 0)
            {
                // Get the current value
                String CurrentValue = oValuesEditor.GetValue(NodeIndex);

                if (MaxFuzzIndex < (CurrentValue.Length + 1) * InsertValues.Length)
                    MaxFuzzIndex = (CurrentValue.Length + 1) * InsertValues.Length;

                if (FuzzIndex >= MaxFuzzIndex)
                {
                    // Reset, for other fuzzers that will be used without Fuzzer.Initialise being called
                    MaxFuzzIndex = CountOfCurrentNodes;
                    return false;
                }

                // Calulcate the start position
                int startpos = FuzzIndex / InsertValues.Length;

                // Make sure we have got something to edit
                //if (FuzzIndex >= (CurrentValue.Length + 1) * InsertValues.Length)
                //    throw new SkipStateOutOfRangeException();
                if(startpos >= (CurrentValue.Length + 1))
                    throw new SkipStateOutOfRangeException();

                // Get the config insertion value
                String InsertString = StringCreateFn(CurrentValue, InsertValues, (FuzzIndex % InsertValues.Length));

                // Check we are in the right range
                if (startpos < Range.StartIndex)
                    throw new SkipStateOutOfRangeException();
                if ((Range.Length != 0) && (startpos >= Range.StartIndex + Range.Length))
                    return false;   // Once our start position is beyond our range, we have finished this fuzzer.
                if ((startpos - Range.StartIndex) % Range.StepSize != 0)
                    throw new SkipStateOutOfRangeException();

                // Create the new value
                StringBuilder NewValue = new StringBuilder();
                NewValue.Append(CurrentValue, 0, startpos);
                NewValue.Append(InsertString);
                if (startpos < CurrentValue.Length)
                    NewValue.Append(CurrentValue, startpos, CurrentValue.Length - startpos);

                ChangeToValue(NewValue.ToString(), NodeIndex);
            }
            else
                throw new SkipStateNoAllCaseException();

            return true;
        }
    }
}
