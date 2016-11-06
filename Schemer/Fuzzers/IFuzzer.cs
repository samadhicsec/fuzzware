using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common.DataSchema.Restrictions;
using Fuzzware.Common.Encoding;
using Fuzzware.Schemer.Editors;

namespace Fuzzware.Schemer.Fuzzers
{
    public interface IFuzzer : IFuzzState
    {
        string[] Methods();
        void Initialise();
        void Fuzz();
        void Restore();
        void End();
    }

    interface ITypeFuzzer : IFuzzer
    {
        IValuesEditor ValuesEditor
        {
            set;
        }

        Coder FuzzCoder
        {
            set;
        }

        IFacetRestrictions FacetRestrictions
        {
            get;
        }
    }

    interface IChildNodesFuzzer : IFuzzer
    {
        IChildNodesEditor ChildNodesEditor
        {
            set;
        }
    }
}
