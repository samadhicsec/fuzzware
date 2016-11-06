using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Fuzzware.Common.XML
{
    /// <summary>
    /// An object to store a list of XPathNavigators that point to schema objects of a certain name, however it will act as a sub-list 
    /// of those XPathNavigators that are all of the same XMLObjectIdentifier.
    /// </summary>
    public class XPathObjectList
    {
        XmlQualifiedName oQName;
        XPathNavigator[] oOrigObjectList;
        List<XPathNavigator> oTypedObjectList;

        public XPathObjectList(XPathNavigator[] NodeList, XMLObjectIdentifier oXMLIdentifier, XmlSchemaSet oSchemaSet)
        {
            oOrigObjectList = NodeList;
            oQName = oXMLIdentifier.QualifiedName;

            // Create reduced list of XPath nodes, accounting for type
            oTypedObjectList = new List<XPathNavigator>();
            for (int i = 0; i < oOrigObjectList.Length; i++)
            {
                // Create XMLIdentifier from the XPathNavigator
                XMLObjectIdentifier oOrigNode = null;
                if (oXMLIdentifier is XMLElementIdentifier)
                {
                    XmlSchemaElement oSchemaElement = oOrigObjectList[i].SchemaInfo.SchemaElement;
                    // If the schema element is a ref, this will be handled in XMLElementIdentifier
                    oOrigNode = new XMLElementIdentifier(oQName, oSchemaElement, oSchemaSet);
                }
                else if (oXMLIdentifier is XMLAttributeIdentifier)
                {
                    XmlSchemaAttribute oSchemaAttribute = oOrigObjectList[i].SchemaInfo.SchemaAttribute;
                    // If the schema attribute is a ref, this will be handled in XMLAttributeIdentifier
                    oOrigNode = new XMLAttributeIdentifier(oQName, oSchemaAttribute, oSchemaSet);
                }

                // Compare created XMLIdentifier to oXMLIdentifier
                if (oOrigNode.CompareTo(oXMLIdentifier) == 0)
                    oTypedObjectList.Add(oOrigObjectList[i]);
            }
        }

        /// <summary>
        /// Returns the count of the XPathNavigators that have the specified type
        /// </summary>
        public int Count
        {
            get { return oTypedObjectList.Count; }
        }

        /// <summary>
        /// Returns the XPathNavigator of the specified type at the index given
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public XPathNavigator this[int index]
        {
            get { return oTypedObjectList[index]; }
        }

        /// <summary>
        /// Remove XPathNavigator at index from the list of XPathNavigators of the specified type
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            oTypedObjectList.RemoveAt(index);
        }

        /// <summary>
        /// Returns the relative node index of a XPathNavigator in the list of typed XPathNavigators of that name
        /// </summary>
        /// <param name="NodePointer"></param>
        /// <returns></returns>
        public int RelativeNodeIndex(XPathNavigator NodePointer)
        {
            for (int i = 0; i < oTypedObjectList.Count; i++)
            {
                if (NodePointer == oTypedObjectList[i])
                    return i;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find relative node index for " + oQName.ToString(), Log.LogType.Error);
            return 0;
        }

        /// <summary>
        /// Returns the unique node index of a XPathNavigator in the list of all XPathNavigators of that name
        /// </summary>
        /// <param name="NodePointer"></param>
        /// <returns></returns>
        public int UniqueNodeIndex(XPathNavigator NodePointer)
        {
            for (int i = 0; i < oOrigObjectList.Length; i++)
            {
                if (NodePointer == oOrigObjectList[i])
                    return i;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find unique node index for " + oQName.ToString(), Log.LogType.Error);
            return 0;
        }

        /// <summary>
        /// Returns the XPathNavigator for Node at index in the array of all XPathNavigators for the current Object name.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns></returns>
        public XPathNavigator XPathFromAbsoluteIndex(int index)
        {
            if((index < 0) || (index > oOrigObjectList.Length))
                Log.Write(MethodBase.GetCurrentMethod(), "Index for absolute node out of range", Log.LogType.Error);

            return oOrigObjectList[index];
        }

        /// <summary>
        /// Returns true if the name of this node has a unique type i.e. there are not two nodes with this name that have different types.
        /// </summary>
        public bool NodeNameUnique()
        {
            return (oOrigObjectList.Length == oTypedObjectList.Count);
        }
    }
}
