using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Schemer.Fuzzers.EnumFuzzing
{
    class EnumFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public EnumFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateEnumFuzzers(TypeRestrictor oTypeRestrictor, ObjectDBEntry oSchemaObject, List<ITypeFuzzer> ITypeFuzzerList)
        {
            ITypeFuzzerList.Add(new EnumFuzzer(oTypeRestrictor, oConfigData, oPreComp) as ITypeFuzzer);
        }
    }
}
