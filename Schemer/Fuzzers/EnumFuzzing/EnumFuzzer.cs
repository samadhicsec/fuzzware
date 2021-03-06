﻿using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Schemer.Fuzzers.EnumFuzzing
{
    class EnumFuzzer : FuzzerBase, ITypeFuzzer
    {
        public EnumFuzzer(TypeRestrictor oTypeRestrictor, ConfigData oConfigData, PreCompData oPreCompData)
            : base(oConfigData, oPreCompData)
        {
            Add(GetType().Name, FuzzConfigDefinedValues);

            ConfigDefinedValues = oTypeRestrictor.GetEnumerationValues();
        }
    }
}
