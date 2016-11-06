using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Fuzzware.Common;

namespace Fuzzware.Common.MethodInterface
{
    /// <summary>
    /// Converts an XML representation of a series of interface method calls into a LibraryNode object.
    /// </summary>
    public class LibraryNode
    {
        LibraryDescription oLibraryDescription;
        List<InterfaceNode> oInterfaceNodes;

        public LibraryNode(LibraryDescription Desc)
        {
            oLibraryDescription = Desc;
        }

        public LibraryDescription Description
        {
            get
            {
                return oLibraryDescription;
            }
        }

        public List<InterfaceNode> InterfaceNodes
        {
            get
            {
                return oInterfaceNodes;
            }
        }

        public bool Deserialise(XPathNavigator XPathNav)
        {
            XPathNavigator XPathNavLocal = XPathNav.Clone();

            // Check the Library name and namespace
            if (!XPathNavLocal.LocalName.Equals(oLibraryDescription.Name) ||
                !XPathNavLocal.NamespaceURI.Equals(oLibraryDescription.Namespace))
                return false;

            oInterfaceNodes = new List<InterfaceNode>();

            // Check if there are interfaces to deserialise
            if (0 == oLibraryDescription.Interfaces.Count)
                return true;

            if (!XPathNavLocal.MoveToFirstChild())
                return false;

            // The Library node could have many Interfaces, in any order.
            do
            {
                bool bFoundInterface = false;
                // Loop through and find the interface to deserialise
                for (int i = 0; i < oLibraryDescription.Interfaces.Count; i++)
                {
                    InterfaceNode Node = new InterfaceNode(oLibraryDescription.Interfaces[i]);
                    if (Node.Deserialise(XPathNavLocal))
                    {
                        // Add to the list of methods to call
                        oInterfaceNodes.Add(Node);
                        bFoundInterface = true;
                        break;
                    }
                }
                if (!bFoundInterface)
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not find interface '" + XPathNavLocal.LocalName + "'.  Skipping.", Log.LogType.Warning);
            }
            while (XPathNavLocal.MoveToNext());

            return true;
        }
    }
}
