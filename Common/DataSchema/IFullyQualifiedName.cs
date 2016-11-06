using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Common.DataSchema
{
    /// <summary>
    /// The IFullyQualifiedName interface is used to allow data descriptions to manage data elements by a unique name amongst
    /// all data elements that exist.  A QualifiedName is a combination of a Namespace and a Name, a FullyQualifiedName is the
    /// combination of a data elements QualifiedName and the QualifiedName of all of it's parent data elements.
    /// </summary>
    public interface IFullyQualifiedName : IQualifiedName, IComparer<IFullyQualifiedName>
    {
        /// <summary>
        /// Gets the FullyQualifiedName
        /// </summary>
        String FullyQualifiedName
        {
            get;
        }

        /// <summary>
        /// Gets the parent FullyQualifiedName
        /// </summary>
        IFullyQualifiedName ParentFQN
        {
            get;
        }

        /// <summary>
        /// Compares two IFullyQualifiedName and returns a value indicating their relative sort order.
        /// </summary>
        /// <param name="FQN1"></param>
        /// <param name="FQN2"></param>
        /// <returns></returns>
        new int Compare(IFullyQualifiedName FQN1, IFullyQualifiedName FQN2);

        object CastToBase<T>();
    }
}
