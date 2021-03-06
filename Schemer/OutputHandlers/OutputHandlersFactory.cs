using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Schemer.OutputHandlers
{
    class OutputHandlersFactory
    {
        static public OutputHandler GetOutputHandler(EvaluationMethod oOutputFn)
        {
            if (oOutputFn.Item is FileStore)
                return new OutputToFileHandler();
            else if (oOutputFn.Item is OutputToExe)
                return new OutputToExeHandler();
            else if (oOutputFn.Item is OutputToNetwork)
                return new OutputToNetworkHandler();
            else if (oOutputFn.Item is UserDefinedCode)
                return new ExtensibleOutputHandler();
            else
                Log.Write(MethodBase.GetCurrentMethod(), "Unrecognised output choice", Log.LogType.Error);

            return null;
        }
    }
}
