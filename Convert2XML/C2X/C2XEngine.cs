using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.XML;
using Fuzzware.Common.Encoding;
using Fuzzware.ConvertFromXML;
using Fuzzware.ConvertFromXML.Processors;

namespace Fuzzware.Convert2XML.C2X
{
    class C2XEngine
    {
        // XML information
        XmlSchemaSet oSchemaSet;
        XmlDocument oXMLDoc;
        String Prefix;
        Encoding OutputEncoding;
        OutputSettings oSettings;

        // Raw input references
        byte[] Input;
        int InputOffset;
        
        // Progress
        C2XProgress oC2XProgress;
        C2XProgressEventHandler dProgressEventHandler;

        // Error tracking
        int BaseCount = 1000000;
        int TabCount = 1000000;
        String ErrorNodePath;

        // Processing Instruction tracking
        private struct PIRef
        {
            public string PI;
            public String TargetNodeName;
            public String TargetNodeID;
            public ParticleDBEntry ParticleDBEntry;
            public int LengthOfTargetNode;
            public String TypeIDOfTargetNode;
        }
        List<PIRef> PIParticleList;
        int IDdifferentiator = 0;

        public C2XEngine(XmlSchemaSet oSchemaSet, Encoding OutputEncoding, ObjectDataBase ElementDB, C2XProgressEventHandler ProgressEventHandler)
        {
            this.oSchemaSet = oSchemaSet;
            this.OutputEncoding = OutputEncoding;
            oSettings = new OutputSettings();
            oSettings.XmlSettings.OmitXmlDeclaration = true;
            oSettings.Encoding = OutputEncoding.EncodingName;
            oSettings.ObjectDB = ElementDB;
            dProgressEventHandler = ProgressEventHandler;
        }

        /// <summary>
        /// Track errors on failed matches
        /// </summary>
        private void SetErrorInfo(ParticleDBEntry ParticleDBEntry)
        {
            String Desc = "";
            if (ParticleDBEntry is ElementDBEntry)
            {
                Desc = (ParticleDBEntry as ElementDBEntry).Name.Name;
            }
            else
            {
                if (ParticleDBEntry.Particle is XmlSchemaSequence)
                    Desc = "<Seq>";
                else if (ParticleDBEntry.Particle is XmlSchemaChoice)
                    Desc = "<Choice>";
                else if (ParticleDBEntry.Particle is XmlSchemaAll)
                    Desc = "<All>";
                else if (ParticleDBEntry.Particle is XmlSchemaAny)
                    Desc = "<Any>";
                else
                    Desc = "<Unknown Particle>";
            }

            if (String.IsNullOrEmpty(ErrorNodePath))
                ErrorNodePath = Desc;
            else
                ErrorNodePath = Desc + " " + (TabCount++) + " " + ErrorNodePath;
        }

        private void ClearErrorInfo()
        {
            ErrorNodePath = "";
            TabCount = BaseCount;
        }

        /// <summary>
        /// Format error for output
        /// </summary>
        private String FormatErrorInfo()
        {
            for (int i = BaseCount; i < TabCount; i++)
            {
                ErrorNodePath = ErrorNodePath.Replace(i.ToString(), "\n" + (new String(' ', 2*(TabCount - i))));
            }
            return ErrorNodePath;
        }

        /// <summary>
        /// Main function for converting byte array to XmlDocument
        /// </summary>
        public bool Run(byte[] InputFile, ElementDBEntry RootParticleDBEntry, String prefix, out XmlDocument retXMLDoc)
        {
            // Set up class variables
            Prefix = prefix;
            PIParticleList = new List<PIRef>();
            retXMLDoc = null;
            
            // Create a new XmlDocument
            oXMLDoc = new XmlDocument();

            // Add Xml declaration
            XmlDeclaration Decl = oXMLDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            oXMLDoc.AppendChild(Decl);

            // Create the root element
            XmlElement Element = oXMLDoc.CreateElement(prefix, RootParticleDBEntry.Name.Name, RootParticleDBEntry.Name.Namespace);
            oC2XProgress = new C2XProgress(Element, dProgressEventHandler);

            Input = InputFile;
            InputOffset = 0;
            int BytesUsed = 0;
            if (!Match(RootParticleDBEntry, 0, Element, 0, Input.Length, out BytesUsed))
            {
                SetErrorInfo(RootParticleDBEntry);
                Log.Write(MethodBase.GetCurrentMethod(), "Failed to match input to schema.  Path to failed node:\n" + FormatErrorInfo(), Log.LogType.Error);
                return false;
            }
            if (BytesUsed != Input.Length)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Not all input bytes (" + BytesUsed + " of " + Input.Length + ") were used in matching to the schema", Log.LogType.Error);
                return false;
            }
            
            // The root node may the source of a PI command, so we may need to add a PI before it (like file length)
            List<String> AppNameList = new List<string>();
            List<String> PIList = new List<string>();
            if (IsInputToProcInstFn(RootParticleDBEntry, AppNameList, PIList))
            {
                for (int i = 0; i < AppNameList.Count; i++)
                {
                    XmlProcessingInstruction ProcInst = oXMLDoc.CreateProcessingInstruction(AppNameList[i], PIList[i]);
                    oXMLDoc.AppendChild(ProcInst);
                }
            }
            // Add the root node
            oXMLDoc.AppendChild(Element);

            retXMLDoc = oXMLDoc;
            return true;
        }

        /// <summary>
        /// Match an unknown particle type.  This will determine the particle type and try to match based on the type.
        /// </summary>
        private bool Match(ParticleDBEntry ParticleDBEntry, int SubParticleIndex, XmlElement ParentElement, 
                            int ParentParticleCurLength, int ParentParticleTotalLength, out int BytesUsed)
        {
            // Initialise
            int ByteCount = 0;
            BytesUsed = 0;

            // Get reference to particle we are matching
            ParticleDBEntry SubParticleDBEntry = ParticleDBEntry.SubParticles[SubParticleIndex];

            // Determine if the particle is a group of particles or an actual element
            if(SubParticleDBEntry.Particle is XmlSchemaGroupBase)
            {
                if (MatchParticleGroup(ParticleDBEntry, SubParticleIndex, ParentElement, (ParentParticleTotalLength - ParentParticleCurLength), out ByteCount))
                {
                    BytesUsed = ByteCount;
                    ClearErrorInfo();
                    return true;
                }
            }
            else if(SubParticleDBEntry.Particle is XmlSchemaElement) 
            {
                System.Diagnostics.Debug.WriteLine("Matching Element: " + (SubParticleDBEntry as ElementDBEntry).Name.Name);
                int OccursCount = 0;
                int MaxOccursCount = GetMaxOccursCount(SubParticleDBEntry as ElementDBEntry);

                while ((OccursCount < MaxOccursCount) &&        // Check maximum number of occurrences
                       (InputOffset < Input.Length) &&          // Check bytes left in array
                       ((BytesUsed + ParentParticleCurLength) < ParentParticleTotalLength) )    // Check against parent length
                {
                    bool AddToParent = false;
                    // Create new element
                    XmlElement Element = oXMLDoc.CreateElement(Prefix, (SubParticleDBEntry as ElementDBEntry).Name.Name, (SubParticleDBEntry as ElementDBEntry).Name.Namespace);
                    XmlElement oProgressElement = oC2XProgress.AddElement(Element.Prefix, Element.LocalName, Element.NamespaceURI);
                    
                    // For complex type elements, recurse and match child particles
                    if ((SubParticleDBEntry.Particle as XmlSchemaElement).ElementSchemaType is XmlSchemaComplexType)
                    {
                        // Make a copy of the pending PI looking for an Id
                        List<PIRef> CopyOfPIParticleList = new List<PIRef>(PIParticleList);

                        if (MatchParticleGroup(SubParticleDBEntry, 0, Element, (ParentParticleTotalLength - ParentParticleCurLength), out ByteCount))
                            AddToParent = true;
                        else
                        {
                            // Reset the PendPIs as if we added or removed any this could cause confusion (since the list is global)
                            if (PIParticleList.Count != CopyOfPIParticleList.Count)
                                PIParticleList = CopyOfPIParticleList;
                        }
                    }
                    // For simple type elements, try to match input bytes to the simple data types
                    else if ((SubParticleDBEntry.Particle as XmlSchemaElement).ElementSchemaType is XmlSchemaSimpleType)
                    {
                        if (MatchSimpleType(ParticleDBEntry, SubParticleIndex, Element, (ParentParticleTotalLength - ParentParticleCurLength), out ByteCount))
                            AddToParent = true;
                    }

                    // Determine if we need to add an XML processing instruction
                    if (AddToParent)
                    {
                        String AppName = null;
                        String PI = null;
                        // Check if we need to add a PI to update the value of this node e.g. length nodes
                        if (IsOutputFromProcInstFn(SubParticleDBEntry as ElementDBEntry, Element, out AppName, out PI))
                        {
                            XmlProcessingInstruction ProcInst = oXMLDoc.CreateProcessingInstruction(AppName, PI);
                            ParentElement.AppendChild(ProcInst);
                            oC2XProgress.AddPI(AppName, PI, oProgressElement);
                        }
                        List<String> AppNameList = new List<string>();
                        List<String> PIList = new List<string>();
                        // Check if we need to add a PI to identify this node e.g. it's length is specified somewhere
                        if (IsInputToProcInstFn(SubParticleDBEntry as ElementDBEntry, AppNameList, PIList))
                        {
                            for (int i = 0; i < AppNameList.Count; i++)
                            {
                                XmlProcessingInstruction ProcInst = oXMLDoc.CreateProcessingInstruction(AppNameList[i], PIList[i]);
                                ParentElement.AppendChild(ProcInst);
                                oC2XProgress.AddPI(AppNameList[i], PIList[i], oProgressElement);
                            }
                        }
                        // Add element to parent
                        ParentElement.AppendChild(Element);
                        oC2XProgress.FinaliseElement(oProgressElement);
                        BytesUsed += ByteCount;
                        OccursCount++;
                    }
                    else
                    {
                        oC2XProgress.RemoveElement(oProgressElement);
                        break;
                    }
                }
                // Keep track or clear errors
                if (OccursCount > 0)
                    ClearErrorInfo();
                else
                {
                    //Log.Write(MethodBase.GetCurrentMethod(), "Match Failed at:\n" + ParentElement.OuterXml, Log.LogType.Info);
                    SetErrorInfo(SubParticleDBEntry as ElementDBEntry);
                }

                return (OccursCount > 0);
            }
            else
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Unsupported particle type", Log.LogType.Error);
            }

            return false;
        }

        /// <summary>
        /// Matches, complex type particles, sequences and choices.
        /// </summary>
        private bool MatchParticleGroup(ParticleDBEntry ParentParticleDBEntry, int SubParticleIndex, XmlElement ParentElement, int MaxLength, out int BytesUsed)
        {
            BytesUsed = 0;
            // Get reference to particle we are matching
            ParticleDBEntry ParticleDBEntry = ParentParticleDBEntry.SubParticles[SubParticleIndex];

            // Initialise
            int OccursCount = 0;
            int MaxOccursCount = GetMaxOccursCount(ParticleDBEntry);
            int Length = MaxLength;
            bool? MatchedOnZeroOcc = null;
            
            // Get TypeID if we are matching a choice
            String TypeID = "";
            if ((ParentParticleDBEntry.Particle is XmlSchemaElement) && (ParticleDBEntry.Particle is XmlSchemaChoice))
            {
                TypeID = GetChoiceTypeID((ParentParticleDBEntry as ElementDBEntry).Name.Name);
            }

            while ((OccursCount < MaxOccursCount) &&   // Check maximum number of occurrences
                   (InputOffset < Input.Length) &&     // Check bytes left in array
                   (BytesUsed < Length))               // Check against parent length
            {
                int i = 0;
                int CurrentInputOffset = InputOffset;

                // Go through each of the sub-particles
                for (; i < ParticleDBEntry.SubParticles.Length; i++)
                {
                    // Since one of the sub-particles might contain the length of the parent, we continually check the length
                    if (ParentParticleDBEntry.Particle is XmlSchemaElement)
                    {
                        int NewLength = HasPrescribedLength((ParentParticleDBEntry as ElementDBEntry).Name.Name);
                        if (NewLength < Length)
                            Length = NewLength;
                    }
            
                    // If the TypeID is set, then we are just looking for that
                    if (!String.IsNullOrEmpty(TypeID))
                    {
                        if (!ParticleHasTypeID(ParticleDBEntry.SubParticles[i], TypeID))
                            continue;
                    }

                    int ParticleByteCount = 0;
                    // Make a copy of the pending PI looking for an Id
                    List<PIRef> CopyOfPIParticleList = new List<PIRef>(PIParticleList);
                    // Get the number of children before the match
                    int ChildCountBeforeMatch = ParentElement.ChildNodes.Count;
                    
                    // Check for a MATCH
                    if (!Match(ParticleDBEntry, i, ParentElement, BytesUsed, Length, out ParticleByteCount))
                    {
                        // NO MATCH
                        // Reset the PendPIs as if we added or removed any this could cause confusion (since the list is global)
                        if (PIParticleList.Count != CopyOfPIParticleList.Count)
                            PIParticleList = CopyOfPIParticleList;

                        int SubParticleMinOccurs = (int)ParticleDBEntry.SubParticles[i].Particle.MinOccurs; 
                        // On no match, check if it's a problem for sequences
                        if (ParticleDBEntry.Particle is XmlSchemaSequence)
                        {
                            // Unless minOccurs == 0 for this particle this is a problem
                            if (0 != SubParticleMinOccurs)
                            {
                                InputOffset = CurrentInputOffset;
                                break;  // out of for
                            }
                        }

                        if(0 == SubParticleMinOccurs)   // set match on zero occurrences only if all sub-particles match of zero occurrence
                            MatchedOnZeroOcc = (MatchedOnZeroOcc == null)?true:((bool)MatchedOnZeroOcc && true);

                        // Remove any children added as part of a partial match on the current choice
                        if (ParentElement.ChildNodes.Count > ChildCountBeforeMatch)
                        {
                            // Remove from the progress object
                            oC2XProgress.RemoveChildrenOfLastXmlElement(ParentElement.ChildNodes.Count - ChildCountBeforeMatch);
                            // Remove from the actual output
                            while (ParentElement.ChildNodes.Count > ChildCountBeforeMatch)
                            {
                                ParentElement.RemoveChild(ParentElement.LastChild);
                            }
                        }
                        ClearErrorInfo();
                        continue;   // continue for choices
                    }
                    else
                        MatchedOnZeroOcc = false;

                    ClearErrorInfo();

                    // MATCH FOUND
                    BytesUsed += ParticleByteCount;

                    if (ParticleDBEntry.Particle is XmlSchemaChoice)
                        break;  // out of for

                    if (BytesUsed == Length)
                        break;
                }
                

                // Increase occurrence count if we found a match, or if we matched on a zero occurrence sub particle
                if ( ((ParticleDBEntry.Particle is XmlSchemaChoice) && ((i != ParticleDBEntry.SubParticles.Length) || (MatchedOnZeroOcc == true))) ||
                    ((ParticleDBEntry.Particle is XmlSchemaSequence) && ((i == ParticleDBEntry.SubParticles.Length) || (BytesUsed == Length))) )
                {
                    OccursCount++;
                    // If we MatchedOnZeroOcc then return
                    if (MatchedOnZeroOcc == true)
                        break;  // out of while  
                }
                else    // we didn't match on any sub-particles
                    break;  // out of while
            }
            if (OccursCount > 0)
                ClearErrorInfo();
            else
                SetErrorInfo(ParticleDBEntry);
                                
            return (OccursCount > 0);
        }

        /// <summary>
        /// If certain criteria are fulfilled, then we can convert some bytes into an actual XML element
        /// </summary>
        private bool MatchSimpleType(ParticleDBEntry ParentParticleDBEntry, int SubParticleIndex, XmlElement ParentElement, int MaxLengthFromParent, out int BytesUsed)
        {
            String DebugString = null;     // Used to display to the debug window the value matched
            BytesUsed = 0;
            // Get reference to particle we are matching
            ElementDBEntry ParticleDBEntry = ParentParticleDBEntry.SubParticles[SubParticleIndex] as ElementDBEntry;

            XmlSchemaElement SchElement = ParticleDBEntry.ElementParticle as XmlSchemaElement;
            List<String> EnumList = null;
            int Length = -1;
            String NodeValue = null;

            // Do some preliminary work here
            ElementDBEntry NextSiblingParticleDBEntry = null;
            XmlSchemaElement NextSchElement = null;
            if (SubParticleIndex < ParentParticleDBEntry.SubParticles.Length - 1)
            {
                NextSiblingParticleDBEntry = ParentParticleDBEntry.SubParticles[SubParticleIndex + 1] as ElementDBEntry;
                // Check if the next sibling is a fixed element, that will be the terminator for the current element
                if(null != NextSiblingParticleDBEntry)
                    NextSchElement = NextSiblingParticleDBEntry.Particle as XmlSchemaElement;
            }

            // Now check if we can match it

            // If it has a fixed value we can add it
            if (!String.IsNullOrEmpty(SchElement.FixedValue))
            {
                if (IsSpecificValue(SchElement.FixedValue, Input, InputOffset, ParticleDBEntry, out Length))
                {
                    // Get the fixed value as a byte array
                    NodeValue = SchElement.FixedValue;
                    InputOffset += Length;
                    BytesUsed = Length;
                    DebugString = "Found Specific Value: " + NodeValue;
                }
            }
            // If it is a enumeration we can add it
            else if (null != (EnumList = GetEnumValues(SchElement)))
            {
                for (int i = 0; i < EnumList.Count; i++)
                {
                    if (IsSpecificValue(EnumList[i], Input, InputOffset, ParticleDBEntry, out Length))
                    {
                        NodeValue = EnumList[i];
                        InputOffset += Length;
                        BytesUsed = Length;
                        DebugString = "Found Enum Value: " + NodeValue;
                        break;
                    }
                }
            }
            // If it has a length described somewhere else we can add it
            else if (Int32.MaxValue != (Length = HasPrescribedLength(SchElement.QualifiedName.Name)))
            {
                NodeValue = ConvertBytesToString(Input, InputOffset, Length, ParticleDBEntry);
                InputOffset += Length;
                BytesUsed = Length;
                DebugString = "Found Value with Prescribed Length: " + NodeValue;
            }
            // If it has a fixed size, we can add it, but only easily if it has maxOccurs == 1, otherwise difficult to tell
            // if the next 'size' of bytes is another occurrence of this one, or the first of the next element
            else if (-1 != (Length = HasFixedLength(ParticleDBEntry)))
            {
                NodeValue = ConvertBytesToString(Input, InputOffset, Length, ParticleDBEntry);
                InputOffset += Length;
                BytesUsed = Length;
                DebugString = "Found Value with Fixed Length: " + NodeValue;
            }
            // If it is a numeric type e.g. int or real, expressed as a string, then we can add it
            else if(GetNumericString(Input, InputOffset, ParticleDBEntry, out NodeValue, out Length))
            {
                InputOffset += Length;
                BytesUsed = Length;
                DebugString = "Found Value with Numeric String: " + NodeValue;
            }
            // If it is followed by an element with a fixed value or is an enum, should we add (unreliable)?
            // HTTP is an example where we need to do it this way and it's fine
            else if ((null != NextSchElement) && ((!String.IsNullOrEmpty(NextSchElement.FixedValue)) || (null != (EnumList = GetEnumValues(NextSchElement)))))
            {
                if (!String.IsNullOrEmpty(NextSchElement.FixedValue))
                {
                    EnumList = new List<string>();
                    EnumList.Add(NextSchElement.FixedValue);
                }

                // Serialise the terminating element
                Coder.OutputAsType outputType = Coder.OutputAsType.Unchanged;
                SchemaAttributeCommands.GetOutputAs(oSettings, NextSiblingParticleDBEntry.ObjectId, out outputType);
                List<byte[]> ListFixedValBytes = new List<byte[]>();
                foreach (string ConstValue in EnumList)
                    ListFixedValBytes.Add(EncodingHelper.EncodeForOutput(OutputEncoding.GetBytes(ConstValue), XMLHelper.ConvertXmlTypeCodeToDataSchemaType(XMLHelper.GetSchemaType(NextSchElement).TypeCode), outputType, OutputEncoding, OutputEncoding));

                // Serialise any escape characters for the terminating element
                string EscapeString;
                byte[] FixedEscapeValBytes = null;
                if (SchemaAttributeCommands.GetEscapeStr(oSettings, NextSiblingParticleDBEntry.ObjectId as XMLElementIdentifier, out EscapeString))
                {
                    FixedEscapeValBytes = EncodingHelper.EncodeForOutput(OutputEncoding.GetBytes(EscapeString), XMLHelper.ConvertXmlTypeCodeToDataSchemaType(XMLHelper.GetSchemaType(NextSchElement).TypeCode), outputType, OutputEncoding, OutputEncoding);
                }

                // Find the terminating element in the byte array
                int startindex = InputOffset;
                bool Found = false;
                while (startindex < Input.Length)
                {
                    // Find a potential match on the first byte
                    for (int j = 0; j < ListFixedValBytes.Count; j++)
                    {
                        if ((ListFixedValBytes[j].Length > 0) && (Input[startindex] == ListFixedValBytes[j][0]))
                        {
                            Found = true;
                            byte[] FixedValBytes = ListFixedValBytes[j];
                            // Look for a full match
                            for (int i = 1; i < FixedValBytes.Length; i++)
                                if (Input[startindex + i] != FixedValBytes[i])
                                {
                                    Found = false;
                                    break;
                                }

                            // Check that it is not escaped
                            if ((Found) && (FixedEscapeValBytes != null) && (startindex - FixedEscapeValBytes.Length >= InputOffset))
                            {
                                if (ByteArrayCompare(Input, (uint)(startindex - FixedEscapeValBytes.Length),
                                    FixedEscapeValBytes, 0, (uint)FixedEscapeValBytes.Length))
                                {
                                    Found = false;
                                    break;
                                }
                            }

                            if (Found)
                            {
                                // If foundindex == -1 then it was not found
                                NodeValue = ConvertBytesToString(Input, InputOffset, startindex - InputOffset, ParticleDBEntry);
                                BytesUsed = (startindex - InputOffset);
                                InputOffset += (startindex - InputOffset);
                                DebugString = "Found Value terminated by Fixed/Enum Value: " + NodeValue;
                                break;
                            }
                        }
                    }
                    if (Found)
                        break;

                    // Try next byte
                    startindex++;
                }


                //if (FixedValBytes.Length > 0)
                //{

                //    while (-1 != (foundindex = Array.IndexOf<byte>(Input, FixedValBytes[0], startindex)))
                //    {
                //        bool Found = true;
                //        for (int i = 0; i < FixedValBytes.Length; i++)
                //            if (Input[foundindex + i] != FixedValBytes[i])
                //            {
                //                Found = false;
                //                break;
                //            }
                //        if (Found)
                //            break;
                //        startindex += FixedValBytes.Length;
                //    }
                //    // If foundindex == -1 then it was not found
                //    if (-1 != foundindex)
                //    {
                //        NodeValue = ConvertBytesToString(Input, InputOffset, foundindex - InputOffset, SchElement);
                //        BytesUsed = (foundindex - InputOffset);
                //        InputOffset += (foundindex - InputOffset);
                //        System.Diagnostics.Debug.Write("Found Value terminated by Fixed/Enum Value: " + NodeValue);
                //    }
                //}
                //if (Found)
                //    break;
            }
            // If it has a regular expression facet, try to use that to identify it
            else if (null != ParticleDBEntry.FacetRestrictor.GetPattern())
            {
                Regex regexp = ParticleDBEntry.FacetRestrictor.GetPattern();
                // Try to find a match
                StringBuilder matchstring = new StringBuilder();
                int offset = InputOffset;
                matchstring.Append(ASCIIEncoding.ASCII.GetString(Input, offset, 1));
                while ((offset < Input.Length - 1) && !regexp.IsMatch(matchstring.ToString()))
                {
                    offset++;
                    matchstring.Append(ASCIIEncoding.ASCII.GetString(Input, offset, 1));
                }
                // Make sure the match is for the entire match string, and not a substring
                if ((offset < Input.Length - 1) && (regexp.Match(matchstring.ToString()).Length == matchstring.Length))
                {
                    // Continue adding characters until the regex fails
                    while ((offset < Input.Length - 1) && regexp.IsMatch(matchstring.ToString()))
                    {
                        MatchCollection mc1 = regexp.Matches(matchstring.ToString());
                        bool MaxLengthMatchFound = false;
                        // Because shorter strings will match, make sure there is a match to the full string
                        foreach (Match m1 in mc1)
                        {
                            if (m1.Length == matchstring.Length)
                            {
                                MaxLengthMatchFound = true;
                                break;
                            }
                        }
                        // If no match of the full string, then there is no match
                        if (!MaxLengthMatchFound)
                            break;
                        // Increase the length of the string to find the largest match
                        offset++;
                        matchstring.Append(ASCIIEncoding.ASCII.GetString(Input, offset, 1));
                    }
                    if (offset < Input.Length - 1)
                        matchstring.Remove(matchstring.Length - 1, 1);
                    // We have a hit
                    NodeValue = matchstring.ToString();
                    BytesUsed = matchstring.Length;
                    InputOffset += matchstring.Length;
                    DebugString = "Found Value by regular expression: " + NodeValue;
                }
            }
            // These last 2 are really last resorts
            else if (MaxLengthFromParent < Int32.MaxValue)
            {
                NodeValue = ConvertBytesToString(Input, InputOffset, MaxLengthFromParent, ParticleDBEntry);
                InputOffset += MaxLengthFromParent;
                BytesUsed = MaxLengthFromParent;
                DebugString = "Found Value with Length specified from parent: " + NodeValue;
            }
            // Dodgy attempt to read in strings, if the next element is a number then read in characters up until something
            // that isn't a character, and hope the next element number doesn't corresspond to a character value.  Only supports
            // ASCII
            else if ((SchElement.ElementSchemaType.TypeCode == XmlTypeCode.String) && (null != NextSchElement) && IsNumberType(NextSchElement))
            {
                Length = 0;
                while ((InputOffset + Length < Input.Length) &&
                    (Input[InputOffset + Length] >= 32) && (Input[InputOffset + Length] <= 126))
                {
                    Length++;
                }
                if (Length > 0)
                {
                    NodeValue = ASCIIEncoding.ASCII.GetString(Input, InputOffset, Length);
                    InputOffset += Length;
                    BytesUsed = Length;
                    DebugString = "Found String Value followed by Number: " + NodeValue;
                }
            }
            

            // If we successfully create a node value (a node value can be an empty string)
            if ((NodeValue != null) && (ParticleDBEntry.FacetRestrictor.Validate(NodeValue)) && (BytesUsed <= MaxLengthFromParent))
            {
                if(!String.IsNullOrEmpty(DebugString))
                    System.Diagnostics.Debug.WriteLine(DebugString);
                
                // Create an XmlElement and add to the parent
                if ((-1 != NodeValue.IndexOf('&')) || (-1 != NodeValue.IndexOf('<')))
                {
                    XmlCDataSection CData = oXMLDoc.CreateCDataSection(NodeValue);
                    ParentElement.AppendChild(CData);
                    oC2XProgress.AddCDataSection(NodeValue);
                }
                else
                {
                    XmlText text = oXMLDoc.CreateTextNode(NodeValue);
                    ParentElement.AppendChild(text);
                    oC2XProgress.AddXmlText(NodeValue);
                }
                ClearErrorInfo();
                return true;
            }

            SetErrorInfo(ParticleDBEntry);
            return false;
        }

        /// <summary>
        /// If the ParticleDBEntry is an input to a PI function then it needs an Id PI so the target can be found.
        /// We search the whole list to insure that if this node is the input to 2 different PI function, then 2
        /// PI Id's will be added.
        /// </summary>
        private bool IsInputToProcInstFn(ElementDBEntry ParticleDBEntry, List<String> AppName, List<String> PI)
        {
            bool ret = false;
            List<string> PIFn = new List<string>();
            // Search the list of PI for targets corresponding to this node
            for (int i = PIParticleList.Count - 1; i >= 0; i--)
            {
                if ((PIParticleList[i].TargetNodeName == ParticleDBEntry.Name.Name) &&
                    (!PIFn.Contains(PIParticleList[i].PI)) )
                {
                    // Return the PI to add to the parent XmlElement
                    AppName.Add("Schemer");
                    PI.Add("Id=\"" + PIParticleList[i].TargetNodeID + "\"");
                    // Remember that we added it for this function, as we never add it for more than 1 function
                    PIFn.Add(PIParticleList[i].PI);
                    // Delete this PIRef from the list
                    PIParticleList.RemoveAt(i);
                    ret = true;
                }
            }
            return ret;
        }

        /// <summary>
        /// If the ParticleDBEntry is an output of a PI function then it will be updated by a PI, and this PI and its target
        /// will be specified as part of the name of the node
        /// </summary>
        private bool IsOutputFromProcInstFn(ElementDBEntry ParticleDBEntry, XmlElement Element, out String AppName, out String PI)
        {
            AppName = null;
            PI = null;
            String PIFn = null;
            String TargetNode = null;
            if (GetPIFnFromName(ParticleDBEntry.Name.Name, out PIFn, out TargetNode))
            {
                // Add to list of PIRef
                PIRef PIentry = new PIRef();
                PIentry.PI = PIFn;
                PIentry.ParticleDBEntry = ParticleDBEntry;
                PIentry.TargetNodeID = TargetNode + IDdifferentiator++;
                PIentry.TargetNodeName = TargetNode;
                PIentry.LengthOfTargetNode = GetLengthIfLengthFunction(PIFn, Element, (ParticleDBEntry.Particle as XmlSchemaElement).ElementSchemaType);
                PIentry.TypeIDOfTargetNode = GetTypeIDIfTypeIDFunction(PIFn, Element);
                
                // REMOVED: For length nodes the fuzzers AddInteger etc don't work if we set the template value to 0, as currently
                // the template value is used for fuzzing and it is not updated prior to fuzzing.  Generally this is a safer practice
                // as the length node might have a restriction on it that disallows it being 0.  The ideal solution however to update
                // the node before fuzzing as this would allow more flexability moving forward.
                //// If it's a length node, then zero it, so we can check if it is getting updated properly via the PI.
                //// If the node type is hexBinary then don't zero it, otherwise we could be violating the schema
                //if (((PIentry.LengthOfTargetNode > 0) && (PIentry.LengthOfTargetNode < Int32.MaxValue)) && 
                //    ((ParticleDBEntry.Particle as XmlSchemaElement).ElementSchemaType.TypeCode != XmlTypeCode.HexBinary))
                //    Element.InnerText = "0";

                PIParticleList.Add(PIentry);
                
                // We don't want to write out PI's for TypeID's
                if (0 == PIFn.CompareTo("TypeID"))
                    return false;
                // Return the PI so it can be added to the parent XmlElement
                AppName = "Schemer";
                PI = PIFn + "=\"" + PIentry.TargetNodeID + "\"";

                // If the node we are updating has length restriction on it then we need to add another PI to ensure
                // we respect this when outputting.
                ElementDBEntry oElementDBEntry = ParticleDBEntry as ElementDBEntry;
                //XmlSchemaElement SchElement = ParticleDBEntry.Particle as XmlSchemaElement;
                XmlSchemaType SchType = XMLHelper.GetSchemaType(oElementDBEntry.ElementParticle);
                if ((SchType is XmlSchemaSimpleType) && (null != (SchType as XmlSchemaSimpleType).Content)
                    && ((SchType as XmlSchemaSimpleType).Content is XmlSchemaSimpleTypeRestriction))
                {
                    XmlSchemaSimpleTypeRestriction Rest = (SchType as XmlSchemaSimpleType).Content as XmlSchemaSimpleTypeRestriction;
                    foreach (XmlSchemaFacet Facet in Rest.Facets)
                    {
                        if (Facet is XmlSchemaLengthFacet)
                        {
                            Coder.OutputAsType outputAs = Coder.OutputAsType.Unchanged;
                            SchemaAttributeCommands.GetOutputAs(oSettings, oElementDBEntry.ObjectId, out outputAs);

                            if (outputAs == Coder.OutputAsType.BinaryLittleEndian)
                            {
                                PI = PI + " KeepEndBytes=\"" + Facet.Value + "\"";
                            }
                            else
                            {
                                PI = PI + " KeepStartBytes=\"" + Facet.Value + "\"";
                            }
                             
                            break;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to recover the a PI and target from a node name.  Expects node to be named like 'ByteLengthOfFirstName',
        /// from this it would return 'ByteLength' as the PI and 'FirstName' as the target node.
        /// </summary>
        private bool GetPIFnFromName(String NodeName, out String PIFn, out String TargetNode)
        {
            PIFn = null;
            TargetNode = null;
            List<XmlSchemaAttribute> ProcInstAtts = XMLProcInstCommands.GetProcInstCommands();

            // Find a prefix match of the PI name and the NodeName
            for (int i = 0; i < ProcInstAtts.Count; i++)
            {
                String SearchString = ProcInstAtts[i].Name + "Of";
                if(NodeName.StartsWith(SearchString, StringComparison.CurrentCultureIgnoreCase))
                {
                    PIFn = ProcInstAtts[i].Name;
                    TargetNode = NodeName.Substring(SearchString.Length);
                    if (!String.IsNullOrEmpty(TargetNode))
                        return true;
                }
            }
            return false;
        }

        private int GetLengthIfLengthFunction(String PIFnName, XmlElement Element, XmlSchemaType SchType)
        {
            String XmlNodeValue = Element.InnerText;
            System.Globalization.NumberStyles Style = System.Globalization.NumberStyles.Integer;

            if (SchType.TypeCode == XmlTypeCode.HexBinary)
            {
                //XmlNodeValue = "0x" + XmlNodeValue;
                Style = System.Globalization.NumberStyles.HexNumber;
            }
            
            if (PIFnName.Equals("ByteLength", StringComparison.CurrentCultureIgnoreCase))
            {
                int length = Int32.MaxValue;
                if (Int32.TryParse(XmlNodeValue, Style, null, out length))
                    return length;
            }
            else if (PIFnName.Equals("WordLength", StringComparison.CurrentCultureIgnoreCase))
            {
                int length = Int32.MaxValue;
                if (Int32.TryParse(XmlNodeValue, Style, null, out length))
                    return 2*length;
            }
            else if (PIFnName.Equals("CharLength", StringComparison.CurrentCultureIgnoreCase))
            {
                int length = Int32.MaxValue;
                if (Int32.TryParse(Element.Value, Style, null, out length))
                {
                    // Return the number of characters multiplied by the number of bytes per character
                    Encoding Enc = OutputEncoding;
                    return length * Enc.GetMaxByteCount(1);
                }
            }
            // 'Count' is used to specify the number of occurrences of another element, e.g. the length of an array of them
            else if (PIFnName.Equals("Count", StringComparison.CurrentCultureIgnoreCase))
            {
                int length = 0;
                if (Int32.TryParse(XmlNodeValue, Style, null, out length))
                    return length;
                else
                    return 0;
            }
            return Int32.MaxValue;
        }

        private String GetTypeIDIfTypeIDFunction(String PIFnName, XmlElement Element)
        {
            if (PIFnName.Equals("TypeID", StringComparison.CurrentCultureIgnoreCase))
            {
                return Element.InnerText;
            }
            return "";
        }

        private bool IsSpecificValue(String Value, byte[] Input, int InputOffset, ElementDBEntry ParticleDBEntry, out int Length)
        {
            // Mock up the method we normally output nodes with to get the fixed value as it would be output
            Coder.OutputAsType outputAs = Coder.OutputAsType.Unchanged;
            SchemaAttributeCommands.GetOutputAs(oSettings, ParticleDBEntry.ObjectId, out outputAs);
            //OutputNode tempOutputNode = new OutputNode(oSchemaSet, OutputEncoding, null, null);
            //tempOutputNode.WriteStartElement(Prefix, ParticleDBEntry.Name.Name, ParticleDBEntry.Name.Namespace);
            byte[] FixedValBytes = EncodingHelper.EncodeForOutput(OutputEncoding.GetBytes(Value), ParticleDBEntry.DataSchemaType, outputAs, OutputEncoding, OutputEncoding);
            // Check its a match
            if (FixedValBytes.Length <= (Input.Length - InputOffset))
            {
                for (int i = 0; i < FixedValBytes.Length; i++)
                {
                    if (FixedValBytes[i] != Input[InputOffset + i])
                    {
                        Length = 0;
                        return false;
                    }
                }
            }
            else
            {
                Length = 0;
                return false;
            }
            Length = FixedValBytes.Length;
            return true;
        }

        private int HasPrescribedLength(String NodeName)
        {
            // Search for node name that has a valid length set
            for (int i = PIParticleList.Count - 1; i >= 0; i--)
            {
                if (!PIParticleList[i].PI.Equals("Count", StringComparison.CurrentCultureIgnoreCase) && 
                    (PIParticleList[i].TargetNodeName.Equals(NodeName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (PIParticleList[i].LengthOfTargetNode < Int32.MaxValue)
                        return PIParticleList[i].LengthOfTargetNode;
                }
            }
            return Int32.MaxValue;
        }

        private int HasFixedLength(ElementDBEntry ParticleDBEntry)
        {
            // Check if its a binary number
            Coder.OutputAsType outputAs = Coder.OutputAsType.Unchanged;
            SchemaAttributeCommands.GetOutputAs(oSettings, ParticleDBEntry.ObjectId, out outputAs);
            
            if ((outputAs == Coder.OutputAsType.BinaryBigEndian) || (outputAs == Coder.OutputAsType.BinaryLittleEndian))
            {
                // It needs to be a number type
                switch (ParticleDBEntry.DataSchemaType)
                {
                    case DataSchemaTypeCode.UnsignedLong:
                    case DataSchemaTypeCode.Long:
                        return 8;
                    case DataSchemaTypeCode.UnsignedInt:
                    case DataSchemaTypeCode.Int:
                        return 4;
                    case DataSchemaTypeCode.UnsignedShort:
                    case DataSchemaTypeCode.Short:
                        return 2;
                    case DataSchemaTypeCode.UnsignedByte:
                    case DataSchemaTypeCode.Byte:
                        return 1;
                    case DataSchemaTypeCode.Hex:
                        // Ok, it could be a 3 digit hexBinary
                        break;
                    default:
                        Log.Write(MethodBase.GetCurrentMethod(), "Expected '" + Enum.GetName(typeof(DataSchemaTypeCode), ParticleDBEntry.DataSchemaType) + "' to be a number type", Log.LogType.Error);
                        return -1;
                }
            }

            // Check if it has a facet restriction on its length
            if (ParticleDBEntry.ElementParticle.ElementSchemaType is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType SimpleType = ParticleDBEntry.ElementParticle.ElementSchemaType as XmlSchemaSimpleType;
                if (SimpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    foreach (XmlSchemaFacet Facet in (SimpleType.Content as XmlSchemaSimpleTypeRestriction).Facets)
                    {
                        if (Facet is XmlSchemaLengthFacet)
                        {
                            return Int32.Parse((Facet as XmlSchemaLengthFacet).Value);
                        }
                    }
                }
            }

            // It doesn't have a fixed length
            return -1;
        }

        private List<String> GetEnumValues(XmlSchemaElement SchElement)
        {
            // Check if it has a facet restriction on its length
            if (SchElement.ElementSchemaType is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType SimpleType = SchElement.ElementSchemaType as XmlSchemaSimpleType;
                if (SimpleType.Content is XmlSchemaSimpleTypeRestriction)
                {
                    List<String> EnumList = new List<string>();
                    foreach (XmlSchemaFacet Facet in (SimpleType.Content as XmlSchemaSimpleTypeRestriction).Facets)
                    {
                        if (Facet is XmlSchemaEnumerationFacet)
                        {
                            EnumList.Add((Facet as XmlSchemaEnumerationFacet).Value);
                        }
                    }
                    if(EnumList.Count > 0)
                        return EnumList;
                    else
                        return null;
                }
            }
            return null;
        }

        private String ConvertBytesToString(byte[] Input, int InputOffset, int Length, ElementDBEntry SchElement)
        {
            if (InputOffset + Length > Input.Length)
                Log.Write(MethodBase.GetCurrentMethod(), "The specified node byte length was greater than the number of bytes left to read", Log.LogType.Error);

            // Get byte array of value
            byte[] bytevalue = new byte[Length];
            Array.Copy(Input, InputOffset, bytevalue, 0, Length);

            // Based on the outputAs convert back to a string
            Coder.OutputAsType outputAs = Coder.OutputAsType.Unchanged;
            SchemaAttributeCommands.GetOutputAs(oSettings, SchElement.ObjectId, out outputAs);

            String OutputString = null;
            switch (outputAs)
            {
                case Coder.OutputAsType.BinaryBigEndian:
                    // Convert the big endian number to a number string
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytevalue);
                    OutputString = ConvertBytesToStringNumber(bytevalue, SchElement);
                    break;
                case Coder.OutputAsType.BinaryLittleEndian:
                    // Convert the little endian number to a number string
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(bytevalue);
                    OutputString = ConvertBytesToStringNumber(bytevalue, SchElement);
                    break;
                case Coder.OutputAsType.Decoded:
                    HexCoder XXCoder = new HexCoder();
                    OutputString = XXCoder.Encode(bytevalue);
                    break;
                //case NodeProcessor.OutputAsType.Base64:

                //    break;
                //case NodeProcessor.OutputAsType.MSBase64:

                //    break;
                //case NodeProcessor.OutputAsType.Hex:

                //    break;
                default:
                    Encoding CharEncoding = OutputEncoding;
                    int Offset = 0;
                    // Handle byte order marks that indicate a string in a certain format
                    if ((bytevalue.Length >= 3) && (bytevalue[0] == 0xEF) && (bytevalue[1] == 0xBB) && (bytevalue[2] == 0xBF))
                    {
                        CharEncoding = UTF8Encoding.UTF8;
                        Offset = 3;
                    }
                    else if ((bytevalue.Length >= 2) && (bytevalue[0] == 0xFE) && (bytevalue[1] == 0xFF))
                    {
                        CharEncoding = UnicodeEncoding.BigEndianUnicode;
                        Offset = 2;
                    }
                    else if ((bytevalue.Length >= 2) && (bytevalue[0] == 0xFF) && (bytevalue[1] == 0xFE))
                    {
                        CharEncoding = UnicodeEncoding.Unicode;
                        Offset = 2;
                    }
                    else if ((bytevalue.Length >= 4) && (bytevalue[0] == 0x00) && (bytevalue[1] == 0x00) && (bytevalue[2] == 0xFE) && (bytevalue[3] == 0xFF))
                    {
                        if(bytevalue.Length%4 != 0)
                            throw new Exception("Encountered a big endian UTF32 string but it was not a multiple of 4 bytes");
                        // Reverse endianness
                        for (int i = 0; i < bytevalue.Length; i += 4)
                            Array.Reverse(bytevalue, i, 4);
                        CharEncoding = UnicodeEncoding.UTF32;
                        Offset = 4;    
                        //throw new Exception("Encountered a big endian UTF32 string, .Net has no ability to decode this.");
                    }
                    else if ((bytevalue.Length >= 4) && (bytevalue[0] == 0xFF) && (bytevalue[1] == 0xFE) && (bytevalue[2] == 0x00) && (bytevalue[3] == 0x00))
                    {
                        CharEncoding = UnicodeEncoding.UTF32;
                        Offset = 4;
                    }
                    else if ((bytevalue.Length >= 3) && (bytevalue[0] == 0x2B) && (bytevalue[1] == 0x2F) && (bytevalue[2] == 0x76))
                    {
                        CharEncoding = UTF7Encoding.UTF7;
                        Offset = 4;
                    }

                    OutputString = CharEncoding.GetString(bytevalue, Offset, bytevalue.Length - Offset);
                    break;
            }
            return OutputString;
        }

        private String ConvertBytesToStringNumber(byte[] NumberBytes, ElementDBEntry Element)
        {
            switch (Element.DataSchemaType)
            {
                case DataSchemaTypeCode.UnsignedLong:
                    return (BitConverter.ToUInt64(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.UnsignedInt:
                    return (BitConverter.ToUInt32(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.UnsignedShort:
                    return (BitConverter.ToUInt16(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.UnsignedByte:
                    return NumberBytes[0].ToString();
                case DataSchemaTypeCode.Long:
                    return (BitConverter.ToInt64(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.Int:
                    return (BitConverter.ToInt32(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.Short:
                    return (BitConverter.ToInt16(NumberBytes, 0)).ToString();
                case DataSchemaTypeCode.Byte:
                    return ((char)NumberBytes[0]).ToString();
                case DataSchemaTypeCode.Hex:
                    Coder HexCoder = Coder.AssignCoder(Coder.EncodingType.Hex);
                    Array.Reverse(NumberBytes);
                    return HexCoder.Encode(NumberBytes);
                default:
                    Log.Write(MethodBase.GetCurrentMethod(), "Conversion from '" + Enum.GetName(typeof(XmlTypeCode), Element.DataSchemaType) + "' to a number string not supported", Log.LogType.Error);
                    break;
            }
            return null;
        }

        private bool IsNumberType(XmlSchemaElement Element)
        {
            switch (Element.ElementSchemaType.TypeCode)
            {
                case XmlTypeCode.UnsignedByte:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.UnsignedShort:
                case XmlTypeCode.Byte:
                case XmlTypeCode.Int:
                case XmlTypeCode.Long:
                case XmlTypeCode.Short:
                case XmlTypeCode.Float:
                case XmlTypeCode.Double:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check to see if the ElementName parameter matches any of the TypeIDs that have been encountered and recorded so far
        /// </summary>
        /// <param name="ElementName"></param>
        /// <returns></returns>
        private String GetChoiceTypeID(String ElementName)
        {
            // Check to see if the name of the element containing the choice has a type ID associated with it
            for(int i = 0; i < PIParticleList.Count; i++)
            {
                if(0 == ElementName.CompareTo(PIParticleList[i].TargetNodeName))
                {
                    // Return the type ID
                    if(!String.IsNullOrEmpty(PIParticleList[i].TypeIDOfTargetNode))
                    {
                        // We delete the entry in the list so the PI doesn't get printed
                        String ret = PIParticleList[i].TypeIDOfTargetNode;
                        PIParticleList.RemoveAt(i);
                        return ret;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// Get the maximum number of times the particle can occur.  This could be set by a 'Count' from another node.
        /// </summary>
        private int GetMaxOccursCount(ParticleDBEntry ParticleDBEntry)
        {
            int maxOccurs = (ParticleDBEntry.Particle.MaxOccurs > Int32.MaxValue ? Int32.MaxValue : (int)ParticleDBEntry.Particle.MaxOccurs);
            if (ParticleDBEntry is ElementDBEntry)
            {
                String ElementName = (ParticleDBEntry as ElementDBEntry).Name.Name;
                // Check to see if the name of the element has an array count
                for (int i = 0; i < PIParticleList.Count; i++)
                {
                    if (PIParticleList[i].PI.Equals("Count", StringComparison.CurrentCultureIgnoreCase) &&
                        (0 == ElementName.CompareTo(PIParticleList[i].TargetNodeName)))
                    {
                        // Return the Count
                        maxOccurs = PIParticleList[i].LengthOfTargetNode;
                        break;
                    }
                }
            }
            return maxOccurs;
        }

        /// <summary>
        /// Match the child TypeID specified in the schema of a child of a choice particle against the TypeID parameter
        /// </summary>
        /// <param name="ParticleDBEntry"></param>
        /// <param name="TypeID"></param>
        /// <returns></returns>
        private bool ParticleHasTypeID(ParticleDBEntry ParticleDBEntry, String TypeID)
        {
            if (null == ParticleDBEntry.Particle.UnhandledAttributes)
                return false;
            // Search the unhandled attributes of the particle
            for (int i = 0; i < ParticleDBEntry.Particle.UnhandledAttributes.Length; i++)
            {
                XmlAttribute Att = ParticleDBEntry.Particle.UnhandledAttributes[i];
                if (0 == Att.Value.CompareTo(TypeID))
                    return true;
            }
            return false;
        }

        private bool ByteArrayCompare(byte[] Array1, uint Array1Offset, byte[] Array2, uint Array2Offset, uint Length)
        {
            if ((Array1 == null) || (Array2 == null))
                return false;

            for (int i = 0; i < Length; i++)
            {
                if (Array1[Array1Offset + i] != Array2[Array2Offset + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the simple type is numeric (int or float) and is output as a string
        /// </summary>
        /// <param name="Input">The byte array</param>
        /// <param name="InputOffset">Index into Input</param>
        /// <param name="oElementDBEntry">The element to check</param>
        /// <param name="NumStr">The string found</param>
        /// <param name="BytesUsed">The number of bytes used</param>
        /// <returns>true if the current element represents a numeric string, false otherwise</returns>
        private bool GetNumericString(byte[] Input, int InputOffset, ElementDBEntry oElementDBEntry, out String NumStr, out int BytesUsed)
        {
            BytesUsed = 0;
            NumStr = null;
            bool IsIntType = false;
            bool IsFloatType = false;
            // Check the type to ensure it is a numeric type
            switch (oElementDBEntry.DataSchemaType)
            {
                case DataSchemaTypeCode.Decimal:
                case DataSchemaTypeCode.Double:
                case DataSchemaTypeCode.Float:
                    IsFloatType = true;
                    break;
                case DataSchemaTypeCode.Byte:
                case DataSchemaTypeCode.Int:
                case DataSchemaTypeCode.Integer:
                case DataSchemaTypeCode.Long:
                case DataSchemaTypeCode.NegativeInteger:
                case DataSchemaTypeCode.NonNegativeInteger:
                case DataSchemaTypeCode.NonPositiveInteger:
                case DataSchemaTypeCode.PositiveInteger:
                case DataSchemaTypeCode.Short:
                case DataSchemaTypeCode.UnsignedByte:
                case DataSchemaTypeCode.UnsignedInt:
                case DataSchemaTypeCode.UnsignedLong:
                case DataSchemaTypeCode.UnsignedShort:
                    IsIntType = true;
                    break;
            }

            if (!IsIntType && !IsFloatType)
                return false;

            Coder.OutputAsType outputType = Coder.OutputAsType.Unchanged;
            SchemaAttributeCommands.GetOutputAs(oSettings, oElementDBEntry.ObjectId, out outputType);
            
            // Find the number of bytes to add for each character, it depends on the output type
            int BytesPerChar = OutputEncoding.GetByteCount("a");  // TODO: this is lame
            switch (outputType)
            {
                case Coder.OutputAsType.ASCIIString:
                case Coder.OutputAsType.UTF7String:
                case Coder.OutputAsType.UTF8String:
                case Coder.OutputAsType.UTF8StringWithBOM:
                    BytesPerChar = 1;
                    break;
                case Coder.OutputAsType.UTF16BEString:
                case Coder.OutputAsType.UTF16LEString:
                    BytesPerChar = 2;
                    break;
                case Coder.OutputAsType.UTF32BEString:
                case Coder.OutputAsType.UTF32LEString:
                    BytesPerChar = 4;
                    break;
                case Coder.OutputAsType.Unchanged:
                    // Our default case
                    break;
                default:
                    // We are not outputting as a string
                    return false;
            }
            
            // Make a copy of the max amount of bytes that is reasonable for a number;
            // UInt64.MaxValue = +18,446,744,073,709,551,615 or 27 characters, or max 108 bytes (in UTF32)
            // Double.MaxValue = +1.79769313486232e308 or 22 characters
            int MaxCharByteCount = 108;

            // Create a buffer to hold bytes
            byte[] numBuf = new byte[0];
            //if (Input.Length - InputOffset < MaxCharByteCount)
            //    MaxCharByteCount = Input.Length - InputOffset;
            
            // Add bytes to buffer, convert to string, if we are successful, keep on adding bytes until we fail
            for (int i = 0; i < MaxCharByteCount; i += BytesPerChar)
            {
                if (InputOffset + i + BytesPerChar > Input.Length)
                    break;

                Array.Resize<byte>(ref numBuf, numBuf.Length + BytesPerChar);
                Array.Copy(Input, InputOffset + i, numBuf, i, BytesPerChar);
                
                // Convert to string
                String NumStrTemp = EncodingHelper.ConvertBytesToString(numBuf, outputType, OutputEncoding);

                // Convert to number
                long int64res;
                ulong uint64res;
                double dblres;
                System.Globalization.NumberStyles IntNS = System.Globalization.NumberStyles.AllowLeadingSign |
                                                            System.Globalization.NumberStyles.AllowThousands;
                System.Globalization.NumberStyles FloatNS = System.Globalization.NumberStyles.AllowLeadingSign |
                                                            System.Globalization.NumberStyles.AllowThousands |
                                                            System.Globalization.NumberStyles.AllowDecimalPoint |
                                                            System.Globalization.NumberStyles.AllowExponent;
                if ( (IsIntType &&
                    ((Int64.TryParse(NumStrTemp, IntNS, new System.Globalization.NumberFormatInfo(), out int64res)) ||
                     (UInt64.TryParse(NumStrTemp, IntNS, new System.Globalization.NumberFormatInfo(), out uint64res)))) ||
                     (IsFloatType &&
                     (Double.TryParse(NumStrTemp, FloatNS, new System.Globalization.NumberFormatInfo(), out dblres))) )
                {
                    // We found a string
                    NumStr = NumStrTemp;
                    BytesUsed = numBuf.Length;
                }
                else if(NumStr != null)
                {
                    // We added more bytes and our string was not a number, lets use the number we found
                    break;
                }
            }

            return true;
        }
    }
}
