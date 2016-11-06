using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Fuzzware.Common.XML
{
    public class ObjectDataBase
    {
        private SortedDictionary<XMLObjectIdentifier, ObjectDBEntry> oIDObjectDictionary;
        private List<XMLObjectIdentifier> oObjectIdList;
        private XmlSchemaSet oSchemaSet;

        public ObjectDataBase(XmlSchemaSet SchemaSet)
        {
            oSchemaSet = SchemaSet;
            oIDObjectDictionary = new SortedDictionary<XMLObjectIdentifier, ObjectDBEntry>();
            oObjectIdList = new List<XMLObjectIdentifier>();
        }

        public ObjectDBEntry this[XMLObjectIdentifier Id]
        {
            get
            {
                return oIDObjectDictionary[Id];
            }
        }

        public int Count
        {
            get { return oIDObjectDictionary.Count; }
        }

        public void Add(XMLElementIdentifier Id, ElementDBEntry oElementDBEntry)
        {
            // We add to the dictionary XMLElementIdentifier that are unique by name, type, min and max.  We do this so we can accurately
            // recover min and max occurs for a given element (rather than 1 min and max occurs for a name/type combination).
            oIDObjectDictionary.Add(Id, oElementDBEntry);

            // We add to the list XMLElementIdentifier that are unique by name and type.  We do this so we can easily recover a list of types
            // that we still reference by name.
            XMLElementIdentifier NewId = new XMLElementIdentifier(Id.QualifiedName, Id.SchemaType as XmlSchemaElement, oSchemaSet);
            if (!oObjectIdList.Contains(NewId))
                oObjectIdList.Add(NewId);
        }

        public void Add(XMLAttributeIdentifier Id, AttributeDBEntry oAttributeDBEntry)
        {
            // We add to the dictionary XMLAttributeIdentifier that are unique by name, type, min and max.  We do this so we can accurately
            // recover min and max occurs for a given element (rather than 1 min and max occurs for a name/type combination).
            oIDObjectDictionary.Add(Id, oAttributeDBEntry);

            // We add to the list XMLAttributeIdentifier that are unique by name and type.  We do this so we can easily recover a list of types
            // that we still reference by name.
            XMLAttributeIdentifier NewId = new XMLAttributeIdentifier(Id.QualifiedName, Id.SchemaType as XmlSchemaAttribute, oSchemaSet);
            if (!oObjectIdList.Contains(NewId))
                oObjectIdList.Add(NewId);
        }

        public bool Contains(XMLObjectIdentifier Id)
        {
            return oIDObjectDictionary.ContainsKey(Id);
        }

        public ObjectDBEntry[] this[XmlQualifiedName Name]
        {
            get
            {
                List<ObjectDBEntry> Elements = new List<ObjectDBEntry>();
                foreach (XMLObjectIdentifier Id in oIDObjectDictionary.Keys)
                {
                    if (Name.ToString().Equals(Id.QualifiedName.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        Elements.Add(oIDObjectDictionary[Id]);
                }
                return Elements.ToArray();
            }
        }

        public ObjectDBEntry this[int i]
        {
            get
            {
                return this[ObjectIdList[i]];
            }
        }

        public bool ObjectNameUnique(XmlQualifiedName Name)
        {
            ObjectDBEntry[] Elements = this[Name];
            if (Elements.Length == 1)
                return true;
            return false;
        }

        public List<XMLObjectIdentifier> ObjectIdList
        {
            get
            {
                return oObjectIdList;
            }
        }
    }
}
