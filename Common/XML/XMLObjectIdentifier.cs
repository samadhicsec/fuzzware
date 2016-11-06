using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Fuzzware.Common.XML
{
    /// <summary>
    /// An XMLObjectIdentifier is a unique identifier for an XmlSchemaElement or XmlSchemaAttribute.  Schema Elements/Attributes can have the
    /// same name so we need more than that to identify them, so we use their LineNumber and LinePosition in the XSD file as a 
    /// differentiator.  Different instances of XmlSchemaElements can also have different minOccurs and maxOccurs, so we can keep track
    /// of that as well so we can differentiate, but we don't have too (this is important for Occurrence fuzzing).
    /// </summary>
    public abstract class XMLObjectIdentifier : IComparable<XMLObjectIdentifier>
    {
        protected XmlQualifiedName oQualifiedName;
        protected XmlSchemaObject oSchemaType;
        protected int minOccurs = -1;
        protected int maxOccurs = -1;

        public virtual XmlQualifiedName QualifiedName
        {
            get { return oQualifiedName; }
        }

        public virtual XmlSchemaObject SchemaType
        {
            get { return oSchemaType; }
        }

        public virtual int MinOccurs
        {
            get { return minOccurs; }
        }

        public virtual int MaxOccurs
        {
            get { return maxOccurs; }
        }

        #region IComparable<XMLElementIdentifier> Members

        public virtual int CompareTo(XMLObjectIdentifier other)
        {
            int cmp = QualifiedName.ToString().CompareTo(other.QualifiedName.ToString());
            if (cmp != 0)
                return cmp;

            // So because the SchemaType object reference will be different depending if we get it from the schema or the 
            // post schema validation from an XPathNavigator we need to compare something else.  The combination of 
            // LineNumber and LinePosition seems like it is probably unique.
            if (SchemaType.LineNumber < other.SchemaType.LineNumber)
                return -1;
            else if (SchemaType.LineNumber > other.SchemaType.LineNumber)
                return 1;
            if (SchemaType.LinePosition < other.SchemaType.LinePosition)
                return -1;
            else if (SchemaType.LinePosition > other.SchemaType.LinePosition)
                return 1;

            if ((minOccurs != -1) && (maxOccurs != -1) && (other.MinOccurs != -1) && (other.MaxOccurs != -1))
            {
                if (minOccurs != other.MinOccurs)
                    return minOccurs - other.MinOccurs;
                if (maxOccurs != other.MaxOccurs)
                    return maxOccurs - other.MaxOccurs;
            }

            return 0;
        }

        #endregion
    }
}
