using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Schemer.InputHandlers
{
    class InputHandlerFactory
    {
        static public InputHandler GetInputHandler(Fuzzware.Schemas.AutoGenerated.DataInputHandler oInput)
        {
            if (oInput.Item is XMLFileInput)
                return new XMLInput();
            else if (oInput.Item is Fuzzware.Schemas.AutoGenerated.Convert2XMLInput)
                return new Convert2XMLInput();
            else if (oInput.Item is PDMLInput)
                return new PDMLInputHandler();
            else if (oInput.Item is ProtocolDefnInput)
                return new ProtocolDefnInputHandler();
            else if (oInput.Item is UserDefinedCode)
                return new UserInputHandler();
            else
                Log.Write(MethodBase.GetCurrentMethod(), "Unrecognised input choice", Log.LogType.Error);

            return null;
        }
    }
}
