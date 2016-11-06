using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.OccurrenceFuzzing
{
    class OccurrenceFuzzerFactory
    {
        ConfigData oConfigData;
        PreCompData oPreComp;

        public OccurrenceFuzzerFactory(ConfigData ConfigData, PreCompData PreComp)
        {
            oConfigData = ConfigData;
            oPreComp = PreComp;
        }

        public void CreateOccurrenceFuzzers(XMLElementIdentifier oElementId, List<IFuzzer> IFuzzerList)
        {
            IFuzzerList.Add(new OccurrenceFuzzer(oConfigData, oPreComp, oElementId));
            IFuzzerList.Add(new OrderFuzzer(oConfigData, oPreComp, oElementId));
        }

        public void CreateAtrOccurrenceFuzzers(XMLElementIdentifier oElementId, List<IFuzzer> IFuzzerList)
        {
            IFuzzerList.Add(new AtrOppositeOccurrence(oConfigData, oPreComp));
            IFuzzerList.Add(new AtrIsolateOccurrence(oConfigData, oPreComp));
        }

    }
}
