using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Fuzzware.Common;
using Fuzzware.Schemer.Statistics;

namespace Fuzzware.Schemer.Fuzzers
{
    /// <summary>
    /// Contains a group of FuzzerBase's so that multiple types of fuzzers can be combined into one logical fuzzer.
    /// </summary>
    public class FuzzerGroup : IFuzzer
    {
        List<IFuzzer> oFuzzerBases;
        int iCurrentFuzzerIndex;
        int iEndFuzzerIndex;

        public FuzzerGroup()
        {
            oFuzzerBases = new List<IFuzzer>();
        }

        /// <summary>
        /// Adds an IFuzzer to the list of fuzzers in this group
        /// </summary>
        /// <param name="oFuzzerbase">The IFuzzer to add</param>
        public void Add(IFuzzer oFuzzer)
        {
            oFuzzerBases.Add(oFuzzer);
        }

        /// <summary>
        /// Adds a List of  IFuzzers to the list of fuzzers in this group
        /// </summary>
        /// <param name="oFuzzerbase">The IFuzzers to add</param>
        public void Add(List<IFuzzer> oFuzzers)
        {
            oFuzzerBases.AddRange(oFuzzers);
        }

        #region IFuzzer Members

        public String[] Methods()
        {
            List<String> FuzzMethods = new List<string>();
            for (int i = 0; i < oFuzzerBases.Count; i++)
                for(int j = 0; j < oFuzzerBases[i].Methods().Length; j++)
                    FuzzMethods.Add(oFuzzerBases[i].Methods()[j]);

            return FuzzMethods.ToArray();
        }

        /// <summary>
        /// Initialises all of the fuzzers in this group
        /// </summary>
        public void Initialise()
        {
            for (int i = 0; i < oFuzzerBases.Count; i++)
                oFuzzerBases[i].Initialise();

            StringBuilder FuzzerList = new StringBuilder("Fuzzing methods to be used: ");
            String[] FuzzMethods = Methods();
            for (int i = 0; i < FuzzMethods.Length - 1; i++)
                FuzzerList.Append(FuzzMethods[i] + ", ");
            if(FuzzMethods.Length - 1 >= 0)
                FuzzerList.Append(FuzzMethods[FuzzMethods.Length - 1]);
            else
                FuzzerList.Append("None");
            Log.Write(MethodBase.GetCurrentMethod(), FuzzerList.ToString(), Log.LogType.LogOnlyInfo);

            iCurrentFuzzerIndex = 0;
            iEndFuzzerIndex = oFuzzerBases.Count - 1;
        }

        public void Fuzz()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            oFuzzerBases[iCurrentFuzzerIndex].Fuzz();

            // If we just executed the 'Finished' delegate, skip evaluation and end FuzzerBase
            if (oFuzzerBases[iCurrentFuzzerIndex].IsFinished)
            {
                FuzzerStats.SubtractFromCountOfPossibleTestCases();
                throw new SkipStateFuzzerInGroupFinished();
            }
        }

        public void Restore()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            oFuzzerBases[iCurrentFuzzerIndex].Restore();
        }

        public void End()
        {
            if (0 == oFuzzerBases.Count)
                return;
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            oFuzzerBases[iCurrentFuzzerIndex].End();
        }

        #endregion

        #region IFuzzState Members

        public void Next()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            oFuzzerBases[iCurrentFuzzerIndex].Next();

            // Check if the current fuzzer has finished.  
            if (oFuzzerBases[iCurrentFuzzerIndex].IsFinished)
            {
                //Only increment if less then the end fuzzer index. If iCurrentFuzzerIndex == iEndFuzzerIndex then IsFinished will be true
                if(iCurrentFuzzerIndex < iEndFuzzerIndex)
                    iCurrentFuzzerIndex++;
            }
        }

        public void NextFuzzIndex()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            oFuzzerBases[iCurrentFuzzerIndex].NextFuzzIndex();
        }

        public bool NextNodeIndex()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            return oFuzzerBases[iCurrentFuzzerIndex].NextNodeIndex();
        }

        public bool NextFuzzType()
        {
            Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
            return oFuzzerBases[iCurrentFuzzerIndex].NextFuzzType();
        }

        public void SetState(string NodeIndexStr, string TypeStr, uint FuzzIndex)
        {
            // We have to find the fuzzer that has this Type
            for (int i = 0; i < oFuzzerBases.Count; i++)
            {
                string[] FuzzMethods = oFuzzerBases[i].Methods();
                for (int j = 0; j < FuzzMethods.Length; j++)
                {
                    if (FuzzMethods[j].Equals(TypeStr, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Set its state
                        oFuzzerBases[i].SetState(NodeIndexStr, TypeStr, FuzzIndex);
                        iCurrentFuzzerIndex = i;
                        return;
                    }
                }
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Setting state failed, unable to locate a fuzzer in the fuzzer group with a FuzzType of '" + TypeStr + "', ignoring", Log.LogType.Warning);
        }

        public void SetEndState(string NodeIndexStr, string TypeStr, uint FuzzIndex)
        {
            // We have to find the fuzzer that has this Type
            for (int i = 0; i < oFuzzerBases.Count; i++)
            {
                string[] FuzzMethods = oFuzzerBases[i].Methods();
                for (int j = 0; j < FuzzMethods.Length; j++)
                {
                    if (FuzzMethods[j].Equals(TypeStr, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Set its end state
                        oFuzzerBases[i].SetEndState(NodeIndexStr, TypeStr, FuzzIndex);
                        // Note which fuzzer is the last one
                        iEndFuzzerIndex = i;
                        return;
                    }
                }
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Setting end state failed, unable to locate a fuzzer in the fuzzer group with a FuzzType of '" + TypeStr + "', ignoring", Log.LogType.Warning);
        }

        public bool IsFinished
        {
            get 
            {
                if (iCurrentFuzzerIndex >= iEndFuzzerIndex)
                {
                    // Three scenarios
                    // - There are no fuzzers in this group
                    // - The end FuzzerBase.Next moved the fuzzer type to 'Finished'
                    // - The end FuzzerBase has an end state and the last Next call set IsFinished = true
                    if ((0 == oFuzzerBases.Count) ||
                        (oFuzzerBases[iEndFuzzerIndex].Type.Equals("Finished", StringComparison.CurrentCultureIgnoreCase)) ||
                        (oFuzzerBases[iEndFuzzerIndex].IsFinished))
                        return true;
                }
                return false;
            }
        }

        public int FuzzIndex
        {
            get
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                return oFuzzerBases[iCurrentFuzzerIndex].FuzzIndex;
            }
            set
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                oFuzzerBases[iCurrentFuzzerIndex].FuzzIndex = value;
            }
        }

        public int NodeIndex
        {
            get
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                return oFuzzerBases[iCurrentFuzzerIndex].NodeIndex;
            }
            set
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                oFuzzerBases[iCurrentFuzzerIndex].NodeIndex = value;
            }
        }

        public string Type
        {
            get
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                return oFuzzerBases[iCurrentFuzzerIndex].Type;
            }
            set
            {
                Debug.Assert((iCurrentFuzzerIndex >= 0) && (iCurrentFuzzerIndex < oFuzzerBases.Count));
                oFuzzerBases[iCurrentFuzzerIndex].Type = value;
            }
        }

        #endregion
    }
}
