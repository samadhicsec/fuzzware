using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.ConvertFromXML;
using Fuzzware.Evaluate;
using Fuzzware.Convert2XML;

namespace Fuzzware.Schemer.PreOutputHandlers
{
    public class ProtocolPreOutputHandler : PreOutputHandler
    {
        OutputSettings oOutputSettings;
        ProtocolDefnInputHandler oProtocolInputHandler; 
        OutputToNetworkHandler oNetworkOutputHandler;

        public override void Initialise(object Settings, InputHandler oInputHandler, OutputHandler oOutputHandler)
        {
            if(!(Settings is OutputSettings))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'OutputSettings', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            oOutputSettings = Settings as OutputSettings;

            if(!(oInputHandler is ProtocolDefnInputHandler))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected oInputHandler object of type 'ProtocolDefnInputHandler', got '" + oInputHandler.GetType().ToString() + "'", Log.LogType.Error);

            oProtocolInputHandler = oInputHandler as ProtocolDefnInputHandler;

            if (!(oOutputHandler is OutputToNetworkHandler))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected oOutputHandler object of type 'OutputToNetworkHandler', got '" + oOutputHandler.GetType().ToString() + "'", Log.LogType.Error);

            oNetworkOutputHandler = oOutputHandler as OutputToNetworkHandler;
        }

        // If we are fuzzing a protocol, then we need to run the protocol to the message we are fuzzing.
        public override MemoryStream Output(MemoryStream XMLMemoryStream)
        {
            if (!oNetworkOutputHandler.RunProtocol(oProtocolInputHandler.ProtocolDefn, oOutputSettings))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Specified protocol did not run correctly", Log.LogType.Error);
            }
            return XMLMemoryStream;
        }
    }
}
