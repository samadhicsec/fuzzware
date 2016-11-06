using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Common.XML.Restrictions
{
    public class FacetRestrictions
    {
        public static void Apply(IFacetRestrictions FacetRestrictions, XmlSchemaObjectCollection Facets)
        {
            foreach (XmlSchemaFacet Facet in Facets)
            {
                if (Facet is XmlSchemaLengthFacet)
                {
                    FacetRestrictions.SetExactLength(Facet.Value);
                }
                else if (Facet is XmlSchemaMinLengthFacet)
                {
                    FacetRestrictions.SetMinLength(Facet.Value);
                }
                else if (Facet is XmlSchemaMaxLengthFacet)
                {
                    FacetRestrictions.SetMaxLength(Facet.Value);
                }
                else if (Facet is XmlSchemaPatternFacet)
                {
                    FacetRestrictions.SetPattern(Facet.Value);
                }
                else if (Facet is XmlSchemaEnumerationFacet)
                {
                    FacetRestrictions.SetEnumerationValue(Facet.Value);
                }
                else if (Facet is XmlSchemaMaxInclusiveFacet)
                {
                    FacetRestrictions.SetMaxInclusiveValue(Facet.Value);
                }
                else if (Facet is XmlSchemaMaxExclusiveFacet)
                {
                    FacetRestrictions.SetMaxExclusiveValue(Facet.Value);
                }
                else if (Facet is XmlSchemaMinInclusiveFacet)
                {
                    FacetRestrictions.SetMinInclusiveValue(Facet.Value);
                }
                else if (Facet is XmlSchemaMinExclusiveFacet)
                {
                    FacetRestrictions.SetMinExclusiveValue(Facet.Value);
                }
                else if (Facet is XmlSchemaFractionDigitsFacet)
                {
                    FacetRestrictions.SetMaxFracDigits(Facet.Value);
                }
                else if (Facet is XmlSchemaTotalDigitsFacet)
                {
                    FacetRestrictions.SetMaxTotalDigits(Facet.Value);
                }
                else if (Facet is XmlSchemaWhiteSpaceFacet)
                {
                    // Intentionally do nothing, this does not appear relevant to fuzzing.
                }
            }
        }

        public static TypeRestrictor GetTypeRestrictor(DataSchemaTypeCode TypeCode)
        {
            switch (TypeCode)
            {
                // Run string fuzzers on all the following
                case DataSchemaTypeCode.AnyAtomicType:
                    return new AnySimpleTypeRestrictor();
                case DataSchemaTypeCode.String:
                    return new StringTypeRestrictor();
                case DataSchemaTypeCode.NormalizedString:
                    return new NormalisedStringTypeRestrictor();
                case DataSchemaTypeCode.Token:
                    return new TokenTypeRestrictor();
                case DataSchemaTypeCode.Language:
                    return new LanguageTypeRestrictor();
                case DataSchemaTypeCode.Name:
                    return new NameTypeRestrictor();
                case DataSchemaTypeCode.NmToken:
                    return new NMTokenTypeRestrictor();
                case DataSchemaTypeCode.NCName:
                    return new NCNameTypeRestrictor();
                case DataSchemaTypeCode.Id:
                    return new IDTypeRestrictor();
                case DataSchemaTypeCode.Idref:
                    return new IDREFTypeRestrictor();
                case DataSchemaTypeCode.Entity:
                    return new ENTITYTypeRestrictor();
                case DataSchemaTypeCode.Date:
                    return new DateRestrictor();
                case DataSchemaTypeCode.Time:
                    return new TimeRestrictor();
                case DataSchemaTypeCode.DateTime:
                    return new DateTimeRestrictor();
                case DataSchemaTypeCode.GDay:
                    return new DayRestrictor();
                case DataSchemaTypeCode.GMonth:
                    return new MonthRestrictor();
                case DataSchemaTypeCode.GMonthDay:
                    return new MonthDayRestrictor();
                case DataSchemaTypeCode.GYear:
                    return new YearRestrictor();
                case DataSchemaTypeCode.GYearMonth:
                    return new YearMonthRestrictor();
                case DataSchemaTypeCode.Duration:
                    return new AnySimpleTypeRestrictor();   // TODO: Implement Duration TypeREstrictors
                case DataSchemaTypeCode.Decimal:
                    return new DecimalTypeRestrictor();
                case DataSchemaTypeCode.Float:
                    return new SingleTypeRestrictor();
                case DataSchemaTypeCode.Double:
                    return new DoubleTypeRestrictor();
                case DataSchemaTypeCode.Integer:
                    return new IntegerTypeRestrictor();
                case DataSchemaTypeCode.NonPositiveInteger:
                    return new NonPositiveIntegerTypeRestrictor();
                case DataSchemaTypeCode.NegativeInteger:
                    return new NegativeIntegerTypeRestrictor();
                case DataSchemaTypeCode.NonNegativeInteger:
                    return new NonNegativeIntegerTypeRestrictor();
                case DataSchemaTypeCode.PositiveInteger:
                    return new PositiveIntegerTypeRestrictor();
                case DataSchemaTypeCode.Long:
                    return new LongTypeRestrictor();
                case DataSchemaTypeCode.Int:
                    return new IntTypeRestrictor();
                case DataSchemaTypeCode.Short:
                    return new ShortTypeRestrictor();
                case DataSchemaTypeCode.Byte:
                    return new ByteTypeRestrictor();
                case DataSchemaTypeCode.UnsignedLong:
                    return new UnsignedLongTypeRestrictor();
                case DataSchemaTypeCode.UnsignedInt:
                    return new UnsignedIntTypeRestrictor();
                case DataSchemaTypeCode.UnsignedShort:
                    return new UnsignedShortTypeRestrictor();
                case DataSchemaTypeCode.UnsignedByte:
                    return new UnsignedByteTypeRestrictor();
                case DataSchemaTypeCode.Boolean:
                    return new BooleanTypeRestrictor();
                case DataSchemaTypeCode.None:
                    return new AnySimpleTypeRestrictor();
                case DataSchemaTypeCode.Base64:
                    return new Base64BinaryTypeRestrictor();
                case DataSchemaTypeCode.Hex:
                    return new HexBinaryTypeRestrictor();
                case DataSchemaTypeCode.AnyUri:
                    return new AnyURITypeRestrictor();
                default:
                    Log.Write(MethodBase.GetCurrentMethod(), "The type of '" + Enum.GetName(typeof(DataSchemaTypeCode), TypeCode) + "' is not supported", Log.LogType.Error);
                    break;
            }
            return null;
        }

        /// <summary>
        /// Returns the XmlSchemaEnumerationFacet of the XmlSchemaSimpleType if it has one, otherwise returns null.
        /// </summary>
        public static XmlSchemaEnumerationFacet HasEnumerationFacet(XmlSchemaSimpleType oSimpleType)
        {
            if (oSimpleType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction oRestrictions = oSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                foreach (XmlSchemaFacet oFacet in oRestrictions.Facets)
                {
                    if (oFacet is XmlSchemaEnumerationFacet)
                    {
                        return oFacet as XmlSchemaEnumerationFacet;
                    }
                }
            }
            return null;
        }
    }
}
