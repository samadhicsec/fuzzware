using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Fuzzware.Common.DataSchema.Restrictions
{
    /// <summary>
    /// [Definition:]   dateTime values may be viewed as objects with integer-valued year, month, day, hour and minute properties, a 
    /// decimal-valued second property, and a boolean timezoned property. Each such object also has one decimal-valued method or computed 
    /// property, timeOnTimeline, whose value is always a decimal number; the values are dimensioned in seconds, the integer 0 is 
    /// 0001-01-01T00:00:00 and the value of timeOnTimeline for other dateTime values is computed using the Gregorian algorithm as modified 
    /// for leap-seconds. The timeOnTimeline values form two related "timelines", one for timezoned values and one for non-timezoned values. 
    /// Each timeline is a copy of the ·value space· of decimal, with integers given units of seconds. 
    /// 
    /// dateTime has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class DateTimeRestrictor : AnySimpleTypeRestrictor
    {
        DateTime MaxInclusiveDataTime;
        bool bMaxInclusiveDataTimeSet = false;
        DateTime MaxExclusiveDataTime;
        bool bMaxExclusiveDataTimeSet = false;
        DateTime MinInclusiveDataTime;
        bool bMinInclusiveDataTimeSet = false;
        DateTime MinExclusiveDataTime;
        bool bMinExclusiveDataTimeSet = false;

        #region Facet Restrictions Not Supported
        public override void SetExactLength(String Length)
        {
            throw new Exception("Length facet restriction not supported.");
        }

        public override void SetMinLength(String Length)
        {
            throw new Exception("Minimum Length facet restriction not supported.");
        }

        public override void SetMaxLength(String Length)
        {
            throw new Exception("Maximum Length facet restriction not supported.");
        }

        public override void SetMaxFracDigits(string MaxFracDigits)
        {
            throw new Exception("Maximum Fractional Digits facet restriction not supported.");
        }

        public override void SetMaxTotalDigits(string MaxTotalDigits)
        {
            throw new Exception("Maximum Total Digits facet restriction not supported.");
        }
        #endregion

        public override bool Validate(object o)
        {
            String str;

            if (null == o)
                return false;

            // Object should be a string
            try
            {
                str = o as String;
            }
            catch (InvalidCastException)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not cast input object to String", Log.LogType.Error);
                return false;
            }
            catch (Exception e)
            {
                Log.Write(e);
                return false;
            };

            if (String.IsNullOrEmpty(str))
                return false;

            DateTime oDateTime;
            if (!DateTime.TryParse(str, out oDateTime))
                return false;

            if (!CheckMaxInclusive(oDateTime))
                return false;

            if (!CheckMinInclusive(oDateTime))
                return false;

            if (!CheckMaxExclusive(oDateTime))
                return false;

            if (!CheckMinExclusive(oDateTime))
                return false;

            if (!CheckPattern(str))
                return false;

            return true;
        }

        protected virtual bool CheckMaxInclusive(DateTime oDateTime)
        {
            if (String.IsNullOrEmpty(MaxInclusiveStrValue))
                return true;

            if (!bMaxInclusiveDataTimeSet)
                if (!DateTime.TryParse(MaxInclusiveStrValue, out MaxInclusiveDataTime))
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MaxInclusiveStrValue + "' to DateTime", Log.LogType.Error);

            if (oDateTime <= MaxInclusiveDataTime)
                return true;

            return false;
        }

        protected virtual bool CheckMinInclusive(DateTime oDateTime)
        {
            if (String.IsNullOrEmpty(MinInclusiveStrValue))
                return true;

            if (!bMinInclusiveDataTimeSet)
                if (!DateTime.TryParse(MinInclusiveStrValue, out MinInclusiveDataTime))
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MinInclusiveStrValue + "' to DateTime", Log.LogType.Error);

            if (oDateTime >= MinInclusiveDataTime)
                return true;

            return false;
        }

        protected virtual bool CheckMaxExclusive(DateTime oDateTime)
        {
            if (String.IsNullOrEmpty(MaxExclusiveStrValue))
                return true;

            if (!bMaxExclusiveDataTimeSet)
                if (!DateTime.TryParse(MaxExclusiveStrValue, out MaxExclusiveDataTime))
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MaxExclusiveStrValue + "' to DateTime", Log.LogType.Error);

            if (oDateTime < MaxExclusiveDataTime)
                return true;

            return false;
        }

        protected virtual bool CheckMinExclusive(DateTime oDateTime)
        {
            if (String.IsNullOrEmpty(MinExclusiveStrValue))
                return true;

            if (!bMinExclusiveDataTimeSet)
                if (!DateTime.TryParse(MinExclusiveStrValue, out MinExclusiveDataTime))
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MinExclusiveStrValue + "' to DateTime", Log.LogType.Error);

            if (oDateTime > MinExclusiveDataTime)
                return true;

            return false;
        }
    }

    /// <summary>
    /// [Definition:]  time represents an instant of time that recurs every day. The ·value space· of time is the space of time of day values 
    /// as defined in § 5.3 of [ISO 8601]. Specifically, it is a set of zero-duration daily time instances. 
    /// 
    /// time has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class TimeRestrictor : DateTimeRestrictor
    {

    }

    /// <summary>
    /// [Definition:]   The ·value space· of date consists of top-open intervals of exactly one day in length on the timelines of dateTime, 
    /// beginning on the beginning moment of each day (in each timezone), i.e. '00:00:00', up to but not including '24:00:00' (which is 
    /// identical with '00:00:00' of the next day). For nontimezoned values, the top-open intervals disjointly cover the nontimezoned timeline, 
    /// one per day. For timezoned values, the intervals begin at every minute and therefore overlap.
    /// 
    /// time has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class DateRestrictor : DateTimeRestrictor
    {

    }

    /// <summary>
    /// This is the lazy, non-implementation of DatePart restrictor.  Invalid DataParts will be caught when the schema is compiled.
    /// 
    /// TODO: Implement this properly
    /// </summary>
    public class DatePartRestrictor : AnySimpleTypeRestrictor
    {
        #region Facet Restrictions Not Supported
        public override void SetExactLength(String Length)
        {
            throw new Exception("Length facet restriction not supported.");
        }

        public override void SetMinLength(String Length)
        {
            throw new Exception("Minimum Length facet restriction not supported.");
        }

        public override void SetMaxLength(String Length)
        {
            throw new Exception("Maximum Length facet restriction not supported.");
        }

        public override void SetMaxFracDigits(string MaxFracDigits)
        {
            throw new Exception("Maximum Fractional Digits facet restriction not supported.");
        }

        public override void SetMaxTotalDigits(string MaxTotalDigits)
        {
            throw new Exception("Maximum Total Digits facet restriction not supported.");
        }
        #endregion

        public override bool Validate(object o)
        {
            String str;

            if (null == o)
                return false;

            // Object should be a string
            try
            {
                str = o as String;
            }
            catch (InvalidCastException)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not cast input object to String", Log.LogType.Error);
                return false;
            }
            catch (Exception e)
            {
                Log.Write(e);
                return false;
            };

            if (String.IsNullOrEmpty(str))
                return false;

            return true;
        }
    }

    /// <summary>
    /// [Definition:]   gYear represents a gregorian calendar year. The ·value space· of gYear is the set of Gregorian calendar years as 
    /// defined in § 5.2.1 of [ISO 8601]. Specifically, it is a set of one-year long, non-periodic instances e.g. lexical 1999 to represent 
    /// the whole year 1999, independent of how many months and days this year has. 
    /// 
    /// gYear has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class YearRestrictor : DatePartRestrictor
    {

    }

    /// <summary>
    /// [Definition:]   gYearMonth represents a specific gregorian month in a specific gregorian year. The ·value space· of gYearMonth is the 
    /// set of Gregorian calendar months as defined in § 5.2.1 of [ISO 8601]. Specifically, it is a set of one-month long, non-periodic 
    /// instances e.g. 1999-10 to represent the whole month of 1999-10, independent of how many days this month has. 
    /// 
    /// gYearMonth has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class YearMonthRestrictor : DatePartRestrictor
    {

    }

    /// <summary>
    /// [Definition:]   gMonth is a gregorian month that recurs every year. The ·value space· of gMonth is the space of a set of calendar months 
    /// as defined in § 3 of [ISO 8601]. Specifically, it is a set of one-month long, yearly periodic instances.
    /// 
    /// gMonth has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class MonthRestrictor : DatePartRestrictor
    {

    }

    /// <summary>
    /// [Definition:]   gMonthDay is a gregorian date that recurs, specifically a day of the year such as the third of May. Arbitrary recurring 
    /// dates are not supported by this datatype. The ·value space· of gMonthDay is the set of calendar dates, as defined in § 3 of [ISO 8601]. 
    /// Specifically, it is a set of one-day long, annually periodic instances.
    /// 
    /// gMonthDay has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class MonthDayRestrictor : DatePartRestrictor
    {

    }

    /// <summary>
    /// [Definition:]   gDay is a gregorian day that recurs, specifically a day of the month such as the 5th of the month. Arbitrary recurring 
    /// days are not supported by this datatype. The ·value space· of gDay is the space of a set of calendar dates as defined in § 3 of 
    /// [ISO 8601]. Specifically, it is a set of one-day long, monthly periodic instances.
    /// 
    /// gDay has the following ·constraining facets·: 
    ///
    /// •pattern
    /// •enumeration
    /// •whiteSpace
    /// •maxInclusive
    /// •maxExclusive
    /// •minInclusive
    /// •minExclusive
    /// </summary>
    public class DayRestrictor : DatePartRestrictor
    {

    }
}
