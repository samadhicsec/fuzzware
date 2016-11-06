using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Common.MethodInterface;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    /// <summary>
    /// A convenience class for dealing with ParamDescs
    /// </summary>
    public class ParamDescViewModel : ViewModelBase
    {
        protected ParameterDesc m_oParameterDesc;
        
        public ParamDescViewModel(ParameterDesc oParameterDesc)
        {
            m_oParameterDesc = oParameterDesc;
        }
        public ParamDescViewModel(string oParameterName)
        {
            m_oParameterDesc = new ParameterDesc();
            m_oParameterDesc.Name = oParameterName;
        }
        public ParameterDesc ParamDesc
        {
            get { return m_oParameterDesc; }
        }

        public override string ToString()
        {
            return m_oParameterDesc.Name;
        }
    }
}
