using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PortableDriver
{
   public class Script
    {
        public string xmlLoc;
        protected string[] arrUrl;
        public Script() { 
        xmlLoc = Environment.GetFolderPath(Environment.SpecialFolder.System);
        }
        public Script(string xmlLoc)
        {
            this.xmlLoc = xmlLoc;
        }
        public void setXmlLoc(string inp)
        {
            xmlLoc = inp;
        }
        public void setArrayOfURLs(string[] input)
        {
            this.arrUrl = input;
        }
        public string getXmlLoc() 
        { return xmlLoc; 
        }    
    }
    public class Config : Script
    {
        protected string defPath;
        protected bool runSilent, rebootAfter;
        public Config() : base()
        {
            defPath = @"C:\Windows\Setup";
            runSilent = true;
            rebootAfter = true;
        }
        public void setDefaultPath(string path)
        {
            defPath = path;
        }
        public void setRunSilent(bool option)
        {
            runSilent = option;
        }
        public void setRebootAfter(bool option)
        {
            rebootAfter = option;
        }
        public string getDefaultPath() { return defPath; }
        public bool getRunSilent() { return runSilent; }
        public bool getRebootAfter() { return rebootAfter;}
    }
    public class MakeXML : Config
    {
        protected XmlDocument body = new XmlDocument();
        protected XmlElement mainScript;

        public MakeXML() : base()
        {
            mainScript = body.CreateElement("MainScript");
        }

        public void addToXML(int order, string type, string url, string args)
        {
            XmlElement script = body.CreateElement("Script");

            XmlElement orderElement = body.CreateElement("Order");
            orderElement.InnerText = order.ToString();
            script.AppendChild(orderElement);

            XmlElement typeElement = body.CreateElement("Type");
            typeElement.InnerText = type;
            script.AppendChild(typeElement);

            XmlElement urlElement = body.CreateElement("URL");
            urlElement.InnerText = url;
            script.AppendChild(urlElement);

            XmlElement argsElement = body.CreateElement("Args");
            argsElement.InnerText = args;
            script.AppendChild(argsElement);

            mainScript.AppendChild(script);
        }

        public void compileScript()
        {
            XmlElement top = body.CreateElement("PortableDriver");
            XmlElement config = body.CreateElement("Config");
            top.AppendChild(config);

            XmlElement defaultPathElement = body.CreateElement("DefaultPath");
            defaultPathElement.InnerText = defPath;
            config.AppendChild(defaultPathElement);

            XmlElement runSilentElement = body.CreateElement("RunSilent");
            runSilentElement.InnerText = runSilent.ToString();
            config.AppendChild(runSilentElement);

            XmlElement rebootAfterElement = body.CreateElement("RebootAfter");
            rebootAfterElement.InnerText = rebootAfter.ToString();
            config.AppendChild(rebootAfterElement);

            XmlElement scripts = body.CreateElement("Scripts");
            scripts.AppendChild(mainScript);
            top.AppendChild(scripts);

            body.AppendChild(top);
            body.Save(xmlLoc + "\\autoDriver.xml");
        }
    }


}
