using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using Fuzzware.Common;
using Fuzzware.Evaluate.Statistics;
using Fuzzware.Schemer.Statistics;

namespace Fuzzware.Schemer
{
    class Program
    {
        static void Copyright()
        {
            Console.WriteLine("Schemer.exe 2009 (c) dave@fuzzware.net");
        }

        static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("     Schemer.exe <XmlConfigFile>");
            Console.WriteLine();
            Console.WriteLine("Where <XmlConfigFile> is an Xml Configuration file that is an instance of Configuration.xsd");
            Console.WriteLine();
        }
        
        static void Main(string[] args)
        {
            try
            {
                Copyright();
                if (0 == args.Length)
                {
                    Usage();
                    Environment.Exit(1);
                }

                Engine engine = new Engine(args[0]);

                // Check if we are just testing to see if everything is configured correctly
                Dispatcher Dispatch = new Dispatcher(engine.ConfigData, engine.PreComp);
                Dispatch.TestDispatch(engine.PreComp.XMLDoc);
                if (engine.ConfigData.Config.testConfig)
                {
                    return;
                }

                // FUZZ!!!!!!!!!!!!!!!!!!!
                engine.Fuzz(Dispatch);
            }
            catch(Exception e)
            {
                try
                {
                    if (!(e is LoggedException))
                        Log.Write(e);
                }
                catch {}
                Log.Write(MethodBase.GetCurrentMethod(), "Graceful exit on unhandled exception", Log.LogType.Status);
            }

            FuzzerStats.LogFuzzerStats();
            OutputStats.LogOutputStats();
        }       
    }
}
