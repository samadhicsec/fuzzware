using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.ConvertFromXML.Processors;

namespace Fuzzware.Evaluate.Statistics
{
    class ByteComparer : EqualityComparer<byte[]>
    {
        private uint Tolerance = 0;
        private int NextHashValue;
        private Dictionary<int, byte[]> HashDictionary;

        public ByteComparer(uint Tolerance)
            : base()
        {
            NextHashValue = 0;
            HashDictionary = new Dictionary<int, byte[]>();
            this.Tolerance = Tolerance;
        }

        public override bool Equals(byte[] x, byte[] y)
        {
            uint ToleranceCount = 0;
            if (x.Length != y.Length)
                return false;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    ToleranceCount++;
                    if(ToleranceCount > Tolerance)
                        return false;
                }
                else
                {
                    ToleranceCount = 0;
                }
            }
            return true;
        }

        public override int GetHashCode(byte[] obj)
        {
            if (0 != Tolerance)
            {
                foreach (KeyValuePair<int, byte[]> kvp in HashDictionary)
                {
                    if (this.Equals(obj, kvp.Value))
                        return kvp.Key;
                }
                byte[] objcopy = new byte[obj.Length];
                Array.Copy(obj, objcopy, obj.Length);
                HashDictionary.Add(NextHashValue, objcopy);
                int ret = NextHashValue;
                NextHashValue++;
                return ret;
            }
            else
            {
                uint CRC = CRC32Fn.CRC32_Function(obj);
                return (int)CRC;
            }
        }
    }

    public class OutputStats
    {
        private struct UniqueRespStat
        {
            public String UniqueRespFuzzerState;
            public uint Count;
        }

        private static String DefaultTestCaseName;
        private static Dictionary<byte[], UniqueRespStat> UniqueResponses;
        private static List<string> TestcasesRequiringKillingApp;

        public static void InitialiseOutputStats(String DefaultTestCaseName, uint Tolerance)
        {
            UniqueResponses = new Dictionary<byte[], UniqueRespStat>(new ByteComparer(Tolerance));
            TestcasesRequiringKillingApp = new List<string>();
            OutputStats.DefaultTestCaseName = DefaultTestCaseName;
        }

        public static bool AddResponse(String Response, String State)
        {
            return AddResponse(Encoding.Unicode.GetBytes(Response), State);
        }

        public static bool AddResponse(byte[] Response, String State)
        {
            bool IsUniqueResponse = false;

            if (UniqueResponses.ContainsKey(Response))
            {
                // Not a unique response
                UniqueRespStat urs = UniqueResponses[Response];
                urs.Count = urs.Count + 1;
                UniqueResponses[Response] = urs;
            }
            else
            {
                // A unique response
                UniqueRespStat urs = new UniqueRespStat();
                urs.UniqueRespFuzzerState = String.Copy(State);
                // If the testcase, then start count at zero
                if(State == OutputStats.DefaultTestCaseName)
                    urs.Count = 0;
                else
                    urs.Count = 1;
                UniqueResponses.Add(Response, urs);
                IsUniqueResponse = true;
            }

            return IsUniqueResponse;
        }

        public static void AddTestcaseRequiredKillingApp(String StateDesc)
        {
            TestcasesRequiringKillingApp.Add(StateDesc);
        }

        public static void LogOutputStats()
        {
            if (null == UniqueResponses)
                return;
            if(UniqueResponses.Count == 0)
                return;

            StringBuilder Stats = new StringBuilder();
            Stats.AppendLine();
            Stats.AppendLine("Output Statistics");
            Stats.AppendLine();
            Stats.AppendLine("Unique Responses : " + UniqueResponses.Count);
            Stats.AppendLine(String.Format("{0,-48}{1, -16}", "Example Test Case", "Count"));
            foreach (KeyValuePair<byte[], UniqueRespStat> kvp in UniqueResponses)
            {
                Stats.AppendLine(String.Format("{0,-48}{1, -16}", kvp.Value.UniqueRespFuzzerState, kvp.Value.Count));
            }

            if (TestcasesRequiringKillingApp.Count > 0)
            {
                Stats.AppendLine();
                Stats.AppendLine(String.Format("{0,-48}{1, -16}", "Testcases requiring application killed", TestcasesRequiringKillingApp.Count));
                for (int i = 0; i < TestcasesRequiringKillingApp.Count; i++)
                    Stats.AppendLine(TestcasesRequiringKillingApp[i]);
            }

            Log.Write(MethodBase.GetCurrentMethod(), Stats.ToString(), Log.LogType.Info);
        }
    }
}
