using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Editors
{
    class AttributeEditor : SimpleTypeEditor
    {
        bool[] AttributeNotPresent;
        List<XmlSchemaAttribute> oFuzzableAttributes;   // Does not contain fixed or prohibited attributes
        List<XPathNavigator>[] XPathAtrNavs;

        public AttributeEditor(XPathObjectList XPathNodes, XMLElementIdentifier oObjId, PreCompData oPreComp)
            : base(XPathNodes, oPreComp)
        {
            oFuzzableAttributes = new List<XmlSchemaAttribute>();
            
            // Get the node we are fuzzing so we can get a list of possible attributes
            ElementDBEntry oElementDBEntry = oPreComp.ObjectDB[oObjId] as ElementDBEntry;
            XmlSchemaComplexType oComplexSchemaType = oElementDBEntry.SchemaType as XmlSchemaComplexType;
            foreach (DictionaryEntry AttDictEntry in oComplexSchemaType.AttributeUses)
            {
                XmlSchemaAttribute oSchemaAttribute = AttDictEntry.Value as XmlSchemaAttribute;
                // We don't want to follow any ref, as the use value is only specified on the attribute definition in the element
                // If the attribute is required or prohibited then there is no point in fuzzing it
                if (!(oSchemaAttribute.Use == XmlSchemaUse.Required) && !(oSchemaAttribute.Use == XmlSchemaUse.Prohibited))
                    oFuzzableAttributes.Add(oSchemaAttribute);
            }
        }

        public bool MoveToAttribute(XmlSchemaAttribute Attribute)
        {
            AttributeNotPresent = new bool[XPathNodes.Count];
            //int index = 0;
            // Move each XPathNavigator (which point to each instance of this schema type in the XML) to their attribute, of the
            // type of attribute being fuzzed
            for (int i = 0; i < XPathNodes.Count; i++)
            {
                XPathNavigator XPathNav = XPathNodes[i];
                if (!XPathNav.MoveToAttribute(Attribute.QualifiedName.Name, Attribute.QualifiedName.Namespace))
                {
                    AttributeNotPresent[i] = true;
                    // Add the attribute and a default value
                    //XPathNav.CreateAttribute(oPreComp.NamespacePrefixDict[Attribute.SchemaTypeName.Namespace], Attribute.SchemaTypeName.Name, Attribute.SchemaTypeName.Namespace, Attribute.DefaultValue);
                    XPathNav.CreateAttribute(oPreComp.NamespacePrefixDict[Attribute.QualifiedName.Namespace], Attribute.QualifiedName.Name, Attribute.QualifiedName.Namespace, Attribute.DefaultValue);

                    // Move the XPathNavigator to the attribute
                    //if (!XPathNav.MoveToAttribute(Attribute.SchemaTypeName.Name, Attribute.SchemaTypeName.Namespace))
                    if (!XPathNav.MoveToAttribute(Attribute.QualifiedName.Name, Attribute.QualifiedName.Namespace))
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not move to attribute just added", Log.LogType.Warning);
                        return false;
                    }
                }
            }

            return true;
        }

        public bool ReturnFromAttribute()
        {
            for (int i = 0; i < XPathNodes.Count; i++)
            {
                XPathNavigator XPathNav = XPathNodes[i];
                if (AttributeNotPresent[i])
                {
                    // Remove the attribute just added
                    XPathNav.DeleteSelf();
                }
                else if (!XPathNav.MoveToParent())
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not move to parent", Log.LogType.Warning);
                    return false;
                }
            }
            return true;
        }

        public List<XmlSchemaAttribute> FuzzableAttributes
        {
            get
            {
                return oFuzzableAttributes;
            }
        }

        public override void Initialise()
        {
            base.Initialise();

            // Get XPathNavs to each of the attributes of all the nodes
            XPathAtrNavs = new List<XPathNavigator>[XPathNodes.Count];

            for (int i = 0; i < XPathNodes.Count; i++)
            {
                XPathAtrNavs[i] = new List<XPathNavigator>();
                XPathNavigator Nav = XPathNodes[i].Clone();

                if (!Nav.MoveToFirstAttribute())
                    continue;

                // Add all attributes of the node
                do
                {
                    XPathAtrNavs[i].Add(Nav.Clone());
                }
                while (Nav.MoveToNextAttribute());
            }
        }

        public override void RestoreValue(int NodeIndex)
        {
            //base.RestoreValue(NodeIndex);

            if (NodeIndex > XPathNodes.Count)
                return;

            int StartIndex = 0;
            int EndIndex = 0;

            if (-1 == NodeIndex)
            {
                EndIndex = XPathNodes.Count - 1;
            }
            else
            {
                StartIndex = EndIndex = NodeIndex;
            }

            for (int i = StartIndex; i <= EndIndex; i++)
            {
                // Delete all the attributes of this node
                XPathNavigator Nav = XPathNodes[i].Clone();
                while (Nav.MoveToFirstAttribute())
                {
                    Nav.DeleteSelf();
                }

                // Restore the Attributes of the node
                for (int j = 0; j < XPathAtrNavs[i].Count; j++)
                {
                    XPathNavigator Atr = XPathAtrNavs[i][j];
                    Nav.CreateAttribute(Atr.Prefix, Atr.LocalName, Atr.NamespaceURI, Atr.Value);
                }
            }
        }

        public bool CheckAttributeExists(XmlQualifiedName AttName, int NodeIndex)
        {
            XPathNavigator Nav = XPathNodes[NodeIndex];
            if (Nav.MoveToAttribute(AttName.Name, AttName.Namespace))
            {
                Nav.MoveToParent();
                return true;
            }
            return false;
        }

        public void DeleteAttribute(XmlQualifiedName AttName, int NodeIndex)
        {
            XPathNavigator Nav = XPathNodes[NodeIndex];
            if (!Nav.MoveToAttribute(AttName.Name, AttName.Namespace))
                return;

            // This moves the nav to the parent node
            Nav.DeleteSelf();
        }

        /// <summary>
        /// This creates an attribute on the node at NodeIndex, the value of the attribute is the default value as specified in the
        /// schema.  The point here is to make the attribute exist, not create a sensible value for it, although if it is fixed or
        /// has a default then this is used.
        /// </summary>
        /// <param name="Atr"></param>
        /// <param name="NodeIndex"></param>
        public void CreateAttribute(XmlSchemaAttribute Atr, int NodeIndex)
        {
            XPathNavigator Nav = XPathNodes[NodeIndex];
            if (CheckAttributeExists(Atr.QualifiedName, NodeIndex))
                return;

            String AtrValue = (!String.IsNullOrEmpty(Atr.FixedValue)) ? Atr.FixedValue : Atr.DefaultValue;
            Nav.CreateAttribute(oPreComp.NamespacePrefixDict[Atr.QualifiedName.Namespace], Atr.QualifiedName.Name, Atr.QualifiedName.Namespace, AtrValue);
        }
    }
}
