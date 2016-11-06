using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Fuzzware.Convert2XML.C2X
{
    public class C2XProgressEventArgs : EventArgs
    {
        string Converted2XMLSoFar;

        public C2XProgressEventArgs(XmlDocument oXmlDoc)
        {
            XmlWriterSettings writer = new XmlWriterSettings();
            writer.Indent = true;
            writer.NewLineHandling = NewLineHandling.None;
            StringBuilder output = new StringBuilder();
            XmlWriter oXmlWriter = XmlWriter.Create(output, writer);
            oXmlDoc.WriteTo(oXmlWriter);
            oXmlWriter.Flush();
            Converted2XMLSoFar = output.ToString();
        }
        public string XML
        {
            get { return Converted2XMLSoFar; }
        }
    }

    public delegate void C2XProgressEventHandler(object sender, C2XProgressEventArgs a);

    class C2XProgress
    {
        XmlDocument oXmlDoc;
        List<XmlNode> oXmlNodes;
        C2XProgressEventHandler dProgressEventHandler;

        public C2XProgress(XmlElement oRootElement, C2XProgressEventHandler ProgressEventHandler)
        {
            dProgressEventHandler = ProgressEventHandler;

            oXmlDoc = new XmlDocument();
            XmlDeclaration Decl = oXmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            oXmlDoc.AppendChild(Decl);

            oXmlNodes = new List<XmlNode>();

            XmlElement oXmlElement = oXmlDoc.CreateElement(oRootElement.Prefix, oRootElement.LocalName, oRootElement.NamespaceURI);
            oXmlDoc.AppendChild(oXmlElement);
            oXmlNodes.Add(oXmlElement);
        }

        private XmlElement GetLastXmlElementInList()
        {
            for (int i = oXmlNodes.Count - 1; i >= 0; i--)
                if (oXmlNodes[i] is XmlElement)
                    return oXmlNodes[i] as XmlElement;
            return null;
        }

        private XmlElement GetParentElement(XmlElement oXmlElement)
        {
            bool bFound = false;
            for (int i = oXmlNodes.Count - 1; i >= 0; i--)
            {
                // Find the child element in oXmlNodes
                if (!bFound)
                {
                    if (oXmlNodes[i] == oXmlElement)
                        bFound = true;
                }
                else // Once found, check each XmlElements children for a match
                {
                    if (oXmlNodes[i] is XmlElement)
                    {
                        XmlElement PossibleParent = oXmlNodes[i] as XmlElement;
                        for (int j = PossibleParent.ChildNodes.Count - 1; j >= 0; j--)
                        {
                            if (PossibleParent.ChildNodes[j] == oXmlElement)
                                return PossibleParent;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Add an element to the progress tracking of C2X.
        /// </summary>
        public XmlElement AddElement(string prefix, string localname, string namespaceURI)
        {
            XmlElement oXmlElement = oXmlDoc.CreateElement(prefix, localname, namespaceURI);
            XmlElement oParentXmlElement = GetLastXmlElementInList();
            oParentXmlElement.AppendChild(oXmlElement);
            oXmlNodes.Add(oXmlElement);
            FireProgressEvent();
            return oXmlElement;
        }

        /// <summary>
        /// Add a processing instruction to the progress tracking of C2X.
        /// </summary>
        public void AddPI(string target, string data, XmlElement oXmlElement)
        {
            XmlProcessingInstruction ProcInst = oXmlDoc.CreateProcessingInstruction(target, data);
            //XmlElement oParentXmlElement = GetLastXmlElementInList();
            XmlElement oParentXmlElement = GetParentElement(oXmlElement);
            oParentXmlElement.InsertBefore(ProcInst, oXmlElement);
            //oParentXmlElement.AppendChild(ProcInst);
            //oXmlNodes.Add(ProcInst);
            FireProgressEvent();
        }

        /// <summary>
        /// Add a CDATA to the progress tracking of C2X.
        /// </summary>
        public void AddCDataSection(string NodeValue)
        {
            XmlCDataSection CData = oXmlDoc.CreateCDataSection(NodeValue);
            XmlElement oParentXmlElement = GetLastXmlElementInList();
            oParentXmlElement.AppendChild(CData);
            //oXmlNodes.Add(CData);
            FireProgressEvent();
        }

        /// <summary>
        /// Add a text to the progress tracking of C2X.
        /// </summary>
        public void AddXmlText(string NodeValue)
        {
            XmlText text = oXmlDoc.CreateTextNode(NodeValue);
            XmlElement oParentXmlElement = GetLastXmlElementInList();
            oParentXmlElement.AppendChild(text);
            //oXmlNodes.Add(text);
            FireProgressEvent();
        }

        /// <summary>
        /// To finalise an element is to remove it from tracking as it will not be updated anymore.  It it still possible it will
        /// be removed from the output via RemoveChildrenOfLastXmlElement, as occasionally choices get committed and need to be 
        /// removed.
        /// </summary>
        public void FinaliseElement(XmlElement oXmlElement)
        {
            int index = oXmlNodes.IndexOf(oXmlElement);
            oXmlNodes.RemoveRange(index, oXmlNodes.Count - index);
            FireProgressEvent();
        }

        /// <summary>
        /// Remove an element that we started to add (via AddElement)
        /// </summary>
        public void RemoveElement(XmlElement oXmlElement)
        {
            XmlElement oParentXmlElement = GetParentElement(oXmlElement);
            oParentXmlElement.RemoveChild(oXmlElement);

            int index = oXmlNodes.IndexOf(oXmlElement);
            oXmlNodes.RemoveRange(index, oXmlNodes.Count - index);
            //FireProgressEvent();
        }

        /// <summary>
        /// Remove Count children from the last XmlElement from the internal C2XProgress tracking of elements.
        /// </summary>
        /// <param name="Count"></param>
        public void RemoveChildrenOfLastXmlElement(int Count)
        {
            if(Count <= 0)
                return;

            int index = oXmlNodes.Count - 1;
            while ((index >= 0) && !(oXmlNodes[index] is XmlElement))
                index--;
            if(index >= 0)
            {
                XmlElement oXmlElement = oXmlNodes[index] as XmlElement;
                
                while((Count > 0) && (oXmlElement.ChildNodes.Count > 0))
                {
                    oXmlElement.RemoveChild(oXmlElement.LastChild);
                    Count--;
                }
            }
        }

        private void FireProgressEvent()
        {
            if(null != dProgressEventHandler)
                dProgressEventHandler(null, GenerateC2XPEventArgs());
        }

        private C2XProgressEventArgs GenerateC2XPEventArgs()
        {
            return new C2XProgressEventArgs(oXmlDoc);
        }
    }
}
