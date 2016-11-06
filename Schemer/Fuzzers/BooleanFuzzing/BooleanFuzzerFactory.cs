using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.BooleanFuzzing
{
    class BooleanFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public BooleanFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateBooleanFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.Boolean:
                    ITypeFuzzerList.Add(new BooleanFuzzer(oConfigData, oPreComp) as ITypeFuzzer);
                    break;
            }
        }
    }
}
