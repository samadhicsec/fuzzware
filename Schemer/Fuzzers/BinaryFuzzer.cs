using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using Fuzzware.Common;
using Fuzzware.Common.XML;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Schemer.Fuzzers
{
    class BinaryFuzzer : FuzzerBase, ITypeFuzzer
    {
        public delegate void BinaryEditFn(int StartIndex, byte[] FuzzByteValues, ref byte[] OutputBytes);

        protected BinaryValueFuzzer ValueConfig;
        protected byte[][] ReplacementBytesArray;
        protected ValueRange ReplacementBytesBR;
        protected byte[][] InsertBytesArray;
        protected ValueRange InsertBytesBR;
        protected byte[][] AndBytesArray;
        protected ValueRange AndBytesBR;
        protected byte[][] OrBytesArray;
        protected ValueRange OrBytesBR;
        protected byte[][] XOrBytesArray;
        protected ValueRange XOrBytesBR;
        protected int MaxFuzzIndex;
        //protected bool ByteAlign;
        //protected uint ByteIteration;
        //protected uint ByteLocCount;
        //protected bool WordAlign;
        //protected uint WordIteration;
        //protected uint WordLocCount; 
        //protected bool DWordAlign;
        //protected uint DWordIteration;
        //protected uint DWordLocCount;
        //protected bool QWordAlign;
        //protected uint QWordIteration;
        //protected uint QWordLocCount;
        //protected bool InitRandomFuzzing = false;

        public BinaryFuzzer(ConfigData oConfigData, PreCompData oPreCompData, ObjectDBEntry oSchemaElement)
            : base(oConfigData, oPreCompData)
        {
            ValueConfig = oConfigData.STFConfig.BinaryValueFuzzer;

            ConfigDefinedValues = null;

            bool UsingCustomFuzzer = false;
            if (null != ValueConfig)
            {
                // Check if the user has assigned custom fuzzers for this node
                if (ValueConfig.CustomFuzzer != null)
                    for (int i = 0; i < ValueConfig.CustomFuzzer.Length; i++)
                    {
                        if (ValueConfig.CustomFuzzer[i].NodeNamespace.Equals(oSchemaElement.Name.Namespace, StringComparison.CurrentCultureIgnoreCase) &&
                            ValueConfig.CustomFuzzer[i].NodeName.Equals(oSchemaElement.Name.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            AssignValueFuzzers(ValueConfig.CustomFuzzer[i]);
                            UsingCustomFuzzer = true;
                            Log.Write(MethodBase.GetCurrentMethod(), "Custom BinaryValue fuzzer being used", Log.LogType.LogOnlyInfo);
                            break;
                        }
                    }
                // Assign the default fuzzers if there is no custom one
                if (!UsingCustomFuzzer && (ValueConfig.DefaultFuzzers != null))
                    AssignValueFuzzers(ValueConfig.DefaultFuzzers);
            }
            //if (null != ConfigData.ByteValueFuzzers.ReplaceRandomBytes)
            //{
            //    Add("ReplaceRandomBytes", ReplaceRandomBytes);
            //    ByteAlign = ConfigData.ByteValueFuzzers.ReplaceRandomBytes.BoundaryAlign;
            //    ByteIteration = ConfigData.ByteValueFuzzers.ReplaceRandomBytes.IterationCount;
            //    ByteLocCount = ConfigData.ByteValueFuzzers.ReplaceRandomBytes.LocationCount;
            //    InitRandomFuzzing = true;
            //}
            //if (null != ConfigData.ByteValueFuzzers.ReplaceRandomWords)
            //{
            //    Add("ReplaceRandomWords", ReplaceRandomWords);
            //    WordAlign = ConfigData.ByteValueFuzzers.ReplaceRandomWords.BoundaryAlign;
            //    WordIteration = ConfigData.ByteValueFuzzers.ReplaceRandomWords.IterationCount;
            //    WordLocCount = ConfigData.ByteValueFuzzers.ReplaceRandomWords.LocationCount;
            //    InitRandomFuzzing = true;
            //}
            //if (null != ConfigData.ByteValueFuzzers.ReplaceRandomDWords)
            //{
            //    Add("ReplaceRandomDWords", ReplaceRandomDWords);
            //    DWordAlign = ConfigData.ByteValueFuzzers.ReplaceRandomDWords.BoundaryAlign;
            //    DWordIteration = ConfigData.ByteValueFuzzers.ReplaceRandomDWords.IterationCount;
            //    DWordLocCount = ConfigData.ByteValueFuzzers.ReplaceRandomDWords.LocationCount;
            //    InitRandomFuzzing = true;
            //}
            //if (null != ConfigData.ByteValueFuzzers.ReplaceRandomQWords)
            //{
            //    Add("ReplaceRandomQWords", ReplaceRandomQWords);
            //    QWordAlign = ConfigData.ByteValueFuzzers.ReplaceRandomQWords.BoundaryAlign;
            //    QWordIteration = ConfigData.ByteValueFuzzers.ReplaceRandomQWords.IterationCount;
            //    QWordLocCount = ConfigData.ByteValueFuzzers.ReplaceRandomQWords.LocationCount;
            //    InitRandomFuzzing = true;
            //}

        }

        public override void Initialise()
        {
            base.Initialise();
            MaxFuzzIndex = CountOfCurrentNodes;

            // If we are doing random fuzzing then change State progression to Random and set up iteration count.
            //if (InitRandomFuzzing)
            //{
            //    State.FuzzIndexProgression = FuzzIndexProgressionType.Random;
            //    // Seed the randomness for the state, if it we aren't using a start state
            //    if (0 == State.FuzzIndex)
            //    {
            //        byte[] SeedArray = new byte[4];
            //        RNGCryptoServiceProvider RealRandom = new RNGCryptoServiceProvider();
            //        RealRandom.GetBytes(SeedArray);
            //        int Seed = BitConverter.ToInt32(SeedArray, 0);
            //        State.InitRandom(Seed);
            //        State.FuzzIndex = Seed;
            //    }

            //    Log.Write(MethodBase.GetCurrentMethod(), "Initial random seed - " + ((uint)State.FuzzIndex), Log.LogType.LogOnlyInfo);

            //    // Increase the iteration to cover all the nodes
            //    uint Multiplier = (uint)((1 == State.CountOfCurrentNode)?1:(State.CountOfCurrentNode + 1));
            //    ByteIteration = ByteIteration * Multiplier;
            //    WordIteration = WordIteration * Multiplier;
            //    DWordIteration = DWordIteration * Multiplier;
            //    QWordIteration = QWordIteration * Multiplier;
            //}
        }

        private void AssignValueFuzzers(ByteValueFuzzersType FuzzersType)
        {
            if (null != FuzzersType.ReplaceBytes)
            {
                Add("ReplaceBytes", ReplaceBytes);
                ReplacementBytesArray = GetByteGroup(FuzzersType.ReplaceBytes.ValueGroupRef);
                ReplacementBytesBR = GetValueRange(FuzzersType.ReplaceBytes.ValueRangeRef, ValueConfig.ByteRange);
            }
            if (null != FuzzersType.InsertBytes)
            {
                Add("InsertBytes", InsertBytes);
                InsertBytesArray = GetByteGroup(FuzzersType.InsertBytes.ValueGroupRef);
                InsertBytesBR = GetValueRange(FuzzersType.InsertBytes.ValueRangeRef, ValueConfig.ByteRange);
            }
            if (null != FuzzersType.AndBytes)
            {
                Add("AndBytes", AndBytes);
                AndBytesArray = GetByteGroup(FuzzersType.AndBytes.ValueGroupRef);
                AndBytesBR = GetValueRange(FuzzersType.AndBytes.ValueRangeRef, ValueConfig.ByteRange);
            }
            if (null != FuzzersType.OrBytes)
            {
                Add("OrBytes", OrBytes);
                OrBytesArray = GetByteGroup(FuzzersType.OrBytes.ValueGroupRef);
                OrBytesBR = GetValueRange(FuzzersType.OrBytes.ValueRangeRef, ValueConfig.ByteRange);
            }
            if (null != FuzzersType.XOrBytes)
            {
                Add("XOrBytes", XOrBytes);
                XOrBytesArray = GetByteGroup(FuzzersType.XOrBytes.ValueGroupRef);
                XOrBytesBR = GetValueRange(FuzzersType.XOrBytes.ValueRangeRef, ValueConfig.ByteRange);
            }
        }

        public override void  End()
        {
 	        base.End();
            // Since we may have changed the way the State FuzzIndex progresses, make sure we change it back to its default incremental
            //State.FuzzIndexProgression = FuzzIndexProgressionType.Incremental;
        }

        private byte[][] GetByteGroup(String ID)
        {
            for (int i = 0; i < ValueConfig.ByteGroup.Length; i++)
            {
                if (ID == ValueConfig.ByteGroup[i].ID)
                    return ValueConfig.ByteGroup[i].ByteValue;
            }
            Log.Write(MethodBase.GetCurrentMethod(), "Could not find a ByteGroup with ID='" + ID + "'.  The associated ByteValueFuzzer will be skipped.", Log.LogType.Warning);
            return null;
        }

        // Replacement functions

        public bool ReplaceBytes(int FuzzIndex, int NodeIndex)
        {
            return EditBytes(FuzzIndex, NodeIndex, ReplacementBytesArray, ReplacementBytesBR, ReplaceBytesEditFn);
        }

        protected void ReplaceBytesEditFn(int StartIndex, byte[] FuzzByteValues, ref byte[] OutputBytes)
        {
            for (int i = 0; i < FuzzByteValues.Length; i++)
                OutputBytes[StartIndex + i] = FuzzByteValues[i];
        }

        // Binary operation functions

        public bool AndBytes(int FuzzIndex, int NodeIndex)
        {
            return EditBytes(FuzzIndex, NodeIndex, ReplacementBytesArray, AndBytesBR, AndBytesEditFn);
        }

        protected void AndBytesEditFn(int StartIndex, byte[] FuzzByteValues, ref byte[] OutputBytes)
        {
            for (int i = 0; i < FuzzByteValues.Length; i++)
                OutputBytes[StartIndex + i] = (byte)(OutputBytes[StartIndex + i] & FuzzByteValues[i]);
        }

        public bool OrBytes(int FuzzIndex, int NodeIndex)
        {
            return EditBytes(FuzzIndex, NodeIndex, ReplacementBytesArray, OrBytesBR, OrBytesEditFn);
        }

        protected void OrBytesEditFn(int StartIndex, byte[] FuzzByteValues, ref byte[] OutputBytes)
        {
            for (int i = 0; i < FuzzByteValues.Length; i++)
                OutputBytes[StartIndex + i] = (byte)(OutputBytes[StartIndex + i] | FuzzByteValues[i]);
        }

        public bool XOrBytes(int FuzzIndex, int NodeIndex)
        {
            return EditBytes(FuzzIndex, NodeIndex, ReplacementBytesArray, XOrBytesBR, XOrBytesEditFn);
        }

        protected void XOrBytesEditFn(int StartIndex, byte[] FuzzByteValues, ref byte[] OutputBytes)
        {
            for (int i = 0; i < FuzzByteValues.Length; i++)
                OutputBytes[StartIndex + i] = (byte)(OutputBytes[StartIndex + i] ^ FuzzByteValues[i]);
        }

        // Random replacement functions
        //private void InitRandom()
        //{
        //    // Seed the randomness for the state, if it we aren't using a start state
        //    byte[] SeedArray = new byte[4];
        //    RNGCryptoServiceProvider RealRandom = new RNGCryptoServiceProvider();
        //    RealRandom.GetBytes(SeedArray);
        //    int Seed = BitConverter.ToInt32(SeedArray, 0);
        //    State.InitRandom(Seed);
        //    State.FuzzIndex = Seed;
        //}
        //public bool ReplaceRandomBytes(int FuzzIndex, int NodeIndex)
        //{
        //    State.FuzzIndexProgression = FuzzIndexProgressionType.Random;
        //    // Seed the randomness for the state, if it we aren't using a start state
        //    if (0 == State.FuzzIndex)
        //        InitRandom();
        //    // Check if we should finish fuzzing this node (recall we multiplied the iteration count by the number of nodes)
        //    if (0 == ByteIteration)
        //    {
        //        Log.Write(MethodBase.GetCurrentMethod(), "Final random fuzz index - " + ((uint)State.FuzzIndex), Log.LogType.LogOnlyInfo);
        //        return false;
        //    }
        //    EditRandomBytes(FuzzIndex, NodeIndex, 1, (ByteAlign ? 1 : 1), ByteLocCount);
        //    ByteIteration--;
        //    return true;
        //}

        //public bool ReplaceRandomWords(int FuzzIndex, int NodeIndex)
        //{
        //    State.FuzzIndexProgression = FuzzIndexProgressionType.Random;
        //    // Seed the randomness for the state, if it we aren't using a start state
        //    if (0 == State.FuzzIndex)
        //        InitRandom();
        //    // Check if we should finish fuzzing this node (recall we multiplied the iteration count by the number of nodes)
        //    if (0 == WordIteration)
        //    {
        //        Log.Write(MethodBase.GetCurrentMethod(), "Final random fuzz index - " + ((uint)State.FuzzIndex), Log.LogType.LogOnlyInfo);
        //        return false;
        //    }
        //    EditRandomBytes(FuzzIndex, NodeIndex, 2, (WordAlign ? 2 : 1), WordLocCount);
        //    WordIteration--;
        //    return true;
        //}

        //public bool ReplaceRandomDWords(int FuzzIndex, int NodeIndex)
        //{
        //    State.FuzzIndexProgression = FuzzIndexProgressionType.Random;
        //    // Seed the randomness for the state, if it we aren't using a start state
        //    if (0 == State.FuzzIndex)
        //        InitRandom();
        //    // Check if we should finish fuzzing this node (recall we multiplied the iteration count by the number of nodes)
        //    if (0 == DWordIteration)
        //    {
        //        Log.Write(MethodBase.GetCurrentMethod(), "Final random fuzz index - " + ((uint)State.FuzzIndex), Log.LogType.LogOnlyInfo);
        //        return false;
        //    }
        //    EditRandomBytes(FuzzIndex, NodeIndex, 4, (DWordAlign ? 4 : 1), DWordLocCount);
        //    DWordIteration--;
        //    return true;
        //}

        //public bool ReplaceRandomQWords(int FuzzIndex, int NodeIndex)
        //{
        //    State.FuzzIndexProgression = FuzzIndexProgressionType.Random;
        //    // Seed the randomness for the state, if it we aren't using a start state
        //    if (0 == State.FuzzIndex)
        //        InitRandom();
        //    // Check if we should finish fuzzing this node (recall we multiplied the iteration count by the number of nodes)
        //    if (0 == QWordIteration)
        //    {
        //        Log.Write(MethodBase.GetCurrentMethod(), "Final random fuzz index - " + ((uint)State.FuzzIndex), Log.LogType.LogOnlyInfo);
        //        return false;
        //    }
        //    EditRandomBytes(FuzzIndex, NodeIndex, 8, (QWordAlign ? 8 : 1), QWordLocCount);
        //    QWordIteration--;
        //    return true;
        //}

        //private void EditRandomBytes(int FuzzIndex, int NodeIndex, int ByteCount, int Align, uint LocationCount)
        //{
        //    int StartNodeIndex = NodeIndex;
        //    int EndNodeIndex = NodeIndex;
        //    if (-1 == NodeIndex)
        //    {
        //        StartNodeIndex = 0;
        //        EndNodeIndex = State.CountOfCurrentNode - 1;
        //    }

        //    // Use the FuzzIndex to create the required offset and random bytes
        //    // Set the seed to be different for every combination of FuzzIndex and NodeIndex.
        //    Random PRNG = new Random(unchecked(FuzzIndex*(NodeIndex + 2)));
        //    for (int i = StartNodeIndex; i <= EndNodeIndex; i++)
        //    {
        //        // Get the encoded value
        //        String Value = oValuesEditor.GetValue(i);
        //        byte[] EditValues = new byte[0];
        //        // Make sure it is something
        //        if (String.IsNullOrEmpty(Value))
        //        {
        //            continue;
        //        }
        //        // Decode the value to binary
        //        EditValues = Coder.DecodeToBytes(Value);

        //        for (uint j = 0; j < LocationCount; j++)
        //        {
        //            if (EditValues.Length < ByteCount)
        //                continue;
        //            // Get offset (it will be minValue <= Offset < maxValue)
        //            int Offset = PRNG.Next(0, EditValues.Length - ByteCount + 1);
        //            Offset = Offset - (Offset % Align);
        //            // Get random byte(s)
        //            byte[] RandomBytes = new byte[ByteCount];
        //            PRNG.NextBytes(RandomBytes);
        //            // Edit value
        //            ReplaceBytesEditFn(Offset, RandomBytes, ref EditValues);
        //        }

        //        // Encode back to a string
        //        String NewValue = Coder.Encode(EditValues);
        //        // I am reasonably confident that no validation needs to occur
        //        // Update value
        //        oValuesEditor.ChangeValue(i, NewValue);
        //    }
        //}

        private bool EditBytes(int FuzzIndex, int NodeIndex, byte[][] ByteArrays, ValueRange ByteRange, BinaryEditFn EditFunction)
        {
            if (null == ValueConfig)
                return false;

            if (null == ByteArrays)
                return false;

            // Get the value that will contain the bytes we will replace
            if (NodeIndex >= 0)
            {
                // Get the encoded value
                String Value = oValuesEditor.GetValue(NodeIndex);

                byte[] EditValues = new byte[0];

                // Make sure it is something
                if (!String.IsNullOrEmpty(Value))
                {
                    // Decode the value to binary
                    EditValues = Coder.DecodeToBytes(Value);
                }

                // Since different nodes will be of different lengths, we need to continue fuzzing even if one node is very short
                if (MaxFuzzIndex < ByteArrays.Length * EditValues.Length)
                    MaxFuzzIndex = ByteArrays.Length * EditValues.Length;
                if (FuzzIndex >= MaxFuzzIndex)
                {
                    // Reset, for other fuzzers that will be used without Fuzzer.Initialise being called
                    MaxFuzzIndex = CountOfCurrentNodes;
                    return false;
                }

                // Make sure we have got something to edit
                if (FuzzIndex >= ByteArrays.Length * EditValues.Length)
                    throw new SkipStateOutOfRangeException();
                

                // Convert the config replacement value to the right type
                byte[] FuzzBytes = ByteArrays[FuzzIndex % (ByteArrays.Length)];

                // Make the replacement
                int startpos = FuzzIndex / ByteArrays.Length;

                // Check we are in the right range
                if (startpos < ByteRange.StartIndex)
                    throw new SkipStateOutOfRangeException();
                if ((ByteRange.Length != 0) && (startpos + FuzzBytes.Length > ByteRange.StartIndex + ByteRange.Length))
                    throw new SkipStateOutOfRangeException();
                if((EditValues.Length - startpos)%ByteRange.StepSize != 0)
                    throw new SkipStateOutOfRangeException();

                if (startpos + FuzzBytes.Length <= EditValues.Length)
                {
                    // Execute the binary edit function
                    EditFunction(startpos, FuzzBytes, ref EditValues);
                }
                else
                {
                    throw new SkipStateOutOfRangeException();
                }

                // Encode back to a string
                String NewValue = Coder.Encode(EditValues);

                // Validate it
                if (Facets.Validate(NewValue))
                {
                    // Get the Editor to change the value
                    oValuesEditor.ChangeValue(NodeIndex, NewValue);
                }
                else
                    throw new SkipStateException();
            }
            else
                throw new SkipStateNoAllCaseException();
                // At this stage changing all the values at the same time seems like it would be fruitless

            return true;
        }

        public bool InsertBytes(int FuzzIndex, int NodeIndex)
        {
            if (null == ValueConfig)
                return false;

            if (null == InsertBytesArray)
                return false;

            // Get the value that will contain the bytes we will replace
            if (NodeIndex >= 0)
            {
                // Get the encoded value
                String Value = oValuesEditor.GetValue(NodeIndex);

                // Decode the value to binary
                byte[] EditValues = Coder.DecodeToBytes(Value);

                // When inserting we can always insert, even if the byte array is empty
                if (MaxFuzzIndex < InsertBytesArray.Length * (EditValues.Length + 1))
                    MaxFuzzIndex = InsertBytesArray.Length * (EditValues.Length + 1);
                if (FuzzIndex >= MaxFuzzIndex)
                {
                    // Reset
                    MaxFuzzIndex = CountOfCurrentNodes;
                    return false;
                }

                if (FuzzIndex >= InsertBytesArray.Length * (EditValues.Length + 1))
                    throw new SkipStateOutOfRangeException();

                // Get the config insertion value
                byte[] FuzzBytes = InsertBytesArray[FuzzIndex % (InsertBytesArray.Length)];

                // Calculate the start position
                int startpos = FuzzIndex / InsertBytesArray.Length;

                // Check we are in the right section
                if (startpos < InsertBytesBR.StartIndex)
                    throw new SkipStateOutOfRangeException();
                if ((InsertBytesBR.Length != 0) && ((startpos + FuzzBytes.Length) > (InsertBytesBR.StartIndex + InsertBytesBR.Length)))
                    throw new SkipStateOutOfRangeException();
                if ((EditValues.Length - startpos) % InsertBytesBR.StepSize != 0)
                    throw new SkipStateOutOfRangeException();

                byte[] NewEditValues = new byte[EditValues.Length + FuzzBytes.Length];
                // Make the insertion
                Array.Copy(EditValues, 0, NewEditValues, 0, startpos);
                Array.Copy(FuzzBytes, 0, NewEditValues, startpos, FuzzBytes.Length);
                if(startpos < EditValues.Length)
                    Array.Copy(EditValues, startpos, NewEditValues, startpos + FuzzBytes.Length, EditValues.Length - startpos);

                // Encode back to a string
                String NewValue = Coder.Encode(NewEditValues);

                // Validate it
                if (Facets.Validate(NewValue))
                {
                    // Get the Editor to change the value
                    oValuesEditor.ChangeValue(NodeIndex, NewValue);
                }
                else
                    throw new SkipStateException();
            }
            else
                throw new SkipStateNoAllCaseException();
                // At this stage changing all the values at the same time seems like it would be fruitless

            return true;
        }

        #region IFuzzer Members

        #endregion
    }
}
