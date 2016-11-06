using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Fuzzware.Common;
using Fuzzware.Common.XML;

namespace Fuzzware.Schemer.Editors
{
    public interface IEditor
    {
        int NodeCount();
    }

    // This treats all node values as strings, fuzzers will modify those strings.
    public interface IValuesEditor : IEditor
    {
        void Initialise();
        // We should be able to fuzz each node individually and all the nodes at once
        void ChangeValue(int NodeIndex, string NewValue);
        void ChangeAllValues(string NewValue);
        String GetValue(int NodeIndex);
        String[] GetAllValues();
        void RestoreValue(int NodeIndex);
    }

    // This treats the child nodes as opaque XML, Fuzzers will include, exclude and repeat child nodes.
    interface IChildNodesEditor : IEditor
    {
        /// <summary>
        /// Initialise the editor.  Creates a Dictionary of all the child nodes, these will be used to create the fuzzing examples.
        /// </summary>
        /// <param name="ReferenceType"></param>
        void Initialise();
        //void Initialise(out Dictionary<XmlQualifiedName, List<XPathNavigator>>[] ExamplesDictionary);

        ParticleInstance GetNodeInstance(int NodeIndex);

        /// <summary>
        /// Returns the type of the Parent node.  This is the type currently being fuzzed.
        /// </summary>
        /// <returns></returns>
        //XmlSchemaComplexType GetParentType();

        /// <summary>
        /// The object that is returned should not be changed.  It does not need to be as the SetNodeArray takes a different type.
        /// </summary>
        /// <returns></returns>
        //TypedXPathNodes GetParentNode();

        /// <summary>
        /// Sets the child nodes of all the currently selected parent nodes.
        /// </summary>
        /// <param name="NodeLists"></param>
        /// <param name="NodeIndex"></param>
        void SetNodeArray(List<XPathNavigator> NodeLists, int NodeIndex);
        
        /// <summary>
        /// Restores all the child nodes
        /// </summary>
        void RestoreChildNodes();
    }
}
