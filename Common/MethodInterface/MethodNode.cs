using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Fuzzware.Common;

namespace Fuzzware.Common.MethodInterface
{
    /// <summary>
    /// Converts an XML representation of a method parameter into a ParameterNode object, which also holds the
    /// value of the parameter.
    /// </summary>
    public class ParameterNode
    {
        ParameterDesc oParameterDesc;
        String ParamStrVal;

        public ParameterNode(ParameterDesc Desc)
        {
            oParameterDesc = Desc;
        }

        public ParameterDesc Description
        {
            get
            {
                return oParameterDesc;
            }
        }

        public String Value
        {
            get
            {
                return ParamStrVal;
            }
        }

        public bool Deserialise(XPathNavigator XPathNav)
        {
            if (XPathNav.LocalName.Equals(oParameterDesc.ParamSchemaElement.Name))
            {
                ParamStrVal = XPathNav.Value;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts an XML representation of a method call into a MethodNode object
    /// </summary>
    public class MethodNode
    {
        MethodDescription oMethodDescription;
        List<ParameterNode> oParameterNodes;

        public MethodNode(MethodDescription Desc)
        {
            oMethodDescription = Desc;
        }

        public MethodDescription Description
        {
            get
            {
                return oMethodDescription;
            }
        }

        public List<ParameterNode> ParameterNodes
        {
            get
            {
                return oParameterNodes;
            }
        }

        public bool Deserialise(XPathNavigator XPathNav)
        {
            XPathNavigator XPathNavLocal = XPathNav.Clone();
            if (XPathNavLocal.LocalName.EndsWith("." + oMethodDescription.MethodName, StringComparison.CurrentCulture))
            {
                oParameterNodes = new List<ParameterNode>();

                // Check if there are parameters to deserialise
                if (0 == oMethodDescription.ParameterDescs.Count)
                    return true;

                // Move to the first child
                if (!XPathNavLocal.MoveToFirstChild())
                {
                    // Maybe all parameters have minOccurs = 0
                    for(int i = 0; i < oMethodDescription.ParameterDescs.Count; i++)
                        if(oMethodDescription.ParameterDescs[i].ParamSchemaElement.MinOccurs != 0)
                            return false;
                    return true;
                }

                // Loop through each parameter and deserialise
                for (int i = 0; i < oMethodDescription.ParameterDescs.Count; i++)
                {
                    ParameterNode PNode = new ParameterNode(oMethodDescription.ParameterDescs[i]);
                    if (!PNode.Deserialise(XPathNavLocal))
                    {
                        // Check whether 0 occurrences of this param is allowed 
                        if (0 == oMethodDescription.ParameterDescs[i].ParamSchemaElement.MinOccurs)
                            continue;
                        return false;
                    }
                    oParameterNodes.Add(PNode);

                    if (i < oMethodDescription.ParameterDescs.Count - 1)
                        if (!XPathNavLocal.MoveToNext())
                        {
                            // Check if tghe remainder of the params are allowed 0 occurrences
                            for(int j = i+1; j < oMethodDescription.ParameterDescs.Count;j++)
                                if (oMethodDescription.ParameterDescs[j].ParamSchemaElement.MinOccurs != 0)
                                    return false;
                            return true;
                        }
                }

                return true;
            }
            return false;
        }
    }
}
