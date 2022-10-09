using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using TEPSClientInstallService_UpdateUtility.Classes;

namespace TEPSClientInstallService_Master_UpdateUtility.Classes
{
    internal class serviceConfigInteractionClass
    {
        private loggingClass loggingClass = new loggingClass();

        private readonly string servicePath = @"C:\Services\Tyler-Client-Install-Master-Service";
        private readonly string configName = "TEPSClientInstallMasterService.exe.config";

        public void configBackUp()
        {
            try
            {
                XDocument doc = XDocument.Load(Path.Combine(servicePath, configName));
                var config = from DBName in doc.Root.Elements("appSettings")
                             select DBName;

                var json = "";

                string s = "DBName";
                foreach (XElement x in FindElements(Path.Combine(servicePath, configName), s))
                {
                    json = JsonConvert.SerializeXNode(x);
                };

                Root test = (Root)JsonConvert.DeserializeObject<Root>(json);

                loggingClass.logEntryWriter(test.add.Value, "info");

                configData.DBname = test.add.Value;
            }
            catch(Exception ex)
            {

            }
        }
        public void configUpdate(string dbName)
        {
            XmlDocument xmldoc = new XmlDocument();

            xmldoc.Load(Path.Combine(servicePath, configName));

            XmlElement node1 = xmldoc.SelectSingleNode("/configuration/appSettings/add") as XmlElement;
            if (node1 != null)
            {
                //node1.InnerText = "something"; // if you want a text
                node1.SetAttribute("value", dbName); // if you want an attribute
                //node1.AppendChild(xmldoc.CreateElement("subnode1")); // if you want a subnode

                xmldoc.Save(Path.Combine(servicePath, configName));
            }
        }

        private static IEnumerable<XElement> FindElements(string filename, string name)
        {
            XElement x = XElement.Load(filename);
            return x.Descendants()
                    .Where(e => e.Name.ToString().Equals(name) ||
                                e.Value.Equals(name) ||
                                e.Attributes().Any(a => a.Name.ToString().Equals(name) ||
                                                        a.Value.Equals(name)));
        }

        public T DeserializeToObject<T>(string filepath) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (StreamReader sr = new StreamReader(filepath))
            {
                return (T)ser.Deserialize(sr);
            }
        }
    }
}

internal class configData
{
    public static string DBname { get; set; }
}

public class Add
{
    [JsonProperty("@key")]
    public string Key { get; set; }

    [JsonProperty("@value")]
    public string Value { get; set; }
}

public class Root
{
    public Add add { get; set; }
}