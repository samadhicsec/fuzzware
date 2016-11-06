﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.Services.Description;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Serialization;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.MethodInterface;

namespace Fuzzware.Convert2XML
{
    /// <summary>
    /// This class allows the input to be a WSDL and example SOAP requests in order to fuzz the Services described in the WSDL.
    /// </summary>
    public class WSDLInputHandler : InputHandler
    {
        WSDLInput oWSDLInput;
        WSDLLibraryDescription oServicesLibrary;

        XmlDefaultValues oXmlDefaultValues;

        public override void Initialise(object Settings, Encoding OutputEncoding)
        {
            this.OutputEncoding = OutputEncoding;
            if (!(Settings is WSDLInput))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'WSDLInput', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            oWSDLInput = Settings as WSDLInput;

            if (String.IsNullOrEmpty(oWSDLInput.OutputDir))
                oWSDLInput.OutputDir = Environment.CurrentDirectory;
            if (!Directory.Exists(oWSDLInput.OutputDir))
                Directory.CreateDirectory(Path.GetFullPath(oWSDLInput.OutputDir));

            // Deserialise the default values
            XmlDefaultValuesLoader oDefValsLoader = new XmlDefaultValuesLoader();
            oXmlDefaultValues = oDefValsLoader.Load(oWSDLInput.MethodsConfig.DefaultValues);
        }

        /// <summary>
        /// Gets the WSDLLibraryDescription.
        /// </summary>
        public WSDLLibraryDescription LibraryDesc
        {
            get
            {
                if (null == oServicesLibrary)
                {
                    oServicesLibrary = new WSDLLibraryDescription(oWSDLInput.WSDLPathAndFile, oWSDLInput.Protocol, oWSDLInput.MethodsConfig, oWSDLInput.OutputDir);
                }
                return oServicesLibrary;
            }
        }

        public override XmlSchemaSet SchemaSet
        {
            get
            {
                if (null == SchemaPaths)
                {
                    if (null == oWSDLInput.OutputDir)
                        oWSDLInput.OutputDir = "";

                    // Create the Xml Schema.
                    if(1 == oWSDLInput.WSDLPathAndFile.Length)
                        Log.Write(MethodBase.GetCurrentMethod(), "Processing WSDL '" + oWSDLInput.WSDLPathAndFile[0] + "'", Log.LogType.Info);
                    else
                        Log.Write(MethodBase.GetCurrentMethod(), "Processing multiple WSDLs", Log.LogType.Info);
                    oServicesLibrary = new WSDLLibraryDescription(oWSDLInput.WSDLPathAndFile, oWSDLInput.Protocol, oWSDLInput.MethodsConfig, oWSDLInput.OutputDir);

                    // Get a list of all namespaces referenced by this WSDL as we should have a schema for each of them
                    string[] SchemaNamespaces = oServicesLibrary.XSDNamespaces;
                    SchemaPaths = new string[SchemaNamespaces.Length];
                    
                    // If we already have an existing XSD files, use that one in case the user has editted it
                    for (int i = 0; i < SchemaNamespaces.Length; i++)
                    {
                        // Populate SchemaPaths
                        SchemaPaths[i] = oServicesLibrary.GetXSDPath(SchemaNamespaces[i]);
                        if (File.Exists(SchemaPaths[i]))
                        //if(false)
                        {
                            if(oServicesLibrary.IsPreExisting(SchemaNamespaces[i]))
                                Log.Write(MethodBase.GetCurrentMethod(), "Using existing input schema file '" + SchemaPaths[i] + "'", Log.LogType.Info);
                            else
                                Log.Write(MethodBase.GetCurrentMethod(), "Using existing input schema file '" + SchemaPaths[i] + "'.  Delete it and re-run to generate a new one", Log.LogType.Info);
                        }
                        else
                        {
                            Log.Write(MethodBase.GetCurrentMethod(), "Creating input schema file '" + SchemaPaths[i] + "'", Log.LogType.Info);

                            // Get the schema to write to file
                            XmlSchema oXmlSchema = oServicesLibrary.GetXmlSchema(SchemaNamespaces[i]);

                            if (!Directory.Exists(Path.GetDirectoryName(Path.GetFullPath(SchemaPaths[i]))))
                                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(SchemaPaths[i])));
                            
                            // Write out the schema
                            using (FileStream fs = new FileStream(SchemaPaths[i], FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                            {
                                oXmlSchema.Write(fs, oServicesLibrary.NamespaceManager);
                            }
                        }
                    }
                }

                return base.SchemaSet;
            }
        }

        public override String XMLFilePath
        {
            get
            {
                if (String.IsNullOrEmpty(XMLPath))
                {
                    if (null == oWSDLInput.OutputDir)
                        oWSDLInput.OutputDir = "";

                    if (null == oServicesLibrary)
                        oServicesLibrary = new WSDLLibraryDescription(oWSDLInput.WSDLPathAndFile, oWSDLInput.Protocol, oWSDLInput.MethodsConfig, oWSDLInput.OutputDir);

                    XMLPath = Path.GetFullPath(Path.Combine(Path.GetFullPath(oWSDLInput.OutputDir), oServicesLibrary.Name + "Input.xml"));
                    //XMLPath = Path.GetFullPath(oServicesLibrary.Name + "Input.xml");

                    // If we already have an existing XML file, use that one in case the user has editted it
                    if (File.Exists(XMLPath))
                    //if(false)
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Using existing XML file '" + XMLPath + "'.  Delete it and re-run to generate a new one", Log.LogType.Info);
                    }
                    else
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Creating XML file '" + XMLPath + "'.", Log.LogType.Info);

                        String Prefix = oServicesLibrary.NamespaceManager.LookupPrefix(oServicesLibrary.Namespace);
                        if (String.IsNullOrEmpty(Prefix))
                        {
                            Prefix = LibraryDescription.PREFIX;
                            oServicesLibrary.NamespaceManager.AddNamespace(Prefix, oServicesLibrary.Namespace);
                        }

                        // Generate a version of the XML
                        XMLGenerator oGen = new XMLGenerator(SchemaSet, Prefix, new XmlQualifiedName(oServicesLibrary.Name, oServicesLibrary.Namespace), oServicesLibrary.NamespaceManager, oXmlDefaultValues);
                        XmlDocument oDoc = oGen.Generate();

                        // Randomize the order of the methods called
                        MethodNodeRandomizer oRandomizer = new MethodNodeRandomizer();
                        oDoc = oRandomizer.RandomizeAndFilterMethodCalls(oDoc, oWSDLInput.MethodsConfig.InitialMethods);

                        XmlWriterSettings oXmlWriterSettings = new XmlWriterSettings();
                        oXmlWriterSettings.Indent = true;
                        Directory.CreateDirectory(Path.GetDirectoryName(XMLPath));
                        XmlWriter oXmlWriter = XmlWriter.Create(XMLPath, oXmlWriterSettings);
                        oDoc.WriteTo(oXmlWriter);
                        oXmlWriter.Flush();
                        oXmlWriter.Close();
                    }
                }
                return XMLPath;
            }
        }

    }
}
