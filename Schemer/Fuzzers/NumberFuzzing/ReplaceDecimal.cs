using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Fuzzers.NumberFuzzing
{
    class ReplaceDecimal : DecimalFuzzerBase
    {
        public ReplaceDecimal(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData, oSchemaElement)
        {
        }

        protected override string GetFuzzerName()
        {
            return GetType().Name;
        }

    }
}
