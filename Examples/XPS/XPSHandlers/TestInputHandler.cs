using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Fuzzware.Extensible;

namespace XPSHandlers
{
    /// <summary>
    /// This is a test implementation of IUserInputHandler that mimics XMLFileInput (the 'XML Input' selection in Fuzzsaw)
    /// </summary>
    class TestInputHandler : IUserInputHandler
    {
        String XMLPathAndFilename;
        String[] XSDPathAndFileName;

        #region IUserInputHandler Members

        public void Initialise(System.Xml.XmlNode[] UserData)
        {
            // Get the xml path
            XmlElement Ele = UserData[0] as XmlElement;

            XPathNavigator Nav = Ele.CreateNavigator();

            if (Nav.Name != "XMLPathAndFilename")
            {
                throw new ExtensibilityException("Expected 'XMLPathAndFilename', got '" + Nav.Name + "'");
            }

            XMLPathAndFilename = Nav.Value;

            XSDPathAndFileName = new string[UserData.Length - 1];
            for (int i = 0; i < XSDPathAndFileName.Length; i++)
            {
                if (Nav.MoveToNext())
                {
                    XSDPathAndFileName[i] = Nav.Value;
                }
            }
        }

        public string GetXMLPathAndFile()
        {
            return XMLPathAndFilename;
        }

        public string[] GetXSDPathAndFile()
        {
            return XSDPathAndFileName;
        }

        #endregion
    }
}
