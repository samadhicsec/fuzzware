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
    /// This fuzzer will divide the values provided by the configuration files to the current value of the integer.
    /// </summary>
    class DivideInteger : AdjustIntegerBase
    {
        public DivideInteger(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {
        }

        protected override string GetFuzzerName()
        {
            return GetType().Name;
        }

        protected override void AssignValueFuzzers(IntegerValueFuzzersType FuzzersType)
        {
            if (FuzzersType.DivideInteger != null)
            {
                Add(GetFuzzerName(), AdjustInteger);
                ConfigDefinedValues = GetValueGroup(FuzzersType.DivideInteger.ValueGroupRef);
            }
        }

        /// <summary>
        /// Divide the passed in Value by a value from the configuration array.  The value from the configuration array
        /// must fit in an Int64.
        /// </summary>
        protected override long OperateOn(int FuzzIndex, long Value)
        {
            Int64 iOperand = 0;
            if (Int64.TryParse(ConfigDefinedValues[FuzzIndex], out iOperand))
            {
                if (iOperand == 0)
                    throw new SkipStateException();
                return Value / iOperand;
            }
            // If the configured operand cannot be made to an Int64, then it cannot be divided to an Int64.
            throw new SkipStateException();
        }

        /// <summary>
        /// Divide the passed in Value by a value from the configuration array.  The value from the configuration array
        /// must be an unsigned value.  Overflows can occur in the multiplication.
        /// </summary>
        protected override ulong OperateOn(int FuzzIndex, ulong Value)
        {
            UInt64 uiOperand = 0;
            if (UInt64.TryParse(ConfigDefinedValues[FuzzIndex], out uiOperand))
            {
                if (uiOperand == 0)
                    throw new SkipStateException();
                return Value / uiOperand;
            }
            throw new SkipStateException();
        }
    }
}
