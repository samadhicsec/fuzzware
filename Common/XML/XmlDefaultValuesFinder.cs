﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common;
using Fuzzware.Common.Encoding;

namespace Fuzzware.Schemas.AutoGenerated
{
    public partial class XmlDefaultValues
    {
        #region Old Code
        /*
        public String FindValueForNode(String NodeName, XmlTypeCode eTypeCode)
        {
            return FindValueForNode(NodeName, eTypeCode, null);
        }

        public String FindValueForNode(String NodeName, XmlTypeCode eTypeCode, String ParentNodeName)
        {
            switch (eTypeCode)
            {
                case XmlTypeCode.Byte:
                case XmlTypeCode.Int:
                case XmlTypeCode.Integer:
                case XmlTypeCode.Long:
                case XmlTypeCode.NegativeInteger:
                case XmlTypeCode.NonNegativeInteger:
                case XmlTypeCode.NonPositiveInteger:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.Short:
                case XmlTypeCode.UnsignedByte:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.UnsignedShort:
                    return FindIntegerValueForNode(NodeName, eTypeCode, ParentNodeName);
                case XmlTypeCode.Decimal:
                case XmlTypeCode.Double:
                case XmlTypeCode.Float:
                    return FindDecimalValueForNode(NodeName, eTypeCode, ParentNodeName);
                case XmlTypeCode.HexBinary:
                    return FindHexValueForNode(NodeName, eTypeCode, ParentNodeName);
                case XmlTypeCode.Boolean:
                    return "true";
                case XmlTypeCode.String:
                default:
                    return FindStringValueForNode(NodeName, eTypeCode, ParentNodeName);
            }
        }

        private String FindIntegerValueForNode(String NodeName, XmlTypeCode eTypeCode, String ParentNodeName)
        {
            String Val = FindValue(NodeName, ParentNodeName, IntegerValues.IntegerValue);
            bool bUseUnsigned = true;
            UInt64 UTypedVal = 0;
            Int64 TypedVal = 0;

            if (String.IsNullOrEmpty(Val))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find a given default value for '" + NodeName + "'" +
                    (String.IsNullOrEmpty(ParentNodeName)?"":("(with parent '" + ParentNodeName + "')")) + 
                    ", using generic default value '" + IntegerValues.DefaultValue + "'", Log.LogType.LogOnlyInfo);
                Val = IntegerValues.DefaultValue;
            }

            // Try to convert string to integer
            if (!UInt64.TryParse(Val, out UTypedVal))
            {
                if (!Int64.TryParse(Val, out TypedVal))
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + Val + "' to integer for '" + NodeName + "'" +
                        (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                        ", using generic default value '" + IntegerValues.DefaultValue + "'", Log.LogType.LogOnlyInfo);
                    return IntegerValues.DefaultValue.ToString();
                }
                bUseUnsigned = false;
            }

            // Note, if would couldn't convert the string into an unsigned long then it must be negative
            if (!bUseUnsigned)
                UTypedVal = (ulong)(-1 * TypedVal);
            // Cast the positive value to the correct type
            switch (eTypeCode)
            {
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.Int:
                    UTypedVal = (uint)UTypedVal;
                    break;
                case XmlTypeCode.UnsignedShort:
                case XmlTypeCode.Short:
                    UTypedVal = (ushort)UTypedVal;
                    break;
                case XmlTypeCode.UnsignedByte:
                case XmlTypeCode.Byte:
                    UTypedVal = (byte)UTypedVal;
                    break;
            }
            // Assign the correct sign and return the value
            if (bUseUnsigned)
                return UTypedVal.ToString();
            else
                return (-1 * ((long)UTypedVal)).ToString();
        }

        private String FindDecimalValueForNode(String NodeName, XmlTypeCode eTypeCode, String ParentNodeName)
        {
            String Val = FindValue(NodeName, ParentNodeName, DecimalValues.DecimalValue);
            Decimal TypedVal = 0;
            if (String.IsNullOrEmpty(Val))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find a given default value for '" + NodeName + "'" +
                    (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                    ", using generic default value '" + DecimalValues.DefaultValue + "'", Log.LogType.LogOnlyInfo);
                return DecimalValues.DefaultValue.ToString();
            }

            // Try to convert string to decimal
            if (!Decimal.TryParse(Val, out TypedVal))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + Val + "' to decimal for '" + NodeName + "'" +
                    (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                    ", using generic default value '" + DecimalValues.DefaultValue + "'", Log.LogType.LogOnlyInfo);
                return DecimalValues.DefaultValue.ToString();
            }

            switch (eTypeCode)
            {
                case XmlTypeCode.Decimal:
                    return TypedVal.ToString();
                case XmlTypeCode.Float:
                    return ((float)TypedVal).ToString();
                case XmlTypeCode.Double:
                    return ((double)TypedVal).ToString();
            }
            return null;
        }

        private String FindHexValueForNode(String NodeName, XmlTypeCode eTypeCode, String ParentNodeName)
        {
            String Val = FindValue(NodeName, ParentNodeName, BinaryValues.BinaryValue);
            byte[] TypedVal = null;
            HexCoder oHexCoder = new HexCoder();
            if (String.IsNullOrEmpty(Val))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find a given default value for '" + NodeName + "'" +
                    (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                    ", using generic default value '" + oHexCoder.Encode(BinaryValues.DefaultValue) + "'", Log.LogType.LogOnlyInfo);
                TypedVal = BinaryValues.DefaultValue;
            }

            // Try to convert string to hex
            if(null != TypedVal)
            {
                try
                {
                    TypedVal = oHexCoder.DecodeToBytes(Val);
                
                }
                catch
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + Val + "' to hex for '" + NodeName + "'" +
                        (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                        ", using generic default value '" + oHexCoder.Encode(BinaryValues.DefaultValue) + "'", Log.LogType.LogOnlyInfo);
                    TypedVal = BinaryValues.DefaultValue;
                }
            }

            return oHexCoder.Encode(TypedVal);
        }

        private String FindStringValueForNode(String NodeName, XmlTypeCode eTypeCode, String ParentNodeName)
        {
            String Val = FindValue(NodeName, ParentNodeName, StringValues.StringValue);

            if (String.IsNullOrEmpty(Val))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find a given default value for '" + NodeName + "'" +
                    (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                    ", using generic default value '" + StringValues.DefaultValue + "'", Log.LogType.LogOnlyInfo);
                Val = StringValues.DefaultValue;
            }

            return Val;
        }

        /// <summary>
        /// Attempts to match the Nodename and ParentNodeName to an entry in the tValues[] array.
        /// </summary>
        private String FindValue(String NodeName, String ParentNodeName, tValue[] Values)
        {
            if (null == Values)
                return null;

            // Search through the array for a matching NodeName and ParentNodeName (if it was passed in)

            bool bCheckParent = !String.IsNullOrEmpty(ParentNodeName);

            for (int i = 0; i < Values.Length; i++)
            {
                // Check the parent first because that is the easiest and quickest check
                if (bCheckParent && !String.IsNullOrEmpty(Values[i].NodeName.ParentsNameEquals))
                    if(!Values[i].NodeName.ParentsNameEquals.Equals(ParentNodeName, StringComparison.CurrentCulture))
                        continue;

                // Match the NodeName against the this tValue
                if (FindMatch(NodeName, Values[i].NodeName))
                    return Values[i].Value;
            }

            return null;
        }

        /// <summary>
        /// Attempts to match the Nodename and ParentNodeName to an entry in the AnyTypeValuesAnyTypeValue[] array.
        /// </summary>
        public XmlQualifiedName FindAnyTypeValueForNode(String NodeName, String ParentNodeName)
        {
            AnyTypeValuesAnyTypeValue[] Values = AnyTypeValues.AnyTypeValue;

            if (null != Values)
            {
                // Search through the array for a matching NodeName and ParentNodeName (if it was passed in)
                bool bCheckParent = !String.IsNullOrEmpty(ParentNodeName);

                for (int i = 0; i < Values.Length; i++)
                {
                    // Check the parent first because that is the easiest and quickest check
                    if (bCheckParent && !String.IsNullOrEmpty(Values[i].NodeName.ParentsNameEquals))
                        if (!Values[i].NodeName.ParentsNameEquals.Equals(ParentNodeName, StringComparison.CurrentCulture))
                            continue;

                    if (FindMatch(NodeName, Values[i].NodeName))
                    {
                        return Values[i].Type;
                    }
                }
            }

            
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find a given default value for '" + NodeName + "'" +
                (String.IsNullOrEmpty(ParentNodeName) ? "" : ("(with parent '" + ParentNodeName + "')")) +
                ", using generic default value for anyType", Log.LogType.LogOnlyInfo);
            return AnyTypeValues.DefaultValue;
        }

        /// <summary>
        /// Returns true if the Node name matches the name in the NodeName object
        /// </summary>
        private bool FindMatch(String NodeName, NodeName MatchName)
        {
            // Check rhe NodeName using the specified method
            switch (MatchName.MatchMethod)
            {
                case NodeNameMatchMethod.Equals:
                    if (NodeName.Equals(MatchName.Value, StringComparison.CurrentCulture))
                        return true;
                    break;
                case NodeNameMatchMethod.StartsWith:
                    if (NodeName.StartsWith(MatchName.Value, StringComparison.CurrentCulture))
                        return true;
                    break;
                case NodeNameMatchMethod.EndsWith:
                    if (NodeName.EndsWith(MatchName.Value, StringComparison.CurrentCulture))
                        return true;
                    break;
                case NodeNameMatchMethod.Contains:
                    // Note Contains does a case-sensitive and culture-insensitive comparison, so convert to lower
                    // to do a case-insensitive match
                    if (NodeName.ToLowerInvariant().Contains(MatchName.Value.ToLowerInvariant()))
                        return true;
                    break;
                case NodeNameMatchMethod.RegEx:
                    try
                    {
                        Regex oRegex = new Regex(MatchName.Value);

                        if (oRegex.IsMatch(NodeName))
                            return true;
                    }
                    catch (Exception e)
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Error processing RegEx '" + MatchName.Value + "' for NodeName '"
                            + NodeName + "'" + Environment.NewLine + e.Message, Log.LogType.Error);
                    }
                    break;
            }
            return false;
        }
        */
        #endregion

        public String DefaultValueForType(XmlTypeCode eTypeCode)
        {
            switch (eTypeCode)
            {
                case XmlTypeCode.Byte:
                case XmlTypeCode.Int:
                case XmlTypeCode.Integer:
                case XmlTypeCode.Long:
                case XmlTypeCode.NegativeInteger:
                case XmlTypeCode.NonNegativeInteger:
                case XmlTypeCode.NonPositiveInteger:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.Short:
                case XmlTypeCode.UnsignedByte:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.UnsignedShort:
                    return IntegerDefaultValue.ToString();
                case XmlTypeCode.Decimal:
                case XmlTypeCode.Double:
                case XmlTypeCode.Float:
                    return DecimalDefaultValue.ToString();
                case XmlTypeCode.HexBinary:
                    HexCoder oHexCoder = new HexCoder();
                    return oHexCoder.Encode(BinaryDefaultValue); ;
                case XmlTypeCode.Boolean:
                    return "true";
                case XmlTypeCode.String:
                default:
                    return StringDefaultValue;
            }
        }

        public XmlQualifiedName DefaultValueForAnyType()
        {
            return AnyTypeDefaultType;
        }

        /// <summary>
        /// Update the nodes in XmlDocument with the default values specified.  Only XmlElements without children are updated.
        /// </summary>
        public void UpdateDefaultValues(XmlDocument oXmlDoc, XmlNamespaceManager oNSmanager)
        {
            if (null == DefaultValues)
                return;

            for (int i = 0; i < DefaultValues.Length; i++)
            {
                if (String.IsNullOrEmpty(DefaultValues[i].XPath))
                    continue;

                XmlNodeList oNodeList = null;
                try
                {
                    oNodeList = oXmlDoc.SelectNodes(DefaultValues[i].XPath, oNSmanager);
                }
                catch (Exception e)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "The error '" + e.Message + "' occurred using the XPath '" + DefaultValues[i].XPath + "'", Log.LogType.Warning);
                    continue;
                }
                if (null == oNodeList)
                    continue;
                if(0 == oNodeList.Count)
                    Log.Write(MethodBase.GetCurrentMethod(), "The default values XPath '" + DefaultValues[i].XPath + "' did not return any nodes", Log.LogType.LogOnlyInfo);

                foreach (XmlNode oXmlNode in oNodeList)
                {
                    if (oXmlNode is XmlElement)
                    {
                        if (oXmlNode.HasChildNodes && (oXmlNode.ChildNodes.Count == 1) &&
                            ((oXmlNode.FirstChild is XmlText) || (oXmlNode.FirstChild is XmlCDataSection)))
                        {
                            XPathNavigator oXPathNav = (oXmlNode as XmlElement).CreateNavigator();
                            oXPathNav.SetValue((null == DefaultValues[i].Value) ? "" : DefaultValues[i].Value);
                        }
                    }
                }
            }
        }
    }
}
