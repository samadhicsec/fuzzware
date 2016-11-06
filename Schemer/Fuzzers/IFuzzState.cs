using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Schemer.Fuzzers
{
    public interface IFuzzState
    {
        /// <summary>
        /// Moves the fuzzer onto the next state
        /// </summary>
        void Next();

        /// <summary>
        /// Moves the state to the next fuzz index
        /// </summary>
        void NextFuzzIndex();

        /// <summary>
        /// Moves the state to the next node index
        /// </summary>
        bool NextNodeIndex();

        /// <summary>
        /// Moves the state to the next fuzzer type
        /// </summary>
        bool NextFuzzType();

        /// <summary>
        /// Sets the current state.
        /// </summary>
        void SetState(string NodeIndexStr, string TypeStr, uint FuzzIndex);

        /// <summary>
        /// Sets the end state.
        /// </summary>
        void SetEndState(string NodeIndexStr, string TypeStr, uint FuzzIndex);

        /// <summary>
        /// True if the fuzzer has finished
        /// </summary>
        bool IsFinished
        {
            get;
        }

        /// <summary>
        /// Gets or sets the current fuzz index
        /// </summary>
        int FuzzIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current NodeIndex
        /// </summary>
        int NodeIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current type of fuzzer
        /// </summary>
        string Type
        {
            get;
            set;
        }

    }
}
