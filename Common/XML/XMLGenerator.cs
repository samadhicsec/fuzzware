﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Common.XML
{
    /// <summary>
    /// Generates XML based on an XML Schema and default values for nodes.
    /// </summary>
    public class XMLGenerator
    {
        XmlSchemaSet oXmlSchemaSet;
        String Prefix;
        XmlQualifiedName RootNode;
        XmlNamespaceManager oNSManager;
        XmlDefaultValues oXmlDefaultValues;

        public XMLGenerator(XmlSchemaSet XmlSchemaSet, String prefix, XmlQualifiedName RootNodeName, XmlNamespaceManager NSManager, XmlDefaultValues oDefVals)
        {
            oXmlSchemaSet = XmlSchemaSet;
            Prefix = prefix;
            RootNode = RootNodeName;
            oNSManager = NSManager;
            oXmlDefaultValues = oDefVals;

            if (!oXmlSchemaSet.IsCompiled)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "XmlSchemaSet is not compiled", Log.LogType.Error);
            }
        }

        /// <summary>
        /// Generates an XML document based on an XML Schema and default node values
        /// </summary>
        /// <returns>Generated XML document</returns>
        public XmlDocument Generate()
        {
            // Create a new XmlDocument
            XmlDocument oXmlDoc = new XmlDocument();

            // Add Xml declaration
            XmlDeclaration Decl = oXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            oXmlDoc.AppendChild(Decl);

            // Find the root element
            if(!oXmlSchemaSet.GlobalElements.Contains(RootNode))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Specified root node name '" + RootNode.ToString() + "' does not exist in Schema set", Log.LogType.Error);
            }
            XmlSchemaElement oRootSchemaElement = oXmlSchemaSet.GlobalElements[RootNode] as XmlSchemaElement;
            
            // Create the root element
            XmlElement Element = oXmlDoc.CreateElement(Prefix, RootNode.Name, RootNode.Namespace);

            // Generate the children of the root element
            if (oRootSchemaElement.SchemaType is XmlSchemaComplexType)
                GenerateChildrenOfComplexType((oRootSchemaElement.SchemaType as XmlSchemaComplexType).Particle, Element);
            else
                GenerateElement(oRootSchemaElement, Element);

            oXmlDoc.AppendChild(Element);

            // Update the values of the elements with any specified default values
            oXmlDefaultValues.UpdateDefaultValues(oXmlDoc, oNSManager);

            return oXmlDoc;
        }

        /// <summary>
        /// Generates children for a complex type
        /// </summary>
        private void GenerateChildrenOfComplexType(XmlSchemaParticle oParentSchemaParticle, XmlElement oParentElement)
        {
            // Determine if the particle is a sequence, choice, all, GroupRef or Any
            // Check for the 3 GroupBase types
            if (oParentSchemaParticle is XmlSchemaSequence)
            {
                GenerateChildrenForSequenceParticle(oParentSchemaParticle as XmlSchemaSequence, oParentElement);
            }
            else if (oParentSchemaParticle is XmlSchemaChoice)
            {
                GenerateChildrenForChoiceParticle(oParentSchemaParticle as XmlSchemaChoice, oParentElement);
            }
            else if (oParentSchemaParticle is XmlSchemaAll)
            {
                GenerateChildrenForAllParticle(oParentSchemaParticle as XmlSchemaAll, oParentElement);
            }
            // Check for 2 of the 3 other particle types
            else if (oParentSchemaParticle is XmlSchemaGroupRef)
            {
                XmlQualifiedName GroupRefName = (oParentSchemaParticle as XmlSchemaGroupRef).RefName;

                if (!oXmlSchemaSet.GlobalElements.Contains(GroupRefName))
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not find '" + GroupRefName.ToString() + "' in the list of global elements", Log.LogType.Error);
                XmlSchemaGroup oSchemaGroup = oXmlSchemaSet.GlobalElements[GroupRefName] as XmlSchemaGroup;

                GenerateChildrenOfComplexType(oSchemaGroup.Particle, oParentElement);
            }
            else if (oParentSchemaParticle is XmlSchemaAny)
            {
                XmlSchemaAny oAny = oParentSchemaParticle as XmlSchemaAny;
                // If we have to add an Any element then we add a random one
                if (oAny.MinOccurs > 0)
                {
                    XmlElement oXmlElement = oParentElement.OwnerDocument.CreateElement("Random");
                    oParentElement.AppendChild(oXmlElement);
                }
            }
        }

        /// <summary>
        /// Generate children for a sequence particle
        /// </summary>
        private void GenerateChildrenForSequenceParticle(XmlSchemaSequence oSchemaSequence, XmlElement oParentElement)
        {
            // We need to repeatedly create this sequence for a maximum of MinOccurs or 1
            for (int i = 0; i < ((oSchemaSequence.MinOccurs > 1)?(int)oSchemaSequence.MinOccurs:1); i++)
            {
                // Got through each XmlSchemaObject in the sequence
                for (int j = 0; j < oSchemaSequence.Items.Count; j++)
                {
                    XmlSchemaObject oSchemaObj = oSchemaSequence.Items[j];
                    if (oSchemaObj is XmlSchemaElement)
                    {
                        // Generate an actual element
                        GenerateElement(oSchemaObj as XmlSchemaElement, oParentElement);
                    }
                    else
                    {
                        // Generate a group of element
                        GenerateChildrenOfComplexType(oSchemaObj as XmlSchemaParticle, oParentElement);
                    }
                }
            }
        }

        /// <summary>
        /// Generate children for a choice particle
        /// </summary>
        private void GenerateChildrenForChoiceParticle(XmlSchemaChoice oSchemaChoice, XmlElement oParentElement)
        {
            // We need to repeatedly create this choice for the minimum of maxOccurs and oSchemaChoice.Items.Count
            int Occurrence = (oSchemaChoice.MaxOccurs < oSchemaChoice.Items.Count) ? (int)oSchemaChoice.MaxOccurs : oSchemaChoice.Items.Count;
            for (int i = 0; i < Occurrence; i++)
            {
                if (0 == oSchemaChoice.Items.Count)
                    break;
                // Generate the first choice    
                XmlSchemaObject oSchemaObj = oSchemaChoice.Items[i];
                if (oSchemaObj is XmlSchemaElement)
                {
                    // Generate an actual element
                    GenerateElement(oSchemaObj as XmlSchemaElement, oParentElement);
                }
                else
                {
                    // Generate a group of element
                    GenerateChildrenOfComplexType(oSchemaObj as XmlSchemaParticle, oParentElement);
                }
            }

            // Generate the other possible choices and add them to our examples List
        }

        /// <summary>
        /// Generate children for an all particle
        /// </summary>
        private void GenerateChildrenForAllParticle(XmlSchemaAll oSchemaAll, XmlElement oParentElement)
        {
            // We need to repeatedly create this sequence for a maximum of MinOccurs or 1
            int Occurrence = (oSchemaAll.MinOccurs > 1) ? (int)oSchemaAll.MinOccurs : 1;
            for (int i = 0; i < Occurrence; i++)
            {
                // Got through each XmlSchemaObject in the sequence
                for (int j = 0; j < oSchemaAll.Items.Count; j++)
                {
                    // Generate each element    
                    XmlSchemaObject oSchemaObj = oSchemaAll.Items[j];
                    if (oSchemaObj is XmlSchemaElement)
                    {
                        // Generate an actual element
                        GenerateElement(oSchemaObj as XmlSchemaElement, oParentElement);
                    }
                    else
                    {
                        // Generate a group of element
                        GenerateChildrenOfComplexType(oSchemaObj as XmlSchemaParticle, oParentElement);
                    }
                }
            }
        }

        /// <summary>
        /// Generates an actual XmlElement
        /// </summary>
        private void GenerateElement(XmlSchemaElement oSchemaElement, XmlElement oParentElement)
        {
            // Get the minOccurs of the element now so if it is a reference we use this value (which we should be using)
            int MinOccurs = (oSchemaElement.MinOccurs > 1) ? (int)oSchemaElement.MinOccurs : 1;

            // Check if it references another element
            if ((oSchemaElement.RefName != null) && (!oSchemaElement.RefName.IsEmpty))
            {
                oSchemaElement = oXmlSchemaSet.GlobalElements[oSchemaElement.RefName] as XmlSchemaElement;
            }

            for (int i = 0; i < MinOccurs; i++)
            {
                String strPrefix = oNSManager.LookupPrefix(oSchemaElement.QualifiedName.Namespace);
                if (String.IsNullOrEmpty(strPrefix))
                    strPrefix = Prefix;
                // Generate the element
                XmlElement oXmlElement = oParentElement.OwnerDocument.CreateElement(strPrefix, oSchemaElement.QualifiedName.Name, oSchemaElement.QualifiedName.Namespace);

                // Special case the anyType scenario
                if ((null != oSchemaElement.ElementSchemaType) 
                    && (oSchemaElement.ElementSchemaType.QualifiedName.Name.Equals("variant"))
                    )
                {
                    // For variants the value we use is the name of the default type of that variant.  There is no 
                    // direct control over the value of the type specified.
                    //XmlQualifiedName DefType = oXmlDefaultValues.FindAnyTypeValueForNode(oXmlElement.LocalName, oParentElement.LocalName);
                    XmlQualifiedName DefType = oXmlDefaultValues.DefaultValueForAnyType();
                    XmlText oText = oParentElement.OwnerDocument.CreateTextNode(DefType.ToString());
                    oXmlElement.AppendChild(oText);
                }
                else if ((XMLHelper.GetSchemaType(oSchemaElement) is XmlSchemaSimpleType))    
                {
                    // TODO: What about restrictions

                    // Assign a default value to the node
                    XmlTypeCode eTypeCode = XMLHelper.GetSchemaType(oSchemaElement).TypeCode;
                    //String DefValue = oXmlDefaultValues.FindValueForNode(oXmlElement.LocalName, eTypeCode, oParentElement.LocalName);
                    String DefValue = oXmlDefaultValues.DefaultValueForType(eTypeCode);
                    XmlText oText = oParentElement.OwnerDocument.CreateTextNode(DefValue);
                    oXmlElement.AppendChild(oText);
                }
                else if (oSchemaElement.ElementSchemaType is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType oSchemaComplexType = oSchemaElement.ElementSchemaType as XmlSchemaComplexType;
                    // Check if this complexContent
                    if (null != oSchemaComplexType.ContentModel)
                    {
                        if (oSchemaComplexType.ContentModel is XmlSchemaSimpleContent)
                        {
                            XmlSchemaSimpleContent SimpleContent = oSchemaComplexType.ContentModel as XmlSchemaSimpleContent;
                            // Get the base type name
                            XmlQualifiedName BaseTypeName = null;
                            if(SimpleContent.Content is XmlSchemaSimpleContentExtension)
                                BaseTypeName = (SimpleContent.Content as XmlSchemaSimpleContentExtension).BaseTypeName;
                            else if(SimpleContent.Content is XmlSchemaSimpleContentRestriction)
                                BaseTypeName = (SimpleContent.Content as XmlSchemaSimpleContentRestriction).BaseTypeName;
                            if (null != BaseTypeName)
                            {
                                // Generate the simple type 
                                XmlSchemaType oXmlSchemaType = XMLHelper.GetTypeFromSchema(BaseTypeName, oXmlSchemaSet);
                                String DefValue = oXmlDefaultValues.DefaultValueForType(oXmlSchemaType.TypeCode);
                                XmlText oText = oParentElement.OwnerDocument.CreateTextNode(DefValue);
                                oXmlElement.AppendChild(oText);
                            }
                        }
                        else
                        {
                            // It's type is an extension or restriction of another complex type
                            XmlSchemaComplexContent ComplexContent = oSchemaComplexType.ContentModel as XmlSchemaComplexContent;
                            if (ComplexContent.Content is XmlSchemaComplexContentRestriction)
                            {
                                GenerateChildrenOfComplexType((ComplexContent.Content as XmlSchemaComplexContentRestriction).Particle, oXmlElement);
                            }
                            else if (ComplexContent.Content is XmlSchemaComplexContentExtension)
                            {
                                // Get the actual SchemaType from the base type
                                XmlSchemaComplexType SchComplexType = oXmlSchemaSet.GlobalTypes[(ComplexContent.Content as XmlSchemaComplexContentExtension).BaseTypeName] as XmlSchemaComplexType;
                                // Remember the base particle
                                XmlSchemaParticle BaseParticle = SchComplexType.Particle;

                                // Create a new XmlSchemaParticle that will be the Base particle in sequence with the extension
                                XmlSchemaParticle SubParticle = null;
                                if (null != (ComplexContent.Content as XmlSchemaComplexContentExtension).Particle)
                                {
                                    XmlSchemaSequence ExtSeq = new XmlSchemaSequence();
                                    // The base particle might have no children and just contain attributes
                                    if (null != BaseParticle)
                                        ExtSeq.Items.Add(BaseParticle);
                                    // Add the extension particle
                                    ExtSeq.Items.Add((ComplexContent.Content as XmlSchemaComplexContentExtension).Particle);
                                    SubParticle = ExtSeq as XmlSchemaParticle;
                                }
                                else
                                    SubParticle = BaseParticle;

                                GenerateChildrenOfComplexType(SubParticle, oXmlElement);
                            }
                        }
                    }
                    else
                    {
                        // A normal complex type, create the children of this node.
                        GenerateChildrenOfComplexType((oSchemaElement.ElementSchemaType as XmlSchemaComplexType).Particle, oXmlElement);
                    }
                }

                // TODO: What about attributes?

                // TODO: Need to handle restrictions and extensions

                // Add new node to it's parent
                oParentElement.AppendChild(oXmlElement);
            }
        }
    }
}
