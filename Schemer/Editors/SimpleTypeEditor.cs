using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Fuzzware.Common.XML;
using Fuzzware.ConvertFromXML.Processors;

namespace Fuzzware.Schemer.Editors
{
    class SimpleTypeEditor : IValuesEditor
    {
        protected PreCompData oPreComp;

        // An array of all the XML nodes of the type currently being fuzzed.
        protected XPathObjectList XPathNodes;
        protected String[] OriginalNodeValues;

        public SimpleTypeEditor(XPathObjectList XPathNodes, PreCompData oPreComp)
        {
            this.oPreComp = oPreComp;
            this.XPathNodes = XPathNodes;
        }

        #region IValuesEditor Members

        public virtual void Initialise()
        {
            OriginalNodeValues = new String[XPathNodes.Count];
            for (int i = 0; i < XPathNodes.Count; i++)
            {
                OriginalNodeValues[i] = XPathNodes[i].Value;
            }
        }

        public int NodeCount()
        {
            return XPathNodes.Count;
        }

        /// <summary>
        /// Returns a XmlProcessingInstruction pointing to the MarkedAsTargetNode PI, or null
        /// if the PI does not exist.
        /// </summary>
        private XmlProcessingInstruction GetMarkedAsTargetNodePI(XPathNavigator XPathNav)
        {
            XmlProcessingInstruction PI = null;
            XPathNavigator XPathNodeClone = XPathNav.Clone();
            while (XPathNodeClone.MoveToPrevious())
            {
                if (((IHasXmlNode)XPathNodeClone).GetNode() is XmlProcessingInstruction)
                {
                    PI = ((IHasXmlNode)XPathNodeClone).GetNode() as XmlProcessingInstruction;
                    // Check to see if it is a PI for our app
                    if (PI.Target == "Schemer")
                    {
                        if (!String.IsNullOrEmpty(PI.Data))
                            if (PI.Data.Equals("TargetNode=\"true\"", StringComparison.CurrentCultureIgnoreCase))
                            {
                                break;
                            }
                    }
                    PI = null;
                }
                else
                    break;
            }
            return PI;
        }

        /// <summary>
        /// Returns true if the node pointed to by XPathNav has been marked as a TargetNode.
        /// </summary>
        private bool MarkedAsTargetNode(XPathNavigator XPathNav)
        {
            if (GetMarkedAsTargetNodePI(XPathNav) != null)
                return true;
            return false;
        }

        private void MarkAsTargetNode(int NodeIndex)
        {
            XPathNavigator XPathNode = XPathNodes[NodeIndex].Clone();
            // Since we use a SimpleTypeEditor for attributes, only add if we are on an element
            if (XPathNode.NodeType == XPathNodeType.Element)
            {
                // Check if we have added it before
                if (MarkedAsTargetNode(XPathNode))
                    return;
                // Insert the 'TargetNode="true"' PI above this node, so we don't update it.  This does not affect 
                // the position of the XPathNav
                XPathNode.InsertBefore("<?Schemer TargetNode=\"true\"?>");
            }
        }

        private void UnMarkAsTargetNode(int NodeIndex)
        {
            XPathNavigator XPathNode = XPathNodes[NodeIndex].Clone();
            // Since we use a SimpleTypeEditor for attributes, only add if we are on an element
            if (XPathNode.NodeType == XPathNodeType.Element)
            {
                XmlProcessingInstruction PI = GetMarkedAsTargetNodePI(XPathNode);
                if(null != PI)
                {
                    XPathNavigator PINav = PI.CreateNavigator();
                    PINav.DeleteSelf();
                }
            }
        }

        public void ChangeValue(int NodeIndex, string NewValue)
        {
            if (NodeIndex >= XPathNodes.Count)
                return;
            if (null == NewValue)
                return;

            if (-1 == NodeIndex)
            {
                ChangeAllValues(NewValue);
            }
            else
            {
                // TODO: Need to catch some exceptions here
                XPathNodes[NodeIndex].SetValue(NewValue);

                MarkAsTargetNode(NodeIndex);
            }

            
        }

        public void ChangeAllValues(string NewValue)
        {
            if (null == NewValue)
                return;

            for (int i = 0; i < XPathNodes.Count; i++)
            {
                XPathNodes[i].SetValue(NewValue);
                MarkAsTargetNode(i);
            }  
        }

        public String GetValue(int NodeIndex)
        {
            if (NodeIndex >= XPathNodes.Count)
                return null;

            if (NodeIndex >= 0)
                return XPathNodes[NodeIndex].Value;

            return null;
        }

        public String[] GetAllValues()
        {
            String[] output = new String[XPathNodes.Count];

            for (int i = 0; i < XPathNodes.Count; i++)
            {
                output[i] = XPathNodes[i].Value;
            }

            return output;
        }

        public virtual void RestoreValue(int NodeIndex)
        {
            if (NodeIndex >= XPathNodes.Count)
                return;

            if (NodeIndex >= 0)
            {
                XPathNodes[NodeIndex].SetValue(OriginalNodeValues[NodeIndex]);
                UnMarkAsTargetNode(NodeIndex);
            }
            
            if(-1 == NodeIndex)
            {
                for (int i = 0; i < XPathNodes.Count; i++)
                {
                    XPathNodes[i].SetValue(OriginalNodeValues[i]);
                    UnMarkAsTargetNode(i);
                }
            }
        }

        #endregion
    }
}
