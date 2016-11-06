using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Fuzzware.Common.XML;

namespace Fuzzware.Common.MethodInterface
{
    /// <summary>
    /// Randomizes the methods in an instance of methods layed out according to schema created by LibraryDescription 
    /// </summary>
    public class MethodNodeRandomizer
    {
        /// <summary>
        /// Returns a new XmlDocument with the method call order randomized, except for the specified initial methods.  Initial methods
        /// will not appear in the list of methods appearing in random order.
        /// </summary>
        public XmlDocument RandomizeAndFilterMethodCalls(XmlDocument oDoc, string[] oInitialMethods)
        {
            // The array of methods that will be the first used in the script
            XPathNavigator[] oInitialMethodCalls = new XPathNavigator[0];
            // The list of methods that will be randomly added to the script
            List<XPathNavigator> oMethodCalls = new List<XPathNavigator>();

            // Get an XPathNav to the library element
            XPathNavigator oLibraryNav = XMLHelper.GetRootNode(oDoc);
            // Get an XPathNav to the first interface
            XPathNavigator oMethod = oLibraryNav.Clone();
            // Move to the first method
            if (!oMethod.MoveToFirstChild())
                return oDoc;

            // If the user specified a list of initial methods
            List<string> InitialMethodList;
            if (null == oInitialMethods)
                InitialMethodList = new List<string>();
            else
            {
                InitialMethodList = new List<string>(oInitialMethods);
                oInitialMethodCalls = new XPathNavigator[oInitialMethods.Length];
            }

            // Create a list of XPathNavs to all the method calls

            // Loop through all the methods
            do
            {
                String MethodName = oMethod.LocalName;
                bool bAlreadyAdded = false;

                // Check if this method is one of the initial methods we need to use
                for (int i = 0; i < InitialMethodList.Count; i++)
                {
                    if (MethodName.Equals(InitialMethodList[i], StringComparison.CurrentCulture))
                    {
                        // If it is, add it to our list of initial methods, if we haven't added that method already.
                        // If we have already added that method, then drop this method from being called in the random list.
                        if (null == oInitialMethodCalls[i])
                            oInitialMethodCalls[i] = oMethod.Clone();   // Note, we have to add the initial methods in the correct position

                        bAlreadyAdded = true;
                        break;
                    }
                }

                if (!bAlreadyAdded)
                    // Otherwise add it to the list of methods that will be added in random order
                    oMethodCalls.Add(oMethod.Clone());

            }
            while (oMethod.MoveToNext());

            //Create new XmlDocument
            XmlDocument oRandomDoc = new XmlDocument();

            // Add Xml declaration
            XmlDeclaration Decl = oRandomDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            oRandomDoc.AppendChild(Decl);

            XmlElement oLibraryNode = oRandomDoc.CreateElement(oLibraryNav.Prefix, oLibraryNav.LocalName, oLibraryNav.NamespaceURI);

            // Initialise PRNG (we want this to generate the same random order each time)
            Random oRanGen = new Random(0);

            // Add initial method calls
            for (int i = 0; i < oInitialMethodCalls.Length; i++)
            {
                if (null == oInitialMethodCalls[i])
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "An initial method of '" + InitialMethodList[i]
                        + "' was specified but could not be found.  Skipping", Log.LogType.Warning);
                    continue;
                }

                XPathNavigator MethodToAdd = oInitialMethodCalls[i];
                // Import the method
                XmlNode oMethodXmlNode = oRandomDoc.ImportNode(((IHasXmlNode)MethodToAdd).GetNode(), true);
                // Add method to the library
                oLibraryNode.AppendChild(oMethodXmlNode);
            }

            // Add method calls and then remove that method
            while (oMethodCalls.Count > 0)
            {
                int PositionToAdd = oRanGen.Next(oMethodCalls.Count);

                XPathNavigator MethodToAdd = oMethodCalls[PositionToAdd];

                // Import the method
                XmlNode oMethodXmlNode = oRandomDoc.ImportNode(((IHasXmlNode)MethodToAdd).GetNode(), true);
                // Add method to the library
                oLibraryNode.AppendChild(oMethodXmlNode);

                oMethodCalls.RemoveAt(PositionToAdd);
            }

            oRandomDoc.AppendChild(oLibraryNode);
            return oRandomDoc;

        }
    }
}
