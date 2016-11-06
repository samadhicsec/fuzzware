using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Fuzzware.Extensible
{
    /// <summary>
    /// User code that implements this interface can be called by Evaluate giving the user precise control on the output of test cases.
    /// </summary>
    public interface IUserOutputHandler
    {
        /// <summary>
        /// Called to pass the user code the array of System.Xml.XmlNode's that the user entered into the configuration XML file.
        /// </summary>
        /// <param name="UserData">The UserDefinedData XML nodes specified in the UserDefinedPreOutput section of the configuration XML file</param>
        /// <param name="UserInputHandler">The IUserInputHandler if one was specified, otherwise null</param>
        void Initialise(XmlNode[] UserData, IUserInputHandler UserInputHandler);

        /// <summary>
        /// Called to output the current test case.
        /// </summary>
        /// <param name="XMLMemoryStream">Contains the current test case</param>
        /// <param name="StateDesc">Contains a description of the state</param>
        /// <returns>true to continue, false to stop</returns>
        bool Output(MemoryStream TestCase, string StateDesc);
    }
}
