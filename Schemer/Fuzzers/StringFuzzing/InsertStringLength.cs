﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Schemer.Fuzzers.StringFuzzing
{
    class InsertStringLength : StringLengthFuzzerBase
    {
        public InsertStringLength(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {

        }

        protected override void AssignLengthFuzzers(StringLengthFuzzersType FuzzersType)
        {
            if (FuzzersType.InsertStringLength != null)
            {
                Add("InsertStringLength", InsertStringLengthFn);
                Lengths = GetLengthGroup(FuzzersType.InsertStringLength.ValueGroupRef);
                oValueRange = GetValueRange(FuzzersType.InsertStringLength.ValueRangeRef, LengthConfig.LengthRange);
            }
        }

        /// <summary>
        /// inserts into the current value of the node a long string of length specified from the user configured list,
        /// and using the LengthRepetitionString specified in the configuration.
        /// </summary>
        public bool InsertStringLengthFn(int FuzzIndex, int NodeIndex)
        {
            if (null == Lengths)
                return false;

            return InsertStringValue(FuzzIndex, NodeIndex, Lengths, oValueRange);
        }
    }
}
