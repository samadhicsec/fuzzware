using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common;
using Fuzzware.Common.DataSchema;
using Fuzzware.Common.XML;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Schemer
{
    class SkipStateException : Exception
    {
        public SkipStateException()
            : base()
        {

        }
    }

    class SkipStateOutOfRangeException : SkipStateException
    {
        public SkipStateOutOfRangeException()
            : base()
        {

        }
    }

    class SkipStateNoAllCaseException : SkipStateException
    {
        public SkipStateNoAllCaseException()
            : base()
        {

        }
    }
    
    //public enum FuzzIndexProgressionType
    //{
    //    Incremental,
    //    Random
    //}

    interface IFuzzerState
    {
        Mutex ControlMutex
        {
            get;
        }

        bool InitialiseForFuzzing();

        void NextState();

        String ToString();

        //void InitRandom(int Seed);

        bool Finished
        {
            get;
            set;
        }

        bool Skip
        {
            get;
            set;
        }

        ElementDBEntry CurrentSchemaElement
        {
            get;
        }

        int CountOfCurrentNode
        {
            get;
        }

        String Category
        {
            set;
            get;
        }

        int FuzzIndex
        {
            get;
            set;
        }

        int NodeIndex
        {
            get;
            set;
        }

        bool EndState
        {
            get;
        }

        //FuzzIndexProgressionType FuzzIndexProgression
        //{
        //    get;
        //    set;
        //}
    }

    /// <summary>
    /// The purpose of this class is to keep track of, and be able to set, the state of the fuzzer
    /// </summary>
    class State : IFuzzerState
    {
        private ConfigData oConfigData;
        private PreCompData oPreComp;

        // Allow the fuzzing to be controlled
        public Mutex oControlMutex;

        public static String DefaultTestcaseName = "OutputWithoutFuzzing";
        // Keep track the of the schema element we are fuzzing
        private int ElementNodeListIndex;
        // The state should know the data pointers being fuzzed so it can get the unique node index to tell the user.
        private XPathNodeList oNodeList;

        // Keep track of the nodes of a particular type we are fuzzing
        private int TotalNodesOfCurrentType;
        private int iNodeIndex;
        
        // Keep track of particular fuzzer being used
        private String CurrentCategory;
        private int iFuzzIndex;
        private bool FinishedState;
        private bool SkipThisState;
        private bool bEndState;
        //FuzzIndexProgressionType eFuzzIndexProgression;
        //private Random PRNG;

        // Have access to configuration file info
        Configuration Config;
        int StartStateNodeListIndex;
        int EndStateNodeListIndex;
        int EndStateNodeIndex;
        bool StartStateSpecified;

        // Fuzzing attribute is special case
        bool bFuzzingAttribute;
        String sAttributeName;

        // Record a state history
        public struct StateHistoryEntry
        {
            public DateTime Time;
            public XmlQualifiedName Node;
            public int NodeIndex;
            public int FuzzIndex;
            public String StateDesc;
        }
        List<StateHistoryEntry> oHistory;

        public State(ConfigData oConfigData, PreCompData oPreComp)
        {
            this.oConfigData = oConfigData;
            this.oPreComp = oPreComp;

            oControlMutex = new Mutex();
            oHistory = new List<StateHistoryEntry>();
            bEndState = false;

            //eFuzzIndexProgression = FuzzIndexProgressionType.Incremental;
            ElementNodeListIndex = -1;

            Config = oConfigData.Config;

            // We should be able to read the state in here from configuration files.
            if (Config.input.StartState != null)
            {
                // Get the namespace
                String NS = "";
                foreach (KeyValuePair<String, String> NSPrefixKVP in oPreComp.NamespacePrefixDict)
                {
                    if (Config.input.StartState.NodeNamespacePrefix == NSPrefixKVP.Value)
                    {
                        if (!String.IsNullOrEmpty(NSPrefixKVP.Key))
                        {
                            NS = NSPrefixKVP.Key;
                            break;
                        }
                    }
                }
                //if (String.IsNullOrEmpty(NS))
                //    Log.Write(MethodBase.GetCurrentMethod(), "Start state node prefix'" + Config.input.EndState.NodeNamespacePrefix + "' could not be found", Log.LogType.Warning);

                // Set the initial node
                XmlQualifiedName QName = new XmlQualifiedName(Config.input.StartState.NodeName, NS);
                // Try to find the element identifier
                if ((oPreComp.ElementDB[QName]).Length == 0)
                    Log.Write(MethodBase.GetCurrentMethod(), "No nodes of name " + QName.ToString() + " were found to use as the specified start node", Log.LogType.Error);
                XMLElementIdentifier QNameId = oPreComp.ElementDB[QName][0].ElementId;
                if (!String.IsNullOrEmpty(NS))
                {
                    StartStateNodeListIndex = oPreComp.ElementNodeList.FindIndex(
                        delegate(XMLElementIdentifier target) { if (target.CompareTo(QNameId) == 0) return true; else return false; });
                }
                else
                {
                    //  Search for element by trying all namespaces
                    foreach (KeyValuePair<String, String> NSPrefixKVP in oPreComp.NamespacePrefixDict)
                    {
                        QName = new XmlQualifiedName(Config.input.StartState.NodeName, NSPrefixKVP.Key);
                        if ((oPreComp.ElementDB[QName]).Length != 0)
                        {
                            QNameId = oPreComp.ElementDB[QName][0].ElementId;
                            if (-1 != (StartStateNodeListIndex = oPreComp.ElementNodeList.FindIndex(
                                delegate(XMLElementIdentifier target) { if (target.CompareTo(QNameId) == 0) return true; else return false; })))
                                break;
                        }
                    }
                }
                if (-1 == StartStateNodeListIndex)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "Start state node '" + QName + "' could not be found, ignoring", Log.LogType.Warning);
                    StartStateNodeListIndex = 0;
                }
                // Set the start node list index
                ElementNodeListIndex = StartStateNodeListIndex;
                StartStateSpecified = true;     // This makes sure we don't increment when we begin fuzzing
                    
                // Set which node in the XML, of the current schema type we are fuzzing, to start fuzzing at
                if (Config.input.StartState.NodeIndex == "All")
                {
                    iNodeIndex = -1;
                }
                else
                {
                    uint num = 0;
                    if (UInt32.TryParse(Config.input.StartState.NodeIndex, out num))
                    {
                        iNodeIndex = (int)num;
                    }
                    else
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not set start node index, could not parse '" + Config.input.StartState.NodeIndex + "' as a UInt32", Log.LogType.Warning);
                        iNodeIndex = -1;
                    }
                }
                // Set the type of fuzzer to use
                CurrentCategory = Config.input.StartState.FuzzCategory;
                // Set the initial fuzzer case to try
                iFuzzIndex = (int)Config.input.StartState.FuzzIndex;

                // Set the node list, as this is needed if we set the start element (the State.ToString() needs it to print out the start node)
                oNodeList = XMLHelper.SelectNodesOfType(CurrentElementId, oPreComp.XMLDoc, oPreComp.NamespacePrefixDict);
            }

            // Record the end state node name
            EndStateNodeListIndex = 0;
            if (Config.input.EndState != null)
            {
                // Get the namespace
                String NS = "";
                foreach (KeyValuePair<String, String> NSPrefixKVP in oPreComp.NamespacePrefixDict)
                {
                    if (Config.input.EndState.NodeNamespacePrefix == NSPrefixKVP.Value)
                    {
                        if (!String.IsNullOrEmpty(NSPrefixKVP.Key))
                        {
                            NS = NSPrefixKVP.Key;
                            break;
                        }
                    }
                }
                //if (String.IsNullOrEmpty(NS))
                //    Log.Write(MethodBase.GetCurrentMethod(), "End state node prefix'" + Config.input.EndState.NodeNamespacePrefix + "' could not be found", Log.LogType.Warning);

                // Record the final node
                XmlQualifiedName QName = new XmlQualifiedName(Config.input.EndState.NodeName, NS);
                // Try to find the element identifier
                if ((oPreComp.ElementDB[QName]).Length == 0)
                    Log.Write(MethodBase.GetCurrentMethod(), "No nodes of name " + QName.ToString() + " were found to use as the specified end node", Log.LogType.Error);
                XMLElementIdentifier QNameId = oPreComp.ElementDB[QName][0].ElementId;
                if (!String.IsNullOrEmpty(NS))
                {
                    EndStateNodeListIndex = oPreComp.ElementNodeList.FindIndex(
                        delegate(XMLElementIdentifier target) { if (target.CompareTo(QNameId) == 0) return true; else return false; });
                }
                else
                {
                    //  Search for element by trying all namespaces
                    foreach (KeyValuePair<String, String> NSPrefixKVP in oPreComp.NamespacePrefixDict)
                    {
                        QName = new XmlQualifiedName(Config.input.StartState.NodeName, NSPrefixKVP.Key);
                        if ((oPreComp.ElementDB[QName]).Length != 0)
                        {
                            QNameId = oPreComp.ElementDB[QName][0].ElementId;
                            if (-1 != (EndStateNodeListIndex = oPreComp.ElementNodeList.FindIndex(
                                delegate(XMLElementIdentifier target) { if (target.CompareTo(QNameId) == 0) return true; else return false; })))
                                break;
                        }
                    }
                }
                if (-1 == EndStateNodeListIndex)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "End state node '" + QName + "' could not be found, ignoring", Log.LogType.Warning);
                    EndStateNodeListIndex = oPreComp.ElementNodeList.Count;
                }

                // Set which node in the XML, of the current schema type we are fuzzing, to end fuzzing at
                if (Config.input.EndState.NodeIndex == "All")
                {
                    EndStateNodeIndex = -1;
                }
                else
                {
                    uint num = 0;
                    if (UInt32.TryParse(Config.input.EndState.NodeIndex, out num))
                    {
                        EndStateNodeIndex = (int)num;
                    }
                    else
                    {
                        Log.Write(MethodBase.GetCurrentMethod(), "Could not set end node index, could not parse '" + Config.input.EndState.NodeIndex + "' as a UInt32", Log.LogType.Warning);
                        EndStateNodeIndex = -1;
                    }
                }

                RecordHistory();
            }
        }

        public bool MoveToNextSchemaElement()
        {
            // Special case for when a start state is specified, so we can print out the start state before entering the fuzzing loop
            if (StartStateSpecified)
            {
                StartStateSpecified = false;
                return true;
            }

            if (ElementNodeListIndex < oPreComp.ElementNodeList.Count - 1)
            {
                ElementNodeListIndex++;
                // Check for the end state condition
                if ((Config.input.EndState != null) && (ElementNodeListIndex > EndStateNodeListIndex))
                {
                    bEndState = true;
                    return false;
                }
                return true;
            }
            return false;
        }

        public ElementDBEntry CurrentSchemaElement
        {
            get
            {
                if ((ElementNodeListIndex >= 0) && (ElementNodeListIndex < oPreComp.ElementNodeList.Count))
                    return oPreComp.ElementDB[oPreComp.ElementNodeList[ElementNodeListIndex]];
                return null;
            }
        }

        public XMLElementIdentifier CurrentElementId
        {
            get
            {
                if ((ElementNodeListIndex >= 0) && (ElementNodeListIndex < oPreComp.ElementNodeList.Count))
                    return oPreComp.ElementNodeList[ElementNodeListIndex];
                return null;
            }
        }

        public int CountOfCurrentNode
        {
            get
            {
                return TotalNodesOfCurrentType;
            }
            //set
            //{
            //    TotalNodesOfCurrentType = value;
            //}
        }

        public XPathNodeList NodeList
        {
            set
            {
                oNodeList = value;
                TotalNodesOfCurrentType = oNodeList.Count;
            }
        }

        public Mutex ControlMutex
        {
            get { return oControlMutex; }
        }

        public List<StateHistoryEntry> History
        {
            get { return oHistory; }
        }

        public bool InitialiseForFuzzing()
        {
            //eFuzzIndexProgression = FuzzIndexProgressionType.Incremental;

            // If the node index is not the same as the start node index, then do nothing (we assume this means we have already processed the
            // start node, and this node is after it)
            if ((Config.input.StartState != null) && (StartStateNodeListIndex == ElementNodeListIndex))
            {
                // Check that the EndStateNodeIndex is correct
                if (EndStateNodeIndex >= TotalNodesOfCurrentType)
                {
                    Log.Write(MethodBase.GetCurrentMethod(), "The specified end state node index is greater than the number of nodes of this type.  Setting to max.", Log.LogType.Warning);
                    EndStateNodeIndex = TotalNodesOfCurrentType - 1;
                }
                // Note, the rest of the initialisation happened in the constructor, except at that stage we didn't know the number of nodes
                // of this type.
                // Ok, since we do attribute fuzzing before occurrence fuzzing, and if we set the start state to occurrence fuzzing then the 
                // attribute fuzzing will change the category, so we ensure that while we are on the StartStateNodeListIndex we reset
                // the category and fuzz index.
                // Set the type of fuzzer to use
                CurrentCategory = oConfigData.Config.input.StartState.FuzzCategory;
                // Set the initial fuzzer case to try
                iFuzzIndex = (int)oConfigData.Config.input.StartState.FuzzIndex;
            }
            else
            {
                // Default start state
                iNodeIndex = -1;
                iFuzzIndex = 0;
                CurrentCategory = "";

                // There is a minor issue here that if a fuzzer wants to progress FuzzIndex randomly, then the first iteration
                // will always use FuzzIndex = 0, but from then it will be random.
            }

            // If there is only 1 node then fuzzing all and the just the 1 is the same, so just fuzz the one.
            if(1 == TotalNodesOfCurrentType)
                iNodeIndex = 0;

            Finished = false;

            return true;
        }

        //public void InitRandom(int Seed)
        //{
        //    PRNG = new Random(Seed);
        //}
       
        public void NextState()
        {
            // If there is an end state check it
            if (Config.input.EndState != null)
            {
                // Check the elements match, the node index, the category and fuzz index
                if ((EndStateNodeListIndex == ElementNodeListIndex) && 
                    (iNodeIndex >= EndStateNodeIndex) &&
                    (Category == Config.input.EndState.FuzzCategory) &&
                    (iFuzzIndex >= (int)Config.input.EndState.FuzzIndex) )
                {
                    // Set the state to finished
                    Finished = true;
                    return;
                }
            }

            // Check if there are any more nodes of this type
            if (iNodeIndex < TotalNodesOfCurrentType - 1)
            {
                iNodeIndex++;
                return;
            }

            // There aren't so reset the current node to be the first,  which depends ...
            // If there is only 1 node then fuzzing all and the just the 1 is the same, so just fuzz the one.  Alternatively, if the name
            // of the node is not unique then fuzzing all is difficult so we don't, which should be changed (TODO).
            if ((1 == TotalNodesOfCurrentType) || (!oNodeList.NodeNameUnique()))
                iNodeIndex = 0;
            else
                iNodeIndex = -1;
            
            //if (eFuzzIndexProgression == FuzzIndexProgressionType.Random)
            //{
            //    // Next random fuzz test case
            //    PRNG = new Random(iFuzzIndex);
            //    iFuzzIndex = PRNG.Next();
            //}
            //else  // Increment to the next fuzz test case
            //    iFuzzIndex++;

            // Increment to the next fuzz test case
            iFuzzIndex++;

            // We don't care if we don't try to record history for node index changes (since they are never recorded anyway)
            // or the end state (and fuzzing stops)
            RecordHistory();
        }

        public override string ToString()
        {
            if (Category == State.DefaultTestcaseName)
                return State.DefaultTestcaseName;

            StringBuilder StateString = new StringBuilder();

            // Set the element name
            if (null != this.CurrentSchemaElement)
            {
                String prefix = "";
                oPreComp.NamespacePrefixDict.TryGetValue(this.CurrentSchemaElement.Name.Namespace, out prefix);
                if (!String.IsNullOrEmpty(prefix))
                    StateString.Append(prefix + "-");
                StateString.Append(this.CurrentSchemaElement.Name.Name);
                if(bFuzzingAttribute)
                {
                    StateString.Append("(");
                    StateString.Append(sAttributeName);
                    StateString.Append(")");
                }
                StateString.Append("-");
            }

            // Set the NodeIndex
            if (this.NodeIndex == -1)
                StateString.Append("All-");
            else  // Make sure we output the node index that is unique for all nodes of this name
                StateString.Append(oNodeList.UniqueNodeIndex(oNodeList[this.NodeIndex]).ToString() + "-");

            // Set the FuzzCategory
            if (!String.IsNullOrEmpty(this.Category))
                StateString.Append(this.Category + "-");

            // Set the FuzzIndex
            StateString.Append(((uint)this.FuzzIndex));

            if (StateString[StateString.Length - 1] == '-')
                StateString.Remove(StateString.Length - 1, 1);

            return StateString.ToString();
        }

        /// <summary>
        /// Records the history of the state.  Makes note what time the node changes or the fuzz index changes a large amount.
        /// </summary>
        private void RecordHistory()
        {
            lock (oHistory)
            {
                StateHistoryEntry Entry = new StateHistoryEntry();
                Entry.Time = DateTime.Now;
                Entry.Node = CurrentSchemaElement.Name;
                //Entry.NodeIndex = iNodeIndex;
                if(iNodeIndex == -1)
                    Entry.NodeIndex = iNodeIndex;
                else
                    Entry.NodeIndex = oNodeList.UniqueNodeIndex(oNodeList[this.NodeIndex]);
                Entry.FuzzIndex = iFuzzIndex;
                Entry.StateDesc = this.ToString();

                if (0 == oHistory.Count)
                {
                    oHistory.Add(Entry);
                    return;
                }

                StateHistoryEntry LastEntry = oHistory[oHistory.Count - 1];

                // Check if the node has changed
                if (Entry.Node.ToString() == LastEntry.Node.ToString())
                {
                    oHistory.Add(Entry);
                    return;
                }
                // Check if the fuzz index has changed by 1000
                int Factor = (TotalNodesOfCurrentType > 1) ? TotalNodesOfCurrentType + 1 : 1;
                if (Entry.FuzzIndex >= LastEntry.FuzzIndex + 1000 / Factor)
                {
                    oHistory.Add(Entry);
                    return;
                }
            }
        }

        public void PrintHistory(DateTime Time, int PrevStatesToShow)
        {
            lock (oHistory)
            {
                int MatchingState = -1;
                // Find the index of the first state DateTime greater than the Time passed in
                for (int i = oHistory.Count - 1; i >= 0; i--)
                {
                    // Is the History value less than the Time value
                    if (oHistory[i].Time.CompareTo(Time) < 0)
                    {
                        MatchingState = i + 1;
                        break;
                    }
                }

                if (-1 == MatchingState)
                    return;
                if (MatchingState > oHistory.Count - 1)
                    MatchingState = oHistory.Count - 1;

                // Create desciption of previous states
                StringBuilder HistDesc = new StringBuilder();
                HistDesc.AppendLine("Showing previous " + PrevStatesToShow + " recorded states occurring before " + Time.ToShortDateString() + " " + Time.ToLongTimeString());
                for (int i = 0; (i < PrevStatesToShow) && (MatchingState - i > 0); i++)
                {
                    HistDesc.Append("\t\t");
                    HistDesc.AppendLine(oHistory[MatchingState - i].StateDesc);
                }
                Log.Write(MethodBase.GetCurrentMethod(), HistDesc.ToString(), Log.LogType.Info);
            }
        }

        public String Category
        {
            get
            {
                return CurrentCategory;
            }
            set
            {
                CurrentCategory = value;
            }
        }

        public int FuzzIndex
        {
            get
            {
                return iFuzzIndex;
            }
            set
            {
                iFuzzIndex = value;
            }
        }

        public int NodeIndex
        {
            get
            {
                return iNodeIndex;
            }
            set
            {
                iNodeIndex = value;
            }
        }

        public bool Finished
        {
            get
            {
                return FinishedState;
            }
            set
            {
                FinishedState = value;
            }
        }

        public bool Skip
        {
            get
            {
                return SkipThisState;
            }
            set
            {
                SkipThisState = value;
            }
        }

        public bool FuzzingAttribute
        {
            set
            {
                bFuzzingAttribute = value;
            }
        }

        public String AttributeName
        {
            set
            {
                sAttributeName = value;
            }
        }

        public bool EndState
        {
            get
            {
                return bEndState;
            }
            set
            {
                bEndState = value;
            }
        }

        //public FuzzIndexProgressionType FuzzIndexProgression
        //{
        //    get
        //    {
        //        return eFuzzIndexProgression;
        //    }

        //    set
        //    {
        //        eFuzzIndexProgression = value;
        //    }
        //}
    }

}
