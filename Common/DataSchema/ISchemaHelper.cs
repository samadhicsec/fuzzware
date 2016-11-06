using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Common.DataSchema
{
    public interface ISchemaHelper
    {
        /// <summary>
        /// Return a qualified name.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Namespace"></param>
        /// <returns></returns>
        //IQualifiedName GenQName(String Name, String Namespace);

        /// <summary>
        /// Return a fully qualified name
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Namespace"></param>
        /// <param name="ParentName"></param>
        /// <returns></returns>
        //IFullyQualifiedName GenFQName(String Name, String Namespace, IFullyQualifiedName ParentName);

        /// <summary>
        /// Returns the IDataDoc that has been reloaded
        /// </summary>
        /// <param name="Doc"></param>
        /// <param name="SchemaSet"></param>
        /// <returns></returns>
        IDataDoc ReloadDataDoc(String Doc, ISchemaSet SchemaSet);
    }
}
