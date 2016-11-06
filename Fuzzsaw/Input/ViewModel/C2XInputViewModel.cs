﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Fuzzware.Fuzzsaw;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Fuzzsaw.Common.ViewModel;
using Fuzzware.Schemas.AutoGenerated;
using System.Xml;
using System.Xml.Schema;
using Fuzzware.Common;
using Fuzzware.Common.Encoding;
using Fuzzware.Common.XML;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    public class C2XInputViewModel : InputBaseViewModel, IInputHandler
    {
        #region Dependency Properties
        static readonly DependencyProperty XSDFilesProperty = DependencyProperty.Register("XSDFiles", typeof(FilesControlViewModel), typeof(C2XInputViewModel));
        public FilesControlViewModel XSDFiles
        {
            get { return (FilesControlViewModel)GetValue(XSDFilesProperty); }
            set { SetValue(XSDFilesProperty, value); }
        }

        static readonly DependencyProperty NodesProperty = DependencyProperty.Register("Nodes", typeof(ObservableCollection<object>), typeof(C2XInputViewModel));
        public ObservableCollection<object> Nodes
        {
            get { return (ObservableCollection<object>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        static readonly DependencyProperty RootNodeProperty = DependencyProperty.Register("RootNode", typeof(XmlQualifiedName), typeof(C2XInputViewModel));
        public XmlQualifiedName RootNode
        {
            get { return (XmlQualifiedName)GetValue(RootNodeProperty); }
            set { SetValue(RootNodeProperty, value); }
        }

        static readonly DependencyProperty RawFileProperty = DependencyProperty.Register("RawFile", typeof(FileControlViewModel), typeof(C2XInputViewModel));
        public FileControlViewModel RawFile
        {
            get { return (FileControlViewModel)GetValue(RawFileProperty); }
            set { SetValue(RawFileProperty, value); }
        }
        #endregion

        #region Commands

        #region UpdateNodes
        RelayCommand m_oUpdateNodesCommand;

        public ICommand UpdateNodesCommand
        {
            get
            {
                if (null == m_oUpdateNodesCommand)
                    m_oUpdateNodesCommand = new RelayCommand(UpdateNodesExecute);
                return m_oUpdateNodesCommand;
            }
        }

        public void UpdateNodesExecute()
        {
            // Check if the schemas have been changed since last time
            if (!SchemasChanged())
                return;

            // Complain if there are no schemas
            if (0 == XSDFiles.Filenames.Count)
            {
                ShowErrorInComboBox("Please specify input XML Schemas in order to populate this list");
                return;
            }

            try
            {
                // Capture errors written to the log
                CaptureLog();
                // Store the paths so we can detect a change
                oSchemas = XSDFiles.Filenames.ToArray<string>();
                // Make sure all the paths are fully qualified
                string[] oFQSchemas = XSDFiles.Filenames.ToArray<string>();
                for (int i = 0; i < oFQSchemas.Length; i++)
                    if (!System.IO.Path.IsPathRooted(oFQSchemas[i]))
                        oFQSchemas[i] = System.IO.Path.Combine(Fuzzsaw.ProjectDirectory, oFQSchemas[i]);

                // Recompile the schemas to get the list of elements
                List<string> oTargetNamespaces = new List<string>();
                XmlSchemaSet oXmlSchemaSet = null;
                try
                {
                    oXmlSchemaSet = XMLHelper.LoadAndCompileSchema(oFQSchemas, null, out oTargetNamespaces);
                }
                catch { }   // If there is an error it will be written to our Log stream

                Nodes.Clear();
                if ((null != oXmlSchemaSet) && (oXmlSchemaSet.GlobalElements.Values.Count > 0))
                {
                    // Add all the elements to the ComboBox
                    foreach (XmlSchemaObject oSchemaObj in oXmlSchemaSet.GlobalElements.Values)
                    {
                        XmlSchemaElement oSchemaElement = oSchemaObj as XmlSchemaElement;
                        if (null != oSchemaElement)
                        {
                            Nodes.Add(oSchemaElement.QualifiedName);
                        }
                    }
                }
                else
                {
                    ShowErrorInComboBox("An error occurred compiling the input XML Schemas" + Environment.NewLine + m_oLogOutput.ToString());
                }
            }
            catch (Exception err)
            {
                // Make we aren't listening on the Log anymore
                ShowErrorInComboBox(err.Message);
            }
            StopCapturingLog();

        }
        #endregion

        #region TestConversion
        RelayCommand<Window> m_oTestConversionCommand;

        public ICommand TestConversionCommand
        {
            get
            {
                if (null == m_oTestConversionCommand)
                    m_oTestConversionCommand = new RelayCommand<Window>(TestConversionExecute);
                return m_oTestConversionCommand;
            }
        }

        /// <summary>
        /// Open the test conversion window
        /// </summary>
        public void TestConversionExecute(Window oWindow)
        {
            // TODO: Is there are way to ensure only 1 window is open?  Currently this command is invoked with a new window object each time
            if (null == oWindow)
                return;

            Convert2XMLInput oConvert2XMLInput = this.DataInputHandler.Item as Convert2XMLInput;
            Encoding oOutputEncoding = EncodingHelper.LoadEncoding((App.Current.MainWindow.DataContext as MainWindowViewModel).CurrentProject.Config.Output.ConvertFromXML.OutputEncoding);
            TestC2XWindowViewModel oTestC2XWindowViewModel = new TestC2XWindowViewModel(oConvert2XMLInput, oOutputEncoding);
            oWindow.DataContext = oTestC2XWindowViewModel;
            oWindow.Show();
        }
        #endregion

        #endregion

        protected string[] oSchemas;

        protected void ShowErrorInComboBox(String error)
        {
            Nodes.Clear();
            string[] sep = {Environment.NewLine};
            string[] errorlines = error.Split(sep, StringSplitOptions.None);
            for(int i = 0; i < errorlines.Length; i++)
                Nodes.Add(errorlines[i]);
            oSchemas = null;
        }

        /// <summary>
        /// Return true if the stored schemas differ from the ones listed in the control
        /// </summary>
        protected bool SchemasChanged()
        {
            if (null == oSchemas)
            {
                oSchemas = XSDFiles.Filenames.ToArray<string>();
                return true;
            }
            if (oSchemas.Length != XSDFiles.Filenames.Count)
            {
                oSchemas = XSDFiles.Filenames.ToArray<string>();
                return true;
            }
            for (int i = 0; i < oSchemas.Length; i++)
            {
                if (!oSchemas[i].Equals(XSDFiles.Filenames[i]))
                {
                    oSchemas = XSDFiles.Filenames.ToArray<string>();
                    return true;
                }
            }
            return false;
        }

        public C2XInputViewModel()
        {
            XSDFiles = new FilesControlViewModel();
            XSDFiles.Title = "Choose input XSDs";
            XSDFiles.DefaultExtension = ".xsd";
            XSDFiles.Filter = "XSD files (.xsd)|*.xsd";
            XSDFiles.UseRelativePaths = true;
            XSDFiles.Filenames = new ObservableCollection<string>();

            RawFile = new FileControlViewModel();
            RawFile.Title = "Choose the file to convert";
            RawFile.DefaultExtension = "";
            RawFile.Filter = "";
            RawFile.UseRelativePaths = true;

            Nodes = new ObservableCollection<object>();
        }

        #region IInputHandler Members

        Type IInputHandler.GetDataInputHandlerItemType()
        {
            return typeof(Convert2XMLInput);
        }

        public DataInputHandler DataInputHandler
        {
            get
            {
                Convert2XMLInput oC2XInput = new Convert2XMLInput();
                oC2XInput.XSDPathAndFilename = XSDFiles.Filenames.ToArray<string>();
                oC2XInput.Convert2XML = new Fuzzware.Schemas.AutoGenerated.Convert2XML();
                oC2XInput.Convert2XML.SourceFile = RawFile.Filename;
                if ((null != RootNode) && !String.IsNullOrEmpty(RawFile.Filename))
                {
                    oC2XInput.Convert2XML.RootNodeName = RootNode.Name;
                    oC2XInput.Convert2XML.RootNodeNamespace = RootNode.Namespace;
                    oC2XInput.Convert2XML.OutputNamespacePrefix = "c2x";
                    oC2XInput.Convert2XML.SourceFile = RawFile.Filename;
                    oC2XInput.Convert2XML.OutputXMLFile = RawFile.Filename + ".xml";
                }
                m_oDataInputHandler.Item = oC2XInput;
                return m_oDataInputHandler;
            }
            set
            {
                if (!(value.Item is Convert2XMLInput))
                    return;

                m_oDataInputHandler = value;
                Convert2XMLInput oC2XInput = (Convert2XMLInput)m_oDataInputHandler.Item;
                XSDFiles.Filenames.Clear();
                if(null != oC2XInput.XSDPathAndFilename)
                    foreach(String Filename in oC2XInput.XSDPathAndFilename)
                        XSDFiles.Filenames.Add(Filename);

                // Update the available nodes for selection as the RootNode.  We need to do this as otherwise we cannot
                // bind RootNode to the combobox SelectedItem as it won't exist in the list
                if (XSDFiles.Filenames.Count > 0)
                    UpdateNodesExecute();
                
                if (null != oC2XInput.Convert2XML)
                {
                    RawFile.Filename = oC2XInput.Convert2XML.SourceFile;
                    RootNode = new XmlQualifiedName(oC2XInput.Convert2XML.RootNodeName, oC2XInput.Convert2XML.RootNodeNamespace);
                }
            }
        }

        #endregion
    }
}
