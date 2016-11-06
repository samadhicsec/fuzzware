﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using System.Diagnostics;
using Fuzzware.Fuzzsaw.Common;
using Fuzzware.Fuzzsaw.FuzzingConfig;
using Fuzzware.Schemas.AutoGenerated;
using Fuzzware.Schemer.AutoGenerated;

namespace Fuzzware.Fuzzsaw
{
    public class Project
    {
        private const String CONFIG_NS = "urn:Fuzzware.Schemas.Configuration";
        private const String STFC_NS = "urn:Fuzzware.Schemas.SimpleTypeFuzzerConfig";
        private const String CTFC_NS = "urn:Fuzzware.Schemas.ComplexTypeFuzzerConfig";

        private Configuration m_oConfig;
        private SimpleTypeFuzzerConfig m_oSTFC;
        private ComplexTypeFuzzerConfig m_oCTFC;
        //private STFCView m_oSTFConfig;
        //private CTFCView m_oCTFConfig;

        private string m_ProjectDirectory;
        private string m_ProjectName;

        public Project(String ProjDir, String Name)
        {
            m_ProjectDirectory = Path.GetFullPath(ProjDir);
            if (!Directory.Exists(m_ProjectDirectory))
                Directory.CreateDirectory(m_ProjectDirectory);

            // Since everything else relies on Fuzzsaw.ProjectDirectory, set this ASAP
            Fuzzsaw.ProjectDirectory = m_ProjectDirectory;

            if(String.IsNullOrEmpty(Name))
                m_ProjectName = m_ProjectDirectory.Substring(m_ProjectDirectory.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            else
                m_ProjectName = Name;
        }

        public Project(String ProjDir) : this(ProjDir, null) { }

        public string ProjectDirectory
        {
            get { return m_ProjectDirectory; }
        }

        public string ProjectName
        {
            get { return m_ProjectName; }
        }

        public Configuration Config
        {
            get { return m_oConfig; }
        }

        public SimpleTypeFuzzerConfig STFC
        {
            get { return m_oSTFC; }
            set { m_oSTFC = value; }
        }

        public ComplexTypeFuzzerConfig CTFC
        {
            get { return m_oCTFC; }
            set { m_oCTFC = value; }
        }

        public String ConfigLocation
        {
            get{ return Path.GetFullPath(Path.Combine(m_ProjectDirectory, Fuzzsaw.DefaultConfigurationFilename)); }
        }

        //public STFCView STFCView
        //{
        //    get { return m_oSTFConfig; }
        //}

        //public CTFCView CTFCView
        //{
        //    get { return m_oCTFConfig; }
        //}

        /// <summary>
        /// Creates a New Project
        /// </summary>
        /// <returns>True if opening was successful</returns>
        public bool New()
        {
            if (String.IsNullOrEmpty(m_ProjectDirectory) || String.IsNullOrEmpty(m_ProjectName))
                return false;

            // Create the new files
            if (!CreateConfigFiles())
                return false;

            return LoadConfigFiles();
        }

        /// <summary>
        /// Create Configuration.xml, copy it from the resources (along with Simple and Complex type configs)
        /// </summary>
        private bool CreateConfigFiles()
        {
            try
            {
                // Copy from resources directory
                if (!Directory.Exists(Fuzzsaw.ResourcesDirectory))
                    HelperCommands.ShowError.Execute(new ErrorHelper(this, "Resources directory '" + Fuzzsaw.ResourcesDirectory + "' does not exist", true), App.Current.MainWindow);

                // Get Configuration.xml
                String ConfigFile = Path.Combine(Fuzzsaw.ResourcesDirectory, Fuzzsaw.DefaultConfigurationFilename);
                if (!File.Exists(ConfigFile))
                    HelperCommands.ShowError.Execute(new ErrorHelper(this, "The default Configuration file '" + ConfigFile + "' does not exist", true), App.Current.MainWindow);

                File.Copy(ConfigFile, ConfigLocation, true);

                // Get SimpleTypeFuzzerConfig.xml
                ConfigFile = Path.Combine(Fuzzsaw.ResourcesDirectory, Fuzzsaw.DefaultSimpleTypeFuzzerConfigFilename);
                if (!File.Exists(ConfigFile))
                    HelperCommands.ShowError.Execute(new ErrorHelper(this, "The default SimpleTypeFuzzerConfig file '" + ConfigFile + "' does not exist", true), App.Current.MainWindow);

                File.Copy(ConfigFile, Path.Combine(m_ProjectDirectory, Fuzzsaw.DefaultSimpleTypeFuzzerConfigFilename), true);

                // Get ComplexTypeFuzzerConfig.xml
                ConfigFile = Path.Combine(Fuzzsaw.ResourcesDirectory, Fuzzsaw.DefaultComplexTypeFuzzerConfigFilename);
                if (!File.Exists(ConfigFile))
                    HelperCommands.ShowError.Execute(new ErrorHelper(this, "The default ComplexTypeFuzzerConfig file '" + ConfigFile + "' does not exist", true), App.Current.MainWindow);

                File.Copy(ConfigFile, Path.Combine(m_ProjectDirectory, Fuzzsaw.DefaultComplexTypeFuzzerConfigFilename), true);
            }
            catch (Exception e)
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, e.Message, false), App.Current.MainWindow);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Opens an existing Project
        /// </summary>
        /// <returns>True if opening was successful</returns>
        public bool Open()
        {
            if (String.IsNullOrEmpty(m_ProjectDirectory) || String.IsNullOrEmpty(m_ProjectName))
                return false;

            if (!File.Exists(ConfigLocation))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "There is no project in this directory as the file '" + ConfigLocation + "' does not exist", true), App.Current.MainWindow);
                return false;
            }

            return LoadConfigFiles();
        }

        /// <summary>
        /// Loads an existing or new project
        /// </summary>
        protected bool LoadConfigFiles()
        {
            // Load Configuration.xml
            String ConfigFilename = Path.GetFullPath(Path.Combine(m_ProjectDirectory, Fuzzsaw.DefaultConfigurationFilename));
            if(!File.Exists(ConfigFilename))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "File '" + ConfigFilename + "' does not exist", true), App.Current.MainWindow);
                return false;
            }
            m_oConfig = Common.Common.DeserializeFile<Configuration>(ConfigFilename);
            if (null == m_oConfig)
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "Could not deserialise file '" + ConfigFilename + "'", true), App.Current.MainWindow);
                return false;
            }
            
            // Load SimpleTypeFileConfig
            if ((null == m_oConfig.XmlConfigFiles) || String.IsNullOrEmpty(m_oConfig.XmlConfigFiles.SimpleTypeFuzzerXML))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "No SimpleTypeFuzzerConfig specified in the Configuration file", true), App.Current.MainWindow);
                return false;
            }
            String STFCFilename = m_oConfig.XmlConfigFiles.SimpleTypeFuzzerXML;
            if(!Path.IsPathRooted(STFCFilename))
                STFCFilename = Path.GetFullPath(Path.Combine(m_ProjectDirectory, STFCFilename));
            if (!File.Exists(STFCFilename))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "Cannot find file '" + STFCFilename + "'", true), App.Current.MainWindow);
                return false;
            }
            m_oSTFC = Common.Common.DeserializeFile<SimpleTypeFuzzerConfig>(STFCFilename);
            if (null == m_oSTFC)
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "Could not deserialise file '" + STFCFilename + "'", true), App.Current.MainWindow);
                return false;
            }
            //m_oSTFConfig = new STFCView(oSTFC);

            // Load ComplexTypeFileConfig
            if ((null == m_oConfig.XmlConfigFiles) || String.IsNullOrEmpty(m_oConfig.XmlConfigFiles.ComplexTypeFuzzerXML))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "No ComplexTypeFuzzerConfig specified in the Configuration", true), App.Current.MainWindow);
                return false;
            }
            String CTFCFilename = m_oConfig.XmlConfigFiles.ComplexTypeFuzzerXML;
            if (!Path.IsPathRooted(CTFCFilename))
                CTFCFilename = Path.GetFullPath(Path.Combine(m_ProjectDirectory, CTFCFilename));
            if (!File.Exists(CTFCFilename))
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "Cannot find file '" + CTFCFilename + "'", true), App.Current.MainWindow);
                return false;
            }
            m_oCTFC = Common.Common.DeserializeFile<ComplexTypeFuzzerConfig>(CTFCFilename);
            if (null == m_oCTFC)
            {
                HelperCommands.ShowError.Execute(new ErrorHelper(this, "Could not deserialise file '" + CTFCFilename + "'", true), App.Current.MainWindow);
                return false;
            }
            //m_oCTFConfig = new CTFCView(oCTFC);

            return true;
        }

        /// <summary>
        /// Saves this Project
        /// </summary>
        /// <returns>A string describing any errors that occurred</returns>
        public bool Save()
        {
            String Configtempfile = null;
            String STFCtempfile = null;
            String CTFCtempfile = null;
            try
            {
                // Save the Configuration file
                if (String.IsNullOrEmpty(ConfigLocation))
                    throw new Exception();
                Configtempfile = Common.Common.SerializeFile<Configuration>(m_oConfig, CONFIG_NS);
                if (String.IsNullOrEmpty(Configtempfile))
                    throw new Exception();

                // Save the SimpleTypesConfigFile
                String STFCFilename = m_oConfig.XmlConfigFiles.SimpleTypeFuzzerXML;
                if (String.IsNullOrEmpty(STFCFilename))
                    throw new Exception();
                if (!Path.IsPathRooted(STFCFilename))
                    STFCFilename = Path.GetFullPath(Path.Combine(m_ProjectDirectory, STFCFilename));
                //STFCtempfile = SerializeFile<SimpleTypeFuzzerConfig>(m_oSTFConfig.SimpleTypeConfig, STFC_NS);
                STFCtempfile = Common.Common.SerializeFile<SimpleTypeFuzzerConfig>(m_oSTFC, STFC_NS);
                if (String.IsNullOrEmpty(STFCtempfile))
                    throw new Exception();

                // Save the ComplexTypesConfigFile
                String CTFCFilename = m_oConfig.XmlConfigFiles.ComplexTypeFuzzerXML;
                if (String.IsNullOrEmpty(CTFCFilename))
                    throw new Exception();
                if (!Path.IsPathRooted(CTFCFilename))
                    CTFCFilename = Path.GetFullPath(Path.Combine(m_ProjectDirectory, CTFCFilename));
                //CTFCtempfile = SerializeFile<ComplexTypeFuzzerConfig>(m_oCTFConfig.ComplexTypeConfig, CTFC_NS);
                CTFCtempfile = Common.Common.SerializeFile<ComplexTypeFuzzerConfig>(m_oCTFC, CTFC_NS);
                if (String.IsNullOrEmpty(CTFCtempfile))
                    throw new Exception();

                // copy temp files to config file
                try
                {
                    // Move the temp file to the same directory as the target file, so they are on the same volume (otherwise 
                    // File.Replace won't work)
                    FileAndPathHelper.Move(Path.GetFullPath(Configtempfile), Path.GetFullPath(ConfigLocation));
                    FileAndPathHelper.Move(Path.GetFullPath(STFCtempfile), Path.GetFullPath(STFCFilename));
                    FileAndPathHelper.Move(Path.GetFullPath(CTFCtempfile), Path.GetFullPath(CTFCFilename));
                }
                catch (Exception e3)
                {
                    MessageBox.Show(e3.Message, "An error occurred over-writing a configuration file", MessageBoxButton.OK);
                    throw;
                }
            }
            catch
            {
                CleanUpTempFilesFromSave(Configtempfile, STFCtempfile, CTFCtempfile);
                return false;
            }
            return true;
        }

        private void CleanUpTempFilesFromSave(String ConfigTemp, String STFCTemp, String CTFCTemp)
        {
            try
            {
                if (!String.IsNullOrEmpty(ConfigTemp) && File.Exists(ConfigTemp))
                    File.Delete(ConfigTemp);
            }
            catch { }
            try
            {
                if (!String.IsNullOrEmpty(STFCTemp) && File.Exists(STFCTemp))
                    File.Delete(STFCTemp);
            }
            catch { }
            try
            {
                if (!String.IsNullOrEmpty(CTFCTemp) && File.Exists(CTFCTemp))
                    File.Delete(CTFCTemp);
            }
            catch { }
        }

        public void ExecuteFuzzer()
        {
            // Save the configuration file
            //Save();   // Should be saved already.
#if DEBUG
            String SchemerExe = Path.Combine("G:\\Tools\\Fuzzware\\Schemer\\bin\\Debug\\", "Schemer.exe");
#else
            String SchemerExe = Path.Combine(Fuzzsaw.InstallationDirectory, "Schemer.exe");
#endif
            // Make sure we can find the Schemer executable
            if (!File.Exists(SchemerExe))
            {
                System.Windows.MessageBox.Show("Could not find 'Schemer.exe' at '" + SchemerExe + "'", "Unable to locate Schemer.exe", MessageBoxButton.OK);
                return;
            }

            StringBuilder FmtCmdLine = new StringBuilder();
            // This tells cmd.exe to execute the command in quotes and remain (not close)
            FmtCmdLine.Append("/K \"\"");
            FmtCmdLine.Append(SchemerExe);
            FmtCmdLine.Append("\" \"");
            FmtCmdLine.Append(ConfigLocation);
            FmtCmdLine.Append("\"\"");

            Process OutputExe = new Process();

            OutputExe.StartInfo.FileName = "cmd.exe";
            OutputExe.StartInfo.WorkingDirectory = m_ProjectDirectory;
            OutputExe.StartInfo.Arguments = FmtCmdLine.ToString();
            OutputExe.StartInfo.UseShellExecute = true;

            // Execute!!
            OutputExe.Start();
        }
    }
}
