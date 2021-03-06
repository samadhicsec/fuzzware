﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common.ViewModel;
using Fuzzware.Schemas.AutoGenerated;

namespace Fuzzware.Fuzzsaw.Input.ViewModel
{
    public class XMLInputViewModel : InputBaseViewModel, IInputHandler
    {
        #region Dependency Properties

        static readonly DependencyProperty XSDFilesProperty = DependencyProperty.Register("XSDFiles", typeof(FilesControlViewModel), typeof(XMLInputViewModel));
        public FilesControlViewModel XSDFiles
        {
            get { return (FilesControlViewModel)GetValue(XSDFilesProperty); }
            set { SetValue(XSDFilesProperty, value); }
        }

        static readonly DependencyProperty XMLFileProperty = DependencyProperty.Register("XMLFile", typeof(FileControlViewModel), typeof(XMLInputViewModel));
        public FileControlViewModel XMLFile
        {
            get { return (FileControlViewModel)GetValue(XMLFileProperty); }
            set { SetValue(XMLFileProperty, value); }
        }

        #endregion

        public XMLInputViewModel()
        {
            XSDFiles = new FilesControlViewModel();
            XSDFiles.Title = "Choose input XSDs";
            XSDFiles.DefaultExtension = ".xsd";
            XSDFiles.Filter = "XSD files (.xsd)|*.xsd";
            XSDFiles.UseRelativePaths = true;
            XSDFiles.Filenames = new ObservableCollection<string>();

            XMLFile = new FileControlViewModel();
            XMLFile.Title = "Choose the input XML";
            XMLFile.DefaultExtension = ".xml";
            XMLFile.Filter = "XML files (.xml)|*.xml";
            XMLFile.UseRelativePaths = true;
        }

        #region IInputHandler Members

        Type IInputHandler.GetDataInputHandlerItemType()
        {
            return typeof(XMLFileInput);
        }

        public DataInputHandler DataInputHandler
        {
            get
            {
                XMLFileInput oXMLFileInput = new XMLFileInput();
                oXMLFileInput.XMLPathAndFilename = XMLFile.Filename;
                oXMLFileInput.XSDPathAndFilename = XSDFiles.Filenames.ToArray<String>();
                m_oDataInputHandler.Item = oXMLFileInput;

                return m_oDataInputHandler;
            }
            set
            {
                if (!(value.Item is XMLFileInput))
                    return;

                m_oDataInputHandler = value;

                XMLFileInput oXMLFileInput = (m_oDataInputHandler.Item as XMLFileInput);
                XMLFile.Filename = oXMLFileInput.XMLPathAndFilename;
                XSDFiles.Filenames.Clear();
                for (int i = 0; (null != oXMLFileInput.XSDPathAndFilename) && (i < oXMLFileInput.XSDPathAndFilename.Length); i++)
                    if(!String.IsNullOrEmpty(oXMLFileInput.XSDPathAndFilename[i]))
                        XSDFiles.Filenames.Add(oXMLFileInput.XSDPathAndFilename[i]);
            }
        }

        #endregion
    }
}
