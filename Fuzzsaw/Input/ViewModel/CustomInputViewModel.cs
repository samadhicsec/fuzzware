﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common.ViewModel;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    public class CustomInputViewModel : InputBaseViewModel, IInputHandler
    {
        #region Dependency Properties

        static readonly DependencyProperty CustomCodeProperty = DependencyProperty.Register("CustomCode", typeof(UserDefinedCodeControlViewModel), typeof(CustomInputViewModel));
        public UserDefinedCodeControlViewModel CustomCode
        {
            get { return (UserDefinedCodeControlViewModel)GetValue(CustomCodeProperty); }
            set { SetValue(CustomCodeProperty, value); }
        }

        #endregion

        public CustomInputViewModel()
        {
            CustomCode = new UserDefinedCodeControlViewModel();
        }

        #region IInputHandler Members

        public Type GetDataInputHandlerItemType()
        {
            return typeof(Fuzzware.Schemas.AutoGenerated.UserDefinedCode);
        }

        public Fuzzware.Schemas.AutoGenerated.DataInputHandler DataInputHandler
        {
            get
            {
                m_oDataInputHandler.Item = CustomCode.GetUserDefinedCode();

                return m_oDataInputHandler;
            }
            set
            {
                if (!(value.Item is Fuzzware.Schemas.AutoGenerated.UserDefinedCode))
                    return;

                m_oDataInputHandler = value;
                CustomCode = new UserDefinedCodeControlViewModel((Fuzzware.Schemas.AutoGenerated.UserDefinedCode)m_oDataInputHandler.Item);
            }
        }

        #endregion
    }
}
