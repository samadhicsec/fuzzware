using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Fuzzware.Common.XML
{
    public class XMLAttributeIdentifier : XMLObjectIdentifier
    {
        public XMLAttributeIdentifier(XmlQualifiedName QualifiedName, XmlSchemaAttribute SchemaAttribute, XmlSchemaSet SchemaSet, bool SetMinMax)
        {
            if (null == QualifiedName)
                Log.Write(MethodBase.GetCurrentMethod(), "QualifiedName was null", Log.LogType.Error);
            if (null == SchemaAttribute)
                Log.Write(MethodBase.GetCurrentMethod(), "AttSchemaType was null", Log.LogType.Error);

            oQualifiedName = QualifiedName;

            // Need not to change SchemaAttribute so we get the correct Use value
            if (!SchemaAttribute.RefName.IsEmpty)
                oSchemaType = XMLHelper.GetAttributeFromSchema(SchemaSet, SchemaAttribute);
            else
                oSchemaType = SchemaAttribute;

            if (SetMinMax)
            {
                // Set min max occurs
                if (SchemaAttribute.Use == XmlSchemaUse.Required)
                    minOccurs = 1;
                else
                    minOccurs = 0;

                if (SchemaAttribute.Use == XmlSchemaUse.Prohibited)
                    maxOccurs = 0;
                else
                    maxOccurs = 1;
            }
        }

        public XMLAttributeIdentifier(XmlQualifiedName QualifiedName, XmlSchemaAttribute AttSchemaType, XmlSchemaSet SchemaSet)
            : this(QualifiedName, AttSchemaType, SchemaSet, false)
        {
        }
    }
}
