using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.RandomFuzzing
{
    class RandomFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public RandomFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateRandomFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.Decimal:
                case DataSchemaTypeCode.Float:
                case DataSchemaTypeCode.Double:
                    ITypeFuzzerList.Add(new RandomDecimal(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
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
                    ITypeFuzzerList.Add(new RandomInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
                case DataSchemaTypeCode.Hex:
                case DataSchemaTypeCode.Base64:
                case DataSchemaTypeCode.None:
                    ITypeFuzzerList.Add(new RandomBinary(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new RandomBitFlip(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
            }
        }
    }
}
