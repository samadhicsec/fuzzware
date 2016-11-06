using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Common.MethodInterface;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    /// <summary>
    /// The View Model for specifying default values for interfaces
    /// </summary>
    public class DefaultValueViewModel : ViewModelBase
    {
        #region Dependency Properties
        #region Method
        /// <summary>
        /// Method DependencyProperty
        /// </summary>
        public static DependencyProperty MethodProperty =
                DependencyProperty.Register("Method", typeof(MethodInfoViewModel), typeof(DefaultValueViewModel));

        public MethodInfoViewModel Method
        {
            get { return (MethodInfoViewModel)GetValue(MethodProperty); }
            set { SetValue(MethodProperty, value); }
        }
        #endregion

        #region Parameter
        /// <summary>
        /// ParameterName DependencyProperty
        /// </summary>
        public static DependencyProperty ParameterProperty =
                DependencyProperty.Register("Parameter", typeof(ParamDescViewModel), typeof(DefaultValueViewModel));

        public ParamDescViewModel Parameter
        {
            get { return (ParamDescViewModel)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }
        #endregion

        #region ParameterCondition
        /// <summary>
        /// ParameterCondition DependencyProperty
        /// </summary>
        public static DependencyProperty ParameterConditionProperty =
                DependencyProperty.Register("ParameterCondition", typeof(string), typeof(DefaultValueViewModel));

        public string ParameterCondition
        {
            get { return (string)GetValue(ParameterConditionProperty); }
            set { SetValue(ParameterConditionProperty, value); }
        }
        #endregion

        #region ConditionValue
        /// <summary>
        /// ConditionValue DependencyProperty
        /// </summary>
        public static DependencyProperty ConditionValueProperty =
                DependencyProperty.Register("ConditionValue", typeof(string), typeof(DefaultValueViewModel));

        public string ConditionValue
        {
            get { return (string)GetValue(ConditionValueProperty); }
            set { SetValue(ConditionValueProperty, value); }
        }
        #endregion

        #region DefaultValue
        /// <summary>
        /// DefaultValue DependencyProperty
        /// </summary>
        public static DependencyProperty DefaultValueProperty =
                DependencyProperty.Register("DefaultValue", typeof(string), typeof(DefaultValueViewModel));

        public string DefaultValue
        {
            get { return (string)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }
        #endregion
        #endregion

        #region Public Properties
        public static string[] Conditions
        {
            get { return m_Conditions; }
        }

        /// <summary>
        /// The collection of all methods of a library
        /// </summary>
        public ObservableCollection<MethodInfoViewModel> AllMethodsWithInputParameters
        {
            get { return m_oAllMethodsWithInputParameters; }
        }

        /// <summary>
        /// The collection of all input parameters of all methods of a library
        /// </summary>
        public ObservableCollection<ParamDescViewModel> AllInputParams
        {
            get { return m_oAllInputParams; }
        }
        #endregion
        static string[] m_Conditions = { "Named method/parameter: ",
                                              "Any parameter name containing ",
                                              "Any parameter name starting with ",
                                              "Any parameter name ending with "};
        
        public const string ALL_METHODS = "(All Methods)";
        public const string ALL_PARAMETERS = "(All Parameters)";

        public const String prefix = LibraryDescription.PREFIX;
        public const String AllMethodsAllParameters = @"//*";
        //public readonly String AllMethodsSpecificParameter = @"//" + prefix + @":{0}";
        //public readonly String SpecificMethodAllParameters = @"//" + prefix + @":{0}/*";
        //public readonly String SpecificMethodAndParameter = @"//" + prefix + @":{0}/" + prefix + @":{1}";
        public readonly String AllMethodsSpecificParameter = @"//*[(local-name(.) = '{0}')]";
        public readonly String SpecificMethodAllParameters = @"//*[(local-name(..) = '{0}')]";
        public readonly String SpecificMethodAndParameter = @"//*[(local-name(..) = '{0}') and (local-name(.) = '{1}')]";
        public const String ContainsParameter = @"//*[contains(local-name(.), '{0}')]";
        public const String StartsWithParameter = @"//*[starts-with(local-name(.), '{0}')]";
        public const String EndsWithParameter = @"//*[ends-with(local-name(.), '{0}')]";

        protected ObservableCollection<MethodInfoViewModel> m_oAllMethodsWithInputParameters;
        protected ObservableCollection<ParamDescViewModel> m_oAllInputParams;

        public DefaultValueViewModel(ObservableCollection<MethodInfoViewModel> AllMethodsWithInputParameters, ObservableCollection<ParamDescViewModel> AllInputParams)
        {
            m_oAllMethodsWithInputParameters = AllMethodsWithInputParameters;
            m_oAllInputParams = AllInputParams;

            Method = new MethodInfoViewModel(ALL_METHODS, m_oAllInputParams);
            ParameterDesc oParameterDesc = new ParameterDesc();
            oParameterDesc.Name = ALL_PARAMETERS;
            Parameter = new ParamDescViewModel(oParameterDesc);
            ParameterCondition = Conditions[0];
            ConditionValue = "";
            DefaultValue = "";
        }

        public string GetXPath()
        {
            if (ParameterCondition.Equals(Conditions[0]))
            {
                bool bIsAllMethods = Method.MethodName.Equals(ALL_METHODS);
                bool bIsAllParams = Parameter.ParamDesc.Name.Equals(ALL_PARAMETERS);
                if (bIsAllMethods && bIsAllParams)
                    return AllMethodsAllParameters;
                else if (bIsAllMethods && !bIsAllParams)
                    return String.Format(AllMethodsSpecificParameter, Parameter.ParamDesc.Name);
                else if (!bIsAllMethods && bIsAllParams)
                    return String.Format(SpecificMethodAllParameters, Method.MethodName);
                else if (!bIsAllMethods && !bIsAllParams)
                    return String.Format(SpecificMethodAndParameter, Method.MethodName, Parameter.ParamDesc.Name);
            }
            else if (ParameterCondition.Equals(Conditions[1]))
                return String.Format(ContainsParameter, ConditionValue);
            else if (ParameterCondition.Equals(Conditions[2]))
                return String.Format(StartsWithParameter, ConditionValue);
            else if (ParameterCondition.Equals(Conditions[3]))
                return String.Format(EndsWithParameter, ConditionValue);
            return "";
        }

        /// <summary>
        /// Populate this View Model from a XPath expression
        /// </summary>
        public bool SetFromXPath(String XPath, String XPathValue)
        {
            try
            {
                String MatchValue;
                int ConditionIndex = -1;
                if (XPath.StartsWith("//*[contains"))
                    ConditionIndex = 1;
                else if (XPath.StartsWith("//*[starts-with"))
                    ConditionIndex = 2;
                else if (XPath.StartsWith("//*[ends-with"))
                    ConditionIndex = 3;
                // If it's one of the 3 above
                if ((-1 != ConditionIndex) && (-1 != XPath.IndexOf('\'')) && (-1 != XPath.LastIndexOf('\'')))
                {
                    // Find the first and last quote characters
                    MatchValue = XPath.Substring(XPath.IndexOf('\'') + 1, XPath.LastIndexOf('\'') - (XPath.IndexOf('\'') + 1));
                    ParameterCondition = Conditions[ConditionIndex];
                    ConditionValue = MatchValue;
                    DefaultValue = XPathValue;
                }
                // The odd All Methods and All Parameters option
                else if (XPath.Equals(DefaultValueViewModel.AllMethodsAllParameters))
                {
                    ParameterCondition = DefaultValueViewModel.Conditions[0];
                    Method = new MethodInfoViewModel(DefaultValueViewModel.ALL_METHODS, m_oAllInputParams);
                    Parameter = new ParamDescViewModel(DefaultValueViewModel.ALL_PARAMETERS);
                    DefaultValue = XPathValue;
                }
                // It's a combination of method and parameter
                //else if (-1 != XPath.IndexOf(prefix))
                //{
                //    ParameterCondition = Conditions[0];
                //    // Specifc Parameter is in use
                //    if (XPath.IndexOf(prefix) == XPath.LastIndexOf(prefix))
                //    {
                //        // Only parameter is specified
                //        Parameter = GetParameter(XPath.Substring(XPath.IndexOf(prefix) + prefix.Length + 1));
                //    }
                //    else
                //    {
                //        // Method and parameter is specified
                //        int MethodStartIndex = XPath.IndexOf(prefix) + prefix.Length + 1;
                //        Method = GetMethod(XPath.Substring(MethodStartIndex, XPath.LastIndexOf('/') - MethodStartIndex));
                //        Parameter = GetParameter(XPath.Substring(XPath.LastIndexOf(prefix) + prefix.Length + 1));
                //    }
                //    DefaultValue = XPathValue;
                //}
                else
                {
                    ParameterCondition = Conditions[0];
                    Method = new MethodInfoViewModel(DefaultValueViewModel.ALL_METHODS, m_oAllInputParams);
                    Parameter = new ParamDescViewModel(DefaultValueViewModel.ALL_PARAMETERS);
                    // Try to find the method
                    const string strMethodMatch = "local-name(..) = '";
                    int index = XPath.IndexOf(strMethodMatch);
                    if ((-1 != index) && (-1 != XPath.IndexOf('\'', index + strMethodMatch.Length + 1)))
                    {
                        Method = GetMethod(XPath.Substring(index + strMethodMatch.Length, XPath.IndexOf('\'', index + strMethodMatch.Length + 1) - (index + strMethodMatch.Length)));
                    }
                    // Try to find parameter
                    const string strParamMatch = "local-name(.) = '";
                    index = XPath.IndexOf(strParamMatch);
                    if ((-1 != index) && (-1 != XPath.IndexOf('\'', index + strParamMatch.Length + 1)))
                    {
                        Parameter = GetParameter(XPath.Substring(index + strParamMatch.Length, XPath.IndexOf('\'', index + strParamMatch.Length + 1) - (index + strParamMatch.Length)));
                    }
                    DefaultValue = XPathValue;
                }
            }
            catch 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to find the MethodInfoViewModel matching the name
        /// </summary>
        private MethodInfoViewModel GetMethod(string MethodName)
        {
            if (null != m_oAllMethodsWithInputParameters)
            {
                for (int i = 0; i < m_oAllMethodsWithInputParameters.Count; i++)
                {
                    if (MethodName.Equals(m_oAllMethodsWithInputParameters[i].MethodName))
                        return m_oAllMethodsWithInputParameters[i];
                }
            }
            return new MethodInfoViewModel(MethodName, (IEnumerable<ParamDescViewModel>)null);
        }

        /// <summary>
        /// Tries to find the ParamDescViewModel matching the name
        /// </summary>
        private ParamDescViewModel GetParameter(string ParamName)
        {
            if (null != m_oAllInputParams)
            {
                for (int i = 0; i < m_oAllInputParams.Count; i++)
                {
                    if (ParamName.Equals(m_oAllInputParams[i].ParamDesc.Name))
                        return m_oAllInputParams[i];
                }
            }
            return new ParamDescViewModel(ParamName);
        }
    }
}
