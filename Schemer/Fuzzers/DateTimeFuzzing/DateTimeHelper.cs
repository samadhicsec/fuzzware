using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Schemer.Fuzzers.DateTimeFuzzing
{
    class DateTimeHelper
    {
        private static char[] TimeZoneStartChars = { '-', '+', 'Z' };

        /// <summary>
        /// Checks from position startindex of string str, if a bad time zone is present
        /// </summary>
        /// <param name="startindex"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsBadTimezone(int startindex, string str)
        {
            if (str.Length <= startindex)
                return false;

            if (startindex == str.LastIndexOfAny(TimeZoneStartChars))
            {
                // Check if the 'Z' is appended (UTC time)
                if (str.Substring(startindex, 1).Equals("Z", StringComparison.InvariantCultureIgnoreCase) && (startindex + 1 == str.Length))
                    return false;
                // Check if +tt:tt or -tt:tt is present
                if ((startindex + 6 == str.Length) && (str.LastIndexOf(':') == str.Length - 3))
                    return false;
            }

            return true;
        }

        // Technically any date type that includes a year should support a negative year, and should support years with mor than 4 digits.
        // However, MS .Net does not seem to support either, so this means that we can't compile an XML file with years in these formats,
        // so even if we tried to support them here, the test case would be skipped because the XML won't compile.

        #region DateTime helper functions
        /// <summary>
        /// A DateTime has the format YYYY-MM-DDThh:mm:ss with optional suffixs .s, .ss, .sss, Z, +t:tt, -t:tt
        /// </summary>
        /// <param name="DateTimeValue"></param>
        /// <returns></returns>
        public static bool IsDateTime(string DateTimeValue)
        {
            if ((DateTimeValue.Length >= 19) &&
                (-1 != DateTimeValue.IndexOf('T')))
                return true;            
            return false;
        }

        /// <summary>
        /// Converts a string of type DataSchemaTypeCode.DateTime to any of the lower precision date or time types
        /// </summary>
        /// <param name="TypeCode"></param>
        /// <param name="DateTimeValue"></param>
        /// <returns></returns>
        public static string ConvertDateTimeTo(DataSchemaTypeCode TypeCode, string DateTimeValue)
        {
            DateTime res;
            if (DateTime.TryParse(DateTimeValue, out res))
            {
                int index = -1;
                switch (TypeCode)
                {
                    case DataSchemaTypeCode.Date:
                        index = DateTimeValue.IndexOf('T');
                        if (index != -1)
                            return DateTimeValue.Substring(0, index);
                        break;
                    case DataSchemaTypeCode.Time:
                        index = DateTimeValue.IndexOf('T');
                        if (index != -1)
                            return DateTimeValue.Substring(index + 1, DateTimeValue.Length - (index + 1));
                        break;
                    case DataSchemaTypeCode.GYear:
                        return FormatYear(res.Year.ToString());
                    case DataSchemaTypeCode.GYearMonth:
                        return FormatYearMonth(res.Year.ToString(), res.Month.ToString());
                    case DataSchemaTypeCode.GMonth:
                        return FormatMonth(res.Month.ToString());
                    case DataSchemaTypeCode.GMonthDay:
                        return FormatMonthDay(res.Month.ToString(), res.Day.ToString());
                    case DataSchemaTypeCode.GDay:
                        return FormatDay(res.Day.ToString());
                }
            }
            return DateTimeValue;
        }
        #endregion

        #region Date helper functions
        /// <summary>
        /// A Date has the format YYYY-MM-DD plus time zone identifier
        /// </summary>
        /// <param name="DateTimeValue"></param>
        /// <returns></returns>
        public static bool IsDate(string DateValue)
        {
            if ((DateValue.Length >= 10) && 
                (4 == DateValue.IndexOf('-')) &&
                (7 == DateValue.IndexOf('-', 5))
                )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string of type DataSchemaTypeCode.Date to any of the lower precision date or time types
        /// </summary>
        /// <param name="TypeCode"></param>
        /// <param name="DateValue"></param>
        /// <returns></returns>
        public static string ConvertDateTo(DataSchemaTypeCode TypeCode, string DateValue)
        {
            DateTime res;
            if (DateTime.TryParse(DateValue, out res))
            {
                switch (TypeCode)
                {
                    case DataSchemaTypeCode.GYear:
                        return FormatYear(res.Year.ToString()) + DateValue.Substring(10);
                    case DataSchemaTypeCode.GYearMonth:
                        return FormatYearMonth(res.Year.ToString(), res.Month.ToString()) + DateValue.Substring(10);
                    case DataSchemaTypeCode.GMonth:
                        return FormatMonth(res.Month.ToString()) + DateValue.Substring(10);
                    case DataSchemaTypeCode.GMonthDay:
                        return FormatMonthDay(res.Month.ToString(), res.Day.ToString()) + DateValue.Substring(10);
                    case DataSchemaTypeCode.GDay:
                        return FormatDay(res.Day.ToString()) + DateValue.Substring(10);
                }
            }
            return DateValue;
        }
        #endregion

        #region Time helper functions
        /// <summary>
        /// A Time has the format hh:mm:ss with optional suffixes .s, .ss, .sss, Z, +t:tt, -t:tt
        /// </summary>
        /// <param name="TimeValue"></param>
        /// <returns></returns>
        public static bool IsTime(string TimeValue)
        {
            if ((TimeValue.Length >= 8) &&
                (2 != TimeValue.IndexOf(':')) &&
                (5 != TimeValue.IndexOf(':', 3)))
                return true;
            return false;
        }
        #endregion

        #region Year helper functions
        /// <summary>
        /// A Year has the format YYYY, technically it is allowed more than 4 numbers, and a time zone (Z or +-tt:tt)
        /// </summary>
        /// <param name="YearValue"></param>
        /// <returns></returns>
        public static bool IsYear(string YearValue)
        {
            if ((YearValue.Length >= 4) &&
                (-1 == YearValue.IndexOf('-')))
                return true;
            return false;
        }

        /// <summary>
        /// Returns a formatted year
        /// </summary>
        /// <param name="Year">Should be an integer in string format</param>
        /// <returns></returns>
        private static string FormatYear(String Year)
        {
            int iYear = 0;
            //string prefix = String.Empty;
            //if (0 == Year.IndexOf('-'))
            //{
            //    prefix = "-";
            //    Year = Year.Substring(1);
            //}
            //string suffix = String.Empty;
            //int index = Year.IndexOfAny(new char[] { '-', '+', 'Z' });
            //if (-1 != index)
            //{
            //    suffix = Year.Substring(index);
            //    Year = Year.Substring(0, index - 1);
            //}
            if (!Int32.TryParse(Year, out iYear))
                return null;
            //return prefix + iYear.ToString("0000") + suffix;
            return iYear.ToString("0000");
        }
        #endregion

        #region YearMonth helper functions
        /// <summary>
        /// A YearMonth has the format YYYY-MM plus optional time zone
        /// </summary>
        /// <param name="DateTimeValue"></param>
        /// <returns></returns>
        public static bool IsYearMonth(string YearMonthValue)
        {
            if((YearMonthValue.Length >= 7) && (4 == YearMonthValue.IndexOf('-')) && !IsBadTimezone(7, YearMonthValue))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string of type DataSchemaTypeCode.GYearMonth to DataSchemaTypeCode.GYear or DataSchemaTypeCode.GMonth, for other
        /// DataSchemaTypeCode's the original string is returned.
        /// </summary>
        /// <param name="TypeCode"></param>
        /// <param name="YearMonthValue"></param>
        /// <returns></returns>
        public static string ConvertYearMonthTo(DataSchemaTypeCode TypeCode, string YearMonthValue)
        {
            if (IsYearMonth(YearMonthValue))
            {
                switch (TypeCode)
                {
                    case DataSchemaTypeCode.GYear:
                        return FormatYear(YearMonthValue.Substring(0, 4)) + YearMonthValue.Substring(7);
                    case DataSchemaTypeCode.GMonth:
                        return FormatMonth(YearMonthValue.Substring(5, 2)) + YearMonthValue.Substring(7);
                }
            }
            return YearMonthValue;
        }

        private static string FormatYearMonth(String Year, String Month)
        {
            int iYear = 0;
            if (!Int32.TryParse(Year, out iYear))
                return null;
            int iMonth = 0;
            if (!Int32.TryParse(Month, out iMonth))
                return null;
            return iYear.ToString("0000") + "-" + iMonth.ToString("00");
        }
        #endregion

        #region Month helper functions
        /// <summary>
        /// A Month has the format --MM plus opional time zone
        /// </summary>
        /// <param name="MonthValue"></param>
        /// <returns></returns>
        public static bool IsMonth(string MonthValue)
        {
            if ((MonthValue.Length >= 4) &&
                (0 == MonthValue.IndexOf('-')) &&
                (1 == MonthValue.IndexOf('-', 1)) &&
                !IsBadTimezone(4, MonthValue))
                return true;
            return false;
        }

        private static string FormatMonth(String Month)
        {
            int iMonth = 0;
            if (!Int32.TryParse(Month, out iMonth))
                return null;
            return "--" + iMonth.ToString("00"); ;
        }
        #endregion

        #region MonthDay helper functions
        /// <summary>
        /// A MonthDay has the format --MM-DD plus opional time zone
        /// </summary>
        /// <param name="DateTimeValue"></param>
        /// <returns></returns>
        public static bool IsMonthDay(string MonthDayValue)
        {
            if ((MonthDayValue.Length >= 7) && 
                (0 == MonthDayValue.IndexOf('-')) && 
                (1 == MonthDayValue.IndexOf('-', 1)) &&
                (4 == MonthDayValue.IndexOf('-', 2)) &&
                !IsBadTimezone(7, MonthDayValue)
                )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string of type DataSchemaTypeCode.GMonthDay to DataSchemaTypeCode.GMonth or DataSchemaTypeCode.GDay, for other
        /// DataSchemaTypeCode's the original string is returned.
        /// </summary>
        /// <param name="TypeCode"></param>
        /// <param name="MonthDayValue"></param>
        /// <returns></returns>
        public static string ConvertMonthDayTo(DataSchemaTypeCode TypeCode, string MonthDayValue)
        {
            if (IsMonthDay(MonthDayValue))
            {
                switch (TypeCode)
                {
                    case DataSchemaTypeCode.GMonth:
                        return FormatMonth(MonthDayValue.Substring(2, 2)) + MonthDayValue.Substring(7);
                    case DataSchemaTypeCode.GDay:
                        return FormatDay(MonthDayValue.Substring(5, 2)) + MonthDayValue.Substring(7);
                }
            }
            return MonthDayValue;
        }

        private static string FormatMonthDay(String Month, String Day)
        {
            int iMonth = 0;
            if (!Int32.TryParse(Month, out iMonth))
                return null;
            int iDay = 0;
            if (!Int32.TryParse(Day, out iDay))
                return null;
            return "--" + iMonth.ToString("00") + "-" + iDay.ToString("00");
        }
        #endregion

        #region Day helper functions
        /// <summary>
        /// A Day has the format ---DD plus opional time zone
        /// </summary>
        /// <param name="DayValue"></param>
        /// <returns></returns>
        public static bool IsDay(string DayValue)
        {
            if ((DayValue.Length >= 5) &&
                (0 == DayValue.IndexOf('-')) &&
                (1 == DayValue.IndexOf('-', 1)) &&
                (2 == DayValue.IndexOf('-', 2)) &&
                !IsBadTimezone(5, DayValue))
                return true;
            return false;
        }

        private static string FormatDay(String Day)
        {
            int iDay = 0;
            if (!Int32.TryParse(Day, out iDay))
                return null;
            return "---" + iDay.ToString("00");
        }
        #endregion
    }
}
