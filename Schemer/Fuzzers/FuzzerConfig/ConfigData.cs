using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace SchemaXMLFuzzer.Fuzzers.FuzzerConfig
{
    class ConfigData
    {
        XmlDocument oXMLDoc;
        String TargetNSPrefix;
        String TargetNS;

        public ConfigData(String ConfigDataSchema, String ConfigDataXML)
        {
            // Load and verify the config data
            XmlSchemaSet oSchemaSet = new XmlSchemaSet();
            oSchemaSet.XmlResolver = null;
            //oSchemaSet.ValidationEventHandler += new ValidationEventHandler(MyValidationEventHandler);

            // Load schema
            StreamReader sr = new StreamReader(ConfigDataSchema);
            XmlSchema oSchema = XmlSchema.Read(sr, null);
            TargetNS = oSchema.TargetNamespace;

            oSchemaSet.Add(oSchema);

            // Compile the schema, we want any error thrown here to be propogated up
            oSchemaSet.Compile();

            // Validate the XML againsts the schema, we want any error thrown here to be propogated up
            oXMLDoc = new XmlDocument();
            // Create the validating reader and specify schema validation.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = oSchemaSet;
            //settings.ValidationEventHandler += MyValidationEventHandler;

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(ConfigDataXML, settings);
            // Parse the file. 
            oXMLDoc.Load(reader);

            TargetNSPrefix = oXMLDoc.GetPrefixOfNamespace(TargetNS);
        }

        public XPathNavigator[] GetXPathNodes(String ElementName)
        {
            XmlQualifiedName Name = new XmlQualifiedName(ElementName, TargetNS);
            return XMLHelper.SelectNodesOfType(Name, oXMLDoc);
        }
    }

    class SimpleTypeConfigData
    {
        private static bool Initialised;
        private static ConfigData configData;

        public SimpleTypeConfigData()
        {
            if (!Initialised)
            {
                configData = new ConfigData(XMLFuzzConfig.Config.schemaConfigFiles.SimpleTypeFuzzerSchema, XMLFuzzConfig.Config.xmlConfigFiles.SimpleTypeFuzzerXML);
                Initialised = true;
            }
        }

        public XPathNavigator[] GetXPathNodes(String ElementName)
        {
            return configData.GetXPathNodes(ElementName);
        }
    }

    class ComplexTypeConfigData
    {
        private static bool Initialised;
        private static ConfigData configData;
        private static uint[] UnboundedValues;
        
        public ComplexTypeConfigData()
        {
            if (!Initialised)
            {
                configData = new ConfigData(XMLFuzzConfig.Config.schemaConfigFiles.ComplexTypeFuzzerSchema, XMLFuzzConfig.Config.xmlConfigFiles.ComplexTypeFuzzerXML);
                Initialised = true;
            }
        }

        public XPathNavigator[] GetXPathNodes(String ElementName)
        {
            return configData.GetXPathNodes(ElementName);
        }

        static public int GetUnboundedValuesCount()
        {
            if (null == UnboundedValues)
                ComplexTypeConfigData.GetUnboundedValues(0);

            if (null == UnboundedValues)
                return 0;

            return UnboundedValues.Length;
        }

        static public uint GetUnboundedValues(int Index)
        {
            if (null == UnboundedValues)
            {
                ComplexTypeConfigData config = new ComplexTypeConfigData(); 

                // Get from the Configuration XML the number of different test cases
                XPathNavigator[] Occurs = config.GetXPathNodes("Occurs");

                UnboundedValues = new uint[Occurs.Length];

                for (int i = 0; i < Occurs.Length; i++)
                {
                    if (!UInt32.TryParse(Occurs[i].Value, out UnboundedValues[i]))
                    {
                        Log.Write("ComplexTypeConfigData.GetUnboundedValues", "Could not convert '" + Occurs[i].Value + "' to an UInt32", Log.LogType.Warning);
                    }
                }

                // Could order from smallest to highest, and remove duplicates, but doesn't seem worth the effort 
            }

            if ((Index < 0) || (Index >= UnboundedValues.Length))
            {
                Log.Write("ComplexTypeConfigData.GetUnboundedValues", "Requested index for Occurs list is out of range", Log.LogType.Warning);
            }

            return UnboundedValues[Index];
        }
    }
}
