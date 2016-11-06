using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fuzzware.Convert2XML;
using Fuzzware.Evaluate;

namespace Fuzzware.Schemer.PreOutputHandlers
{
    public abstract class PreOutputHandler
    {
        public abstract void Initialise(object Settings, InputHandler oInputHandler, OutputHandler oOutputHandler);

        public abstract MemoryStream Output(MemoryStream XMLMemoryStream);
    }
}
