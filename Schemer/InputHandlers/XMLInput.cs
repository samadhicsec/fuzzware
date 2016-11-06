using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Fuzzware.Common;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Schemer.InputHandlers
{
    /// <summary>
    /// The input is the path to the XML and XML Schema file that the user has specified.
    /// </summary>
    public class XMLInput : InputHandler
    {
        public override void Initialise(object Settings, Encoding OutputEncoding)
        {
            this.OutputEncoding = OutputEncoding;
            if (!(Settings is XMLFileInput))
                Log.Write(MethodInfo.GetCurrentMethod(), "Expected Settings object of type 'XMLFileInput', got '" + Settings.GetType().ToString() + "'", Log.LogType.Error);

            XMLFileInput oXMLFileInput = Settings as XMLFileInput;

            // Check the XML path
            if(String.IsNullOrEmpty(oXMLFileInput.XMLPathAndFilename))
                Log.Write(MethodInfo.GetCurrentMethod(), "The XMLPathAndFilename was empty", Log.LogType.Error);
            // Get the path to the XML input
            XMLPath = oXMLFileInput.XMLPathAndFilename;

            // Check the XSD paths
            if (0 == oXMLFileInput.XSDPathAndFilename.Length)
                Log.Write(MethodInfo.GetCurrentMethod(), "No XSDPathAndFilename elments were supplied", Log.LogType.Error);
            for(int i = 0; i < oXMLFileInput.XSDPathAndFilename.Length; i++)
                if (String.IsNullOrEmpty(oXMLFileInput.XSDPathAndFilename[i]))
                    Log.Write(MethodInfo.GetCurrentMethod(), "The XSDPathAndFilename at position " + i+1 + " was empty", Log.LogType.Error);
            // Get the path to the XML Schemas
            SchemaPaths = oXMLFileInput.XSDPathAndFilename;
        }
    }
}
