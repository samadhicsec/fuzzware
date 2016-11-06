using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Fuzzware.Extensible
{
    /// <summary>
    /// User code that implements this interface can be called from Schemer giving the user precise control on the test cases passed 
    /// to the output.
    /// </summary>
    public interface IUserPreOutputHandler
    {
        /// <summary>
        /// Called to pass the user code the array of System.Xml.XmlNode's that the user entered into the Configuration XML file.
        /// </summary>
        /// <param name="UserData">The UserDefinedData XML nodes specified in the PreOutput section of the Configuration XML file</param>
        /// <param name="UserInputHandler">The IUserInputHandler if one was specified, otherwise null</param>
        void Initialise(XmlNode[] UserData, IUserInputHandler UserInputHandler);

        /// <summary>
        /// Called to pass the current fuzzed test case.
        /// </summary>
        /// <param name="XMLMemoryStream">Contains the current test case</param>
        /// <returns>A MemoryStream containing the test case to pass to the output</returns>
        MemoryStream Output(MemoryStream TestCase);
    }
}
