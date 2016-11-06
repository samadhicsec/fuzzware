using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.DateTimeFuzzing
{
    class DateTimeFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public DateTimeFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateDateTimeFuzzers(DataSchemaTypeCode TypeCode, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            switch (TypeCode)
            {
                case DataSchemaTypeCode.DateTime:
                case DataSchemaTypeCode.Date:
                case DataSchemaTypeCode.GYear:
                case DataSchemaTypeCode.GYearMonth:
                case DataSchemaTypeCode.GMonth:
                case DataSchemaTypeCode.GMonthDay:
                case DataSchemaTypeCode.GDay:
                case DataSchemaTypeCode.Time:
                    ITypeFuzzerList.Add(new ReplaceDateTime(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    //ITypeFuzzerList.Add(new AddInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    //ITypeFuzzerList.Add(new SubtractInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    //ITypeFuzzerList.Add(new MultiplyInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    //ITypeFuzzerList.Add(new DivideInteger(oConfigData, oPreComp, oSchemaObject) as ITypeFuzzer);
                    break;
            }
        }
    }
}
