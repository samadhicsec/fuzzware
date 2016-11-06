using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.Interop
{
    public class FuncDescHelper
    {
        ComTypes.ITypeInfo oTypeInfo;
        ComTypes.FUNCDESC stFuncDesc;
        IntPtr pFuncDesc;

        String FuncName;
        String FuncNamePrefix = "";
        List<String> oParameterOrder = new List<string>();
        List<ParamDescHelper> oParamDescs = new List<ParamDescHelper>();
        ParamDescHelper oReturnType;

        public FuncDescHelper(ComTypes.ITypeInfo ITypeInfo, int FuncDescIndex)
        {
            oTypeInfo = ITypeInfo;

            oTypeInfo.GetFuncDesc(FuncDescIndex, out pFuncDesc);
            stFuncDesc = (ComTypes.FUNCDESC)Marshal.PtrToStructure(pFuncDesc, typeof(ComTypes.FUNCDESC));

            // Get the function and parameter names
            string[] oParameterNames = new string[stFuncDesc.cParams + 1];    // Add 1 for method/function name
            int iNamesCount;
            oTypeInfo.GetNames(stFuncDesc.memid, oParameterNames, oParameterNames.Length, out iNamesCount);
            FuncName = oParameterNames[0];

            // Alter the name by the kind of method
            if (ComTypes.INVOKEKIND.INVOKE_PROPERTYGET == stFuncDesc.invkind)
                FuncNamePrefix = MethodInterface.MethodDescription.INVOKE_PROPERTYGET_PREFIX;
            else if (ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT == stFuncDesc.invkind)
            {
                FuncNamePrefix = MethodInterface.MethodDescription.INVOKE_PROPERTYPUT_PREFIX;
                // For put, no parameter name is returned, so let's add one
                if (iNamesCount + 1 == oParameterNames.Length)
                    oParameterNames[iNamesCount++] = "value";
            }
            else if (ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF == stFuncDesc.invkind)
            {
                FuncNamePrefix = MethodInterface.MethodDescription.INVOKE_PROPERTYPUTREF_PREFIX;
                // For putref, no parameter name is returned, so let's add one
                if (iNamesCount + 1 == oParameterNames.Length)
                    oParameterNames[iNamesCount++] = "value";
            }

            oReturnType = null;
            // Set the return type.  stFuncDesc.elemdescFunc SHOULD hold the return type, however it is completely unreliable
            // - it might not, if the ComTypes.PARAMFLAG.PARAMFLAG_FRETVAL is not set, then we are suspicious
            if ( (stFuncDesc.elemdescFunc.desc.paramdesc.wParamFlags & ComTypes.PARAMFLAG.PARAMFLAG_FRETVAL) == ComTypes.PARAMFLAG.PARAMFLAG_FRETVAL)
                oReturnType = new ParamDescHelper(oTypeInfo, stFuncDesc.elemdescFunc, FuncName + "Result");
            // - if this actually contains some type information then it probably is the return type
            else if(stFuncDesc.elemdescFunc.tdesc.vt != 0)
                oReturnType = new ParamDescHelper(oTypeInfo, stFuncDesc.elemdescFunc, FuncName + "Result");
            // - if however this is a GET method, we NEED a return type, so set it here, and we can override it later if need be
            else if (ComTypes.INVOKEKIND.INVOKE_PROPERTYGET == stFuncDesc.invkind)
                oReturnType = new ParamDescHelper(oTypeInfo, stFuncDesc.elemdescFunc, FuncName + "Result");

            // Reality check (thanks to RealPlayer), PUT methods cannot have return values
            if ((ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT == stFuncDesc.invkind) && (null != oReturnType))
            {
                Log.Write(MethodBase.GetCurrentMethod(), "Found PUT method '" + FuncName + "' that has a return value.  Ignoring this return value", Log.LogType.LogOnlyInfo);
                oReturnType = null;
            }

            // Get the Parameter Descriptions 
            for (int j = 1; j < iNamesCount; j++)
            {
                ComTypes.ELEMDESC oElemDesc = (ComTypes.ELEMDESC)Marshal.PtrToStructure((IntPtr)((int)stFuncDesc.lprgelemdescParam + (j - 1) * Marshal.SizeOf(typeof(ComTypes.ELEMDESC))), typeof(ComTypes.ELEMDESC));

                // If we haven't set the return type, check to see if we can use one of the parameters
                if ((oElemDesc.desc.paramdesc.wParamFlags & ComTypes.PARAMFLAG.PARAMFLAG_FRETVAL) == ComTypes.PARAMFLAG.PARAMFLAG_FRETVAL)
                {
                    oReturnType = new ParamDescHelper(oTypeInfo, oElemDesc, oParameterNames[j]);
                    continue;
                }

                // Add the parameter
                ParamDescHelper oParamDescHelper = new ParamDescHelper(oTypeInfo, oElemDesc, oParameterNames[j]);
                oParamDescs.Add(oParamDescHelper);
                // Add to the parameter order
                oParameterOrder.Add(oParameterNames[j]);
            }
        }

        ~FuncDescHelper()
        {
            if (pFuncDesc != IntPtr.Zero)
                try
                {
                    oTypeInfo.ReleaseFuncDesc(pFuncDesc);
                }
                catch { }
        }

        /// <summary>
        /// Gets the name of the function
        /// </summary>
        public String Name
        {
            get
            {
                return FuncName;
            }
        }

        /// <summary>
        /// Gets a prefix for the name depending if the function is a 'get', 'put' or 'putref'.  For a standard function the prefix
        /// is an empty string.
        /// </summary>
        public String NamePrefix
        {
            get
            {
                return FuncNamePrefix;
            }
        }

        /// <summary>
        /// Gets an ordered List&lt;string> of the parameter names
        /// </summary>
        public List<string> ParameterOrder
        {
            get
            {
                return oParameterOrder;
            }
        }

        /// <summary>
        /// Gets a list of the parameters of the function
        /// </summary>
        public List<ParamDescHelper> Parameters
        {
            get
            {
                return oParamDescs;
            }
        }

        /// <summary>
        /// Gets the parameter the function returns
        /// </summary>
        public ParamDescHelper ReturnParam
        {
            get
            {
                return oReturnType;
            }
        }
    }
}
