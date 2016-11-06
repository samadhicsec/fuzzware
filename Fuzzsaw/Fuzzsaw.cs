using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace Fuzzware.Fuzzsaw
{
    public static class Fuzzsaw
    {
        private static string m_ProjectsDirectory;
        //private static string m_LogDirectory;
        private static string m_InstallationDirectory;

        // Default registry keys
        private const string FUZZWARE_REGISTRY_KEY = @"Software\Fuzzware";
        private const string PROJECTS_DIR_REGISTRY_VALUE = "FuzzwareProjectsDir";
        private const string DEFAULT_PROJECTS_DIR = "%USERPROFILE%\\Fuzzware Projects\\";
        //private const string LOG_DIR_REGISTRY_VALUE = "FuzzwareLogDir";
        //private const string DEFAULT_LOG_DIR = "Logs\\";

        // Default file & directory names
        private const string DEFAULT_CONFIG_FILENAME = "Configuration.xml";
        private const string DEFAULT_SIMPLETYPE_CONFIG_FILENAME = "SimpleTypeFuzzerConfig.xml";
        private const string DEFAULT_COMPLEXTYPE_CONFIG_FILENAME = "ComplexTypeFuzzerConfig.xml";
        private const string DEFAULT_XMLDEFAULTVALUES_FILENAME = "XmlDefaultValues.xml";
        private const string DEFAULT_SOAPTEMPLATE_FILENAME = "SOAPRequestTemplate.xml";
        private const string SCHEMAS_DIR = "Schemas";
        private const string RESOURCES_DIR = "Resources";
        private const string DEFAULT_OUTPUT_DIR = "out";
        private const string DEFAULT_OUTPUT_EXT = "xml";

        private static string m_ProjectDirectory;

        /// <summary>
        /// Initialise various global properties
        /// </summary>
        public static void Initialise()
        {
            //// Load global property values from the registry
            //RegistryKey oRegKey = Registry.CurrentUser.OpenSubKey(FUZZWARE_REGISTRY_KEY);

            //if (null == oRegKey)
            //{
            //    // Create the key
            //    oRegKey = Registry.CurrentUser.CreateSubKey(FUZZWARE_REGISTRY_KEY);
            //    if (null == oRegKey)
            //        MessageBox.Show("Could not create HKCU key '" + FUZZWARE_REGISTRY_KEY + "'", "Error creating registry key", MessageBoxButton.OK);
            //}

            //if ((null != oRegKey) && (null == oRegKey.GetValue(PROJECTS_DIR_REGISTRY_VALUE)))
            //{
            //    oRegKey = Registry.CurrentUser.OpenSubKey(FUZZWARE_REGISTRY_KEY, true);
            //    if (null == oRegKey)
            //        MessageBox.Show("Could not open HKCU key '" + FUZZWARE_REGISTRY_KEY + "' for writing", "Error opening registry key", MessageBoxButton.OK);
            //    else
            //        oRegKey.SetValue(PROJECTS_DIR_REGISTRY_VALUE, Environment.ExpandEnvironmentVariables(DEFAULT_PROJECTS_DIR));
            //}
            //// Read in projects directory
            //if (null != oRegKey)
            //    m_ProjectsDirectory = (string)oRegKey.GetValue(PROJECTS_DIR_REGISTRY_VALUE);
            //else
            //    m_ProjectsDirectory = Environment.ExpandEnvironmentVariables(DEFAULT_PROJECTS_DIR);

            // Read in the Projects Directory
            ReadRegistryEntry(PROJECTS_DIR_REGISTRY_VALUE, out m_ProjectsDirectory, Environment.ExpandEnvironmentVariables(DEFAULT_PROJECTS_DIR));

            // Make sure m_ProjectsDirectory ends in an '\'.  Do this because calling Path.GetDirectoryName on a string that does
            // end in Path.DirectorySeparatorChar will return its parent directory.
            if (!m_ProjectsDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                m_ProjectsDirectory += Path.DirectorySeparatorChar.ToString();

            // Make sure the projects directory exists
            if (!Directory.Exists(m_ProjectsDirectory))
            {
                Directory.CreateDirectory(m_ProjectsDirectory);
            }

            //// Read in the Log Directory
            //ReadRegistryEntry(LOG_DIR_REGISTRY_VALUE, out m_LogDirectory, DEFAULT_LOG_DIR);
            //// Make sure m_LogDirectory ends in an '\'.  Do this because calling Path.GetDirectoryName on a string that does
            //// end in Path.DirectorySeparatorChar will return its parent directory.
            //if (!m_ProjectsDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            //    m_ProjectsDirectory += Path.DirectorySeparatorChar.ToString();

            // Read in installation directory
            m_InstallationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // TODO: Read in recent projects

            //oRegKey.Close();
        }

        public static bool ReadRegistryEntry(String Key, out String KeyValue, String DefaultValue)
        {
            KeyValue = null;

            // Load global property values from the registry
            RegistryKey oRegKey = Registry.CurrentUser.OpenSubKey(FUZZWARE_REGISTRY_KEY);

            // If that did not work, try to create the key
            if (null == oRegKey)
            {
                // Create the key
                oRegKey = Registry.CurrentUser.CreateSubKey(FUZZWARE_REGISTRY_KEY);
                if (null == oRegKey)
                {
                    MessageBox.Show("Could not create HKCU key '" + FUZZWARE_REGISTRY_KEY + "'", "Error creating registry key", MessageBoxButton.OK);
                    return false;
                }
            }

            // If the value does not exist, write a default one (if passed in)
            if (null == oRegKey.GetValue(Key))
            {
                if ((null != DefaultValue) && !WriteRegistryEntry(Key, DefaultValue))
                    return false;
            }

            // Read the key value
            KeyValue = (string)oRegKey.GetValue(Key);
            // If we didn't read anything, there was a problem, but this might be OK (no DefaultValue for instance)
            if (null == KeyValue)
                return false;

            return true;
        }

        /// <summary>
        /// Write a Fuzzware registry entry
        /// </summary>
        public static bool WriteRegistryEntry(String Key, String KeyValue)
        {
            // Load global property values from the registry
            RegistryKey oRegKey = Registry.CurrentUser.OpenSubKey(FUZZWARE_REGISTRY_KEY, true);

            if (null == oRegKey)
            {
                // Create the key
                oRegKey = Registry.CurrentUser.CreateSubKey(FUZZWARE_REGISTRY_KEY);
                if (null == oRegKey)
                {
                    MessageBox.Show("Could not create HKCU key '" + FUZZWARE_REGISTRY_KEY + "'", "Error creating registry key", MessageBoxButton.OK);
                    return false;
                }
            }
    
            try
            {
                oRegKey.SetValue(Key, KeyValue);
            }
            catch(Exception e)
            {
                MessageBox.Show("The following exception occurred trying to set value '" + Key + "' to '" +
                    KeyValue + "' on HKCU key '" + FUZZWARE_REGISTRY_KEY + "'" + 
                    Environment.NewLine + e.Message, "Error writing to registry key", MessageBoxButton.OK);
                return false;
            }

            oRegKey.Close();
            return true;
        }

        /// <summary>
        /// Gets or sets the Fuzzware Projects Directory
        /// </summary>
        public static string ProjectsDirectory
        {
            get { return m_ProjectsDirectory; }
            set
            {
                String OldValue = m_ProjectsDirectory;
                m_ProjectsDirectory = value;
                if (!WriteRegistryEntry(PROJECTS_DIR_REGISTRY_VALUE, m_ProjectsDirectory))
                    m_ProjectsDirectory = OldValue;
            }
        }

        ///// <summary>
        ///// Gets or sets the Fuzzware Log Directory
        ///// </summary>
        //public static string LogDirectory
        //{
        //    get { return m_LogDirectory; }
        //    set
        //    {
        //        String OldValue = m_LogDirectory;
        //        m_LogDirectory = value;
        //        if (!WriteRegistryEntry(LOG_DIR_REGISTRY_VALUE, m_LogDirectory))
        //            m_LogDirectory = OldValue;
        //    }
        //}

        /// <summary>
        /// Gets the current Fuzzware Project Directory.  If there is no current project, it returns the ProjectsDirectory
        /// </summary>
        public static string ProjectDirectory
        {
            get 
            { 
                if(!String.IsNullOrEmpty(m_ProjectDirectory))
                    return m_ProjectDirectory;
                return ProjectsDirectory;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    m_ProjectDirectory = Path.GetFullPath(value);
                else
                    m_ProjectDirectory = ProjectsDirectory;
                // Set the current directory
                Environment.CurrentDirectory = m_ProjectDirectory;
            }
        }

        /// <summary>
        /// Gets the directory that Fuzzware is installed in
        /// </summary>
        public static string InstallationDirectory
        {
            get { return m_InstallationDirectory; }
        }

        /// <summary>
        /// Get the Fuzzware Resources directory
        /// </summary>
        public static string ResourcesDirectory
        {
            get 
            {
#if DEBUG
                return Path.Combine("G:\\Tools\\Fuzzware", RESOURCES_DIR); 
#else
                return Path.Combine(m_InstallationDirectory, RESOURCES_DIR);
#endif
            }
        }

        /// <summary>
        /// Get the Fuzzware Schemas directory
        /// </summary>
        public static string SchemasDirectory
        {
            get
            {
#if DEBUG
                return Path.Combine("G:\\Tools\\Fuzzware", SCHEMAS_DIR); 
#else
                return Path.Combine(m_InstallationDirectory, SCHEMAS_DIR);
#endif
            }
        }

        public static string DefaultConfigurationFilename
        {
            get { return DEFAULT_CONFIG_FILENAME; }
        }

        public static string DefaultSimpleTypeFuzzerConfigFilename
        {
            get { return DEFAULT_SIMPLETYPE_CONFIG_FILENAME; }
        }

        public static string DefaultComplexTypeFuzzerConfigFilename
        {
            get { return DEFAULT_COMPLEXTYPE_CONFIG_FILENAME; }
        }

        public static string DefaultXmlDefaultValuesFilename
        {
            get { return DEFAULT_XMLDEFAULTVALUES_FILENAME; }
        }

        public static string DefaultSoapTemplateFilename
        {
            get { return DEFAULT_SOAPTEMPLATE_FILENAME; }
        }

        public static string DefaultOutputDir
        {
            get { return DEFAULT_OUTPUT_DIR; }
        }

        public static string DefaultOutputExt
        {
            get { return DEFAULT_OUTPUT_EXT; }
        }
    }
}
