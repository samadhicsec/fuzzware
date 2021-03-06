﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Schemer.Fuzzers.StringFuzzing
{
    class StringLength : StringLengthFuzzerBase
    {
        public StringLength(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {

        }

        protected override void AssignLengthFuzzers(StringLengthFuzzersType FuzzersType)
        {
            if (FuzzersType.StringLength != null)
            {
                Add("StringLength", StringLengthFn);
                Lengths = GetLengthGroup(FuzzersType.StringLength.ValueGroupRef);
            }
        }

        /// <summary>
        /// Replaces the current value of the node with a long string of length specified from the user configured list,
        /// and using the LengthRepetitionString specified in the configuration.
        /// </summary>
        protected bool StringLengthFn(int FuzzIndex, int NodeIndex)
        {
            if (null == Lengths)
                return false;

            return ReplaceStringValue(FuzzIndex, NodeIndex, Lengths);
        }
    }
}
