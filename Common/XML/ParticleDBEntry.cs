using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Reflection;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Common.XML.Restrictions;

namespace Fuzzware.Common.XML
{
    public class ParticleDBEntry : ObjectDBEntry
    {
        protected XmlSchemaParticle oParticle;
        protected ParticleDBEntry[] oSubParticles;
        protected int CountOfOccuranceValues;
        public List<ParticleInstance> Instances;
        public XmlAttribute[] oUnhandledAttributes;

        public XmlSchemaParticle Particle
        {
            get { return oParticle; }
        }

        public ParticleDBEntry[] SubParticles
        {
            get { return oSubParticles; }
        }

        public ParticleDBEntry(XmlSchemaParticle oParticle, uint uiDepth, XmlSchemaSet SchemaSet, ObjectDataBase oObjectDataBase)
        {
            this.oParticle = oParticle;
            this.uiDepth = uiDepth;
            this.oSchemaSet = SchemaSet;
            CountOfOccuranceValues = -1;
            Instances = new List<ParticleInstance>();

            // If this particle contains other particles, add them as sub-particles
            if (oParticle is XmlSchemaAny)
            {
                // TODO.  Should we fuzz particles of type XmlSchemaAny?
                Log.Write(MethodBase.GetCurrentMethod(), "Elements of type XmlSchemaAny will not be fuzzed", Log.LogType.LogOnlyInfo);
            }
            else if (oParticle is XmlSchemaGroupBase)
            {
                XmlSchemaGroupBase Base = oParticle as XmlSchemaGroupBase;
                oSubParticles = new ParticleDBEntry[Base.Items.Count];
                // Go through all the sub-particles
                for (int i = 0; i < Base.Items.Count; i++)
                {
                    // Add the particle differently if its an element.
                    XmlSchemaParticle BaseParticle = Base.Items[i] as XmlSchemaParticle;
                    if (BaseParticle is XmlSchemaElement)
                    {
                        XmlQualifiedName oName = (BaseParticle as XmlSchemaElement).QualifiedName;
                        XmlSchemaType oSchemaType = XMLHelper.GetSchemaType((BaseParticle as XmlSchemaElement));
                        XMLElementIdentifier Id = new XMLElementIdentifier(oName, BaseParticle as XmlSchemaElement, SchemaSet, (BaseParticle as XmlSchemaElement).MinOccurs, (BaseParticle as XmlSchemaElement).MaxOccurs);
                        // If the element already exists, make the sub-particle point to the existing element.  Importantly we are using
                        // max and min occurs to help uniquely identify elements here, this is needed to we can accurately recover the
                        // max and min occurs.
                        if (oObjectDataBase.Contains(Id))
                            oSubParticles[i] = (ElementDBEntry)oObjectDataBase[Id];
                        else
                            oSubParticles[i] = new ElementDBEntry(Base.Items[i] as XmlSchemaParticle, uiDepth + 1, oSchemaSet, oObjectDataBase);
                    }
                    else
                        oSubParticles[i] = new ParticleDBEntry(Base.Items[i] as XmlSchemaParticle, uiDepth + 1, oSchemaSet, oObjectDataBase);                    
                }
            }
            else if (oParticle is XmlSchemaGroupRef)
            {
                // Should be part of the GlobalElements of the schema, find reference
                XmlSchemaGroupRef GroupRef = oParticle as XmlSchemaGroupRef;
                oSubParticles = new ParticleDBEntry[1];
                oSubParticles[0] = new ParticleDBEntry(GroupRef.Particle, uiDepth + 1, oSchemaSet, oObjectDataBase);
            }
        }

        /// <summary>
        /// Returns the number of values in PossibleOccurrenceValues that are greater than or equal than minOccurs and less
        /// than or equal to maxOccurs.
        /// </summary>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        public int GetCountOfOccurrenceValues(uint[] PossibleOccurrenceValues)
        {
            if (-1 == CountOfOccuranceValues)
            {
                if (oParticle.MaxOccursString == "unbounded")
                {
                    CountOfOccuranceValues = 0;
                    for (int i = 0; i < PossibleOccurrenceValues.Length; i++)
                    {
                        if (PossibleOccurrenceValues[i] >= (uint)oParticle.MinOccurs)
                            CountOfOccuranceValues++;
                    }
                }
                else
                {
                    uint MaxOccurs = 0;
                    if (String.IsNullOrEmpty(oParticle.MaxOccursString))
                        MaxOccurs = 1;
                    else
                        MaxOccurs = (uint)Particle.MaxOccurs;
                    CountOfOccuranceValues = 0;
                    // If MaxOccurs == MinOccurs there is no Occurrence fuzzing to be done
                    if ((uint)oParticle.MinOccurs != MaxOccurs)
                    {
                        for (int i = 0; i < PossibleOccurrenceValues.Length; i++)
                        {
                            if ((PossibleOccurrenceValues[i] >= (uint)oParticle.MinOccurs)
                                 && (PossibleOccurrenceValues[i] <= MaxOccurs))
                                CountOfOccuranceValues++;
                        }
                    }
                }
            }
            return CountOfOccuranceValues;
        }

        /// <summary>
        /// Returns the total of GetCountOfOccurrenceValues for this particle and all of its children.  It's children may just be
        /// particles i.e. sequence, choice, so we need to recurse into particles of that type.
        /// </summary>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        public int GetCountOfOccurrenceValuesInclChildren(uint[] PossibleOccurrenceValues)
        {
            int Count = 0;
            // Get count of this particle
            Count += GetCountOfOccurrenceValues(PossibleOccurrenceValues);

            // Go through all the children
            if ((Particle is XmlSchemaElement) || (Particle is XmlSchemaAny))
            {
                return Count;
            }
            else
            {
                for (int i = 0; i < SubParticles.Length; i++)
                {
                    Count += SubParticles[i].GetCountOfOccurrenceValuesInclChildren(PossibleOccurrenceValues);
                }
            }

            return Count;
        }

        /// <summary>
        /// From the range of possible Occurrence values, choose one based on OccurrenceIndex.
        /// </summary>
        /// <param name="OccurrenceIndex"></param>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        public int GetOccurrenceCount(int OccurrenceIndex, uint[] PossibleOccurrenceValues)
        {
            if (OccurrenceIndex >= GetCountOfOccurrenceValues(PossibleOccurrenceValues))
                Log.Write(MethodBase.GetCurrentMethod(), "Requested occurrence index out of range of possible values", Log.LogType.Warning);
            uint MaxOccurs = 0;
            // Check for an unbounded value for maxOccurs.  If the no maxOccurs is specified then MaxOccursString will be null but
            // MaxOccurs should contain the value 1.
            if (!String.IsNullOrEmpty(oParticle.MaxOccursString) &&
                (0 == oParticle.MaxOccursString.CompareTo("unbounded")))
                MaxOccurs = UInt32.MaxValue;
            else
                MaxOccurs = (uint)Particle.MaxOccurs;
            // Always choose from the user supplied list of occurrence values, this is easiest solution to issue of minOccurs=0 maxOccurs=65535,
            // when this is part of the schema it makes no sense to go through such a large range.
            int Count = 0;
            for (int i = 0; i < PossibleOccurrenceValues.Length; i++)
            {
                if (((uint)oParticle.MinOccurs <= PossibleOccurrenceValues[i])
                    && (PossibleOccurrenceValues[i] <= MaxOccurs))
                {
                    if (Count == OccurrenceIndex)
                        return (int)(PossibleOccurrenceValues[i]);

                    Count++;
                }
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not get occurrence value from configuration", Log.LogType.Warning);
            return 0;
        }


        public override XMLObjectIdentifier ObjectId
        {
            get { throw new NotImplementedException(); }
        }

        public override XmlQualifiedName Name
        {
            get { throw new NotImplementedException(); }
        }

        public override XmlSchemaType SchemaType
        {
            get { throw new NotImplementedException(); }
        }

        public override Fuzzware.Common.DataSchema.DataSchemaTypeCode DataSchemaType
        {
            get { throw new NotImplementedException(); }
        }

        public override TypeRestrictor FacetRestrictor
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasSimpleContent
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasComplexContent
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsComplexType
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsSimpleType
        {
            get { throw new NotImplementedException(); }
        }

        public override bool HasFixedValue
        {
            get { throw new NotImplementedException(); }
        }
    }
}
