using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Common.XML
{
    /// <summary>
    /// An XmlSchemaElement can be uniquely identified based on its XmlQualifiedName, XmlSchemaType and its minOccurs and maxOccurs
    /// </summary>
    //public class XMLElementIdentifier : IComparable<XMLElementIdentifier>
    public class XMLElementIdentifier : XMLObjectIdentifier
    {
        public XMLElementIdentifier(XmlQualifiedName QualifiedName, XmlSchemaElement SchemaElement, XmlSchemaSet oSchemaSet)
        {
            if (null == QualifiedName)
                Log.Write(MethodBase.GetCurrentMethod(), "QualifiedName was null", Log.LogType.Error);
            if (null == SchemaElement)
                Log.Write(MethodBase.GetCurrentMethod(), "EleSchemaType was null", Log.LogType.Error);

            oQualifiedName = QualifiedName;

            if (!SchemaElement.RefName.IsEmpty)
                SchemaElement = XMLHelper.GetElementFromSchema(oSchemaSet, SchemaElement);
            oSchemaType = SchemaElement;
        }

        public XMLElementIdentifier(XmlQualifiedName QualifiedName, XmlSchemaElement EleSchemaType, XmlSchemaSet oSchemaSet, decimal MinOccurs, decimal MaxOccurs)
        {
            oQualifiedName = QualifiedName;

            if (!EleSchemaType.RefName.IsEmpty)
                EleSchemaType = XMLHelper.GetElementFromSchema(oSchemaSet, EleSchemaType);
            oSchemaType = EleSchemaType;

            minOccurs = (int)MinOccurs;
            if (MaxOccurs > (decimal)Int32.MaxValue)
                maxOccurs = Int32.MaxValue;
            else
                maxOccurs = (int)MaxOccurs;
        }
    }
}
