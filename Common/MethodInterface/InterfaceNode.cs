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
    /// Converts an XML representation of an interface method calls into an InterfaceNode object.
    /// </summary>
    public class InterfaceNode
    {
        InterfaceDescription oInterfaceDescription;
        List<MethodNode> oMethodNodes;

        public InterfaceNode(InterfaceDescription Desc)
        {
            oInterfaceDescription = Desc;
        }

        public InterfaceDescription Description
        {
            get
            {
                return oInterfaceDescription;
            }
        }

        public List<MethodNode> MethodNodes
        {
            get
            {
                return oMethodNodes;
            }
        }

        public bool Deserialise(XPathNavigator XPathNav)
        {
            XPathNavigator XPathNavLocal = XPathNav.Clone();

            // Check the interface name and namespace
            //if (!XPathNavLocal.LocalName.StartsWith(oInterfaceDescription.Name + ".") ||
            //    !XPathNavLocal.NamespaceURI.Equals(oInterfaceDescription.Namespace))
            //    return false;
            if (!XPathNavLocal.LocalName.StartsWith(oInterfaceDescription.Name + "."))
                return false;

            oMethodNodes = new List<MethodNode>();

            // Check if there are parameters to deserialise
            if (0 == oInterfaceDescription.Methods.Count)
                return true;

            bool bFoundMethod = false;
            // Loop through and find the method to deserialise
            for (int i = 0; i < oInterfaceDescription.Methods.Count; i++)
            {
                MethodNode Node = new MethodNode(oInterfaceDescription.Methods[i]);
                if (Node.Deserialise(XPathNavLocal))
                {
                    // Add to the list of methods to call
                    oMethodNodes.Add(Node);
                    bFoundMethod = true;
                    break;
                }
            }
            if (!bFoundMethod)
                Log.Write(MethodBase.GetCurrentMethod(), "Could not find method '" + XPathNavLocal.LocalName + "'.  Skipping.", Log.LogType.Warning);

            return true;
        }
    }
}
