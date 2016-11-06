using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.Interop
{
    public class ParamDescHelper
    {
        ComTypes.ITypeInfo oTypeInfo;
        ComTypes.ELEMDESC stElemDesc;

        String strParamName;
        bool bIsOutParam;

        public ParamDescHelper(ComTypes.ITypeInfo ITypeInfo, ComTypes.ELEMDESC ElemDesc, String ParamName)
        {
            oTypeInfo = ITypeInfo;
            stElemDesc = ElemDesc;
            strParamName = ParamName;

            if ((stElemDesc.desc.paramdesc.wParamFlags & ComTypes.PARAMFLAG.PARAMFLAG_FOUT) == ComTypes.PARAMFLAG.PARAMFLAG_FOUT)
                bIsOutParam = true;
            else
                bIsOutParam = false;
        }

        /// <summary>
        /// Gets the name of the Parameter
        /// </summary>
        public String ParamName
        {
            get
            {
                return strParamName;
            }
        }

        /// <summary>
        /// Gets whether the parameter is an out parameter
        /// </summary>
        public bool IsOutParam
        {
            get
            {
                return bIsOutParam;
            }
        }

        /// <summary>
        /// Adds XML Schema types describing this PARAMDESC to an existing XML Schema 
        /// </summary>
        /// <param name="oXmlSchema"></param>
        public XmlQualifiedName AddToSchema(XmlSchema oXmlSchema, TypeLibHelper oTypeLibHelper)
        {
            return TypeDescHelper.CreateSchemaTypeFromTYPEDESC(oXmlSchema, oTypeLibHelper, oTypeInfo, stElemDesc.tdesc);
        }
    }
}
