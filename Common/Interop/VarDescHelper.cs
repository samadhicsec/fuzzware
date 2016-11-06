using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.Interop
{
    class VarDescHelper
    {
        ComTypes.ITypeInfo oTypeInfo;
        ComTypes.VARDESC stVarDesc;
        IntPtr pVarDesc = IntPtr.Zero;

        String strName;
        String strDocString;
        int dwHelpContext;
        String strHelpFile;

        public VarDescHelper(ComTypes.ITypeInfo ITypeInfo, int VarDescIndex)
        {
            oTypeInfo = ITypeInfo;

            try
            {
                oTypeInfo.GetVarDesc(VarDescIndex, out pVarDesc);
                stVarDesc = (ComTypes.VARDESC)Marshal.PtrToStructure(pVarDesc, typeof(ComTypes.VARDESC));
            }
            catch (Exception e)
            {
                Log.Write(e);
            }

            oTypeInfo.GetDocumentation(stVarDesc.memid, out strName, out strDocString, out dwHelpContext, out strHelpFile);
        }

        ~VarDescHelper()
        {
            if (IntPtr.Zero != pVarDesc)
                oTypeInfo.ReleaseVarDesc(pVarDesc);
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name
        {
            get
            {
                return strName;
            }
        }

        /// <summary>
        /// Gets the const value of the variable.  If the variable is not const then returns null.
        /// </summary>
        public object ConstValue
        {
            get
            {
                if (stVarDesc.varkind == ComTypes.VARKIND.VAR_CONST)
                {
                    return Marshal.GetObjectForNativeVariant(stVarDesc.desc.lpvarValue);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the Simple Type name of the variable.  If not Simple Type then null is returned
        /// </summary>
        public XmlQualifiedName SimpleTypeName
        {
            get
            {
                return TypeDescHelper.GetSimpleTypeFromTYPEDESC(stVarDesc.elemdescVar.tdesc);
            }
        }

        /// <summary>
        /// Returns the ComTypes.TYPEDESC of the ComTypes.VARDESC
        /// </summary>
        public ComTypes.TYPEDESC TypeDesc
        {
            get
            {
                return stVarDesc.elemdescVar.tdesc;
            }
        }
    }
}
