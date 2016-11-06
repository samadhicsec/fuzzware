using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Common.DataSchema
{
    /// <summary>
    /// The ISchemaParticle is the interface used to retrieve information on a schema particle.  A schema particle is a generic holder for
    /// all different types of particles, where the different types are implementation dependant.
    /// </summary>
    public interface ISchemaParticle
    {
        /// <summary>
        /// Gets the array of sub-particles of this particle
        /// </summary>
        ISchemaParticle[] SubSchemaParticles
        {
            get;
        }

        /// <summary>
        /// Returns the total number of possible occurrence values for this particle, according to the schema.
        /// </summary>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        int GetCountOfOccurrenceValues(uint[] PossibleOccurrenceValues);

        /// <summary>
        /// Returns the total number of possible occurrence values for this particle and recursively its sub-particles, 
        /// according to the schema.
        /// </summary>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        int GetCountOfOccurrenceValuesInclChildren(uint[] PossibleOccurrenceValues);
        
        /// <summary>
        /// From the range of possible Occurrence values, choose one based on OccurrenceIndex.
        /// </summary>
        /// <param name="OccurrenceIndex"></param>
        /// <param name="ParentType"></param>
        /// <param name="PossibleOccurrenceValues"></param>
        /// <returns></returns>
        int GetOccurrenceCount(int OccurrenceIndex, uint[] PossibleOccurrenceValues);
    }
}
