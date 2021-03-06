using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Common.UserCode;
using Fuzzware.Extensible;
using Fuzzware.Convert2XML;
using Fuzzware.Evaluate;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Schemer.PreOutputHandlers
{
    class ExtensiblePreOutputHandler : PreOutputHandler
    {
        UserDefinedCode oUserDefinedCode;
        IUserPreOutputHandler oIUserPreOutputHandler;
        IUserInputHandler oIUserInputHandler = null;

        public override void Initialise(object Settings, InputHandler oInputHandler, OutputHandler oOutputHandler)
        {
            if (!(Settings is UserDefinedCode))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'UserDefinedCode', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            oUserDefinedCode = Settings as UserDefinedCode;

            oIUserPreOutputHandler = CodeLoader.LoadUserCode<IUserPreOutputHandler>(oUserDefinedCode);

            if(oInputHandler is UserInputHandler)
                oIUserInputHandler = oInputHandler.UserInputHandlerInterface;

            try
            {
                oIUserPreOutputHandler.Initialise(oUserDefinedCode.UserDefinedData as XmlNode[], oIUserInputHandler);
            }
            catch (ExtensibilityException e)
            {
                Log.Write(MethodBase.GetCurrentMethod(), "An error occurred in '" + e.Source + "':\nMessage\n   " + e.Message
                    + "\nStackTrace\n" + e.StackTrace, Log.LogType.Error);
            }
        }

        public override MemoryStream Output(MemoryStream TestCase)
        {
            MemoryStream ret = null;
            try
            {
                ret = oIUserPreOutputHandler.Output(TestCase);
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
