using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Fuzzware.Extensible;

namespace XPSHandlers
{
    /// <summary>
    /// Expects user data of the form
    ///     &lt;TestcaseOutputDir>...&lt;/TestcaseOutputDir>
    ///     
    /// Given the location to store the test case i.e. TestcaseOutputDir, will save the testcase here in an appropriate
    /// directory structure so the file can be added back into the XPS file.  The resulting XPS file is then returned.
    /// </summary>
    public class XPSPreOutputHandler : IUserPreOutputHandler
    {
        XPSInputHandler oXPSInputHandler;
        String TestcaseOutputDir;

        #region IUserOutputHandler Members

        public void Initialise(System.Xml.XmlNode[] UserData, IUserInputHandler UserInputHandler)
        {
            oXPSInputHandler = (XPSInputHandler)UserInputHandler;
            GetInfoFromXML(UserData);

            // Check that the location of the input xml is not the same as the location of the output xml
            if (oXPSInputHandler.GetXMLPathAndFile().Equals(Path.Combine(TestcaseOutputDir, oXPSInputHandler.XMLPathAndFilenameInXPS)))
                throw new ExtensibilityException("The XML output location is the same as the input XML location, this will cause the reference input to be overwritten.  The output location needs to be changed.");
        }

        public MemoryStream Output(MemoryStream TestCase)
        {
            // To update a file at a certain place in the zip file, we need a matching dir structure
            String StoredCurDir = Environment.CurrentDirectory;
            Environment.CurrentDirectory = TestcaseOutputDir;
            if(!String.IsNullOrEmpty(Path.GetDirectoryName(oXPSInputHandler.XMLPathAndFilenameInXPS)))
                Directory.CreateDirectory(Path.GetDirectoryName(oXPSInputHandler.XMLPathAndFilenameInXPS));

            // Write memory stream to file
            String PathAndFile = Path.Combine(TestcaseOutputDir, oXPSInputHandler.XMLPathAndFilenameInXPS);
            try
            {
                using (FileStream fs = new FileStream(PathAndFile, FileMode.Create, FileAccess.Write))
                {
                    TestCase.WriteTo(fs);
                    fs.Flush();
                }
            }
            catch
            {
                throw new ExtensibilityException("Error writing to file '" + PathAndFile + "'");
            }

            // Copy the XPS file
            String NewXPSPathAndFilename = Path.Combine(TestcaseOutputDir, "testcase.xps");
            File.Copy(oXPSInputHandler.XPSPathAndFilename, NewXPSPathAndFilename, true);

            // Update the XPS copy with the new file
            Process ZipExe = new Process();
            ZipExe.StartInfo.FileName = oXPSInputHandler.ZipExePath;
            ZipExe.StartInfo.Arguments = "u \"" + NewXPSPathAndFilename + "\" \"" + oXPSInputHandler.XMLPathAndFilenameInXPS + "\"";
            ZipExe.StartInfo.UseShellExecute = false;
            ZipExe.Start();
            while (!ZipExe.HasExited)
            {
                Thread.Sleep(500);
            }

            // If we created a dir, delete it
            if(-1 != oXPSInputHandler.XMLPathAndFilenameInXPS.IndexOf(Path.DirectorySeparatorChar) )
            {
                String NewDir = oXPSInputHandler.XMLPathAndFilenameInXPS.Substring(0, oXPSInputHandler.XMLPathAndFilenameInXPS.IndexOf(Path.DirectorySeparatorChar));
                try
                {
                    Directory.Delete(NewDir, true);
                }
                catch (Exception)
                {
                    // pause and re-try
                    Thread.Sleep(1000);
                    Directory.Delete(NewDir, true);
                }
            }
            Environment.CurrentDirectory = StoredCurDir;

            MemoryStream XPSFileMemStream = null;
            // Read in the XPS file
            try
            {
                using (FileStream fs = new FileStream(NewXPSPathAndFilename, FileMode.Open, FileAccess.Read))
                {
                    byte[] temp = new byte[fs.Length];
                    fs.Read(temp, 0, (int)fs.Length);
                    XPSFileMemStream = new MemoryStream(temp); 
                }
            }
            catch
            {
                throw new ExtensibilityException("Error reading file '" + NewXPSPathAndFilename + "'");
            }
            
            return XPSFileMemStream;
        }

        #endregion

        private void GetInfoFromXML(XmlNode[] UserData)
        {
            XmlElement Ele = UserData[0] as XmlElement;
            XPathNavigator Nav = Ele.CreateNavigator();

            // Get the test case output dir
            if (Nav.Name != "TestcaseOutputDir")
            {
                throw new ExtensibilityException("Expected 'TestcaseOutputDir', got '" + Nav.Name + "'");
            }
            TestcaseOutputDir = Path.GetFullPath(Nav.Value);
            if (!Directory.Exists(TestcaseOutputDir))
            {
                Directory.CreateDirectory(TestcaseOutputDir);
                if (!Directory.Exists(TestcaseOutputDir))
                    throw new ExtensibilityException("Could not create directory '" + TestcaseOutputDir + "'");
            }
        }
    }
}
