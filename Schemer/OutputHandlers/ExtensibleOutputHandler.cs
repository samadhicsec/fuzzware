using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;
using Fuzzware.Common;
using Fuzzware.Common.UserCode;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Convert2XML;
using Fuzzware.Extensible;

namespace Fuzzware.Schemer.OutputHandlers
{
    public class ExtensibleOutputHandler : OutputHandler
    {
        UserDefinedCode oUserDefinedCode;
        IUserInputHandler oIUserInputHandler;
        IUserOutputHandler oIUserOutputHandler;

        public override void Initialise(object Settings, InputHandler oInputHandler)
        {
            oIUserInputHandler = oInputHandler.UserInputHandlerInterface;

            if (!(Settings is UserDefinedCode))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'UserDefinedCode', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            oUserDefinedCode = Settings as UserDefinedCode;

            oIUserOutputHandler = CodeLoader.LoadUserCode<IUserOutputHandler>(oUserDefinedCode);

            // Initialise the output
            try
            {
                // Initialise
                oIUserOutputHandler.Initialise(oUserDefinedCode.UserDefinedData as XmlNode[], oIUserInputHandler);
            }
            catch (ExtensibilityException e)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "An error occurred in '" + e.Source + "':\nMessage\n   " + e.Message
                    + "\nStackTrace\n" + e.StackTrace, Log.LogType.Error);
            }
        }

        public override bool Output(MemoryStream XMLMemoryStream, string StateDesc)
        {
            bool ret = false;
            try
            {
                ret = oIUserOutputHandler.Output(XMLMemoryStream, StateDesc);
            }
            catch (ExtensibilityException e)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "An error occurred in '" + e.Source + "':\nMessage\n   " + e.Message
                    + "\nStackTrace\n" + e.StackTrace, Log.LogType.Error);
            }
            return ret;
        }
    }
}
