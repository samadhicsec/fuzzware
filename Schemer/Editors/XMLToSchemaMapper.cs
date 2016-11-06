using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Editors
{
    class XMLToSchemaMapper
    {
        private PreCompData oPreComp;
        private XmlSchemaSet oSchemaSet;
        private int NodeIndex;

        public ParticleInstance MapXMLToSchema(XPathNavigator ParentNode, int NodeIndex, PreCompData oPreComp)
        {
            this.oPreComp = oPreComp;
            oSchemaSet = oPreComp.SchemaSet;

            this.NodeIndex = NodeIndex;
            XPathNavigator Nav = ParentNode.Clone();
            
            XmlQualifiedName QNameParentNode = new XmlQualifiedName(Nav.LocalName, Nav.NamespaceURI);
            XmlSchemaElement oElement = ParentNode.SchemaInfo.SchemaElement as XmlSchemaElement;
            XMLElementIdentifier Id = new XMLElementIdentifier(QNameParentNode, ParentNode.SchemaInfo.SchemaElement, oSchemaSet, oElement.MinOccurs, oElement.MaxOccurs);
            ParticleDBEntry ParentParticle = (ParticleDBEntry)oPreComp.ObjectDB[Id];

            ParticleInstance ParentInstance = new ParticleInstance(ParentParticle.Particle);

            // Now try to map all the children of the root to their particles
            if ((!Nav.HasChildren) || (0 == ParentParticle.SubParticles.Length))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Occurrence fuzzing limited on instance " + NodeIndex + " of '" + QNameParentNode.ToString() + "' as it has no sub-particles.  Sub-particles of other instances will be used if possible.", Log.LogType.LogOnlyInfo);
                return ParentInstance;
            }
            Nav.MoveToFirstChild();
            MapXMLToParticle(Nav, ParentParticle.SubParticles[0], ParentInstance);
            return ParentInstance;
        }

        private int AddElementInstance(ParticleDBEntry SubParticleDBEntry, List<XPathNavigator> XPathNavSiblings, int XPathSiblingIndex, ParticleInstance ParentParticleInstance)
        {
            XmlSchemaElement SubParticleElement = SubParticleDBEntry.Particle as XmlSchemaElement;
            if (XPathSiblingIndex >= XPathNavSiblings.Count)
                return 0;
            // Get the qualified name of the current XPathNav
            XPathNavigator XPathNav = XPathNavSiblings[XPathSiblingIndex];

            int XPathAllSiblingIndexStart = XPathSiblingIndex;
            int XPathSiblingIndexStart = XPathSiblingIndex;
            // Make sure we skip over PI's or comments, etc.
            while (XPathNav.NodeType != XPathNodeType.Element)
            {
                XPathSiblingIndex++;
                if (XPathSiblingIndex >= XPathNavSiblings.Count)
                    return 0;
                XPathNav = XPathNavSiblings[XPathSiblingIndex];
            }

            XmlQualifiedName XPathNavName = new XmlQualifiedName(XPathNav.LocalName, XPathNav.NamespaceURI);

            int ElementCount = 0;
            // While the XPathNav name and sub-particle name match, add as instance of the sub-particle.  Only
            // add up to MaxOccurs though.
            ParticleInstance Inst;
            while (SubParticleElement.QualifiedName.ToString() == XPathNavName.ToString())
            {
                // Add this XPathNav as an instance of the sub-particle
                Inst = new ParticleInstance(SubParticleDBEntry.Particle);
                Inst.Add(XPathNavSiblings, XPathSiblingIndexStart, XPathSiblingIndex - XPathSiblingIndexStart + 1);
                ParentParticleInstance.SubParticleInstances.Add(Inst);
                SubParticleDBEntry.Instances.Add(Inst);
                ElementCount++;         // Keep track of the number of elements added
                XPathSiblingIndex++;    // Increment to the next XPathNav
                XPathSiblingIndexStart = XPathSiblingIndex; // Note the start position of this possible instance
                if (XPathSiblingIndex >= XPathNavSiblings.Count)
                    break;
                XPathNav = XPathNavSiblings[XPathSiblingIndex];

                if (SubParticleElement.MaxOccursString != "unbounded")
                    if (ElementCount >= SubParticleElement.MaxOccurs)
                        break;

                // Make sure we skip over PI's or comments, etc.
                while (XPathNav.NodeType != XPathNodeType.Element)
                {
                    XPathSiblingIndex++;
                    if (XPathSiblingIndex >= XPathNavSiblings.Count)
                        return (--XPathSiblingIndex) - XPathAllSiblingIndexStart;
                    XPathNav = XPathNavSiblings[XPathSiblingIndex];
                }

                XPathNavName = new XmlQualifiedName(XPathNav.LocalName, XPathNav.NamespaceURI);
            }

            // Reality check, make sure we have added a minimal number of the sub-particle element
            //if (Count < SubParticleElement.MinOccurs)
            //    Log.Write(MethodBase.GetCurrentMethod(), "Count < MinOccurs", Log.LogType.Info);

            return XPathSiblingIndex - XPathAllSiblingIndexStart;
        }

        private int MapXMLToParticle(XPathNavigator Nav, ParticleDBEntry ParticleDBEntry, ParticleInstance ParentParticleInstance)
        {
            // Create XPathNavs of all the sibling elements
            List<XPathNavigator> XPathNavSiblings = new List<XPathNavigator>();
            XPathNavSiblings.Add(Nav.Clone());
            while (Nav.MoveToNext())
            {
                XPathNavSiblings.Add(Nav.Clone());
            }

            int XPathSiblingSeqIndex = 0;   // End position of last instance of current particle type (i.e. when list contains multiple instances)
            int XPathSiblingIndex = 0;      // Current position in our list of sibling particles

            XmlSchemaParticle Particle = ParticleDBEntry.Particle;

            // For the number of times this particle can occur
            for (int SeqCount = 0; ; SeqCount++)
            {
                if (XPathSiblingIndex >= XPathNavSiblings.Count)
                    break;

                // Create current particle instance
                ParticleInstance CurrentParticleInstance = new ParticleInstance(Particle);

                // Go through each sub particle
                for (int i = 0; i < ParticleDBEntry.SubParticles.Length; i++)
                {
                    // TODO - I added the line below because otherwise an out of range exception was possible, need to check this doesn't
                    // break anything
                    if (XPathSiblingIndex >= XPathNavSiblings.Count)
                        break;

                    int Added = 0;
                    if (ParticleDBEntry.SubParticles[i].Particle is XmlSchemaElement)
                    {
                        if(!(Particle is XmlSchemaAll))
                        {
                            Added = AddElementInstance(ParticleDBEntry.SubParticles[i], XPathNavSiblings, XPathSiblingIndex, CurrentParticleInstance);
                        }
                        else
                        {   
                            // The child particles of XmlSchemaAll can only be XmlSchemaElement, and an XmlSchemaAll particle has
                            // maxOccurs = 1 always
                            for (int j = 0; j < ParticleDBEntry.SubParticles.Length; j++)
                            {
                                Added = AddElementInstance(ParticleDBEntry.SubParticles[j], XPathNavSiblings, XPathSiblingIndex, CurrentParticleInstance);
                                if(Added > 0)
                                    break;
                            }
                        }
                    }
                    else if (ParticleDBEntry.SubParticles[i].Particle is XmlSchemaAny)
                    {
                        // TODO - Should we add XmlSchemaAny particle instances?
                    }
                    else if ((ParticleDBEntry.SubParticles[i].Particle is XmlSchemaGroupBase) ||
                        (ParticleDBEntry.SubParticles[i].Particle is XmlSchemaGroupRef))
                    {
                        // Recurse
                        Added = MapXMLToParticle(XPathNavSiblings[XPathSiblingIndex].Clone(), ParticleDBEntry.SubParticles[i], CurrentParticleInstance);
                    }
                    XPathSiblingIndex += Added;

                    if (Particle is XmlSchemaChoice)
                    {
                        // Since this is a choice, if Added > 0, we found the chosen element
                        if (Added > 0)
                            break;
                    }
                }

                // It makes no sense that if we get here and haven't mapped anything, there is no point on looping with the same
                // XPathSiblingIndex, as we will just fail again
                if (0 == (XPathSiblingIndex - XPathSiblingSeqIndex))
                    break;

                // Set (XPathSiblingIndex - XPathSiblingSeqIndex) XPathNavSiblings as the instance of this Particle
                CurrentParticleInstance.Add(XPathNavSiblings, XPathSiblingSeqIndex, XPathSiblingIndex - XPathSiblingSeqIndex);
                // Add the instance as a sub-particle instance of the parent
                ParentParticleInstance.SubParticleInstances.Add(CurrentParticleInstance);
                // Store the example instance against the Particle in the DB
                ParticleDBEntry.Instances.Add(CurrentParticleInstance);

                // Rememeber where this instance finished
                XPathSiblingSeqIndex = XPathSiblingIndex;

                // Make sure we do not try to add more than maxOccurs of this particles
                if (Particle.MaxOccursString != "unbounded")
                    if (SeqCount >= Particle.MaxOccurs - 1)
                        break;
            }
           
            return XPathSiblingSeqIndex;
        }
    }
}
