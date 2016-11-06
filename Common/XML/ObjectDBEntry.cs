using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.DataSchema.Restrictions;

namespace Fuzzware.Common.XML
{
    public abstract class ObjectDBEntry
    {
        protected XmlSchemaSet oSchemaSet;
        protected uint uiDepth;

        public abstract XMLObjectIdentifier ObjectId
        {
            get;
        }

        public abstract XmlQualifiedName Name
        {
            get;
        }

        public abstract XmlSchemaType SchemaType
        {
            get;
        }

        public abstract DataSchemaTypeCode DataSchemaType
        {
            get;
        }

        public abstract TypeRestrictor FacetRestrictor
        {
            get;
        }

        public abstract bool HasSimpleContent
        {
            get;
        }

        public abstract bool HasComplexContent
        {
            get;
        }

        public abstract bool IsComplexType
        {
            get;
        }

        public abstract bool IsSimpleType
        {
            get;
        }

        public abstract bool HasFixedValue
        {
            get;
        }
    }
}
