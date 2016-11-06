using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using Fuzzware.Common;

namespace Fuzzware.Common.Interop
{
    /// <summary>
    /// Helps retrieve type information from a System.Runtime.InteropServices.ComTypes.ITypeLib object
    /// </summary>
    public class TypeLibHelper
    {
        ComTypes.ITypeLib oTypeLib;

        List<TypeInfoHelper> oTypeInfoHelpers;

        String strName;
        String strDocString;
        int dwHelpContext;
        String strHelpFile;

        public TypeLibHelper(ComTypes.ITypeLib ITypeLib)
        {
            oTypeLib = ITypeLib;

            // Get the documentation
            oTypeLib.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);

            oTypeInfoHelpers = new List<TypeInfoHelper>();
        }

        public TypeInfoHelper AddInterfaceTypeInfo(ComTypes.ITypeInfo oTypeInfo)
        {
            TypeInfoHelper oTypeInfoHelper = new TypeInfoHelper(oTypeInfo);

            // See if a TypeInfoHelper with the same name already exists
            bool bAlreadyExists = false;
            for (int i = 0; i < oTypeInfoHelpers.Count; i++)
            {
                if (oTypeInfoHelper.Name.Equals(oTypeInfoHelpers[i].Name))
                {
                    bAlreadyExists = true;
                    oTypeInfoHelper = oTypeInfoHelpers[i];
                    break;
                }
            }

            // Only add oTypeInfoHelper to our list of types if it doesn't already exist and it is actually
            // an interface type
            if (!bAlreadyExists && oTypeInfoHelper.IsInterface)
                oTypeInfoHelpers.Add(oTypeInfoHelper);
            
            return oTypeInfoHelper;
        }

        /// <summary>
        /// Return a list of interface types of the type library
        /// </summary>
        public List<TypeInfoHelper> InterfaceTypeInfoHelpers
        {
            get
            {
                return oTypeInfoHelpers;
            }
        }

        /// <summary>
        /// Gets the name of the TypeLib
        /// </summary>
        public string Name
        {
            get
            {
                return strName;
            }
        }

        /// <summary>
        /// Gets the help documentation for the TypeLib
        /// </summary>
        public string HelpDocumentation
        {
            get
            {
                return strDocString;
            }
        }
    }
}
