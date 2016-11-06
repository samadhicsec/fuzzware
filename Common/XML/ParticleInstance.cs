using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Fuzzware.Common.XML
{
    public class ParticleInstance
    {
        protected List<XPathNavigator> InstanceParticleNodes;
        protected List<ParticleInstance> SubParticleNodes;
        protected XmlSchemaParticle SchemaParticle;
        
        public ParticleInstance(XmlSchemaParticle Particle)
        {
            InstanceParticleNodes = new List<XPathNavigator>();
            SubParticleNodes = new List<ParticleInstance>();
            SchemaParticle = Particle;
        }

        public void Add(XPathNavigator ParticleNode)
        {
            InstanceParticleNodes.Add(ParticleNode.Clone());
        }

        public void Add(List<XPathNavigator> ParticleNodes, int StartIndex, int Count)
        {
            for (int i = StartIndex; (i < (StartIndex + Count)) && (i < ParticleNodes.Count); i++)
            {
                InstanceParticleNodes.Add(ParticleNodes[i].Clone());
            }
        }

        /// <summary>
        /// Return a list of all sub-particles instances of a certain type from all the children
        /// </summary>
        public List<ParticleInstance> GetAllSubParticlesOfType(XmlSchemaParticle Particle)
        {
            List<ParticleInstance> Instances = new List<ParticleInstance>();
            bool Found = false;
            for (int i = 0; i < SubParticleInstances.Count; i++)
            {
                // We are looking for the same object references
                if (SubParticleInstances[i].Particle.Equals(Particle))
                {
                    Instances.Add(SubParticleInstances[i]);
                }
                else
                {
                    // Instances will occur sequentially amongst the sub-particle instances
                    if (Found)
                        break;
                }
            }
            return Instances;
        }

        /// <summary>
        /// Returns a list of sub-particles instances starting at a certain index, and only those that are sequential
        /// </summary>
        public List<ParticleInstance> GetSubParticlesOfTypeFromIndex(XmlSchemaParticle Particle, int StartIndex)
        {
            List<ParticleInstance> Instances = new List<ParticleInstance>();
            for (int i = StartIndex; i < SubParticleInstances.Count; i++)
            {
                // We are looking for the same object references
                if (SubParticleInstances[i].Particle.Equals(Particle))
                {
                    Instances.Add(SubParticleInstances[i]);
                }
                else
                {
                    break;
                }
            }
            return Instances;
        }

        public List<ParticleInstance> SubParticleInstances
        {
            get
            {
                return SubParticleNodes;
            }
        }

        public List<XPathNavigator> XPathNavs
        {
            get
            {
                return InstanceParticleNodes;
            }
        }

        public XmlSchemaParticle Particle
        {
            get
            {
                return SchemaParticle;
            }
        }
    }
}
