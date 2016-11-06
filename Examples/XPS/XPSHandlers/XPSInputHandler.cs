using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using Fuzzware.Extensible;

namespace XPSHandlers
{
    /// <summary>
    /// Expects user data of the form
    ///     &lt;ZipExePath>...&lt;/ZipExePath>
    ///     &lt;XPSPathAndFilename>...&lt;/XPSPathAndFilename>
    ///     &lt;XMLPathAndFilenameInXPS>...&lt;/XMLPathAndFilenameInXPS>
    ///     &lt;OutputXMLPathAndFilename>...&lt;/OutputXMLPathAndFilename>
    ///     &lt;XSDPathAndFilename>...&lt;/XSDPathAndFilename> (can be many)
    /// 
    /// Will extract the desired XML file from the XPS file and copy it to the specified location.
    /// </summary>
    public class XPSInputHandler : IUserInputHandler 
    {
        public String ZipExePath;
        public String XPSPathAndFilename;
        public String XMLPathAndFilenameInXPS;
        public String OutputXMLPathAndFilename;
        public String[] XSDPathAndFilename;

        #region IUserInputHandler Members

        /// <summary>
        /// Using ZipExePath, it will extract from XPSPathAndFile the file XMLPathAndFileInXPS and copy it to OutputXMLPathAndFile 
        /// (over-writing anything there).
        /// </summary>
        /// <param name="UserData"></param>
        public void Initialise(System.Xml.XmlNode[] UserData)
        {
            // Retrieve user data from XML
            GetInfoFromXML(UserData);

            // Get zip Process
            Process ZipExe = new Process();

            // Delete any existing file at target location
            File.Delete(OutputXMLPathAndFilename);

            // Change current dir
            String StoredCurDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(OutputXMLPathAndFilename);

            // Setup call to Winrar
            ZipExe.StartInfo.FileName = ZipExePath;
            ZipExe.StartInfo.Arguments = "e \"" + XPSPathAndFilename + "\" \"" + XMLPathAndFilenameInXPS + "\"";
            ZipExe.StartInfo.UseShellExecute = false;
            // Extract the file
            ZipExe.Start();
            // Pause to let it do its thing
            while(!ZipExe.HasExited)
                Thread.Sleep(500);
            // Check it worked
            if (!File.Exists(Path.GetFileName(XMLPathAndFilenameInXPS)))
                throw new ExtensibilityException("Extraction failed, no '" + Path.GetFileName(XMLPathAndFilenameInXPS) + "' at '" + Environment.CurrentDirectory + "'");
            // Move output file
            File.Move(Path.GetFileName(XMLPathAndFilenameInXPS), Path.GetFileName(OutputXMLPathAndFilename));
            
            Environment.CurrentDirectory = StoredCurDir;
        }

        /// <summary>
        /// Returns OutputXMLPathAndFile
        /// </summary>
        /// <returns></returns>
        public string GetXMLPathAndFile()
        {
            return OutputXMLPathAndFilename;
        }

        /// <summary>
        /// Returns XSDPathAndFile
        /// </summary>
        /// <returns></returns>
        public string[] GetXSDPathAndFile()
        {
            return XSDPathAndFilename;
        }

        #endregion

        private void GetInfoFromXML(XmlNode[] UserData)
        {
            XmlElement Ele = UserData[0] as XmlElement;
            XPathNavigator Nav = Ele.CreateNavigator();

            // Get the zip exe path
            if (Nav.Name != "ZipExePath")
            {
                throw new ExtensibilityException("Expected 'ZipExePath', got '" + Nav.Name + "'");
            }
            ZipExePath = Path.GetFullPath(Nav.Value);
            if(!File.Exists(ZipExePath))
                throw new ExtensibilityException("No file at '" + Path.GetFullPath(ZipExePath) + "'");

            if (!Nav.MoveToNext())
                throw new ExtensibilityException("Run out of nodes");

            // Get the XPS path
            if (Nav.Name != "XPSPathAndFilename")
            {
                throw new ExtensibilityException("Expected 'XPSPathAndFilename', got '" + Nav.Name + "'");
            }
            XPSPathAndFilename = Path.GetFullPath(Nav.Value);
            if (!File.Exists(XPSPathAndFilename))
                throw new ExtensibilityException("No file at '" + Path.GetFullPath(XPSPathAndFilename) + "'");

            if (!Nav.MoveToNext())
                throw new ExtensibilityException("Run out of nodes");

            // Get the XML path in the XPS
            if (Nav.Name != "XMLPathAndFilenameInXPS")
            {
                throw new ExtensibilityException("Expected 'XMLPathAndFilenameInXPS', got '" + Nav.Name + "'");
            }
            XMLPathAndFilenameInXPS = Nav.Value;

            if (!Nav.MoveToNext())
                throw new ExtensibilityException("Run out of nodes");

            // Get the output XML path
            if (Nav.Name != "OutputXMLPathAndFilename")
            {
                throw new ExtensibilityException("Expected 'OutputXMLPathAndFilename', got '" + Nav.Name + "'");
            }
            OutputXMLPathAndFilename = Path.GetFullPath(Nav.Value);
            if (!Directory.Exists(Path.GetDirectoryName(OutputXMLPathAndFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputXMLPathAndFilename));
                if (!Directory.Exists(Path.GetDirectoryName(OutputXMLPathAndFilename)))
                    throw new ExtensibilityException("Could not create directory '" + Path.GetDirectoryName(OutputXMLPathAndFilename) + "'");
            }

            // Get the XPS XSD files
            XSDPathAndFilename = new string[UserData.Length - 4];
            for (int i = 0; i < XSDPathAndFilename.Length; i++)
            {
                if (Nav.MoveToNext())
                {
                    XSDPathAndFilename[i] = Nav.Value;
                    if (!File.Exists(XSDPathAndFilename[i]))
                        throw new ExtensibilityException("Could not find XSD file '" + XSDPathAndFilename[i] + "'");
                }
            }
        }
    }
}
