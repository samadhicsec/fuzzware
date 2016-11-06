using System;
using System.Collections.Generic;
using System.Text;

namespace Fuzzware.Extensible
{
    /// <summary>
    /// All code that extends Fuzzware componenets should throw an ExtensbilityException when an Exception occurs, 
    /// so the exception can be handled gracefully.
    /// </summary>
    public class ExtensibilityException : Exception
    {
        public ExtensibilityException(String message) : base(message)
        {
            
        }
    }
}
