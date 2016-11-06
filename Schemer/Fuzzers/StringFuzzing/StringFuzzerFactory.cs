using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.StringFuzzing
{
    class StringFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public StringFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateStringFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.AnyAtomicType:
                case DataSchemaTypeCode.AnyUri:
                case DataSchemaTypeCode.String:
                case DataSchemaTypeCode.NormalizedString:
                case DataSchemaTypeCode.Token:
                case DataSchemaTypeCode.Language:
                case DataSchemaTypeCode.Name:
                case DataSchemaTypeCode.NmToken:
                case DataSchemaTypeCode.NCName:
                case DataSchemaTypeCode.Id:
                case DataSchemaTypeCode.Idref:
                case DataSchemaTypeCode.Entity:
                    ITypeFuzzerList.Add(new StringLength(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new InsertStringLength(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new InsertTotalStringLength(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new ReplaceString(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new InsertString(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new EncodeString(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
            }
        }
    }
}
