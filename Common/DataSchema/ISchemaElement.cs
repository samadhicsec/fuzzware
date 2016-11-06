using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Common.DataSchema
{
    /// <summary>
    /// This ISchemaElement is the interface used to retrieve information on a schema element.  A schema element is a specific type of 
    /// schema particle that has specific properties e.g. name, schema type, restrictions, content types
    /// </summary>
    public interface ISchemaElement : ISchemaParticle
    {
        /// <summary>
        /// Gets the FullyQualifiedName of the particle.  Not all particles will have a FullyQualifiedName.
        /// </summary>
        IFullyQualifiedName FQName
        {
            get;
        }

        /// <summary>
        /// Gets the Name of the particle.  Not all particles will have a Name.
        /// </summary>
        //String Name
        //{
        //    get;
        //}

        /// <summary>
        /// Gets the basic type
        /// </summary>
        DataSchemaTypeCode SchemaType
        {
            get;
        }

        /// <summary>
        /// Gets the TypeRestictor for this particle
        /// </summary>
        TypeRestrictor FacetRestrictor
        {
            get;
        }

        /// <summary>
        /// Gets a bool indicating if the particle has simple content (i.e. no children)
        /// </summary>
        bool HasSimpleContent
        {
            get;
        }

        /// <summary>
        /// Gets a bool indicating if the particle has complex content (i.e. children)
        /// </summary>
        bool HasComplexContent
        {
            get;
        }

        /// <summary>
        /// Gets a bool indicating if the element is a complex type
        /// </summary>
        bool IsComplexType
        {
            get;
        }

        /// <summary>
        /// Gets a bool indicating if the element is a simple type
        /// </summary>
        bool IsSimpleType
        {
            get;
        }

        /// <summary>
        /// Gets a bool indicating if the element has a fixed value
        /// </summary>
        bool HasFixedValue
        {
            get;
        }
    }
}
