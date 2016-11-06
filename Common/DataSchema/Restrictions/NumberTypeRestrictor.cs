using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Fuzzware.Common.DataSchema.Restrictions
{
    /// <summary>
    /// [Definition:]  decimal represents a subset of the real numbers, which can be represented by decimal numerals. The ·value space· of decimal 
    /// is the set of numbers that can be obtained by multiplying an integer by a non-positive power of ten, i.e., expressible as i × 10^-n where i 
    /// and n are integers and n >= 0. Precision is not reflected in this value space; the number 2.0 is not distinct from the number 2.00. The 
    /// ·order-relation· on decimal is the order relation on real numbers, restricted to this subset.
    /// 
    /// decimal has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class DecimalTypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String Num;

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
        #endregion

        public DecimalTypeRestrictor()
            : base()
        {
            Num = "";
        }

        protected virtual bool CheckMaxInclusive(String Num)
        {
            if (String.IsNullOrEmpty(MaxInclusiveStrValue))
                return true;

            // Get the signs
            bool LimitIsPositive = true;
            if(MaxInclusiveStrValue.StartsWith("-"))
                LimitIsPositive = false;
            bool NumIsPositive = true;
            if(Num.StartsWith("-"))
                NumIsPositive = false;

            if (!NumIsPositive && LimitIsPositive)
                return true;
            if (NumIsPositive && !LimitIsPositive)
                return false;

            // Comparison of same sign
            if (MaxInclusiveStrValue.Length > Num.Length)
                return true;
            if (MaxInclusiveStrValue.Length < Num.Length)
                return false;

            if (String.Compare(Num, MaxInclusiveStrValue) <= 0)
                return true;

            return false;
        }

        protected virtual bool CheckMaxExclusive(String Num)
        {
            if (String.IsNullOrEmpty(MaxExclusiveStrValue))
                return true;

            // Get the signs
            bool LimitIsPositive = true;
            if (MaxExclusiveStrValue.StartsWith("-"))
                LimitIsPositive = false;
            bool NumIsPositive = true;
            if (Num.StartsWith("-"))
                NumIsPositive = false;

            if (!NumIsPositive && LimitIsPositive)
                return true;
            if (NumIsPositive && !LimitIsPositive)
                return false;

            // Comparison of same sign
            if (MaxExclusiveStrValue.Length > Num.Length)
                return true;
            if (MaxExclusiveStrValue.Length < Num.Length)
                return false;

            if (String.Compare(Num, MaxExclusiveStrValue) < 0)
                return true;

            return false;
        }

        protected virtual bool CheckMinInclusive(String Num)
        {
            if (String.IsNullOrEmpty(MinInclusiveStrValue))
                return true;

            // Get the signs
            bool LimitIsPositive = true;
            if (MinInclusiveStrValue.StartsWith("-"))
                LimitIsPositive = false;
            bool NumIsPositive = true;
            if (Num.StartsWith("-"))
                NumIsPositive = false;

            if (!NumIsPositive && LimitIsPositive)
                return false;
            if (NumIsPositive && !LimitIsPositive)
                return true;

            // Comparison of same sign
            if (MinInclusiveStrValue.Length > Num.Length)
                return false;
            if (MinInclusiveStrValue.Length < Num.Length)
                return true;

            if (String.Compare(Num, MinInclusiveStrValue) >= 0)
                return true;

            return false;
        }

        protected virtual bool CheckMinExclusive(String Num)
        {
            if (String.IsNullOrEmpty(MinExclusiveStrValue))
                return true;

            // Get the signs
            bool LimitIsPositive = true;
            if (MinExclusiveStrValue.StartsWith("-"))
                LimitIsPositive = false;
            bool NumIsPositive = true;
            if (Num.StartsWith("-"))
                NumIsPositive = false;

            if (!NumIsPositive && LimitIsPositive)
                return false;
            if (NumIsPositive && !LimitIsPositive)
                return true;

            // Comparison of same sign
            if (MinExclusiveStrValue.Length > Num.Length)
                return false;
            if (MinExclusiveStrValue.Length < Num.Length)
                return true;

            if (String.Compare(Num, MinExclusiveStrValue) > 0)
                return true;

            return false;
        }

        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Num = (String)o;
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
            }

            // There are no implicit restrictions on decimals, they can be any decimal number

            // Check Max Total Digits
            uint Len = (uint)Num.Length;
            if (Num.Contains("."))
                Len--;

            if (!CheckMaxTotalDigits(Len))
                return false;

            // Check Max Fractional Digits
            if (-1 != Num.IndexOf('.'))
                Len = (uint)Num.Length - (uint)Num.IndexOf('.');
            else
                Len = 0;
            if (!CheckMaxFractionalDigits(Len))
                return false;

            // Check pattern
            if (!CheckPattern(Num))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Num))
                return false;

            // Check Whitespace
            // this seems irrelevant

            // Check MaxInclusive
            if (!CheckMaxInclusive(Num))
                return false;

            // Check MaxExclusive
            if (!CheckMaxExclusive(Num))
                return false;

            // Check MinInclusive
            if (!CheckMinInclusive(Num))
                return false;

            // Check MinExclusive
            if (!CheckMinExclusive(Num))
                return false;

            return true;
        }
    }

    /// <summary>
    /// [Definition:]   integer is ·derived· from decimal by fixing the value of ·fractionDigits· to be 0and disallowing the trailing decimal point. 
    /// This results in the standard mathematical concept of the integer numbers. The ·value space· of integer is the infinite set {...,-2,-1,0,1,2,...}. 
    /// The ·base type· of integer is decimal. 
    /// 
    /// integer has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class IntegerTypeRestrictor : DecimalTypeRestrictor
    {
        public IntegerTypeRestrictor()
            : base()
        {
            Num = "";
        }

        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Num = (String)o;
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
            }

            // There are no implicit restrictions on Integers, they can be any Integers
            // Importantly, they are not limited to a bit size like int, they can be arbitrarily
            // long sequence of digits, hence we treat them as strings.

            // Check Max Total Digits
            uint Len = (uint)Num.Length;

            if (!CheckMaxTotalDigits(Len))
                return false;

            // Check Max Fractional Digits
            // This seems irrelevant

            // Check pattern
            if (!CheckPattern(Num))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Num))
                return false;

            // Check Whitespace
            // This seems irrelevant

            // Check MaxInclusive
            if (!CheckMaxInclusive(Num))
                return false;

            // Check MaxExclusive
            if (!CheckMaxExclusive(Num))
                return false;

            // Check MinInclusive
            if (!CheckMinInclusive(Num))
                return false;

            // Check MinExclusive
            if (!CheckMinExclusive(Num))
                return false;

            return true;
        }
    }

    /// <summary>
    /// [Definition:]   nonPositiveInteger is ·derived· from integer by setting the value of ·maxInclusive· to be 0. This results in the standard 
    /// mathematical concept of the non-positive integers. The ·value space· of nonPositiveInteger is the infinite set {...,-2,-1,0}. The ·base type· 
    /// of nonPositiveInteger is integer.
    /// 
    /// nonPositiveInteger has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class NonPositiveIntegerTypeRestrictor : IntegerTypeRestrictor
    {
        public NonPositiveIntegerTypeRestrictor()
            : base()
        {
            Num = "";

            MaxInclusiveStrValue = "0";
        }
    }

    /// <summary>
    /// [Definition:]   negativeInteger is ·derived· from nonPositiveInteger by setting the value of ·maxInclusive· to be -1. This results in the 
    /// standard mathematical concept of the negative integers. The ·value space· of negativeInteger is the infinite set {...,-2,-1}. The ·base type· 
    /// of negativeInteger is nonPositiveInteger.
    /// 
    /// negativeInteger has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class NegativeIntegerTypeRestrictor : NonPositiveIntegerTypeRestrictor
    {
        public NegativeIntegerTypeRestrictor()
            : base()
        {
            Num = "";

            MaxInclusiveStrValue = "-1";
        }
    }

    /// <summary>
    /// [Definition:]   nonNegativeInteger is ·derived· from integer by setting the value of ·minInclusive· to be 0. This results in the standard 
    /// mathematical concept of the non-negative integers. The ·value space· of nonNegativeInteger is the infinite set {0,1,2,...}. The ·base type· 
    /// of nonNegativeInteger is integer.
    /// 
    /// nonNegativeInteger has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class NonNegativeIntegerTypeRestrictor : IntegerTypeRestrictor
    {
        public NonNegativeIntegerTypeRestrictor()
            : base()
        {
            Num = "";

            MinInclusiveStrValue = "0";
        }
    }

    /// <summary>
    /// [Definition:]   positiveInteger is ·derived· from nonNegativeInteger by setting the value of ·minInclusive· to be 1. This results in the 
    /// standard mathematical concept of the positive integer numbers. The ·value space· of positiveInteger is the infinite set {1,2,...}. The 
    /// ·base type· of positiveInteger is nonNegativeInteger.
    /// 
    /// positiveInteger has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class PositiveIntegerTypeRestrictor : NonNegativeIntegerTypeRestrictor
    {
        public PositiveIntegerTypeRestrictor()
            : base()
        {
            Num = "";

            MinInclusiveStrValue = "1";
        }
    }


    /// <summary>
    /// Generic class for handling restrictions on types that correspond to all the CLR types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NumberTypeRestrictor<T> : IntegerTypeRestrictor where T : IComparable<T>
    {
        protected new T Num;

        protected T MaxInclusiveValue;
        protected T MinInclusiveValue;
        protected T MaxExclusiveValue;
        protected T MinExclusiveValue;

        protected delegate bool TryParse(String str, out T Val);
        protected TryParse Parser;

        #region IFacetRestrictions overrides
        public override void SetMinInclusiveValue(string Val)
        {
            base.SetMinInclusiveValue(Val);

            if (String.IsNullOrEmpty(MinInclusiveStrValue))
                return;

            if (!Parser(MinInclusiveStrValue, out MinInclusiveValue))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MinInclusiveStrValue + "' to type '" + this.GetType().ToString() + "'", Log.LogType.Error); 
            }
        }

        public override void SetMinExclusiveValue(string Val)
        {
            base.SetMinExclusiveValue(Val);

            if (String.IsNullOrEmpty(MinExclusiveStrValue))
                return;

            if (!Parser(MinExclusiveStrValue, out MinExclusiveValue))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MinExclusiveStrValue + "' to type '" + this.GetType().ToString() + "'", Log.LogType.Error); 
            }
        }

        public override void SetMaxInclusiveValue(string Val)
        {
            base.SetMaxInclusiveValue(Val);

            if (String.IsNullOrEmpty(MaxInclusiveStrValue))
                return;

            if (!Parser(MaxInclusiveStrValue, out MaxInclusiveValue))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MaxInclusiveStrValue + "' to type '" + this.GetType().ToString() + "'", Log.LogType.Error); 
            }
        }

        public override void SetMaxExclusiveValue(string Val)
        {
            base.SetMaxExclusiveValue(Val);

            if (String.IsNullOrEmpty(MaxExclusiveStrValue))
                return;

            if (!Parser(MaxExclusiveStrValue, out MaxExclusiveValue))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + MaxExclusiveStrValue + "' to type '" + this.GetType().ToString() + "'", Log.LogType.Error); 
            }
        }
        #endregion

        protected virtual bool CheckMaxInclusive(T Num)
        {
            if (String.IsNullOrEmpty(MaxInclusiveStrValue))
                return true;

            if (Num.CompareTo(MaxInclusiveValue) < 1)
                return true;
            return false;
        }

        protected virtual bool CheckMaxExclusive(T Num)
        {
            if (String.IsNullOrEmpty(MaxExclusiveStrValue))
                return true;

            if (Num.CompareTo(MaxExclusiveValue) < 0)
                return true;
            return false;
        }

        protected virtual bool CheckMinInclusive(T Num)
        {
            if (String.IsNullOrEmpty(MinInclusiveStrValue))
                return true;

            if (Num.CompareTo(MinInclusiveValue) > -1)
                return true;
            return false;
        }

        protected virtual bool CheckMinExclusive(T Num)
        {
            if (String.IsNullOrEmpty(MinExclusiveStrValue))
                return true;

            if (Num.CompareTo(MinExclusiveValue) > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true and the max exlcusive value if one has been set, false if one has not been set.
        /// If GetMaxExclusive returns true then GetMaxInclusive will return false, and vice-versa.
        /// </summary>
        public virtual bool GetMaxExclusive(out T MaxExclusive)
        {
            MaxExclusive = MaxExclusiveValue;
            if (!String.IsNullOrEmpty(MaxExclusiveStrValue))
                return true;
            return false;
        }

        /// <summary>
        /// Returns true and the min exlcusive value if one has been set, false if one has not been set.
        /// If GetMinExclusive returns true then GetMinInclusive will return false, and vice-versa.
        /// </summary>
        public virtual bool GetMinExclusive(out T MinExclusive)
        {
            MinExclusive = MinExclusiveValue;
            if (!String.IsNullOrEmpty(MinExclusiveStrValue))
                return true;
            return false;
        }

        /// <summary>
        /// Returns true and the max inlcusive value if one has been set, false if one has not been set.
        /// If GetMaxInclusive returns true then GetMaxExclusive will return false, and vice-versa.
        /// </summary>
        public virtual bool GetMaxInclusive(out T MaxInclusive)
        {
            MaxInclusive = MaxInclusiveValue;
            if (!String.IsNullOrEmpty(MaxExclusiveStrValue))
                return false;
            return true;
        }

        /// <summary>
        /// Returns true and the min inlcusive value if one has been set, false if one has not been set.
        /// If GetMinInclusive returns true then GetMinExclusive will return false, and vice-versa.
        /// </summary>
        public virtual bool GetMinInclusive(out T MinInclusive)
        {
            MinInclusive = MinInclusiveValue;
            if (!String.IsNullOrEmpty(MinExclusiveStrValue))
                return false;
            return true;
        }

        public override bool Validate(object o)
        {
            String StrNum = null;
            // Assume the object is a string
            try
            {
                StrNum = o as String;
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
            }

            if (!Parser(StrNum, out Num))
            {
                // Not sure why I am reporting an error here?  Surely it should just fail.  Reporting an error is bad as Convert2XML uses
                // Validate and we don't want any errors reported.

                //// Only report this if we can't parse it as a number at all
                //Int64 test1;
                //UInt64 test2;
                //if(!Int64.TryParse(StrNum, out test1) && !UInt64.TryParse(StrNum, out test2))
                //    Log.Write(MethodBase.GetCurrentMethod(), "Could not parse '" + StrNum + "' as a number'", Log.LogType.Error); 
                //    //Log.Write(MethodBase.GetCurrentMethod(), "Could not parse '" + StrNum + "' to type '" + typeof(T).ToString() + "'", Log.LogType.ValidateError); 
                
                return false;
            }
            
            // Check Max Total Digits
            string NumStr = Num.ToString();
            uint Len = (uint)NumStr.Length;

            if (!CheckMaxTotalDigits(Len))
                return false;

            // Check Max Fractional Digits
            // This seems silly?

            // Check pattern
            if (!CheckPattern(NumStr))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Num.ToString()))
                return false;

            // Check Whitespace
            // This seems irrelevant

            // Check MaxInclusive
            if (!CheckMaxInclusive(Num))
                return false;

            // Check MaxExclusive
            if (!CheckMaxExclusive(Num))
                return false;

            // Check MinInclusive
            if (!CheckMinInclusive(Num))
                return false;

            // Check MinExclusive
            if (!CheckMinExclusive(Num))
                return false;

            return true;
        }
    }

    /// <summary>
    /// [Definition:]   long is ·derived· from integer by setting the value of ·maxInclusive· to be 9223372036854775807 and ·minInclusive· to 
    /// be -9223372036854775808. The ·base type· of long is integer.
    /// 
    /// long has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class LongTypeRestrictor : NumberTypeRestrictor<Int64>
    {
        public LongTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Int64.MaxValue;
            MinInclusiveValue = Int64.MinValue;

            Parser = Int64.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   int is ·derived· from long by setting the value of ·maxInclusive· to be 2147483647 and ·minInclusive· to be -2147483648. 
    /// The ·base type· of int is long.
    /// 
    /// int has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class IntTypeRestrictor : NumberTypeRestrictor<Int32>
    {
        public IntTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Int32.MaxValue;
            MinInclusiveValue = Int32.MinValue;

            Parser = Int32.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   short is ·derived· from int by setting the value of ·maxInclusive· to be 32767 and ·minInclusive· to be -32768. The 
    /// ·base type· of short is int.
    /// 
    /// short has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class ShortTypeRestrictor : NumberTypeRestrictor<Int16>
    {
        public ShortTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Int16.MaxValue;
            MinInclusiveValue = Int16.MinValue;

            Parser = Int16.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   byte is ·derived· from short by setting the value of ·maxInclusive· to be 127 and ·minInclusive· to be -128. The ·base type· 
    /// of byte is short.
    /// 
    /// byte has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class ByteTypeRestrictor : NumberTypeRestrictor<SByte>
    {
        public ByteTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = SByte.MaxValue;
            MinInclusiveValue = SByte.MinValue;

            Parser = SByte.TryParse;
        }
    }


    /// <summary>
    /// [Definition:]   unsignedLong is ·derived· from nonNegativeInteger by setting the value of ·maxInclusive· to be 18446744073709551615. The 
    /// ·base type· of unsignedLong is nonNegativeInteger.
    /// 
    /// unsignedLong has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class UnsignedLongTypeRestrictor : NumberTypeRestrictor<UInt64>
    {
        public UnsignedLongTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = UInt64.MaxValue;
            MinInclusiveValue = UInt64.MinValue;

            Parser = UInt64.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   unsignedInt is ·derived· from unsignedLong by setting the value of ·maxInclusive· to be 4294967295. The ·base type· of 
    /// unsignedInt is unsignedLong.
    /// 
    /// unsignedInt has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class UnsignedIntTypeRestrictor : NumberTypeRestrictor<UInt32>
    {
        public UnsignedIntTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = UInt32.MaxValue;
            MinInclusiveValue = UInt32.MinValue;

            Parser = UInt32.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   unsignedShort is ·derived· from unsignedInt by setting the value of ·maxInclusive· to be 65535. The ·base type· of 
    /// unsignedShort is unsignedInt.
    /// 
    /// unsignedShort has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class UnsignedShortTypeRestrictor : NumberTypeRestrictor<UInt16>
    {
        public UnsignedShortTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = UInt16.MaxValue;
            MinInclusiveValue = UInt16.MinValue;

            Parser = UInt16.TryParse;
        }
    }

    /// <summary>
    /// [Definition:]   unsignedByte is ·derived· from unsignedShort by setting the value of ·maxInclusive· to be 255. The ·base type· of 
    /// unsignedByte is unsignedShort.
    /// 
    /// unsignedByte has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class UnsignedByteTypeRestrictor : NumberTypeRestrictor<Byte>
    {
        public UnsignedByteTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Byte.MaxValue;
            MinInclusiveValue = Byte.MinValue;

            Parser = Byte.TryParse;
        }
    }


    /// <summary>
    /// Generic class for handling restrictions on types that correspond for CLR Single and Double types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FractionalNumberTypeRestrictor<T> : NumberTypeRestrictor<T> where T : IComparable<T>
    {
        protected delegate bool DelIsNaN(T Val);
        protected DelIsNaN IsNaN;
        //protected delegate bool DelIsNegativeInfinity(T Val);
        //protected DelIsNegativeInfinity IsNegativeInfinity;
        //protected delegate bool DelIsPositiveInfinity(T Val);
        //protected DelIsPositiveInfinity IsPositiveInfinity;

        protected override bool CheckMaxInclusive(T Num)
        {
            if (String.IsNullOrEmpty(MaxInclusiveStrValue))
                return true;

            // If an inclusive limit is NaN, the only comparable value is NaN, so if Num is NaN then return true, otherwise false;
            if (IsNaN(MaxInclusiveValue))
                return IsNaN(Num);

            // Hopefully the comparison can handle + or - infinity
            if (Num.CompareTo(MaxInclusiveValue) < 1)
                return true;
            return false;
        }

        protected override bool CheckMaxExclusive(T Num)
        {
            if (String.IsNullOrEmpty(MaxExclusiveStrValue))
                return true;

            // NaN is not comparable, and since this is exclusive, even if the limit is NaN this is exclusive
            if (IsNaN(Num))
                return false;

            // Hopefully the comparison can handle + or - infinity
            if (Num.CompareTo(MaxExclusiveValue) < 0)
                return true;
            return false;
        }

        protected override bool CheckMinInclusive(T Num)
        {
            if (String.IsNullOrEmpty(MinInclusiveStrValue))
                return true;

            // If an inclusive limit is NaN, the only comparable value is NaN, so if Num is NaN then return true, otherwise false;
            if (IsNaN(MaxInclusiveValue))
                return IsNaN(Num);

            // Hopefully the comparison can handle + or - infinity
            if (Num.CompareTo(MinInclusiveValue) > -1)
                return true;
            return false;
        }

        protected override bool CheckMinExclusive(T Num)
        {
            if (String.IsNullOrEmpty(MinExclusiveStrValue))
                return true;

            // NaN is not comparable, and since this is exclusive, even if the limit is NaN this is exclusive
            if (IsNaN(Num))
                return false;

            // Hopefully the comparison can handle + or - infinity
            if (Num.CompareTo(MinExclusiveValue) > 0)
                return true;
            return false;
        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
            {
                return false;
            }

            String NumStr = o as String;

            // Check Max Fractional Digits
            uint Len;
            if (-1 != NumStr.IndexOf('.'))
                Len = (uint)NumStr.Length - (uint)NumStr.IndexOf('.');
            else
                Len = 0;
            if (!CheckMaxFractionalDigits(Len))
                return false;

            return true;
        }
    }

    /// <summary>
    /// [Definition:]  The double datatype is patterned after the IEEE double-precision 64-bit floating point type [IEEE 754-1985]. The basic 
    /// ·value space· of double consists of the values m × 2^e, where m is an integer whose absolute value is less than 2^53, and e is an integer 
    /// between -1075 and 970, inclusive. In addition to the basic ·value space· described above, the ·value space· of double also contains the 
    /// following three special values: positive and negative infinity and not-a-number (NaN). The ·order-relation· on double is: x < y iff y - x 
    /// is positive for x and y in the value space. Positive infinity is greater than all other non-NaN values. NaN equals itself but is ·incomparable· 
    /// with (neither greater than nor less than) any other value in the ·value space·.
    /// 
    /// float has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class DoubleTypeRestrictor : FractionalNumberTypeRestrictor<Double>
    {
        public DoubleTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Double.MaxValue;
            MinInclusiveValue = Double.MinValue;

            Parser = Double.TryParse;
            IsNaN = Double.IsNaN;
        }
    }
    
    /// <summary>
    /// [Definition:]  float is patterned after the IEEE single-precision 32-bit floating point type [IEEE 754-1985]. The basic ·value space· of float 
    /// consists of the values m × 2^e, where m is an integer whose absolute value is less than 2^24, and e is an integer between -149 and 104, 
    /// inclusive. In addition to the basic ·value space· described above, the ·value space· of float also contains the following three special 
    /// values: positive and negative infinity and not-a-number (NaN). The ·order-relation· on float is: x < y iff y - x is positive for x and 
    /// y in the value space. Positive infinity is greater than all other non-NaN values. NaN equals itself but is ·incomparable· with (neither 
    /// greater than nor less than) any other value in the ·value space·.
    /// 
    /// float has the following ·constraining facets·: 
    ///     totalDigits
    ///     fractionDigits
    ///     pattern
    ///     whiteSpace
    ///     enumeration
    ///     maxInclusive
    ///     maxExclusive
    ///     minInclusive
    ///     minExclusive
    /// </summary>
    public class SingleTypeRestrictor : FractionalNumberTypeRestrictor<Single>
    {
        public SingleTypeRestrictor()
            : base()
        {
            Num = 0;

            MaxInclusiveValue = Single.MaxValue;
            MinInclusiveValue = Single.MinValue;

            Parser = Single.TryParse;
            IsNaN = Single.IsNaN;
        }
    }
}