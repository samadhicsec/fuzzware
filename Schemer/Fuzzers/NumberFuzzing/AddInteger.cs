﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Schemer.AutoGenerated;
using Fuzzware.Common;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Fuzzers.NumberFuzzing
{
    /// <summary>
    /// This fuzzer will add the values provided by the configuration files to the current value of the integer.
    /// Overflows in the addition can occur.
    /// </summary>
    class AddInteger : AdjustIntegerBase
    {
        public AddInteger(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {
        }

        protected override string GetFuzzerName()
        {
            return GetType().Name;
        }

        protected override void AssignValueFuzzers(IntegerValueFuzzersType FuzzersType)
        {
            if (FuzzersType.AddInteger != null)
            {
                Add(GetFuzzerName(), AdjustInteger);
                ConfigDefinedValues = GetValueGroup(FuzzersType.AddInteger.ValueGroupRef);
            }
        }

        /// <summary>
        /// Adds a value from the configuration array to the passed in Value.  The value from the configuration array
        /// must fit in an Int64.  Overflows can occur in the addition.
        /// </summary>
        protected override long OperateOn(int FuzzIndex, long Value)
        {
            Int64 iAddend = 0;
            if (Int64.TryParse(ConfigDefinedValues[FuzzIndex], out iAddend))
            {
                return Value + iAddend;
            }
            // If the configured addend cannot be made to an Int64, then it cannot be added to an Int64.
            throw new SkipStateException();
        }

        /// <summary>
        /// Adds a value from the configuration array to the passed in Value.  The value from the configuration array
        /// must be >= Int64.MinValue and &lt;= UInt64.MaxValue.  Overflows can occur in the addition.
        /// </summary>
        protected override ulong OperateOn(int FuzzIndex, ulong Value)
        {
            Int64 iAddend = 0;
            if (Int64.TryParse(ConfigDefinedValues[FuzzIndex], out iAddend))
            {
                if (iAddend > 0)
                    return Value + (UInt64)iAddend;
                else
                    return Value - (UInt64)(-1 * iAddend);
            }
            UInt64 uiAddend = 0;
            if (UInt64.TryParse(ConfigDefinedValues[FuzzIndex], out uiAddend))
            {
                return Value + uiAddend;
            }
            // If the configured addend cannot be made to an UInt64, then it cannot be added to an UInt64.
            throw new SkipStateException();            
        }
    }
}
