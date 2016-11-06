using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Common.DataSchema
{
    public interface IElementIdentifier : IComparer<IElementIdentifier>
    {
        IQualifiedName QualifiedName
        {
            get;
        }

        ISchemaType SchemaType
        {
            get;
        }
    }
}
