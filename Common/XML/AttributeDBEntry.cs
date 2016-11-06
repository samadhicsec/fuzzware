using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Common.XML.Restrictions;

namespace Fuzzware.Common.XML
{
    public class AttributeDBEntry : ObjectDBEntry
    {
        XmlSchemaAttribute oSchemaAttribute;
        XMLAttributeIdentifier oAttributeId;
        protected TypeRestrictor oTypeRestrictor;

        public AttributeDBEntry(XmlSchemaAttribute SchemaAttribute, uint uiDepth, XmlSchemaSet SchemaSet)
        {
            this.uiDepth = uiDepth;
            oSchemaSet = SchemaSet;
            oSchemaAttribute = SchemaAttribute;
            oAttributeId = new XMLAttributeIdentifier(oSchemaAttribute.QualifiedName, oSchemaAttribute, SchemaSet, true);
        }

        public override XMLObjectIdentifier ObjectId
        {
            get { return oAttributeId; }
        }

        public override XmlQualifiedName Name
        {
            get { return oSchemaAttribute.QualifiedName; }
        }

        public override XmlSchemaType SchemaType
        {
            // This returns the correct SchemaType even if the SchemaAttribute is a ref
            get { return oSchemaAttribute.AttributeSchemaType; }
        }

        public override DataSchemaTypeCode DataSchemaType
        {
            get { return XMLHelper.ConvertXmlTypeCodeToDataSchemaType(SchemaType.TypeCode); }
        }

        public override TypeRestrictor FacetRestrictor
        {
            get 
            {
                if (null == oTypeRestrictor)
                {
                    // Get the type restrictor (if the type is Simple, or a restriction of a Simple, then the BaseTypeCode will have a 
                    // proper value, otherwise BaseTypeCode will be None, and the TypeRestrictor.Validate will be a pass through function).
                    oTypeRestrictor = FacetRestrictions.GetTypeRestrictor(DataSchemaType);

                    XmlSchemaSimpleType SchType = oSchemaAttribute.AttributeSchemaType;
                    // Need to apply the restrictions
                    if (null != SchType)
                    {
                        if (null != SchType.Content)
                        {
                            if (SchType.Content is XmlSchemaSimpleTypeRestriction)
                            {
                                FacetRestrictions.Apply(oTypeRestrictor, (SchType.Content as XmlSchemaSimpleTypeRestriction).Facets);
                            }
                        }
                    }
                }
                return oTypeRestrictor;
            }
        }

        public override bool HasSimpleContent
        {
            get { return true; }
        }

        public override bool HasComplexContent
        {
            get { return true; }
        }

        public override bool IsComplexType
        {
            get { return false; }
        }

        public override bool IsSimpleType
        {
            get { return true; }
        }

        public override bool HasFixedValue
        {
            get 
            {
                if (!oSchemaAttribute.RefName.IsEmpty)
                    return !String.IsNullOrEmpty(XMLHelper.GetAttributeFromSchema(oSchemaSet, oSchemaAttribute).FixedValue);
                return !String.IsNullOrEmpty(oSchemaAttribute.FixedValue); 
            }
        }
    }
}
