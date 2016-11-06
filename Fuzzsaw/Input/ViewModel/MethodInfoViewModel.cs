using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Common.MethodInterface;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    /// <summary>
    /// A convenience class for dealing with MethodDescriptions
    /// </summary>
    public class MethodInfoViewModel : ViewModelBase
    {
        protected String m_MethodName;
        protected MethodDescription m_oMethodDescription;
        protected List<ParamDescViewModel> m_oInParameters;
        
        /// <summary>
        /// Create the View Model through a name and MethodDescription object
        /// </summary>
        public MethodInfoViewModel(String methodName, MethodDescription methodDescription)
        {
            m_MethodName = methodName;
            m_oMethodDescription = methodDescription;

            m_oInParameters = new List<ParamDescViewModel>();
            for (int i = 0; i < oMethodDescription.ParameterDescs.Count; i++)
            {
                ParameterDesc oParameterDesc = oMethodDescription.ParameterDescs[i];
                if (oParameterDesc.ParamDirection == eParamDirection.In)
                    m_oInParameters.Add(new ParamDescViewModel(oParameterDesc));
            }
        }

        /// <summary>
        /// Create the View Model through a name and list of Params
        /// </summary>
        public MethodInfoViewModel(String methodName, IEnumerable<ParamDescViewModel> Params)
        {
            m_MethodName = methodName;
            m_oInParameters = new List<ParamDescViewModel>();
            if(null != Params)
                m_oInParameters.AddRange(Params);
        }

        public string MethodName
        {
            get { return m_MethodName; }
        }
        public MethodDescription oMethodDescription
        {
            get { return m_oMethodDescription; }
        }
        public List<ParamDescViewModel> InParameters
        {
            get { return m_oInParameters; }
        }
        public override string ToString()
        {
            return m_MethodName;
        }
    }
}
