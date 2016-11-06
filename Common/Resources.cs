using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Fuzzware.Common
{
    public class Resources
    {
        /// <summary>
        /// Attempts to get the resource contained in one of the ResourceAssemblies.  The assemblies should belong to the Fuzzware namespace.
        /// </summary>
        /// <param name="ResourceName">The name of the resource</param>
        /// <param name="ResourceAssemblies">An array of assemblies in which to look for the resource</param>
        /// <returns>A stream of the resource, or null if the resource was not found</returns>
        public static Stream GetResource(String ResourceName, Assembly[] ResourceAssemblies)
        {
            Stream oStream = null;
            // Get Stream from one of the Assemblies
            for (int i = 0; i < ResourceAssemblies.Length; i++)
            {
                oStream = ResourceAssemblies[i].GetManifestResourceStream("Fuzzware." + ResourceAssemblies[i].GetName().Name + ".Resources." + ResourceName);
                if (null != oStream)
                    break;
            }
            return oStream;
        }

        /// <summary>
        /// Attempts to get the resource from any of the assemblies referenced from the current executing assembly.
        /// </summary>
        /// <param name="ResourceName">The name of the resource</param>
        /// <returns>A stream of the resource, or null if the resource was not found</returns>
        public static Stream GetResource(String ResourceName)
        {
            AssemblyName[] oAssemblyNames = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            Array.Resize<AssemblyName>(ref oAssemblyNames, oAssemblyNames.Length + 1);
            oAssemblyNames[oAssemblyNames.Length - 1] = Assembly.GetEntryAssembly().GetName();

            Stream oStream = null;
            // Get Stream from one of the Assemblies
            for (int i = 0; i < oAssemblyNames.Length; i++)
            {
                Assembly oAssembly = Assembly.Load(oAssemblyNames[i]);
                oStream = oAssembly.GetManifestResourceStream("Fuzzware." + oAssemblyNames[i].Name + ".Resources." + ResourceName);
                if (null != oStream)
                    break;
            }
            return oStream;
        }
    }
}
