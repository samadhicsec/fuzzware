using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;

namespace Fuzzware.Schemer.Statistics
{
    class FuzzerStats
    {
        public static int CountOfSchemaTypes;
        public static int CountOfSchemaTypesWithNoExample;
        public static int CountOfXMLNodes;
        public static int CountOfXMLNodesExcluded;
        protected static uint CountOfPossibleTestCases;
        protected static uint CountOfSkippedTestCases;
        public static uint CountOfVerifyFailureSkippedTestCases;

        public static void InitialiseFuzzerStats()
        {
            
        }

        public static void AddToCountOfPossibleTestCases()
        {
            CountOfPossibleTestCases++;
        }

        public static void SubtractFromCountOfPossibleTestCases()
        {
            CountOfPossibleTestCases--;
        }

        public static void AddToCountOfSkippedTestCases()
        {
            CountOfSkippedTestCases++;
        }

        public static void LogFuzzerStats()
        {
            StringBuilder Stats = new StringBuilder();
            Stats.AppendLine(); 
            Stats.AppendLine("Fuzzer Statistics");
            Stats.AppendLine();
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Number Of Schema Types", CountOfSchemaTypes));
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Encountered Schema Types Without Examples", CountOfSchemaTypesWithNoExample));
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Sum Of Discovered XML Nodes", CountOfXMLNodes));
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Sum Of XML Nodes Excluded", CountOfXMLNodesExcluded));
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Sum Of Possible Test Cases", CountOfPossibleTestCases));
            Stats.AppendLine(String.Format("{0, -48}:{1}", "Sum Of Skipped Test Cases", CountOfSkippedTestCases));
            if(CountOfVerifyFailureSkippedTestCases > 0)
                Stats.AppendLine(String.Format("{0, -48}:{1}", "Sum Of Verification Failure Test Cases", CountOfVerifyFailureSkippedTestCases));

            Log.Write(MethodBase.GetCurrentMethod(), Stats.ToString(), Log.LogType.Info);
        }
    }
}
