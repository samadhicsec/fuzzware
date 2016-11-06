using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Fuzzware.Extensible
{
    /// <summary>
    /// Implmenting this interface allows user code to be run that will return the XML file and its associated XML Schema files.
    /// </summary>
    public interface IUserInputHandler
    {
        /// <summary>
        /// Called to pass the user code the array of System.Xml.XmlNode's that the user entered into the configuration XML file.
        /// </summary>
        /// <param name="UserData">Any user defined XML nodes specified in the UserDefinedCode from the input configuration XML</param>
        void Initialise(XmlNode[] UserData);

        /// <summary>
        /// Should return a path (absolute or relative to the Project directory) to the XML Schema files for the input XML file.
        /// </summary>
        /// <returns></returns>
        String[] GetXSDPathAndFile();

        /// <summary>
        /// Should return a path (absolute or relative to the Project directory) to the input XML file.
        /// </summary>
        /// <returns></returns>
        String GetXMLPathAndFile();
    }
}
