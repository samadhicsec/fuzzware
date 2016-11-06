using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Common.DataSchema
{
    public interface IQualifiedName : IComparer<IQualifiedName>
    {
        /// <summary>
        /// Gets the name of the QualifiedName.
        /// </summary>
        String Name
        {
            get;
        }

        /// <summary>
        /// Gets the namespace of the QualifiedName.
        /// </summary>
        String Namespace
        {
            get;
        }

        /// <summary>
        /// Gets the QualifiedName.
        /// </summary>
        String QualifiedName
        {
            get;
        }

        /// <summary>
        /// Return a string representation of the QualifiedName
        /// </summary>
        /// <returns></returns>
        String ToString();

        /// <summary>
        /// Compares two IQualifiedName and returns a value indicating their relative sort order.
        /// </summary>
        /// <param name="FQN1"></param>
        /// <param name="FQN2"></param>
        /// <returns></returns>
        new int Compare(IQualifiedName QN1, IQualifiedName QN2);
    }
}
