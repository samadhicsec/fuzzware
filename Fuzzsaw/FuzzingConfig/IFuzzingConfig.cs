using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuzzware.Fuzzsaw.FuzzingConfig
{
    interface IFuzzingConfig
    {
        void SetSelectedNode(string Node);

        string[] Fuzzers
        {
            get;
            set;
        }

        string[] NodesWithFuzzers
        {
            get;
            set;
        }
    }
}
