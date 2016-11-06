using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Configuration;
using Fuzzware.Common;
using Fuzzware.Common.DataSchema;

        /*
         * An XmlSchemaParticle is abstract and has the following inherited classes
         *  System.Xml.Schema.XmlSchemaParticle     (abstract)
         *      System.Xml.Schema.XmlSchemaGroupBase    (abstract)
         *          System.Xml.Schema.XmlSchemaAll 
         *          System.Xml.Schema.XmlSchemaChoice 
         *          System.Xml.Schema.XmlSchemaSequence
         *      System.Xml.Schema.XmlSchemaAny 
         *      System.Xml.Schema.XmlSchemaElement 
         *      System.Xml.Schema.XmlSchemaGroupRef 
         */

namespace Fuzzware.Common.XML
{
    public class XMLHelper
    {
        class XmlFileResolver : XmlResolver
        {
            public override System.Net.ICredentials Credentials
            {
                set { ; }
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                return new StreamReader(absoluteUri.LocalPath);
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                return new Uri(baseUri.LocalPath + (relativeUri.StartsWith(@"\")?"":@"\") + relativeUri);
            }
        }

        public class XmlQualifiedNameComparer : IComparer<XmlQualifiedName>
        {

            #region IComparer<XmlQualifiedName> Members

            public int Compare(XmlQualifiedName x, XmlQualifiedName y)
            {
                return StringComparer.CurrentCultureIgnoreCase.Compare(x.ToString(), y.ToString());
            }

            #endregion
        }
        
        static String ApplicationName;

        static private void DefaultValidationEventHandler(Object sender, ValidationEventArgs e)
        {
            Log.Write("Default Validation handler = ", e.Message, Log.LogType.Warning);
        }

        static public XmlSchemaSet LoadAndCompileSchema(String Schema, ValidationEventHandler Handler, out String TargetNamespace)
        {
            String[] SchemaArray = new String[1];
            SchemaArray[0] = Schema;
            List<String> TargetNSs;
            XmlSchemaSet oSchemaSet = XMLHelper.LoadAndCompileSchema(SchemaArray, Handler, out TargetNSs);
            TargetNamespace = TargetNSs[0];
            return oSchemaSet;
        }

        /// <summary>
        /// Create Schema objects for all imported xml schemas.  If remote, tell user to download, if it is in the same directory as
        /// the schema that references it, it will be found.  Will recurse if imported schemas import other schemas.
        /// </summary>
        /// <param name="oBaseSchema"></param>
        /// <param name="oBaseSchemaPath"></param>
        /// <param name="IncludesDict"></param>
        /// <param name="Handler"></param>
        static private void GetIncludedSchemas(XmlSchema oBaseSchema, String oBaseSchemaPath, Dictionary<String, XmlSchema> IncludesDict, List<String> TargetNamespaces, ValidationEventHandler Handler)
        {
            // Create and record all included or imported schema
            for (int i = 0; i < oBaseSchema.Includes.Count; i++)
            {
                // XmlSchemaExternal is abstract and can be XmlSchemaImport, XmlSchemaInclude or XmlSchemaRedefine
                XmlSchemaExternal ExtSchema = oBaseSchema.Includes[i] as XmlSchemaExternal;
                String Loc = ExtSchema.SchemaLocation;
                if (String.IsNullOrEmpty(Loc) && (ExtSchema is XmlSchemaImport))
                {
                    if (!String.IsNullOrEmpty((ExtSchema as XmlSchemaImport).Namespace))
                    {
                        // See if the namespace was the target namespace of another listed schema (that we have read in so far)
                        int j = 0;
                        for (; j < TargetNamespaces.Count; j++)
                            if (TargetNamespaces[j].Equals((ExtSchema as XmlSchemaImport).Namespace, StringComparison.CurrentCultureIgnoreCase))
                                break;
                        if(j == TargetNamespaces.Count)
                            Log.Write(MethodBase.GetCurrentMethod(), "Imported Schema from '" + oBaseSchemaPath + "' with namespace '" +
                                (ExtSchema as XmlSchemaImport).Namespace + "' has no location, so can't be loaded.  If this causes " +
                                "schema compilation to fail, locate and add the schema to list of schemas", Log.LogType.Warning);
                    }
                    continue;
                }
                
                // Try to determine if it is a file or not
                bool bIsFile = false;
                Uri UriLoc = null;
                if (Uri.IsWellFormedUriString(Loc, UriKind.Absolute))
                {
                    UriLoc = new Uri(ExtSchema.SchemaLocation);
                    bIsFile = UriLoc.IsFile;
                }
                else
                {
                    // Assume a file
                    bIsFile = true;
                }

                if (bIsFile)
                {
                    if (!Path.IsPathRooted(Loc))
                        Loc = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(oBaseSchemaPath), Loc));
                }
                else
                {
                    // See if the user has made a local copy of the file
                    String XSDFileName = UriLoc.Segments[UriLoc.Segments.Length - 1];
                    Loc = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(oBaseSchemaPath), XSDFileName));
                    if(!File.Exists(Loc))
                    {
                        // We would have to go off box to try to get the Schema, but that is a bad idea
                        Log.Write(MethodBase.GetCurrentMethod(), "The XML Schema '" + UriLoc + "' is located remotely, you should " +
                            "download this file to the '" + Path.GetDirectoryName(oBaseSchemaPath) + "' directory and save it as " +
                            "'" + XSDFileName + "'.  Schemer will be able to find this XML Schema then without you needing to modify " +
                            "the XML Schema that references it", Log.LogType.Error);
                    }
                }
                // Make sure it doesn't already exist
                if (!IncludesDict.ContainsKey(Loc))
                {
                    if (!File.Exists(Loc))
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "The imported XML Schema '" + Loc + "' could not be found", Log.LogType.Warning);
                        continue;
                    }

                    StreamReader sr = new StreamReader(Loc);
                    // Some XSD'd can reference DTDs, which people will logically place in the same dir, however that is not our
                    // working dir, so for the purposes of reading in the schema, change our working dir, but immediately reset.
                    String StoredCurDir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = Path.GetDirectoryName(Loc);
                    // Read in schema
                    XmlSchema oSchema = XmlSchema.Read(sr, Handler);
                    // Reset working dir
                    Environment.CurrentDirectory = StoredCurDir;

                    IncludesDict.Add(Loc, oSchema);

                    if (!String.IsNullOrEmpty(oSchema.TargetNamespace))
                        TargetNamespaces.Add(oSchema.TargetNamespace);
                    else
                        TargetNamespaces.Add("");

                    // Recurse
                    if (oSchema.Includes.Count > 0)
                        GetIncludedSchemas(oSchema, Loc, IncludesDict, TargetNamespaces, Handler);
                }
            }
        }

        static public XmlSchemaSet LoadAndCompileSchema(String[] Schemas, ValidationEventHandler Handler, out List<String> TargetNamespaces)
        {
            XmlSchemaSet oSchemaSet = new XmlSchemaSet();
            //oSchemaSet.XmlResolver = null;
            if (null != Handler)
                oSchemaSet.ValidationEventHandler += Handler;
            else
                oSchemaSet.ValidationEventHandler += XMLHelper.DefaultValidationEventHandler;

            TargetNamespaces = new List<String>();
            try
            {
                // Make a dictionary of all the schemas, ensuring we add all included or imported schemas
                Dictionary<String, XmlSchema> IncludesDict = new Dictionary<string, XmlSchema>();
                oSchemaSet.XmlResolver = new MyXmlResolver(IncludesDict);
                //XmlResolver Resolver = new XmlFileResolver();
                //oSchemaSet.XmlResolver = Resolver;

                for (int i = 0; i < Schemas.Length; i++)
                {
                    if (IncludesDict.ContainsKey(Path.GetFullPath(Schemas[i])))
                        continue;

                    // Load schema
                    StreamReader sr = new StreamReader(Schemas[i]);

                    // Some XSD'd can reference DTDs, which people will logically place in the same dir, however that is not our
                    // working dir, so for the purposes of reading in the schema, change our working dir, but immediately reset.
                    String StoredCurDir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(Schemas[i]));
                    // Read in schema
                    XmlSchema oSchema = XmlSchema.Read(sr, Handler);
                    // Reset working dir
                    Environment.CurrentDirectory = StoredCurDir;
                    // Close the schema stream reader
                    sr.Close();

                    // Include this schema now, before look at included/imported schemas
                    IncludesDict.Add(Path.GetFullPath(Schemas[i]), oSchema);

                    if (oSchema.Includes.Count > 0)
                    {
                        GetIncludedSchemas(oSchema, Schemas[i], IncludesDict, TargetNamespaces, Handler);
                    }

                    // GetIncludedSchemas may have already included this schema (via an included schema with a circular reference)
                    //if(!IncludesDict.ContainsKey(Path.GetFullPath(Schemas[i])))
                    //    IncludesDict.Add(Path.GetFullPath(Schemas[i]), oSchema);

                    if (!TargetNamespaces.Contains(oSchema.TargetNamespace))
                    {
                        if (!String.IsNullOrEmpty(oSchema.TargetNamespace))
                            TargetNamespaces.Add(oSchema.TargetNamespace);
                        else
                            TargetNamespaces.Add("");
                    }
                }

                // Add all the schemas
                XmlSchema[] SchemaArray = new XmlSchema[IncludesDict.Values.Count];
                IncludesDict.Values.CopyTo(SchemaArray, 0);
                for (int i = 0; i < SchemaArray.Length; i++)
                    oSchemaSet.Add(SchemaArray[i]);

            }
            catch (Exception e)
            {
                Log.Write(e);
            }

            // Validate it
            try
            {
                oSchemaSet.Compile();
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return oSchemaSet;
        }

        static public XmlDocument LoadAndCompileXML(String Filename, XmlSchemaSet oSchemaSet, ValidationEventHandler Handler)
        {
            XmlDocument oXMLDoc = new XmlDocument();
            try
            {
                // Create the validating reader and specify schema validation.
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                if ((null == oSchemaSet) || (0 == oSchemaSet.Count))
                    Log.Write(MethodBase.GetCurrentMethod(), "No schemas have been loaded to validate the XML document.", Log.LogType.Error);
                settings.Schemas = oSchemaSet;
                if(null != Handler)
                    settings.ValidationEventHandler += Handler;
                else
                    settings.ValidationEventHandler += XMLHelper.DefaultValidationEventHandler;
                settings.ProhibitDtd = false;

                // Create the XmlReader object.
                XmlReader reader = XmlReader.Create(Filename, settings);
                // Parse the file. 
                oXMLDoc.Load(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return oXMLDoc;
        }

        static private void AddIncludedSchemas(String BaseSchemaName, XmlSchema oBaseSchema, XmlSchemaSet oSchemaSet, Assembly[] SourceAssemblies, ValidationEventHandler Handler)
        {
            // Create list of included schemas
            List<String> InclSchemas = new List<string>();
            // Add the base  schema so if an included schema includes the base we won't recurse
            InclSchemas.Add(BaseSchemaName);
            for (int i = 0; i < oBaseSchema.Includes.Count; i++)
                InclSchemas.Add((oBaseSchema.Includes[i] as XmlSchemaExternal).SchemaLocation);

            // Add all the included schemas
            for (int i = 1; i < InclSchemas.Count; i++)
            {
                try
                {
                    Stream XsdStream = null;
                    // Get Stream from one of the Assemblies
                    for (int j = 0; j < SourceAssemblies.Length; j++)
                    {
                        String[] Resources = SourceAssemblies[j].GetManifestResourceNames();
                        XsdStream = SourceAssemblies[j].GetManifestResourceStream("Fuzzware." + SourceAssemblies[j].GetName().Name + ".Resources." + InclSchemas[i]);
                        if (null != XsdStream)
                            break;
                    }
                    if (null == XsdStream)
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not find schema '" + InclSchemas[i] + "' in resources of assemblies", Log.LogType.Error);

                    // Load schema
                    XmlSchema oSchema = XmlSchema.Read(XsdStream, Handler);

                    oSchemaSet.Add(oSchema);

                    // Add any more includes from the newly included Schema
                    for (int j = 0; j < oSchema.Includes.Count; j++)
                        if (!InclSchemas.Contains((oSchema.Includes[j] as XmlSchemaExternal).SchemaLocation))
                            InclSchemas.Add((oSchema.Includes[j] as XmlSchemaExternal).SchemaLocation);
                }
                catch (Exception e)
                {
                    Log.Write(e);
                }
            }
        }

        static public XmlSchemaSet LoadAndCompileSchema(String Schema, Assembly SourceAssembly, ValidationEventHandler Handler, out String TargetNamespace)
        {
            Assembly[] SourceAssemblies = new Assembly[1];
            SourceAssemblies[0] = SourceAssembly;
            return LoadAndCompileSchema(Schema, SourceAssemblies, Handler, out TargetNamespace);
        }

        static public XmlSchemaSet LoadAndCompileSchema(String Schema, Assembly[] SourceAssemblies, ValidationEventHandler Handler, out String TargetNamespace)
        {
            XmlSchemaSet oSchemaSet = new XmlSchemaSet();
            oSchemaSet.XmlResolver = null;
            if (null != Handler)
                oSchemaSet.ValidationEventHandler += Handler;

            TargetNamespace = "";
            XmlSchema oSchema = null;
            try
            {
                // Get Stream from one of the Assemblies
                Stream XsdStream = Resources.GetResource(Schema, SourceAssemblies);
                
                if (null == XsdStream)
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not find schema '" + Schema + "' in resources of assemblies", Log.LogType.Error);

                // Load schema
                oSchema = XmlSchema.Read(XsdStream, Handler);

                oSchemaSet.Add(oSchema);

                if (!String.IsNullOrEmpty(oSchema.TargetNamespace))
                    TargetNamespace = String.Copy(oSchema.TargetNamespace);
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            
            // Add all included schemas
            AddIncludedSchemas(Schema, oSchema, oSchemaSet, SourceAssemblies, Handler);

            // Validate it
            try
            {
                oSchemaSet.Compile();
            }
            catch (XmlSchemaException e)
            {
                Log.Write(e);
            }
            return oSchemaSet;
        }

        static public String AppName()
        {
            if (String.IsNullOrEmpty(ApplicationName))
            {
                ApplicationName = AppDomain.CurrentDomain.FriendlyName;
                ApplicationName = ApplicationName.Substring(0, ApplicationName.IndexOf('.'));
            }
            return ApplicationName;
        }

        static public XmlSchemaType GetTypeFromSchema(XmlQualifiedName SchEleName, XmlSchemaSet oSchemaSet)
        {
            // See if it is a built in simple type
            XmlSchemaType SchType = XmlSchemaType.GetBuiltInSimpleType(SchEleName);

            if (null != SchType)
                return SchType;

            // See if it is a built in complex type
            SchType = XmlSchemaType.GetBuiltInComplexType(SchEleName);

            if (null != SchType)
                return SchType;

            // This could be a problem, as it doesn't fit into having fully qualified names
            //if (NameParticleDictionary.ContainsKey(SchEleName))
            if(oSchemaSet.GlobalElements.Contains(SchEleName))
            {
                //SchType = XMLHelper.GetSchemaType(NameParticleDictionary[SchEleName].Particle as XmlSchemaElement);
                SchType = XMLHelper.GetSchemaType(oSchemaSet.GlobalElements[SchEleName] as XmlSchemaElement);
            }

            if (null != SchType)
                return SchType;

            // Look in the global types, not strictly an element name then, but we're being flexible
            SchType = oSchemaSet.GlobalTypes[SchEleName] as XmlSchemaType;

            if (null != SchType)
                return SchType;

            if (null == SchType)
                Log.Write(MethodBase.GetCurrentMethod(), "Could not get XmlSchemaType for XmlQualifiedName '" + SchEleName + "'", Log.LogType.Error);

            return SchType;
        }

        static public XmlSchemaType GetSchemaType(XmlSchemaElement SchEle)
        {
            XmlSchemaType SchType = null;
            
            // SchEle.SchemaType will be null if the type is defined elsewhere in the schema
            SchType = SchEle.SchemaType;
            if (null != SchType)
                return SchType;

            // Check to see if it's a built-in type e.g. string
            SchType = SchEle.ElementSchemaType;
            if (null != SchType)
                return SchType;

            if (null == SchType)
                Log.Write(MethodBase.GetCurrentMethod(), "Could not get XmlSchemaType for XmlQualifiedName '" + SchEle.QualifiedName + "'", Log.LogType.Error);

            return SchType;
        }

        static public XmlSchemaAttributeGroup GetAttributeGroupFromSchema(XmlSchemaSet oSchemaSet, XmlSchemaAttributeGroupRef GroupRef)
        {
            if (oSchemaSet.GlobalAttributes.Contains(GroupRef.RefName))
            {
                return oSchemaSet.GlobalAttributes[GroupRef.RefName] as XmlSchemaAttributeGroup;
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find the global attribute group from the attribute group reference '" + GroupRef.RefName + "'", Log.LogType.Error);
            }
            return null;
        }

        static public XmlSchemaGroupBase GetGroupBaseFromSchema(XmlSchemaSet oSchemaSet, XmlSchemaGroupRef GroupRef)
        {
            if (oSchemaSet.GlobalElements.Contains(GroupRef.RefName))
            {
                XmlSchemaGroup ret = oSchemaSet.GlobalElements[GroupRef.RefName] as XmlSchemaGroup;
                return ret.Particle as XmlSchemaGroupBase;
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find the global element group from the element group reference '" + GroupRef.RefName + "'", Log.LogType.Error);
            }
            return null;
        }

        static public XmlSchemaElement GetElementFromSchema(XmlSchemaSet oSchemaSet, XmlSchemaElement EleRef)
        {
            if (oSchemaSet.GlobalElements.Contains(EleRef.RefName))
            {
                return oSchemaSet.GlobalElements[EleRef.RefName] as XmlSchemaElement;
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find the global element from the element reference '" + EleRef.RefName + "'", Log.LogType.Error);
            }
            return null;
        }

        static public XmlSchemaAttribute GetAttributeFromSchema(XmlSchemaSet oSchemaSet, XmlSchemaAttribute AttRef)
        {
            if (oSchemaSet.GlobalAttributes.Contains(AttRef.RefName))
            {
                return oSchemaSet.GlobalAttributes[AttRef.RefName] as XmlSchemaAttribute;
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find the global attribute from the attribute reference '" + AttRef.RefName + "'", Log.LogType.Error);
            }
            return null;
        }

        static public void GetAllAttributesFromCollection(XmlSchemaSet oSchemaSet, XmlSchemaObjectCollection SourceColl, XmlSchemaObjectCollection DestColl)
        {
            foreach (XmlSchemaObject SchObj in SourceColl)
            {
                if (SchObj is XmlSchemaAttribute)
                {
                    DestColl.Add(SchObj);
                }
                else if (SchObj is XmlSchemaAttributeGroupRef)
                {
                    XmlSchemaAttributeGroupRef GroupRef = SchObj as XmlSchemaAttributeGroupRef;
                    XmlSchemaAttributeGroup Group = XMLHelper.GetAttributeGroupFromSchema(oSchemaSet, GroupRef);
                    GetAllAttributesFromCollection(oSchemaSet, Group.Attributes, DestColl);
                }
            }
        }

        static public string GetNamespacePrefix(XmlSerializerNamespaces namespaces, XmlQualifiedName Name)
        {
            XmlQualifiedName[] qualifiedNames = namespaces.ToArray();
            for (int i = 0; i < qualifiedNames.Length; i++)
            {
                if (qualifiedNames[i].Namespace == Name.Namespace)
                {
                    return qualifiedNames[i].Name;
                }
            }
            return "";
        }

        static public string GetNamespacePrefix(XmlSchemaSet oSchemaSet, XmlQualifiedName Name)
        {
            foreach (XmlSchema oSchema in oSchemaSet.Schemas())
            {
                XmlSerializerNamespaces namespaces = oSchema.Namespaces;
                string prefix = GetNamespacePrefix(namespaces, Name);
                if (!String.IsNullOrEmpty(prefix))
                    return prefix;
            }
            return "";
        }

        /// <summary>
        /// Gets the first Namespace given a Namespace-Prefix dictionary and a prefix, but never the empty namespace.
        /// </summary>
        /// <returns>The Namespace of if one isn't found an empty string</returns>
        static public string GetNamespaceFromPrefix(Dictionary<String, String> oNamespacePrefixDict, string prefix)
        {
            String NS = "";
            foreach (KeyValuePair<String, String> NSPrefixKVP in oNamespacePrefixDict)
            {
                if (prefix.Equals(NSPrefixKVP.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!String.IsNullOrEmpty(NSPrefixKVP.Key))
                    {
                        NS = NSPrefixKVP.Key;
                        break;
                    }
                }
            }
            return NS;
        }

        /// <summary>
        /// Gets all the possible Namespaces given a Namespace-Prefix dictionary and a prefix.  Inlcuding the empty namespace.
        /// </summary>
        /// <returns>The Namespace of if one isn't found an empty string</returns>
        static public string[] GetNamespacesFromPrefix(Dictionary<String, String> oNamespacePrefixDict, string prefix)
        {
            if (null == prefix)
                prefix = "";
            List<String> Namespaces = new List<string>();
            foreach (KeyValuePair<String, String> NSPrefixKVP in oNamespacePrefixDict)
            {
                if (prefix.Equals(NSPrefixKVP.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    Namespaces.Add(NSPrefixKVP.Key);
                }
            }
            return Namespaces.ToArray();
        }

        static public XPathNavigator[] SelectElementsOfType(XPathNavigator Navigator, String Prefix, XmlQualifiedName TypeName)
        {
            XPathNodeIterator NodeIter;
            XPathNavigator[] Nodes = null;
            
            try
            {
                if (!String.IsNullOrEmpty(Prefix))
                {
                    XmlNamespaceManager manager = new XmlNamespaceManager(Navigator.NameTable);
                    manager.AddNamespace(Prefix, TypeName.Namespace);
                    XPathExpression query = Navigator.Compile("//" + Prefix + ":" + TypeName.Name);
                    query.SetContext(manager);
                    NodeIter = Navigator.Select(query);
                }
                else
                {
                    NodeIter = Navigator.Select("//" + TypeName.Name);
                }

                // Create array of navigators
                Nodes = new XPathNavigator[NodeIter.Count];
                int index = 0;
                // Make an array of all the XPathNavigator objects, to speed access to their values
                foreach (XPathNavigator XPathNav in NodeIter)
                {
                    Nodes[index] = XPathNav;
                    index++;
                }
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return Nodes;
        }

        static public XPathNavigator[] SelectElementsOfType(XmlQualifiedName TypeName, XmlDocument XmlDoc, Dictionary<string, string> NamespacePrefixDict)
        {
            // If the XmlQualifiedName has a namespace, then we assign a prefix to that namespace for our query.  It doesn't
            // matter if the prefix is the same as the one used in the actual document.  We do this as often there is a default
            // namespace that has no prefix, and this causes the XPath query to fail, as it needs a prefix otherwise it can't
            // distinguish nodes from no namespace and nodes from the default.
            string prefix = "";
            if (!String.IsNullOrEmpty(TypeName.Namespace))
            {
                //prefix = XMLHelper.GetNamespacePrefix(XmlDoc.Schemas, TypeName);
                NamespacePrefixDict.TryGetValue(TypeName.Namespace, out prefix);
                // Let's hope assigning it an arbitrary one will work!!
                if (String.IsNullOrEmpty(prefix))
                    prefix = "a";
            }

            XPathNavigator navigator = XmlDoc.CreateNavigator();

            return XMLHelper.SelectElementsOfType(navigator, prefix, TypeName);
        }

        static public XPathNavigator[] SelectAttributesOfType(XPathNavigator Navigator, String Prefix, XmlQualifiedName TypeName)
        {
            XPathNodeIterator NodeIter;
            XPathNavigator[] Nodes = null;
            try
            {
                if (!String.IsNullOrEmpty(Prefix))
                {
                    XmlNamespaceManager manager = new XmlNamespaceManager(Navigator.NameTable);
                    manager.AddNamespace(Prefix, TypeName.Namespace);
                    XPathExpression query = Navigator.Compile("//@" + Prefix + ":" + TypeName.Name);
                    query.SetContext(manager);
                    NodeIter = Navigator.Select(query);
                }
                else
                {
                    NodeIter = Navigator.Select("//@" + TypeName.Name);
                }

                // Create array of navigators
                Nodes = new XPathNavigator[NodeIter.Count];
                int index = 0;
                // Make an array of all the XPathNavigator objects, to speed access to their values
                foreach (XPathNavigator XPathNav in NodeIter)
                {
                    Nodes[index] = XPathNav;
                    index++;
                }
            }
            catch (Exception e)
            {
                Log.Write(e);
            }
            return Nodes;
        }

        static public XPathNavigator[] SelectAttributesOfType(XmlQualifiedName TypeName, XmlDocument XmlDoc, Dictionary<string, string> NamespacePrefixDict)
        {
            string prefix = "";
            if (!String.IsNullOrEmpty(TypeName.Namespace))
            {
                NamespacePrefixDict.TryGetValue(TypeName.Namespace, out prefix);
                // I don't think this is needed in the attribute case
                //if (String.IsNullOrEmpty(prefix))
                //    prefix = "a";
            }

            XPathNavigator navigator = XmlDoc.CreateNavigator();
            return XMLHelper.SelectAttributesOfType(navigator, prefix, TypeName);
        }

        /// <summary>
        /// This will uniqely select nodes with a certain UniqueIdentifier (a certain name and of a certain type e.g. if there are 
        /// 2 elements of the same name but differing types this will accurately select only those of the requested type.
        /// </summary>
        static public XPathObjectList SelectObjectsOfType(XMLObjectIdentifier Id, XmlDocument XmlDoc, Dictionary<string, string> NamespacePrefixDict)
        {
            XPathNavigator[] res = null;
            if (Id is XMLElementIdentifier)
                res = SelectElementsOfType(Id.QualifiedName, XmlDoc, NamespacePrefixDict);
            else if (Id is XMLAttributeIdentifier)
                res = SelectAttributesOfType(Id.QualifiedName, XmlDoc, NamespacePrefixDict);
            else
                Log.Write(MethodBase.GetCurrentMethod(), "Id was an unrecognised type - '" + Id.GetType().ToString() + "'", Log.LogType.Error);

            return new XPathObjectList(res, Id, XmlDoc.Schemas);
        }

        static public XPathNavigator GetRootNode(XmlDocument oXMLDoc)
        {
            // Get an XPathNav to the root node
            XPathNavigator XPathNav = oXMLDoc.CreateNavigator();
            XPathNav.MoveToRoot();
            XPathNav.MoveToFirstChild();
            while (XPathNav.NodeType != XPathNodeType.Element)
            {
                // Get the XmlQualified Name of the root node
                XPathNav.MoveToNext();
            }
            return XPathNav;
        }

        public static void CreateSchemaObjectDatabase(XmlDocument oXMLDoc, XmlSchemaSet oSchemaSet, ObjectDataBase ElementDictionary)
        {
            XPathNavigator RootNameXPath = XMLHelper.GetRootNode(oXMLDoc);
            XmlQualifiedName QRootName = new XmlQualifiedName(RootNameXPath.LocalName, RootNameXPath.NamespaceURI);
            // Now that we know the root node, we can create our database from it with the knowledge that all other schema objects will
            // be encountered as we descend the hierarchy.
            CreateSchemaObjectDatabase(QRootName, oSchemaSet, ElementDictionary);
        }

        public static void CreateSchemaObjectDatabase(XmlQualifiedName QRootName, XmlSchemaSet oSchemaSet, ObjectDataBase oObjectDataBase)
        {
            ParticleDBEntry ParticleDB = null;
            if (null != QRootName)
            {
                // Get the root nodes schema from the schema set of global elements
                XmlSchemaElement RootSchEle = oSchemaSet.GlobalElements[QRootName] as XmlSchemaElement;

                if (null == RootSchEle)
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not find root node '" + QRootName.ToString() + "'", Log.LogType.Error);

                // Create the database (each entry adds itself to the database)
                ParticleDB = new ElementDBEntry(RootSchEle as XmlSchemaParticle, 0, oSchemaSet, oObjectDataBase);
            }

            // However, when multiple Schemas are used (or a strangley defined Schema), then this will not pick up all the schmea elements,
            // so we search for global ones not in the database, and hope that these and their children will reflect all schema elements
            foreach (XmlSchemaElement Element in oSchemaSet.GlobalElements.Values)
            {
                XMLElementIdentifier Id = new XMLElementIdentifier(Element.QualifiedName, Element, oSchemaSet);
                if (oObjectDataBase.Contains(Id))
                    continue;

                ParticleDB = new ElementDBEntry(Element as XmlSchemaParticle, 0, oSchemaSet, oObjectDataBase);
            }
        }

        /// <summary>
        /// Populates a Dictionary of namespaces and their prefixes.  Includes the empty namespace and empty prefix.
        /// </summary>
        /// <param name="oNamespacePrefixDict">This should be created before it is passed into the function</param>
        /// <param name="oSchemaSet"></param>
        public static void CreateNamespacePrefixDictionary(Dictionary<String, String> oNamespacePrefixDict, XmlSchemaSet oSchemaSet)
        {
            // We add this so that if ever an empty key is passed in our code will not fail
            oNamespacePrefixDict.Add("", "");

            foreach (XmlSchema oSchema in oSchemaSet.Schemas())
            {
                XmlSerializerNamespaces namespaces = oSchema.Namespaces;
                XmlQualifiedName[] qualifiedNames = namespaces.ToArray();
                for (int i = 0; i < qualifiedNames.Length; i++)
                {
                    if (!oNamespacePrefixDict.ContainsKey(qualifiedNames[i].Namespace))
                    {
                        // Only add a namespace with a prefix, it makes everything easy if we can assume there is always a prefix.
                        //if(!String.IsNullOrEmpty(qualifiedNames[i].Name))
                        oNamespacePrefixDict.Add(qualifiedNames[i].Namespace, qualifiedNames[i].Name);
                    }
                    else
                    {
                        // Check if the Namespace key that already exists has a non empty prefix, otherwise update it
                        string prefix = oNamespacePrefixDict[qualifiedNames[i].Namespace];
                        if (String.IsNullOrEmpty(prefix))
                        {
                            oNamespacePrefixDict.Remove(qualifiedNames[i].Namespace);
                            oNamespacePrefixDict.Add(qualifiedNames[i].Namespace, qualifiedNames[i].Name);
                        }
                    }
                }
            }
            // We may not have added the Target namespace of the schemas
            foreach (XmlSchema oSchema in oSchemaSet.Schemas())
            {
                if (!oNamespacePrefixDict.ContainsKey(oSchema.TargetNamespace))
                {
                    // We have little choice but to add it with an empty prefix
                    oNamespacePrefixDict.Add(oSchema.TargetNamespace, "");
                }
            }
        }

        /// <summary>
        /// Converts between the XmlTypeCode of System.Xml and our internal Common.DataSchema.DataSchemaTypeCode
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public static DataSchemaTypeCode ConvertXmlTypeCodeToDataSchemaType(XmlTypeCode Code)
        {
            switch(Code)
            {
                case XmlTypeCode.Base64Binary:
                    return DataSchemaTypeCode.Base64;
                case XmlTypeCode.HexBinary:
                    return DataSchemaTypeCode.Hex;
                default:
                {
                    String value = Enum.GetName(typeof(XmlTypeCode), Code);
                    DataSchemaTypeCode ret = DataSchemaTypeCode.None;
                    try
                    {
                        ret = (DataSchemaTypeCode)Enum.Parse(typeof(DataSchemaTypeCode), value);
                    }
                    catch (Exception)
                    {
                        // The value was not valid
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not convert '" + value + "' to DataSchemaType", Log.LogType.Warning);
                    }
                    return ret;
                }
                    
            }
        }

        /// <summary>
        /// Tries to find the XmlSchemaElement in oXmlSchema with name oElementName.
        /// </summary>
        /// <param name="oXmlSchema">The XmlSchema to search</param>
        /// <param name="oElementName">The XmlSchemaElement name to search for</param>
        /// <returns>The XmlSchemaElement or null if it is not found</returns>
        public static XmlSchemaElement GetElementFromUncompiledSchema(XmlSchema oXmlSchema, String oElementName)
        {
            for (int i = 0; i < oXmlSchema.Items.Count; i++)
            {
                if (oXmlSchema.Items[i] is XmlSchemaElement)
                {
                    XmlSchemaElement oXmlSchemaElement = oXmlSchema.Items[i] as XmlSchemaElement;
                    if (oXmlSchemaElement.Name.Equals(oElementName))
                        return oXmlSchemaElement;
                }
            }
            return null;
        }
    }

    class MyXmlResolver : XmlResolver
    {
        Dictionary<String, XmlSchema> oSchemaDict;
        String[] oSchemaLocations;

        public MyXmlResolver(Dictionary<String, XmlSchema> SchemaDict)
        {
            oSchemaDict = SchemaDict;
        }

        public override System.Net.ICredentials Credentials
        {
            set 
            { 
                throw new NotImplementedException(); 
            }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (oSchemaDict.ContainsKey(absoluteUri.LocalPath))
                return oSchemaDict[absoluteUri.LocalPath];
            return null;
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            Uri oSchemaLocation = base.ResolveUri(baseUri, relativeUri);
            if (!oSchemaDict.ContainsKey(oSchemaLocation.LocalPath))
            {
                if (null == oSchemaLocations)
                {
                    oSchemaLocations = new String[oSchemaDict.Keys.Count];
                    oSchemaDict.Keys.CopyTo(oSchemaLocations, 0);
                }

                // Find Key that ends in relativeUri
                String LocalRelativeUri = relativeUri;
                if(!oSchemaLocation.IsFile)
                {
                    // Get the file name from the URL
                    string[] Segments = oSchemaLocation.Segments;
                    if(Segments.Length > 0)
                        LocalRelativeUri = Segments[Segments.Length - 1];
                }
                for (int i = 0; i < oSchemaLocations.Length; i++)
                {
                    String Dir = Path.GetDirectoryName(oSchemaLocations[i]);
                    String CanonedRelUri = Path.GetFullPath(Path.Combine(Dir, LocalRelativeUri));
                    if (oSchemaDict.ContainsKey(CanonedRelUri))
                    {
                        oSchemaLocation = new Uri(CanonedRelUri, UriKind.Absolute);
                        break;
                    }
                }
            }
            if (!oSchemaDict.ContainsKey(oSchemaLocation.LocalPath))
                Log.Write(MethodBase.GetCurrentMethod(), "Location of XML schema '" + relativeUri + "' was not found amongst list of included/imported/redefined XML schema", Log.LogType.Warning);
            
            return oSchemaLocation;
        }
    }
}
