using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common.XML;
using OutputSettings = Fuzzware.ConvertFromXML.OutputSettings;
using Fuzzware.Convert2XML;

namespace Fuzzware.Schemer
{
    class XmlQualifiedNameComparer : IComparer<XmlQualifiedName>
    {
        #region IComparer<XmlQualifiedName> Members

        public int Compare(XmlQualifiedName x, XmlQualifiedName y)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(x.ToString(), y.ToString());
        }

        #endregion
    }

    public class PreCompData
    {
        // PreCompiled Data
        private String oXMLFile;
        private XmlDocument oXMLDoc;
        private XmlSchemaSet oSchemaSet;
        private ObjectDataBase oObjectDB; 
        private Dictionary<XmlQualifiedName, List<XPathNavigator>> oConfigExamplesDict;
        private Dictionary<String, String> oNamespacePrefixDict;
        private Encoding oOutputEncoding;
        private InputHandler oInputHandler;

        private OutputSettings oSettings;

        public PreCompData(InputHandler oInputHandler, Encoding OutputEncoding)
        {
            this.oInputHandler = oInputHandler;
            oOutputEncoding = OutputEncoding;

            oSchemaSet = oInputHandler.SchemaSet;
            oXMLFile = oInputHandler.XMLFilePath;
            oXMLDoc = oInputHandler.XMLDoc;
            oObjectDB = oInputHandler.ObjectDB;

            oConfigExamplesDict = new Dictionary<XmlQualifiedName, List<XPathNavigator>>();
            oNamespacePrefixDict = new Dictionary<string, string>();
        }

        public PreCompData(XmlSchemaSet oSchemaSet, Encoding oOutputEncoding)
        {
            this.oSchemaSet = oSchemaSet;
            this.oOutputEncoding = oOutputEncoding;

            oObjectDB = new ObjectDataBase(oSchemaSet);
            oConfigExamplesDict = new Dictionary<XmlQualifiedName, List<XPathNavigator>>();
            oNamespacePrefixDict = new Dictionary<string, string>();
        }

        public String XMLFile
        {
            get { return oXMLFile; }
            set { oXMLFile = value; }
        }

        public XmlSchemaSet SchemaSet
        {
            get { return oSchemaSet; }
        }

        /// <summary>
        /// We need to get and set XMLDoc, as the XmlDocument will be reloaded each time we fuzz a node
        /// </summary>
        public XmlDocument XMLDoc
        {
            get { return oXMLDoc; }
            set { oXMLDoc = value; }
        }

        public Encoding OutputEncoding
        {
            get { return oOutputEncoding; }
        }

        public ObjectDataBase ObjectDB
        {
            get { return oObjectDB; }
        }

        public List<XMLObjectIdentifier> ObjectNodeList
        {
            get { return oObjectDB.ObjectIdList; }
        }

        public Dictionary<XmlQualifiedName, List<XPathNavigator>> ConfigExamplesDict
        {
            get { return oConfigExamplesDict; }
        }

        public Dictionary<String, String> NamespacePrefixDict
        {
            get { return oNamespacePrefixDict; }
        }

        public OutputSettings GetOutputSettings(ConfigData oConfigData)
        {
            if (null == oSettings)
            {
                // This data is constant
                oSettings = new OutputSettings(oConfigData.Config.Output.ConvertFromXML);
                oSettings.ObjectDB = oObjectDB;
            }
            // The XML document is reloaded after fuzzing each node, so needs to always be reset here
            oSettings.XMLDoc = oXMLDoc;
            return oSettings;
        }

        public InputHandler InputHandler
        {
            get
            {
                return oInputHandler;
            }
        }
    }
}
