using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PUBLIC.CONTROLLER.LIB.Helpers
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public static class InformationPackage
    {
        public class dataset
        {
            [XmlAttribute(AttributeName = "type")]
            public string type { get; set; }

            [XmlElement]
            public List<String> data { get; set; }

        }

        public class tickets
        {
            public string rootTicket { get; set; }
            public string dataTicket { get; set; }
        }

        public class customer
        {
            public string key { get; set; }
            public string name { get; set; }
            public string solution { get; set; }
        }

        [XmlRoot("ip")]
        public class ip
        {
            [XmlAttribute(AttributeName = "type")]
            public string type { get; set; }

            [XmlAttribute(AttributeName = "version")]
            public string version { get; set; }
            public dataset dataset { get; set; }
            public tickets tickets { get; set; }
            public customer customer { get; set; }
        }

        public static ip LoadIP(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ip));

            FileStream fs = new FileStream(filename, FileMode.Open);

            var ip = (ip)serializer.Deserialize(fs);

            return ip;
        }

        public static void SaveIP(ip ip, string filename)
        {
            var xmlString = "";
            XmlSerializer serializer = new XmlSerializer(typeof(ip));

            using (var sww = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    serializer.Serialize(writer, ip);
                    xmlString = sww.ToString();
                }
            }

            File.WriteAllText(filename, xmlString);
        }
    }
}
