using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.BinaryFuzzing
{
    class BinaryFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public BinaryFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateBinaryFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.Hex:
                case DataSchemaTypeCode.Base64:
                case DataSchemaTypeCode.None:
                    ITypeFuzzerList.Add(new ReplaceBytes(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new InsertBytes(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new AndBytes(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new OrBytes(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    ITypeFuzzerList.Add(new XOrBytes(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
            }
        }
    }
}
