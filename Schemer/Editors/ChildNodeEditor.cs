using System;
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
    class ChildNodeEditor : IChildNodesEditor
    {
        PreCompData oPreComp;
        XPathObjectList XPathParents;
        List<List<XPathNavigator>> OriginalChildNodes;
        List<ParticleInstance> ParentInstances;

        public ChildNodeEditor(XPathObjectList XPathParents, PreCompData oPreComp)
        {
            this.oPreComp = oPreComp;
            this.XPathParents = XPathParents;
            ParentInstances = new List<ParticleInstance>();
        }

        #region IChildNodesEditor Members

        public void Initialise()
        {
            // Record all the children for each parent
            // Do this in a List so we can restore the child nodes
            OriginalChildNodes = new List<List<XPathNavigator>>();
 
            // For each selected node of the type being fuzzed
            for (int i = 0; i < XPathParents.Count; i++)
            {
                // Create storage
                OriginalChildNodes.Add(new List<XPathNavigator>());

                // Check for children
                if (!XPathParents[i].HasChildren)
                    continue;

                XPathNavigator XPathPointer = XPathParents[i];
                // Move to first child
                if (!XPathPointer.MoveToFirstChild())
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not move to first child", Log.LogType.Error);
                }
                
                // Loop through children
                do
                {
                    // Add to storage
                    OriginalChildNodes[i].Add(XPathPointer.Clone());
                } while (XPathPointer.MoveToNext());

                // Reset XPathNav back to parent
                if (!XPathParents[i].MoveToParent())
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Could not move to parent", Log.LogType.Error);
                }
            }

            // Map the XPathNavs XML to particle instances in the SchemerParticleDatabase
            XMLToSchemaMapper Mapper = new XMLToSchemaMapper();
            for (int i = 0; i < XPathParents.Count; i++)
            {
                ParentInstances.Add(Mapper.MapXMLToSchema(XPathParents[i], i, oPreComp));
            }
        }

        public int NodeCount()
        {
            return XPathParents.Count;
        }

        public ParticleInstance GetNodeInstance(int NodeIndex)
        {
            if ((NodeIndex < 0) || (NodeIndex >= ParentInstances.Count))
                if (0 == ParentInstances.Count)
                    return null;
                else
                    Log.Write(MethodBase.GetCurrentMethod(), "Node index out of range", Log.LogType.Error);

            return ParentInstances[NodeIndex];
        }

        public void SetNodeArray(List<XPathNavigator> NodeList, int NodeIndex)
        {
            if (null == NodeList)
                return;

            if (NodeIndex > XPathParents.Count - 1)
                return;

            if (-1 == NodeIndex)
            {
                SetAllNodeArrays(NodeList);
            }
            else
            {
                SetNodeArray(XPathParents[NodeIndex], NodeList);
            }
        }

        public void SetAllNodeArrays(List<XPathNavigator> NodeList)
        {
            for (int i = 0; i < XPathParents.Count; i++)
            {
                SetNodeArray(XPathParents[i], NodeList);
            }
        }

        public void RestoreChildNodes()
        {
            for (int i = 0; i < OriginalChildNodes.Count; i++)
            {
                SetNodeArray(XPathParents[i], OriginalChildNodes[i]);
            }
        }

        #endregion

        private void SetNodeArray(XPathNavigator Parent, List<XPathNavigator> NodeList)
        {
            // Delete all children of the parent
            while(Parent.MoveToFirstChild())
            {
                // Once deleted the XPathNavigator moves back to the parent
                Parent.DeleteSelf();
            }

            // Add all the new nodes
            for (int i = 0; i < NodeList.Count; i++)
            {
                // The Parent XPathNavigator is not moved when a child is added.
                Parent.AppendChild(NodeList[i]);
            }
        }
    }
}
