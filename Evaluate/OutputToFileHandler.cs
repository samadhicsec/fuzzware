using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Evaluate;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Convert2XML;

namespace Fuzzware.Evaluate
{
    public class OutputToFileHandler: OutputHandler
    {
        FileStore oFileStore;

        public override void Initialise(object Settings, InputHandler oInputHandler)
        {
            if (!(Settings is FileStore))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'OutputFile', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            oFileStore = Settings as FileStore;
        }

        public override bool Output(System.IO.MemoryStream XMLMemoryStream, string StateDesc)
        {
            if (null == XMLMemoryStream)
                Log.Write(MethodBase.GetCurrentMethod(), "The input MemoryStream was null", Log.LogType.Error);
            if (null == StateDesc)
                Log.Write(MethodBase.GetCurrentMethod(), "The input State was null", Log.LogType.Error);

            // Write the XML out to file
            String PathAndFile = CreateOutputFilePathString(oFileStore.Directory, StateDesc, oFileStore.FileExtension);

            // Open and output to file
            //try
            //{
            //    using (FileStream fs = new FileStream(PathAndFile, FileMode.Create, FileAccess.Write))
            //    {
            //        XMLMemoryStream.WriteTo(fs);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Write(MethodBase.GetCurrentMethod(), "An error occurred writing the testcase to file for state '" + StateDesc + "'.  Skipping." +
            //        Environment.NewLine + e.Message, Log.LogType.Warning);
            //}
            WriteToFile(XMLMemoryStream, PathAndFile, StateDesc);
            return true;
        }
    }
}
