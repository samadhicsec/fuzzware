using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Schema;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace Fuzzware.Common.MethodInterface
{
    public enum eInvokeKind
    {
        INVOKE_FUNC = ComTypes.INVOKEKIND.INVOKE_FUNC,
        INVOKE_PROPERTYGET = ComTypes.INVOKEKIND.INVOKE_PROPERTYGET,
        INVOKE_PROPERTYPUT = ComTypes.INVOKEKIND.INVOKE_PROPERTYPUT,
        INVOKE_PROPERTYPUTREF = ComTypes.INVOKEKIND.INVOKE_PROPERTYPUTREF,
    }

    public enum eParamDirection
    {
        In = ComTypes.PARAMFLAG.PARAMFLAG_FIN,
        Out = ComTypes.PARAMFLAG.PARAMFLAG_FOUT,
    }

    public class ParameterDesc
    {
        public String Name;
        public XmlSchemaElement ParamSchemaElement;
        public eParamDirection ParamDirection;
    }

    public class MethodDescription
    {
        public const string INVOKE_PROPERTYGET_PREFIX = "get_";
        public const string INVOKE_PROPERTYPUT_PREFIX = "put_";
        public const string INVOKE_PROPERTYPUTREF_PREFIX = "putref_";

        protected String m_MethodName;
        protected String inputMessageNamespace;
        public String OriginalMethodName;
        public String Id;
        public bool bUsesWrapperElement;
        public eInvokeKind InvokeKind = eInvokeKind.INVOKE_FUNC;
        public ParameterDesc ReturnDesc;
        public List<ParameterDesc> ParameterDescs;

        private int iInParameterCount;
        private bool bInParameterCount;
        private int iOutParameterCount;
        private bool bOutParameterCount;

        public String MethodName
        {
            get { return m_MethodName; }
            set { m_MethodName = value; }
        }
        public string InputMessageNamespace
        {
            get { return inputMessageNamespace; }
        }

        public int InParameterCount
        {
            get
            {
                if (!bInParameterCount)
                {
                    if (null == ParameterDescs)
                        Log.Write(MethodBase.GetCurrentMethod(), "Paremeter list has not been initialised", Log.LogType.Error);
                    iInParameterCount = 0;
                    for(int i = 0; i < ParameterDescs.Count; i++)
                    {
                        if(ParameterDescs[i].ParamDirection == eParamDirection.In)
                            iInParameterCount++;
                    }
                    bInParameterCount = true;
                }
                return iInParameterCount;
            }
        }

        public int OutParameterCount
        {
            get
            {
                if (!bOutParameterCount)
                {
                    if (null == ParameterDescs)
                        Log.Write(MethodBase.GetCurrentMethod(), "Paremeter list has not been initialised", Log.LogType.Error);
                    iOutParameterCount = 0;
                    for (int i = 0; i < ParameterDescs.Count; i++)
                    {
                        if (ParameterDescs[i].ParamDirection == eParamDirection.Out)
                            iOutParameterCount++;
                    }
                    bOutParameterCount = true;
                }
                return iOutParameterCount;
            }
        }
    }
}
