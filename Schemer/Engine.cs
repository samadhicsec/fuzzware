using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Collections;
using System.Threading;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Common.Encoding;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Common.XML.Restrictions;
using Fuzzware.Schemer.Statistics;
using Fuzzware.Schemer.Fuzzers;
using Fuzzware.Schemer.Editors;
using Fuzzware.ConvertFromXML.Processors;

namespace Fuzzware.Schemer
{
    /// <summary>
    /// This is the core processing engine of Schemer
    /// </summary>
    class Engine
    {
        private EngineState State;
        private ConfigData oConfigData;
        private PreCompData oPreComp;
        private FuzzerFactory oFuzzerFactory;
        private Monitor oMonitor;
        private Controller oController;
        Thread ControllerThread;

        public Engine(String ConfigXML)
        {
            oConfigData = PreProcessor.ReadConfigData(ConfigXML);
            oPreComp = PreProcessor.CreateCompiledData(oConfigData);

            if (!oPreComp.SchemaSet.IsCompiled)
                Log.Write(MethodBase.GetCurrentMethod(), "The supplied schemas have not compiled properly.  See error messages and check the log.", Log.LogType.Error);
        }

        public ConfigData ConfigData
        {
            get { return oConfigData; }
        }

        public PreCompData PreComp
        {
            get { return oPreComp; }
        }

        /// <summary>
        /// Core fuzzing routine.  Loops through fuzzable objects and creates fuzzers for them, before passing them to the Dispatcher
        /// </summary>
        /// <param name="oDispatcher"></param>
        public void Fuzz(Dispatcher oDispatcher)
        {
            try
            {
                // Initialise statistics
                FuzzerStats.CountOfSchemaTypes = oPreComp.ObjectNodeList.Count;

                // Create the state for the fuzzer, the State will make the fuzzing abide by any configuration settings
                State = new EngineState(oConfigData, oPreComp);

                // Create a controller to be able to pause, skip, start and stop the fuzzing
                StartController(State);

                // Start monitoring
                oMonitor = new Monitor(oConfigData.Config.Output.Monitoring, oController, State);
                oMonitor.Start();

                // Initialise the state, this will set any start or end state
                State.Initialise();

                // Create Fuzzer factory
                oFuzzerFactory = new FuzzerFactory(oConfigData, oPreComp);

                Log.Write(MethodBase.GetCurrentMethod(), "Fuzzing started...", Log.LogType.Status);

                // For each schema object that we can fuzz
                do
                {
                    // When we fuzz and change the contents of the document, it becomes invalid, this affects our ability to use post-schema
                    // validation information, so we re-read and verify the document with each node fuzzed
                    oPreComp.XMLDoc = XMLHelper.LoadAndCompileXML(oPreComp.XMLFile, oPreComp.SchemaSet, null);
                    // We require the XML document to be compiled
                    if (null == oPreComp.XMLDoc.SchemaInfo)
                        Log.Write(MethodBase.GetCurrentMethod(), "The XML file '" + oPreComp.XMLFile + "' has not compiled properly against the supplied XML schemas.  See error messages and check the log.", Log.LogType.Error);

                    // Get the schema object
                    ObjectDBEntry SchObject = State.CurrentSchemaObject;

                    // Select all schema objects of this XMLObjectIdentifier
                    XPathObjectList SchObjectsList = XMLHelper.SelectObjectsOfType(State.CurrentObjectId, oPreComp.XMLDoc, oPreComp.NamespacePrefixDict);
                    
                    if (SchObjectsList == null)
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "XMLHelper.SelectObjectsOfType returned null", Log.LogType.Error);
                    }

                    FuzzerStats.CountOfXMLNodes += SchObjectsList.Count;

                    // TODO: What to do when a node is not present?  This goes to the nature of the fuzzer, either we fuzz by an example 
                    // given to us, or we try to create data of this type.  There is probably a half-way house.
                    if (0 == SchObjectsList.Count)
                    {
                        FuzzerStats.CountOfSchemaTypesWithNoExample++;
                        continue;
                    }

                    // If the schema element is fixed, don't fuzz it
                    if (SchObject.HasFixedValue)
                        SchObjectsList = new XPathObjectList(new XPathNavigator[0], SchObject.ObjectId, oPreComp.SchemaSet);

                    // Immediately exclude nodes based on any relevant commands found in processing instructions preceeding the selected nodes
                    int PreExcludedCount = SchObjectsList.Count;
                    // TODO find a way to exclude attributes
                    XMLProcInstCommands.ExcludeNodes(SchObjectsList);
                    FuzzerStats.CountOfXMLNodesExcluded += (PreExcludedCount - SchObjectsList.Count);

                    // Check if we excluded all the nodes
                    if (0 == SchObjectsList.Count)
                        continue;

                    // It is important that this gets set after we exclude nodes.
                    State.ObjectInstanceList = SchObjectsList;

                    // Get the XmlSchemaType of the current schema object
                    XmlSchemaType SchType = State.CurrentSchemaObject.SchemaType;
                    
                    // Report what schema object we are fuzzing
                    if(State.CurrentObjectId is XMLElementIdentifier)
                        Log.Write(MethodBase.GetCurrentMethod(), "[" + (State.CurrentSchemaObjectIndex+1) + "/" + oPreComp.ObjectNodeList.Count + "] " + 
                            "Fuzzing " + SchObjectsList.Count + " node(s): " + SchObject.Name, Log.LogType.Status);
                    else if (State.CurrentObjectId is XMLAttributeIdentifier)
                        Log.Write(MethodBase.GetCurrentMethod(), "[" + (State.CurrentSchemaObjectIndex+1) + "/" + oPreComp.ObjectNodeList.Count + "] " + 
                            "Fuzzing " + SchObjectsList.Count + " attribute(s): " + SchObject.Name, Log.LogType.Status);

                    // Create a fuzzer group, this will hold the different fuzzers that are applicable to what we are fuzzing
                    FuzzerGroup oFuzzerGroup = new FuzzerGroup();

                    // If it is a simple type
                    if (SchObject.IsSimpleType)
                    {
                        AddSimpleTypeFuzzer(SchObjectsList, SchType as XmlSchemaSimpleType, oFuzzerGroup);
                    }
                    // If it is a complex type
                    else if (SchObject.IsComplexType)
                    {
                        // Create the attribute fuzzer.  Do this regardless complex type.  Add to group.
                        oFuzzerGroup.Add(oFuzzerFactory.CreateAttributeFuzzers(State.CurrentObjectId as XMLElementIdentifier, SchObjectsList));

                        if (SchObject.HasSimpleContent)
                        {
                            // An extension or restriction of a simple type
                            AddSimpleContentTypeFuzzer(SchObjectsList, (SchType as XmlSchemaComplexType).ContentModel as XmlSchemaSimpleContent, oFuzzerGroup);
                        }
                        else if (SchObject.HasComplexContent)
                        {
                            // An extension or restriction of a complex type
                            AddComplexContentTypeFuzzer(SchObjectsList, SchType as XmlSchemaComplexType, oFuzzerGroup);
                        }
                        else
                        {
                            // A typical complex type
                            AddComplexTypeFuzzer(SchObjectsList, SchType as XmlSchemaComplexType, oFuzzerGroup);
                        }
                    }

                    //// Below commented out so log would record that no fuzzers were used with a node, if we just skip Dispatch then
                    //// it is not obvious why no fuzzing was done for node (if for instance it was integer and no integer fuzzers exist)
                    // Make sure fuzzers were added.
                    //if (oFuzzerGroup.Methods().Length > 0)
                    //{
                        // Dispatch to do fuzzing
                        oDispatcher.Dispatch(oPreComp.XMLDoc, State, oFuzzerGroup);
                    //}
                }
                while (State.NextState());

                Log.Write(MethodBase.GetCurrentMethod(), "Fuzzing finished.", Log.LogType.Status);
            }
            catch (Exception)
            {
                try
                {
                    // Log the state
                    if (null != State)
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "State = " + State.ToString(), Log.LogType.Info);
                    }
                    oMonitor.Stop();
                    EndController();
                }
                catch (Exception) { }

                throw;
            }
            
            oMonitor.Stop();
            EndController();
        }

        private void AddSimpleTypeFuzzer(XPathObjectList XPathNodes, XmlSchemaSimpleType SchType, FuzzerGroup oFuzzerGroup)
        {
            SimpleTypeEditor STEditor = new SimpleTypeEditor(XPathNodes, oPreComp);
            AddSimpleTypeFuzzer(STEditor, SchType, oFuzzerGroup);
        }

        private void AddSimpleTypeFuzzer(XPathObjectList XPathNodes, XmlSchemaSimpleType SchType, XmlSchemaObjectCollection ExtraFacets, FuzzerGroup oFuzzerGroup)
        {
            SimpleTypeEditor STEditor = new SimpleTypeEditor(XPathNodes, oPreComp);
            AddSimpleTypeFuzzer(STEditor, SchType, ExtraFacets, oFuzzerGroup);
        }

        private void AddSimpleTypeFuzzer(SimpleTypeEditor STEditor, XmlSchemaSimpleType SchType, FuzzerGroup oFuzzerGroup)
        {
            AddSimpleTypeFuzzer(STEditor, SchType, null, oFuzzerGroup);
        }

        private void AddSimpleTypeFuzzer(SimpleTypeEditor STEditor, XmlSchemaSimpleType SchType, XmlSchemaObjectCollection ExtraFacets, FuzzerGroup oFuzzerGroup)
        {
            // Do reality check, are there any nodes to fuzz
            if (STEditor.NodeCount() <= 0)
                return;

            // Need to check if the nodes are encoded.
            Coder.EncodingType Encoding = Coder.EncodingType.None;
            if( (SchType.TypeCode == XmlTypeCode.Base64Binary) || (SchType.TypeCode == XmlTypeCode.HexBinary) ||
                (SchemaAttributeCommands.GetEncoding(oPreComp.GetOutputSettings(oConfigData), State.CurrentObjectId, out Encoding)) )
            {
                if (SchType.TypeCode == XmlTypeCode.Base64Binary)
                    Encoding = Coder.EncodingType.Base64;
                else if (SchType.TypeCode == XmlTypeCode.HexBinary)
                    Encoding = Coder.EncodingType.Hex;
            }

            DataSchemaTypeCode DataType = DataSchemaTypeCode.None;
            // Need to get the underlying type of encoded nodes
            if (Encoding != Coder.EncodingType.None)
            {
                // Regardless of whether or not the command detailing the underlying type is present we still need to fuzz the data somehow.
                // So this function will return the default value of this command (which will typically be XmlTypeCode.None).
                SchemaAttributeCommands.GetTreatAsEncoded(oPreComp.GetOutputSettings(oConfigData), State.CurrentObjectId, out DataType);
            }
            // If the user did not explicitly set the datatype, get it from the Schema TypeCode
            if(DataType == DataSchemaTypeCode.None)
            {
                DataType = XMLHelper.ConvertXmlTypeCodeToDataSchemaType(SchType.TypeCode);
            }
            if (DataType == DataSchemaTypeCode.None)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "DataType was None", Log.LogType.Warning);
                return;
            }

            // Create facet restrictions based on type
            TypeRestrictor oTypeRestrictor = FacetRestrictions.GetTypeRestrictor(DataType);
            // Based on the type of restriction. use the functions of ITypeRestrictions to limit the fuzzing
            if (null != SchType.Content)
            {
                if (SchType.Content is XmlSchemaSimpleTypeList)
                {
                    // TODO: Not supported yet
                    Log.Write(MethodBase.GetCurrentMethod(), "A Schema type with content XmlSchemaSimpleTypeList is not currently supported", Log.LogType.Warning);
                }
                else if (SchType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    FacetRestrictions.Apply(oTypeRestrictor, (SchType.Content as XmlSchemaSimpleTypeRestriction).Facets);
                }
                else if (SchType.Content is XmlSchemaSimpleTypeUnion)
                {
                    // TODO: Not supported yet
                    Log.Write(MethodBase.GetCurrentMethod(), "A Schema type with content XmlSchemaSimpleTypeUnion is not currently supported", Log.LogType.Warning);
                }
                else
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "SchType.Content is the wrong type!", Log.LogType.Error);
                }
            }
            // Apply any extra Facets passed in
            if (null != ExtraFacets)
            {
                FacetRestrictions.Apply(oTypeRestrictor, ExtraFacets);
            }

            // Create the fuzzer
            List<ITypeFuzzer> Fuzzers = oFuzzerFactory.CreateSimpleTypeFuzzers(DataType, oTypeRestrictor, State.CurrentSchemaObject);
            if (Fuzzers.Count == 0)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Cannot currently fuzz elements of type '" + Enum.GetName(typeof(DataSchemaTypeCode), DataType) + "'", Log.LogType.Warning);
                return;
            }

            // Set up fuzzer properties
            for (int i = 0; i < Fuzzers.Count; i++)
            {
                ITypeFuzzer Fuzzer = Fuzzers[i];
                
                // Set the Coder for the fuzzer
                Fuzzer.FuzzCoder = Coder.AssignCoder(Encoding);

                // Set the class the fuzzer uses to change values
                Fuzzer.ValuesEditor = STEditor;

                // Add the fuzzer to the fuzzer group
                oFuzzerGroup.Add(Fuzzer as IFuzzer);
            }
        }

        private void AddComplexTypeFuzzer(XPathObjectList XPathNodes, XmlSchemaComplexType SchType, FuzzerGroup oFuzzerGroup)
        {
            // Do reality check, are there any node to fuzz
            if (XPathNodes.Count <= 0)
                return;

            if (null == SchType.Particle)
            {
                // TODO This element is probably type anyType, need to decide how we are going to fuzz this, if at all.
                Log.Write(MethodBase.GetCurrentMethod(), "Cannot currently fuzz elements of type 'anyType'", Log.LogType.Warning);
                return;
            }

            // TODO: Current thinking is not to bother changing the order of elements as the only useful place for this fuzzing an
            // XML parser.  There may be some file formats that can accept data in arbitrary order, but I know of no examples.
            //FuzzOrder(Nodes, SchType as XmlSchemaComplexType);

            // Add Complex Type fuzzers to the FuzzerGroup
            //AddOccurrenceFuzzer(XPathNodes, oFuzzerGroup);
            oFuzzerGroup.Add(oFuzzerFactory.CreateComplexTypeFuzzers(State.CurrentObjectId as XMLElementIdentifier, XPathNodes));
        }

        private void AddComplexContentTypeFuzzer(XPathObjectList XPathNodes, XmlSchemaComplexType SchType, FuzzerGroup oFuzzerGroup)
        {
            XmlSchemaComplexContent ComplexContent = SchType.ContentModel as XmlSchemaComplexContent;
            if (ComplexContent.Content is XmlSchemaComplexContentRestriction)
            {
                // Fuzz like a complex type, but with added restrictions.  The restriction element redefines the entire
                // particle so we don't have to sort out particular restrictions on particular elements
                AddComplexTypeFuzzer(XPathNodes, (ComplexContent.Content as XmlSchemaComplexContentRestriction).Particle.Parent as XmlSchemaComplexType, oFuzzerGroup);
            }
            else if (ComplexContent.Content is XmlSchemaComplexContentExtension)
            {
                // Fuzz like a complex type, but with extension.  
                // Elements added via extension are treated as if they were appended to the content model of the base type in sequence
                // Create a parent particle that is a sequence of the base and extension, it's not obvious that you can have extensions of extensions.
                XmlSchemaComplexType SchComplexType = oPreComp.SchemaSet.GlobalTypes[(ComplexContent.Content as XmlSchemaComplexContentExtension).BaseTypeName] as XmlSchemaComplexType;
                // Remember the base particle
                XmlSchemaParticle BaseParticle = SchComplexType.Particle;
                
                XmlSchemaParticle SubParticle = null;
                if (null != (ComplexContent.Content as XmlSchemaComplexContentExtension).Particle)
                {
                    // Create a new particle that is the sequence of the base and extension
                    XmlSchemaSequence ExtSeq = new XmlSchemaSequence();
                    if(null != SchComplexType.Particle)
                        ExtSeq.Items.Add(SchComplexType.Particle);
                    ExtSeq.Items.Add((ComplexContent.Content as XmlSchemaComplexContentExtension).Particle);
                    SubParticle = ExtSeq;
                }
                else
                    SubParticle = BaseParticle;

                // Assign it to the type
                SchComplexType.Particle = SubParticle;
                // Create fuzzer
                AddComplexTypeFuzzer(XPathNodes, SchComplexType, oFuzzerGroup);
                // Restore the original base particle.  This doesn't matter because SchComplexType is only used to check if there are
                // any sub-particles, the actual sub-particle are retrieved from the ObjectDB, and they have been stored there correctly
                SchComplexType.Particle = BaseParticle;
            }
        }

        private void AddSimpleContentTypeFuzzer(XPathObjectList XPathNodes, XmlSchemaSimpleContent SimpleContent, FuzzerGroup oFuzzerGroup)
        {
            if (SimpleContent.Content is XmlSchemaSimpleContentRestriction)
            {
                // Fuzz like a simple type but with added restrictions.  The base type initially will be another SimpleContent type,
                // however the base type might be based on another base type, so we need to find the very last base type.
                XmlSchemaSimpleContentRestriction Restrictions = SimpleContent.Content as XmlSchemaSimpleContentRestriction;
                XmlSchemaType oSchemaType = null;
                if (null == Restrictions.BaseType)
                    oSchemaType = XMLHelper.GetTypeFromSchema(Restrictions.BaseTypeName, oPreComp.SchemaSet);
                else
                    oSchemaType = Restrictions.BaseType;

                while (oSchemaType is XmlSchemaComplexType)
                {
                    oSchemaType = XMLHelper.GetTypeFromSchema(oSchemaType.BaseXmlSchemaType.QualifiedName, oPreComp.SchemaSet);
                }
                AddSimpleTypeFuzzer(XPathNodes, oSchemaType as XmlSchemaSimpleType, Restrictions.Facets, oFuzzerGroup);
            }
            else if (SimpleContent.Content is XmlSchemaSimpleContentExtension)
            {
                // Fuzz like a simple type, and note the added attributes
                XmlSchemaSimpleContentExtension Extensions = SimpleContent.Content as XmlSchemaSimpleContentExtension;
                // Get the type of the base element
                XmlSchemaType oSchType = XMLHelper.GetTypeFromSchema(Extensions.BaseTypeName, oPreComp.SchemaSet);
                AddSimpleTypeFuzzer(XPathNodes, oSchType as XmlSchemaSimpleType, oFuzzerGroup);
            }
        }

        // Not implemented as at this stage it is not obvious of the benefit of permuting the order of an <all>
        //private void FuzzOrder(XPathNavigator[] XPathNodes, XmlSchemaComplexType SchType)
        //{
        //    // The <all> indicator specifies that the child elements can appear in any order, and that each child element must occur only once
        //    // However, minOccurs is 0 or 1 (maxOccurs is 1)
        //    // Fuzz by permuting the order they appear in
        //}

        //private void AddOccurrenceFuzzer(XPathObjectList XPathNodes, FuzzerGroup oFuzzerGroup)
        //{
        //    // For all "Order" and "Group" indicators (any, all, choice, sequence, group name, and group reference) the default value 
        //    // for maxOccurs and minOccurs is 1!

        //    // Create the occurrence fuzzer
        //    OccurrenceFuzzer oOccurrenceFuzzer = new OccurrenceFuzzer(oConfigData, oPreComp, State.CurrentObjectId as XMLElementIdentifier);
        //    // Set IChildNodesFuzzer, which is the class the fuzzer uses to change values
        //    oOccurrenceFuzzer.ChildNodesEditor = new ChildNodeEditor(XPathNodes, oPreComp);
        //    // Add to group
        //    oFuzzerGroup.Add(oOccurrenceFuzzer);
        //}

        private void StartController(EngineState oState)
        {
            oController = new Controller(oState);
            oController.PrintControllerCommands();
            ControllerThread = new Thread(new ThreadStart(oController.KeyboardHandler));
            ControllerThread.Priority = ThreadPriority.Highest;
            ControllerThread.Start();
        }

        private void EndController()
        {
            ControllerThread.Abort();
        }
    }
}
