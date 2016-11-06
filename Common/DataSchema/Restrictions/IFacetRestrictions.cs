using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Fuzzware.Common.DataSchema.Restrictions
{
    public interface IFacetRestrictions
    {
        void SetExactLength(String Length);
        void SetMinLength(String Length);
        void SetMaxLength(String Length);

        void SetPattern(String RegularExpression);
        Regex GetPattern();
        void SetEnumerationValue(String EnumVal);

        void SetMinInclusiveValue(String Val);
        void SetMinExclusiveValue(String Val);
        void SetMaxInclusiveValue(String Val);
        void SetMaxExclusiveValue(String Val);

        void SetMaxFracDigits(String MaxFracDigits);
        void SetMaxTotalDigits(String MaxTotalDigits);

        // This seems irrelevant
        //void SetWhitespace(string Setting);
    }
}
