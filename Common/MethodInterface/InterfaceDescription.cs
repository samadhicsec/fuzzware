using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Serialization;
using Fuzzware.Common.XML;

namespace Fuzzware.Common.MethodInterface
{
    /// <summary>
    /// Describes an Interface which is a collection of Methods.
    /// </summary>
    public class InterfaceDescription
    {
        protected String m_InterfaceName;
        protected String m_InterfaceNS;
        protected List<MethodDescription> m_oMethodDescriptions;
        protected XmlDocument m_oXmlDoc;

        protected InterfaceDescription()
        {

        }

        public String Name
        {
            get { return m_InterfaceName; }
        }

        public String Namespace
        {
            get { return m_InterfaceNS; }
        }

        public List<MethodDescription> Methods
        {
            get { return m_oMethodDescriptions; }
        }

        /// <summary>
        /// We cannot have duplicate method names as this leads to duplicate element in the Schema which will cause validation to fail.
        /// </summary>
        protected void ProcessDuplicateMethodNames()
        {
            for (int i = 0; i < m_oMethodDescriptions.Count - 1; i++)
            {
                // Find all duplicates of this name
                String Name = m_oMethodDescriptions[i].MethodName;

                List<MethodDescription> oDups = new List<MethodDescription>();
                for (int j = i + 1; j < m_oMethodDescriptions.Count; j++)
                {
                    if (Name.Equals(m_oMethodDescriptions[j].MethodName, StringComparison.CurrentCultureIgnoreCase))
                        oDups.Add(m_oMethodDescriptions[j]);
                }

                // If we found duplicates then alter method names
                if (oDups.Count > 0)
                {
                    // Add original to duplicate list
                    oDups.Add(m_oMethodDescriptions[i]);

                    for (int j = 0; j < oDups.Count; j++)
                    {
                        oDups[j].OriginalMethodName = oDups[j].MethodName;
                        //oDups[j].MethodName = oDups[j].MethodName + j.ToString();
                        // Assume that the parameter message name is unique
                        // TODO Check this!!!  This only works for WSDL
                        oDups[j].MethodName = oDups[j].ParameterDescs[0].ParamSchemaElement.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Create reference XmlSchemaElements for each method of the interface
        /// </summary>
        public List<XmlSchemaElement> CreateRefToMethodSchemaElements(Dictionary<string, XmlSchema> oNamespaceSchemaDictionary)
        {
            List<XmlSchemaElement> oMethods = new List<XmlSchemaElement>();
            
            // Create and add all the Interface methods to the Interface
            for (int i = 0; i < m_oMethodDescriptions.Count; i++)
            {
                XmlSchema oXmlSchema = oNamespaceSchemaDictionary[m_oMethodDescriptions[i].InputMessageNamespace];
                XmlSchemaElement oMethodSchemaEle = FindMethodSchemaElement(oXmlSchema, m_oMethodDescriptions[i]);

                XmlSchemaElement oRefMethodSchemaEle = new XmlSchemaElement();
                oRefMethodSchemaEle.RefName = new XmlQualifiedName(oMethodSchemaEle.Name, m_oMethodDescriptions[i].InputMessageNamespace);

                oMethods.Add(oRefMethodSchemaEle);
            }

            return oMethods;
        }

        /// <summary>
        /// Create an XmlSchemaElement for the specified method index.  If the XmlSchemaElement already exists, do nothing.
        /// </summary>
        public void CreateMethodSchemaElement(XmlSchema oXmlSchema, int MethodDescIndex)
        {
            if ((MethodDescIndex < 0) || (MethodDescIndex >= m_oMethodDescriptions.Count))
                Log.Write(MethodBase.GetCurrentMethod(), "MethodDescription index was out of range", Log.LogType.Error);

            MethodDescription oMethodDesc = m_oMethodDescriptions[MethodDescIndex];
            // Some interface descriptions will provide us with a lot of Schema types that we can re-use
            // e.g. WSDL Types.  Rather than reproduce this we re-use it.
            // Try to see if the method comes pre-defined.
            XmlSchemaElement oMethodSchemaEle = FindMethodSchemaElement(oXmlSchema, oMethodDesc);

            if (null == oMethodSchemaEle)
            {
                //<xs:element name="interface.methodname">
                //  <xs:complexType>
                //    <xs:sequence>
                //      <!-- parameters -->
                //    </xs:sequence>
                //  <xs:complexType>
                //</xs:element>

                // We need to create a new method.  We also add it in place to avoid problems with different
                // interfaces having methods of the same name.
                CreateMethodSchemaElement(oMethodDesc, oXmlSchema);
            }
        }

        /// <summary>
        /// Searches the input XmlSchema for an existing match, if found returns a new XmlSchemaElement 
        /// referencing the found match.
        /// </summary>
        public XmlSchemaElement FindMethodSchemaElement(XmlSchema oXmlSchema, MethodDescription oMethodDesc)
        {
            // Look for the method with its fully-qualified name 'Interface.MethodName'
            String NodeName = GetMethodNodeName(oMethodDesc);

            XmlSchemaElement oFoundSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oXmlSchema, NodeName);
            if (null != oFoundSchemaElement)
                return oFoundSchemaElement;

            // The method may already exist with just its method name.  This is true since we update all method names
            // but may have included schema elements from another schema where the method name hasn't been updated.
            NodeName = oMethodDesc.MethodName;
            oFoundSchemaElement = XMLHelper.GetElementFromUncompiledSchema(oXmlSchema, NodeName);
            if (null != oFoundSchemaElement)
            {
                oFoundSchemaElement.Name = GetMethodNodeName(oMethodDesc);
                return oFoundSchemaElement;
            }

            return null;
        }

        /// <summary>
        /// Create the Method schema element
        /// </summary>
        private void CreateMethodSchemaElement(MethodDescription oMethodDesc, XmlSchema oXmlSchema)
        {
            XmlSchemaElement oMethodSchemaElement = new XmlSchemaElement();
            oMethodSchemaElement.Name = GetMethodNodeName(oMethodDesc);
            XmlSchemaComplexType oMSEComplexType = new XmlSchemaComplexType();
            XmlSchemaSequence oMSESequence = new XmlSchemaSequence();
            // Put the current parameters into the wrapper
            for (int i = 0; i < oMethodDesc.ParameterDescs.Count; i++)
                oMSESequence.Items.Add(oMethodDesc.ParameterDescs[i].ParamSchemaElement);
            oMSEComplexType.Particle = oMSESequence;
            oMethodSchemaElement.SchemaType = oMSEComplexType;
            oXmlSchema.Items.Add(oMethodSchemaElement);
        }

        public virtual String GetMethodNodeName(MethodDescription oMethodDesc)
        {
            return Name + "." + oMethodDesc.MethodName;
        }
    }
}
