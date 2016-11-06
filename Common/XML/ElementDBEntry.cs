using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Common.XML.Restrictions;

namespace Fuzzware.Common.XML
{
    public class ElementDBEntry : ParticleDBEntry
    {
        protected XMLElementIdentifier oElementId;
        protected XmlQualifiedName oName;
        protected DataSchemaTypeCode eSchemaType = DataSchemaTypeCode.None;
        protected TypeRestrictor oTypeRestrictor;
        protected XmlSchemaType oSchemaType;
        protected bool bHasSimpleContent;
        protected bool bHasComplexContent;
        protected bool bIsSimpleType;
        protected bool bIsComplexType;
        protected bool bHasFixedValue;

        public ElementDBEntry(XmlSchemaParticle oParticle, uint uiDepth, XmlSchemaSet SchemaSet, ObjectDataBase oObjectDataBase)
            : base(oParticle, uiDepth, SchemaSet, oObjectDataBase)
        {
            oTypeRestrictor = null;
            bHasSimpleContent = false;

            // If this particle contains other particles, add them as sub-particles
            if (oParticle is XmlSchemaElement)
            {
                XmlSchemaElement oSchemaElement = oParticle as XmlSchemaElement;
                // If the element is a reference to another element, point to the reference element
                if ((null != (oParticle as XmlSchemaElement).RefName) && !((oParticle as XmlSchemaElement).RefName.IsEmpty))
                    oSchemaElement = XMLHelper.GetElementFromSchema(SchemaSet, oSchemaElement);
                oName = oSchemaElement.QualifiedName;
                oSchemaType = XMLHelper.GetSchemaType(oSchemaElement);
                // It is imperative that minOccurs/maxOccurs comes from the particle and not element (since we might have changed the element
                // if it was a reference to another element)
                oElementId = new XMLElementIdentifier(oName, oSchemaElement, SchemaSet, (oParticle as XmlSchemaElement).MinOccurs, (oParticle as XmlSchemaElement).MaxOccurs);

                // Record if the element has a fixed value
                if (!String.IsNullOrEmpty(oSchemaElement.FixedValue))
                    bHasFixedValue = true;

                // Add to database of schema objects
                if (!oObjectDataBase.Contains(oElementId))
                {
                    oObjectDataBase.Add(oElementId, this);
                }
                else
                {
                    // This should NEVER occur, as the ParticleDBEntry should check for the existence of the ElementDBENtry before it tries
                    // to create a new one.
                    Log.Write(MethodBase.GetCurrentMethod(), "The Schema Object Database already contains this element", Log.LogType.Error);
                }

                if (oSchemaType is XmlSchemaComplexType)
                {
                    bIsComplexType = true;
                    if (null != (oSchemaType as XmlSchemaComplexType).ContentModel)
                    {
                        if ((oSchemaType as XmlSchemaComplexType).ContentModel is XmlSchemaSimpleContent)
                        {
                            bHasSimpleContent = true;
                            //XmlSchemaSimpleContent SimpleContent = (oSchemaType as XmlSchemaComplexType).ContentModel as XmlSchemaSimpleContent;
                            //// Set the schema type to be the base type
                            //if (SimpleContent.Content is XmlSchemaSimpleContentRestriction)
                            //{
                            //    XmlSchemaSimpleContentRestriction Restrictions = SimpleContent.Content as XmlSchemaSimpleContentRestriction;
                            //    oSchemaType = Restrictions.BaseType.BaseXmlSchemaType;
                            //}
                            //else
                            //{
                            //    XmlSchemaSimpleContentExtension Extensions = SimpleContent.Content as XmlSchemaSimpleContentExtension;
                            //    oSchemaType = XMLHelper.GetTypeFromSchema(Extensions.BaseTypeName, oSchemaSet, oObjectDataBase);
                            //}
                        }
                        else
                        {
                            bHasComplexContent = true;
                            // It's type is an extension or restriction of another complex type
                            XmlSchemaComplexContent ComplexContent = (oSchemaType as XmlSchemaComplexType).ContentModel as XmlSchemaComplexContent;
                            if (ComplexContent.Content is XmlSchemaComplexContentRestriction)
                            {
                                oSubParticles = new ParticleDBEntry[1];
                                oSubParticles[0] = new ParticleDBEntry((ComplexContent.Content as XmlSchemaComplexContentRestriction).Particle, uiDepth + 1, oSchemaSet, oObjectDataBase);
                            }
                            else if (ComplexContent.Content is XmlSchemaComplexContentExtension)
                            {
                                // Get the actual SchemaType from the base type
                                XmlSchemaComplexType SchComplexType = oSchemaSet.GlobalTypes[(ComplexContent.Content as XmlSchemaComplexContentExtension).BaseTypeName] as XmlSchemaComplexType;
                                // Remember the base particle
                                XmlSchemaParticle BaseParticle = SchComplexType.Particle;

                                XmlSchemaParticle SubParticle = null;
                                if (null != (ComplexContent.Content as XmlSchemaComplexContentExtension).Particle)
                                {
                                    XmlSchemaSequence ExtSeq = new XmlSchemaSequence();
                                    // The base particle might have no children and just contain attributes
                                    if(null != BaseParticle)
                                        ExtSeq.Items.Add(BaseParticle);
                                    // Add the extension particle
                                    ExtSeq.Items.Add((ComplexContent.Content as XmlSchemaComplexContentExtension).Particle);
                                    SubParticle = ExtSeq as XmlSchemaParticle;
                                }
                                else
                                    SubParticle = BaseParticle;

                                oSubParticles = new ParticleDBEntry[1];
                                oSubParticles[0] = new ParticleDBEntry(SubParticle, uiDepth + 1, oSchemaSet, oObjectDataBase);
                            }
                        }
                    }
                    else
                    {
                        // A normal complex type containing Sequence, Choice, All.
                        oSubParticles = new ParticleDBEntry[1];
                        oSubParticles[0] = new ParticleDBEntry((oSchemaType as XmlSchemaComplexType).Particle, uiDepth + 1, oSchemaSet, oObjectDataBase);
                    }

                    // Add all the attribute schema objects of this schema element
                    XmlSchemaComplexType oComplexSchemaType = oSchemaType as XmlSchemaComplexType;
                    foreach (DictionaryEntry AttDictEntry in oComplexSchemaType.AttributeUses)
                    {
                        XmlSchemaAttribute oSchemaAttribute = AttDictEntry.Value as XmlSchemaAttribute;
                        // If the schema attribute is a ref, this will be handled in XMLAttributeIdentifier
                        // Note this Id sets min and max occurs.
                        XMLAttributeIdentifier oAttElementId = new XMLAttributeIdentifier(oSchemaAttribute.QualifiedName, oSchemaAttribute, oSchemaSet, true);
                        if (!oObjectDataBase.Contains(oAttElementId))
                        {
                            oObjectDataBase.Add(oAttElementId, new AttributeDBEntry(oSchemaAttribute, uiDepth, oSchemaSet));
                        }
                    }
                }
                else
                {
                    // It's a simple type
                    bIsSimpleType = true;
                }
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "ElementDBEntry passed a particle that was not an XmlSchemaElement", Log.LogType.Error);
            }
        }

        public XmlSchemaElement ElementParticle
        {
            get 
            {
                if ((null != (oParticle as XmlSchemaElement).RefName) && !((oParticle as XmlSchemaElement).RefName.IsEmpty))
                {
                    // Get the reference element
                    XmlSchemaElement RefEle = oSchemaSet.GlobalElements[(oParticle as XmlSchemaElement).RefName] as XmlSchemaElement;
                    return RefEle;
                }
                return oParticle as XmlSchemaElement; 
            }
        }

        public override XMLObjectIdentifier ObjectId
        {
            get { return oElementId; }
        }

        /// <summary>
        /// Unhandled attributes occur on the element, and if that is a reference, on the referenced element, and on the type.
        /// </summary>
        public XmlAttribute[] UnhandledAttributes
        {
            get
            {
                if (null == oUnhandledAttributes)
                {
                    // We can get multiple same unhandled attributes declared (probably), but we need to compare names and not XmlAttribute
                    // objects (so can't use List)
                    Dictionary<String, XmlAttribute> temp = new Dictionary<string, XmlAttribute>();
                    // Get all unhandled attributes on the particle
                    if (null != oParticle.UnhandledAttributes)
                        for (int i = 0; i < oParticle.UnhandledAttributes.Length; i++)
                        {
                            temp.Add(oParticle.UnhandledAttributes[i].LocalName, oParticle.UnhandledAttributes[i]);
                        }
                    // Get all unhandled attributes if the element is a reference
                    if (oParticle is XmlSchemaElement)
                    {
                        if ((null != (oParticle as XmlSchemaElement).RefName) && !((oParticle as XmlSchemaElement).RefName.IsEmpty))
                        {
                            // Get the reference element
                            XmlSchemaElement RefEle = oSchemaSet.GlobalElements[(oParticle as XmlSchemaElement).RefName] as XmlSchemaElement;
                            if (null != RefEle.UnhandledAttributes)
                                for (int i = 0; i < RefEle.UnhandledAttributes.Length; i++)
                                {
                                    if (!temp.ContainsKey(RefEle.UnhandledAttributes[i].LocalName))
                                        temp.Add(RefEle.UnhandledAttributes[i].LocalName, RefEle.UnhandledAttributes[i]);
                                }
                        }
                    }
                    // Get all unhandled attributes from the type for this element
                    if (null != oSchemaType.UnhandledAttributes)
                        for (int i = 0; i < oSchemaType.UnhandledAttributes.Length; i++)
                        {
                            if (!temp.ContainsKey(oSchemaType.UnhandledAttributes[i].LocalName))
                                temp.Add(oSchemaType.UnhandledAttributes[i].LocalName, oSchemaType.UnhandledAttributes[i]);
                        }
                    // Copy to array
                    oUnhandledAttributes = (new List<XmlAttribute>(temp.Values)).ToArray();
                }
                return oUnhandledAttributes;
            }
        }

        public override XmlQualifiedName Name
        {
            get { return oName; }
        }

        public override DataSchemaTypeCode DataSchemaType
        {
            get
            {
                if (eSchemaType == DataSchemaTypeCode.None)
                {
                    eSchemaType = XMLHelper.ConvertXmlTypeCodeToDataSchemaType(oSchemaType.TypeCode);
                }
                return eSchemaType;
            }
        }

        public override XmlSchemaType SchemaType
        {
            get { return oSchemaType; }
        }

        public override TypeRestrictor FacetRestrictor
        {
            get
            {
                if (null == oTypeRestrictor)
                {
                    // Need to get the XmlTypeCode
                    XmlTypeCode BaseTypeCode = XmlTypeCode.None;
                    XmlSchemaObjectCollection ExtraFacets = null;
                    XmlSchemaSimpleType SchType = null;
                    if (oSchemaType is XmlSchemaSimpleType)
                    {
                        SchType = oSchemaType as XmlSchemaSimpleType;
                        BaseTypeCode = SchType.TypeCode;
                    }
                    else if (oSchemaType is XmlSchemaComplexType)
                    {
                        if ((oSchemaType as XmlSchemaComplexType).ContentModel is XmlSchemaSimpleContent)
                        {
                            // An extension or restriction of a simple type
                            XmlSchemaSimpleContent SimpleContent = (oSchemaType as XmlSchemaComplexType).ContentModel as XmlSchemaSimpleContent;
                            if (SimpleContent.Content is XmlSchemaSimpleContentRestriction)
                            {
                                XmlSchemaSimpleContentRestriction Restrictions = SimpleContent.Content as XmlSchemaSimpleContentRestriction;
                                SchType = (Restrictions.BaseType.BaseXmlSchemaType as XmlSchemaSimpleType);
                                BaseTypeCode = SchType.TypeCode;
                                ExtraFacets = Restrictions.Facets;
                            }

                        }
                    }

                    // Get the type restrictor (if the type is Simple, or a restriction of a Simple, then the BaseTypeCode will have a 
                    // proper value, otherwise BaseTypeCode will be None, and the TypeRestrictor.Validate will be a pass through function).
                    oTypeRestrictor = FacetRestrictions.GetTypeRestrictor(XMLHelper.ConvertXmlTypeCodeToDataSchemaType(BaseTypeCode));

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
                        if (null != ExtraFacets)
                        {
                            FacetRestrictions.Apply(oTypeRestrictor, ExtraFacets);
                        }
                    }

                }
                return oTypeRestrictor;
            }
        }

        public override bool HasSimpleContent
        {
            get { return bHasSimpleContent; }
        }

        public override bool HasComplexContent
        {
            get { return bHasComplexContent; }
        }

        public override bool IsComplexType
        {
            get { return bIsComplexType; }
        }

        public override bool IsSimpleType
        {
            get { return bIsSimpleType; }
        }

        public override bool HasFixedValue
        {
            get { return bHasFixedValue; }
        }
    }
}
