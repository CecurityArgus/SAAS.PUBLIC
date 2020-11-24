using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PUBLIC.SERVICE.LIB.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        /// <summary>
        ///
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;
    }

    /// <summary>
    ///
    /// </summary>
    public static class InformationPackage
    {
        /// <summary>
        ///
        /// </summary>
        public class dataset
        {
            /// <summary>
            ///
            /// </summary>
            [XmlAttribute(AttributeName = "type")]
            public string type { get; set; }

            /// <summary>
            ///
            /// </summary>
            [XmlElement]
            public List<String> data { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class tickets
        {
            /// <summary>
            ///
            /// </summary>
            public string rootTicket { get; set; }

            /// <summary>
            ///
            /// </summary>
            public string dataTicket { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class customer
        {
            /// <summary>
            ///
            /// </summary>
            public string key { get; set; }

            /// <summary>
            ///
            /// </summary>
            public string name { get; set; }

            /// <summary>
            ///
            /// </summary>
            public string solution { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        [XmlRoot("ip")]
        public class ip
        {
            /// <summary>
            ///
            /// </summary>
            [XmlAttribute(AttributeName = "type")]
            public string type { get; set; }

            /// <summary>
            ///
            /// </summary>
            [XmlAttribute(AttributeName = "version")]
            public string version { get; set; }

            /// <summary>
            ///
            /// </summary>
            public dataset dataset { get; set; }

            /// <summary>
            ///
            /// </summary>
            public tickets tickets { get; set; }

            /// <summary>
            ///
            /// </summary>
            public customer customer { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public static ip LoadIP(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ip));

            FileStream fs = new FileStream(filename, FileMode.Open);

            var ip = (ip)serializer.Deserialize(fs);

            return ip;
        }

        /// <summary>
        ///
        /// </summary>
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