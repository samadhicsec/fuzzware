using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Fuzzware.Common.DataSchema.Restrictions
{
    /// <summary>
    /// [Definition:]  boolean has the ·value space· required to support the mathematical concept of binary-valued logic: {true, false}.
    /// 
    /// boolean has the following ·constraining facets·: 
    ///     pattern
    ///     whiteSpace
    /// </summary>
    public class BooleanTypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String Val;

        #region Facet Restrictions Not Supported
        public override void SetMinInclusiveValue(string Val)
        {
            throw new Exception("Minimum Inclusive Value facet restriction not supported.");
        }

        public override void SetMinExclusiveValue(string Val)
        {
            throw new Exception("Minimum Exclusive Value facet restriction not supported.");
        }

        public override void SetMaxInclusiveValue(string Val)
        {
            throw new Exception("Maximum Inclusive Value facet restriction not supported.");
        }

        public override void SetMaxExclusiveValue(string Val)
        {
            throw new Exception("Maximum Exclusive Value facet restriction not supported.");
        }

        public override void SetMaxFracDigits(string MaxFracDigits)
        {
            throw new Exception("Maximum Fractional Digits facet restriction not supported.");
        }

        public override void SetMaxTotalDigits(string MaxTotalDigits)
        {
            throw new Exception("Maximum Total Digits facet restriction not supported.");
        }

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

        public BooleanTypeRestrictor()
            : base()
        {
            SetEnumerationValue("true");
            SetEnumerationValue("false");
            SetEnumerationValue("1");
            SetEnumerationValue("0");
        }
        
        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Val = o as String;
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

            // A Boolean can be true or false (canonical), and additionally 1 or 0

            // Check pattern
            if (!CheckPattern(Val))
                return false;

            // Check Enumeration, this is not an official constraining facet, but it provides an easy way to validate the type
            if (!CheckEnumeration(Val))
                return false;

            // Check Whitespace
            // this seems irrelevante

            return true;
        }      
    }

    /// <summary>
    /// [Definition:]   base64Binary represents Base64-encoded arbitrary binary data. The ·value space· of base64Binary is the set of finite-length 
    /// sequences of binary octets. For base64Binary data the entire binary stream is encoded using the Base64 Alphabet in [RFC 2045].
    /// 
    /// base64Binary has the following ·constraining facets·: 
    ///     length
    ///     minLength
    ///     maxLength
    ///     pattern
    ///     enumeration
    ///     whiteSpace
    /// </summary>
    public class Base64BinaryTypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String Val;

        #region Facet Restrictions Not Supported
        public override void SetMinInclusiveValue(string Val)
        {
            throw new Exception("Minimum Inclusive Value facet restriction not supported.");
        }

        public override void SetMinExclusiveValue(string Val)
        {
            throw new Exception("Minimum Exclusive Value facet restriction not supported.");
        }

        public override void SetMaxInclusiveValue(string Val)
        {
            throw new Exception("Maximum Inclusive Value facet restriction not supported.");
        }

        public override void SetMaxExclusiveValue(string Val)
        {
            throw new Exception("Maximum Exclusive Value facet restriction not supported.");
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

        public Base64BinaryTypeRestrictor()
            : base()
        {
            
        }

        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Val = o as String;
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

            // A base64Binary string must be properly encoded
            try
            {
                Convert.FromBase64String(Val);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }

            uint Len;
            String ValCopy;
            int index;
            ValCopy = String.Copy(Val);
            // Remove whitespaces
            while(-1 != (index = ValCopy.IndexOf(' ')))
            {
                ValCopy.Remove(index, 1);
            }
            Len = (uint)ValCopy.Length;
            if(ValCopy.EndsWith("=="))
                Len -= 2;
            else if(ValCopy.EndsWith("="))
                Len--;

            // Check length
            if (!CheckLength(Len))
                return false;

            // Check pattern
            if (!CheckPattern(Val))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Val))
                return false;

            // Check Whitespace
            // This seems irrelevant

            return true;
        }
    }


    /// <summary>
    /// [Definition:]   hexBinary represents arbitrary hex-encoded binary data. The ·value space· of hexBinary is the set of finite-length sequences 
    /// of binary octets.
    /// 
    /// hexBinary has the following ·constraining facets·: 
    ///     length
    ///     minLength
    ///     maxLength
    ///     pattern
    ///     enumeration
    ///     whiteSpace
    /// </summary>
    public class HexBinaryTypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String Val;

        #region Facet Restrictions Not Supported
        public override void SetMinInclusiveValue(string Val)
        {
            throw new Exception("Minimum Inclusive Value facet restriction not supported.");
        }

        public override void SetMinExclusiveValue(string Val)
        {
            throw new Exception("Minimum Exclusive Value facet restriction not supported.");
        }

        public override void SetMaxInclusiveValue(string Val)
        {
            throw new Exception("Maximum Inclusive Value facet restriction not supported.");
        }

        public override void SetMaxExclusiveValue(string Val)
        {
            throw new Exception("Maximum Exclusive Value facet restriction not supported.");
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

        public HexBinaryTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Val = o as String;
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

            // A hexBinary string must be properly encoded
            Regex HexNumbers = new Regex("[0-9a-fA-F]*");

            if(!HexNumbers.IsMatch(Val))
                return false;
            //try
            //{
            //    int i;
            //    for(i = Val.Length - 2; i >= 0; i -=2)
            //    {
            //        Byte.Parse(Val.Substring(i, 2)), System.Globalization.NumberStyles.HexNumber);
            //    }
            //    // If the hex number is an odd number of digits, the first digit is a single hex character
            //    if(i == -1)
            //    {
            //        Byte.Parse(Val.Substring(0, 1)), System.Globalization.NumberStyles.HexNumber);
            //    }                
            //}
            //catch (ArgumentNullException)
            //{
            //    return false;
            //}
            //catch (FormatException)
            //{
            //    return false;
            //}

            // Check length.
            if (!CheckLength((uint)Val.Length/2))
                return false;

            // Check pattern
            if (!CheckPattern(Val))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Val))
                return false;

            // Check Whitespace
            // This seems irrelevant

            return true;
        }
    }

    /// <summary>
    /// [Definition:]   anyURI represents a Uniform Resource Identifier Reference (URI). An anyURI value can be absolute or relative, and may have an 
    /// optional fragment identifier (i.e., it may be a URI Reference). This type should be used to specify the intention that the value fulfills the 
    /// role of a URI as defined by [RFC 2396], as amended by [RFC 2732]. 
    /// 
    /// anyURI has the following ·constraining facets·: 
    ///     length
    ///     minLength
    ///     maxLength
    ///     pattern
    ///     enumeration
    ///     whiteSpace
    /// </summary>
    public class AnyURITypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String Val;

        #region Facet Restrictions Not Supported
        public override void SetMinInclusiveValue(string Val)
        {
            throw new Exception("Minimum Inclusive Value facet restriction not supported.");
        }

        public override void SetMinExclusiveValue(string Val)
        {
            throw new Exception("Minimum Exclusive Value facet restriction not supported.");
        }

        public override void SetMaxInclusiveValue(string Val)
        {
            throw new Exception("Maximum Inclusive Value facet restriction not supported.");
        }

        public override void SetMaxExclusiveValue(string Val)
        {
            throw new Exception("Maximum Exclusive Value facet restriction not supported.");
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

        public AnyURITypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            // Object should be a String
            try
            {
                Val = o as String;
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

            // An anyURI string has quite a complicate syntax, see RFC 2396 (amended by RFC 2732)

            // Check length
            if (!CheckLength((uint)Val.Length))
                return false;

            // Check pattern
            if (!CheckPattern(Val))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(Val))
                return false;

            // Check Whitespace
            // This seems irrelevant

            return true;
        }
    }

}

