using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Common.DataSchema
{
    public enum DataSchemaTypeCode
    {
        AnyAtomicType,
        String,
        NormalizedString,
        Token,
        Language,
        Name,
        NmToken,
        NCName,
        Id,
        Idref,
        Entity,
        Date,
        Time,
        DateTime,
        Duration,
        GDay,
        GMonth,
        GMonthDay,
        GYear,
        GYearMonth,
        Decimal,
        Float,
        Double,
        Integer,
        NonPositiveInteger,
        NegativeInteger,
        NonNegativeInteger,
        PositiveInteger,
        Long,
        Int,
        Short,
        Byte,
        UnsignedLong,
        UnsignedInt,
        UnsignedShort,
        UnsignedByte,
        Boolean,
        Base64,
        MSBase64,
        Hex,
        AnyUri,
        None
    }
}