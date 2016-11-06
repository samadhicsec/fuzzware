using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

// Definitions from http://www.w3.org/TR/xmlschema-2/

namespace Fuzzware.Common.DataSchema.Restrictions
{
    /// <summary>
    /// [Definition:]  The string datatype represents character strings in XML. The ·value space· of string is the set of finite-length sequences of 
    /// characters (as defined in [XML 1.0 (Second Edition)]) that ·match· the Char production from [XML 1.0 (Second Edition)]. A character is an 
    /// atomic unit of communication; it is not further specified except to note that every character has a corresponding Universal Character Set 
    /// code point, which is an integer.
    /// 
    /// string has the following ·constraining facets·: 
    ///     length
    ///     minLength
    ///     maxLength
    ///     pattern
    ///     enumeration
    ///     whiteSpace
    /// </summary>
    public class StringTypeRestrictor : AnySimpleTypeRestrictor
    {
        protected String str;

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

        public StringTypeRestrictor() : base()
        {
            str = "";
        }


        public override bool Validate(object o)
        {
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
            }

            // There are no implicit restrictions on strings, they can be any finite character sequence

            // Check length
            if (!CheckLength((uint)str.Length))
                return false;

            // Check pattern
            if (!CheckPattern(str))
                return false;

            // Check Enumeration
            if (!CheckEnumeration(str))
                return false;

            // Check Whitespace
            // This seems irrelevant

            return true;
        }      
    }

    /// <summary>
    /// [Definition:]   normalizedString represents white space normalized strings. The ·value space· of normalizedString is the set of strings 
    /// that do not contain the carriage return (#xD), line feed (#xA) nor tab (#x9) characters. The ·lexical space· of normalizedString is the 
    /// set of strings that do not contain the carriage return (#xD), line feed (#xA) nor tab (#x9) characters. The ·base type· of 
    /// normalizedString is string.
    /// </summary>
    class NormalisedStringTypeRestrictor : StringTypeRestrictor
    {
        public NormalisedStringTypeRestrictor() : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // Ensure there are not any carriage returns, line feeds or tabs
            if (-1 != str.IndexOf((char)0xD))
                return false;
            if (-1 != str.IndexOf((char)0xA))
                return false;
            if (-1 != str.IndexOf((char)0x9))
                return false;

            // No extra facet restrictions exist for normalisedStrings
            return true;
        }
    }

    /// <summary>
    /// [Definition:]   token represents tokenized strings. The ·value space· of token is the set of strings that do not contain the carriage 
    /// return (#xD), line feed (#xA) nor tab (#x9) characters, that have no leading or trailing spaces (#x20) and that have no internal 
    /// sequences of two or more spaces. The ·lexical space· of token is the set of strings that do not contain the carriage return (#xD), 
    /// line feed (#xA) nor tab (#x9) characters, that have no leading or trailing spaces (#x20) and that have no internal sequences of two or 
    /// more spaces. The ·base type· of token is normalizedString.
    /// </summary>
    class TokenTypeRestrictor : NormalisedStringTypeRestrictor
    {
        public TokenTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // Ensure there are no leading or trailing spaces
            if (str.StartsWith(" "))
                return false;
            if (str.EndsWith(" "))
                return false;

            // Ensure no sequences of 2 or more spaces
            if (-1 != str.IndexOf("  "))
                return false;

            // No extra facet restrictions exist for Tokens
            return true;
        }
    }


    /// <summary>
    /// [Definition:]   language represents natural language identifiers as defined by by [RFC 3066] . The ·value space· of language is the set 
    /// of all strings that are valid language identifiers as defined [RFC 3066] . The ·lexical space· of language is the set of all strings 
    /// that conform to the pattern [a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})* . The ·base type· of language is token.
    /// </summary>
    class LanguageTypeRestrictor : TokenTypeRestrictor
    {
        public LanguageTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // Ensure the string matches the regular expression
            Regex reg = new Regex("[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*");

            if (!reg.IsMatch(str))
                return false;
            
            // No extra facet restrictions exist for Languages
            return true;
        }
    }


    /// <summary>
    /// [Definition: A Name is a token beginning with a letter or one of a few punctuation characters, and continuing with letters, digits, 
    /// hyphens, underscores, colons, or full stops, together known as name characters.] Names beginning with the string " xml", or any 
    /// string which would match (('X'|'x') ('M'|'m') ('L'|'l')) , are reserved for standardization in this or future versions of this 
    /// specification.
    /// 
    /// [4]    NameChar    ::=     Letter | Digit | '.' | '-' | '_' | ':' | CombiningChar | Extender 
    /// [5]    Name    ::=    (Letter | '_' | ':') ( NameChar)* 
    /// </summary>
    class NameTypeRestrictor : TokenTypeRestrictor
    {
        public NameTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // Make sure the name does not start with XML
            if (str.StartsWith("xml", StringComparison.CurrentCultureIgnoreCase))
                return false;

            // Ensure the string matches the regular expression
            Regex reg = new Regex("([a-zA-Z_:]){1}[a-zA-Z0-9.-_:]*");

            if (!reg.IsMatch(str))
                return false;

            // No extra facet restrictions exist for Names
            return true;
        }
    }


    /// <summary>
    /// An Nmtoken (name token) is any mixture of name characters.
    /// [4]    NameChar    ::=     Letter | Digit | '.' | '-' | '_' | ':' | CombiningChar | Extender 
    /// [7]    Nmtoken    ::=    (NameChar)+ 
    /// </summary>
    class NMTokenTypeRestrictor : TokenTypeRestrictor
    {
        public NMTokenTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // Ensure the string matches the regular expression
            Regex reg = new Regex("[a-zA-Z0-9.-_:]*");

            if (!reg.IsMatch(str))
                return false;

            // No extra facet restrictions exist for NMTokens
            return true;
        }
    }


    /// <summary>
    /// [Definition:]   NCName represents XML "non-colonized" Names. The ·value space· of NCName is the set of all strings which ·match· the 
    /// NCName production of [Namespaces in XML]. The ·lexical space· of NCName is the set of all strings which ·match· the NCName production 
    /// of [Namespaces in XML]. The ·base type· of NCName is Name. 
    /// 
    /// [4]  NCName ::=  (Letter | '_') (NCNameChar)*                                       /*  An XML Name, minus the ":" */ 
    /// [5]  NCNameChar ::=  Letter | Digit | '.' | '-' | '_' | CombiningChar | Extender 
    /// </summary>
    class NCNameTypeRestrictor : NameTypeRestrictor
    {
        public NCNameTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // The same as its base (Name), except for the ':'
            if (-1 != str.IndexOf(':'))
                return false;
            
            // No extra facet restrictions exist for NCNames
            return true;
        }
    }


    /// <summary>
    /// [Definition:]   ID represents the ID attribute type from [XML 1.0 (Second Edition)]. The ·value space· of ID is the set of all 
    /// strings that ·match· the NCName production in [Namespaces in XML]. The ·lexical space· of ID is the set of all strings that 
    /// ·match· the NCName production in [Namespaces in XML]. The ·base type· of ID is NCName. 
    /// 
    /// For compatibility (see Terminology (§1.4)) ID should be used only on attributes.
    /// </summary>
    class IDTypeRestrictor : NCNameTypeRestrictor
    {
        public IDTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // No extra facet restrictions exist for IDs
            return true;
        }
    }


    /// <summary>
    /// [Definition:]   IDREF represents the IDREF attribute type from [XML 1.0 (Second Edition)]. The ·value space· of IDREF is the set of 
    /// all strings that ·match· the NCName production in [Namespaces in XML]. The ·lexical space· of IDREF is the set of strings that ·match· 
    /// the NCName production in [Namespaces in XML]. The ·base type· of IDREF is NCName. 
    /// 
    /// For compatibility (see Terminology (§1.4)) this datatype should be used only on attributes.
    /// </summary>
    class IDREFTypeRestrictor : NCNameTypeRestrictor
    {
        public IDREFTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // No extra facet restrictions exist for IDREFs
            return true;
        }
    }


    /// <summary>
    /// [Definition:]   ENTITY represents the ENTITY attribute type from [XML 1.0 (Second Edition)]. The ·value space· of ENTITY is the set of all 
    /// strings that ·match· the NCName production in [Namespaces in XML] and have been declared as an unparsed entity in a document type 
    /// definition. The ·lexical space· of ENTITY is the set of all strings that ·match· the NCName production in [Namespaces in XML]. The ·base 
    /// type· of ENTITY is NCName.
    /// Note:  The ·value space· of ENTITY is scoped to a specific instance document. 
    /// For compatibility (see Terminology (§1.4)) ENTITY should be used only on attributes. 
    /// </summary>
    class ENTITYTypeRestrictor : NCNameTypeRestrictor
    {
        public ENTITYTypeRestrictor()
            : base()
        {

        }

        public override bool Validate(object o)
        {
            if (!base.Validate(o))
                return false;

            // No extra facet restrictions exist for ENTITYs
            return true;
        }
    }
}
