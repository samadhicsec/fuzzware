using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.NumberFuzzing
{
    class NumberFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public NumberFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateNumberFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.Decimal:
                case DataSchemaTypeCode.Float:
                case DataSchemaTypeCode.Double:
                    ITypeFuzzerList.Add(new ReplaceDecimal(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
                case DataSchemaTypeCode.Integer:
                case DataSchemaTypeCode.NonPositiveInteger:
                case DataSchemaTypeCode.NegativeInteger:
                case DataSchemaTypeCode.NonNegativeInteger:
                case DataSchemaTypeCode.PositiveInteger:
                case DataSchemaTypeCode.Long:
                case DataSchemaTypeCode.Int:
                case DataSchemaTypeCode.Short:
                case DataSchemaTypeCode.Byte:
                case DataSchemaTypeCode.UnsignedLong:
                case DataSchemaTypeCode.UnsignedInt:
                case DataSchemaTypeCode.UnsignedShort:
                case DataSchemaTypeCode.UnsignedByte:
                    ITypeFuzzerList.Add(new ReplaceInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new AddInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new SubtractInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new MultiplyInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new DivideInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
            }
        }
    }
}
