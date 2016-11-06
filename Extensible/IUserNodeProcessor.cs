using System;
using System.Collections.Generic;
using System.Text;
using Fuzzware.Common.DataSchema;

namespace Fuzzware.Extensible
{
    public interface IUserNodeProcessor
    {
        /// <summary>
        /// Users can extend ConvertFromXML by providing their own code to convert the value of an XML node into another form.  This
        /// can be specified by adding '&lt;?Schemer UserCode="Id" ...?&gt;' XML Processing Instruction (with the Id of the source XML 
        /// node) directly before the target XML node that will be updated.
        /// </summary>
        /// <param name="operand">Contains the byte representation of the source node e.g. If operand is a string then the byte array will be a byte array of unicode characters.</param>
        /// <param name="SourceNode">The source node</param>
        /// <param name="TargetNode">The target node</param>
        /// <returns>The updated value of the target node</returns>
        byte[] Process(byte[] operand, IOutputNode SourceNode, IOutputNode TargetNode);
    }
}
