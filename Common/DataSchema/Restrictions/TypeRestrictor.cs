using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Fuzzware.Common.DataSchema.Restrictions
{
    public class ValidateFailedException : Exception
    {
        public ValidateFailedException()
            : base()
        {

        }
    }

    public abstract class TypeRestrictor : IFacetRestrictions
    {
        protected uint MinLength;
        protected uint MaxLength;

        protected Regex RegularExpression;
        protected String[] EnumValues;

        protected String MinInclusiveStrValue;
        protected String MaxInclusiveStrValue;
        protected String MinExclusiveStrValue;
        protected String MaxExclusiveStrValue;

        protected uint MaxFractionalDigits;
        protected uint MaxTotalDigits;

        public TypeRestrictor()
        {
            // These are clearly just reasonably values to use
            MinLength = UInt32.MinValue;
            MaxLength = UInt32.MaxValue;            
            MaxFractionalDigits = UInt32.MaxValue;
            MaxTotalDigits = UInt32.MaxValue;
        }

        #region IFacetRestrictions Members

        public virtual void SetExactLength(string Length)
        {
            try
            {
                MinLength = UInt32.Parse(Length);
                MaxLength = UInt32.Parse(Length);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return;
            }
        }

        public virtual void SetMinLength(string Length)
        {
            try
            {
                MinLength = UInt32.Parse(Length);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return;
            }
        }

        public virtual void SetMaxLength(string Length)
        {
            try
            {
                MaxLength = UInt32.Parse(Length);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return;
            }
        }

        public virtual void SetPattern(string RegularExpression)
        {
            try
            {
                this.RegularExpression = new Regex(RegularExpression);
            }
            catch (ArgumentException e)
            {
                Log.Write(e);
            }
        }

        public virtual Regex GetPattern()
        {
            return RegularExpression;
        }

        public virtual uint GetMaxLength()
        {
            return MaxLength;
        }

        public virtual uint GetMinLength()
        {
            return MinLength;
        }

        public virtual void SetEnumerationValue(string EnumVal)
        {
            if (null == EnumVal)
                return;

            if (null == EnumValues)
                EnumValues = new string[0];

            string[] TempEnumValues = new String[EnumValues.Length + 1];
            Array.Copy(EnumValues, TempEnumValues, EnumValues.Length);
            TempEnumValues[EnumValues.Length] = EnumVal;
            EnumValues = TempEnumValues;
        }

        public virtual string[] GetEnumerationValues()
        {
            if(null != EnumValues)
            {
                string[] TempEnumValues = new String[EnumValues.Length];
                Array.Copy(EnumValues, TempEnumValues, EnumValues.Length);
                return TempEnumValues;
            }
            return null;
        }

        public virtual void SetMinInclusiveValue(string Val)
        {
            MinInclusiveStrValue = String.Copy(Val);
        }

        public virtual void SetMinExclusiveValue(string Val)
        {
            MinExclusiveStrValue = String.Copy(Val);
        }

        public virtual void SetMaxInclusiveValue(string Val)
        {
            MaxInclusiveStrValue = String.Copy(Val);
        }

        public virtual void SetMaxExclusiveValue(string Val)
        {
            MaxExclusiveStrValue = String.Copy(Val);
        }

        public virtual void SetMaxFracDigits(string MaxFracDigits)
        {
            try
            {
                MaxFractionalDigits = UInt32.Parse(MaxFracDigits);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return;
            }
        }

        public virtual void SetMaxTotalDigits(string MaxTotalDigits)
        {
            try
            {
                this.MaxTotalDigits = UInt32.Parse(MaxTotalDigits);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return;
            }
        }

        #endregion

        abstract public bool Validate(object o);

        protected virtual bool CheckLength(uint ValueLen)
        {
            if ((ValueLen >= MinLength) && (ValueLen <= MaxLength))
                return true;
            
            return false;
        }

        protected virtual bool CheckPattern(String Value)
        {
            if (null == RegularExpression)
                return true;

            return RegularExpression.Match(Value).Value.Equals(Value);
            //return RegularExpression.IsMatch(Value);
        }

        protected virtual bool CheckEnumeration(String Value)
        {
            if (null == EnumValues)
                return true;
            for (int i = 0; i < EnumValues.Length; i++)
            {
                if (0 == String.Compare(Value, EnumValues[i], false))
                    return true;
            }
            return false;
        }

        protected virtual bool CheckMaxFractionalDigits(uint DigitCount)
        {
            if (DigitCount <= MaxFractionalDigits)
                return true;

            return false;
        }

        protected virtual bool CheckMaxTotalDigits(uint DigitCount)
        {
            if (DigitCount <= MaxTotalDigits)
                return true;

            return false;
        }
    }

    public class AnySimpleTypeRestrictor : TypeRestrictor
    {
        public AnySimpleTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            return true;
        }
    }
}
