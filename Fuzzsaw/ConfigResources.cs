using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fuzzware.Fuzzsaw
{
    public class Purple
    {
        string sPurple = "Purple";
        public string pPurple
        {
            get { return sPurple; }
            set { sPurple = value; }
        }
    }
    public class ConfigResources
    {
        String ConfigFile;
        Purple oPurple;

        public ConfigResources(String FilePath)
        {
            ConfigFile = FilePath;
            oPurple = new Purple();
        }

        //public void AddResources(System.Windows.Data.Binding ResDict)
        public void AddResources(ResourceDictionary ResDict)
        {
            System.Xml.XmlReader reader = System.Xml.XmlReader.Create(ConfigFile);
            System.Xml.XmlDocument oXmlDoc = new System.Xml.XmlDocument();
            oXmlDoc.Load(reader);
            XmlDataProvider xdp = new XmlDataProvider();
            //xdp.Source = new Uri(ConfigFile);
            xdp.Document = oXmlDoc;
            XmlNamespaceMappingCollection xnmc = new XmlNamespaceMappingCollection();
            xnmc.AddNamespace("urn:Fuzzware.Schemas.Configuration", "cfg");
            xdp.XmlNamespaceManager = xnmc;
            
            //xdp.XPath = @"cfg:Configuration\cfg:output\cfg:ConvertFromXML\cfg:outputEncoding";
            xdp.XPath = @"cfg:Configuration";
            //xdp.InitialLoad();
            //object o = xdp.Data;
            ResDict.Add("xmlConfig", xdp);
            //ResDict.DataContext = xnmc;
            //ResDict.Source = xdp;
            
            //return ResDict;
        }

        public String sConfigFile
        {
            get { return ConfigFile; }
            set { ConfigFile = value; }
        }

        public Purple pPurple
        {
            get { return oPurple; }
            set { oPurple = value; }
        }
    }
}
