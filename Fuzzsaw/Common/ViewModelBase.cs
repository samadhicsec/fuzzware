using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using Fuzzware.Common;

namespace Fuzzware.Fuzzsaw.Common
{
    public abstract class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region Dependency Properties

        static readonly DependencyProperty ShowProperty = DependencyProperty.Register("Show", typeof(bool), typeof(ViewModelBase));
        /// <summary>
        /// Whether or not to show the view associated with this view model
        /// </summary>
        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }

        #endregion

        #region Logging Aides
        protected StringBuilder m_oLogOutput;
        protected Log.LogType m_eLogType;
        protected virtual void LogUpdated(object sender, LogEventArgs a)
        {
            if(a.LogType >= m_eLogType)
                m_oLogOutput.AppendLine(a.Message);
        }

        /// <summary>
        /// Captures anything written to Fuzzware.Common.Log at a level of Warning or higher
        /// </summary>
        protected void CaptureLog()
        {
            CaptureLog(Log.LogType.Warning);
        }

        /// <summary>
        /// Captures anything written to Fuzzware.Common.Log at a level of eLogType or higher
        /// </summary>
        protected void CaptureLog(Log.LogType eLogType)
        {
            m_oLogOutput = new StringBuilder();
            m_eLogType = eLogType;
            Log.LogEvent += LogUpdated;
        }

        /// <summary>
        /// Stops a Log capture started with CaptureLog()
        /// </summary>
        protected void StopCapturingLog()
        {
            Log.LogEvent -= LogUpdated;
        }
        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members
    }
}
