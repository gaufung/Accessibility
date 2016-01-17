using System;
using System.Collections.Generic;
using System.Xml;

namespace HighTrainSpatialInfluence.Services.Config
{
    internal class Target
    {
        public Target()
        {
            string xmlPath= AppDomain.CurrentDomain.BaseDirectory + @"config\Target.xml";
            Read(xmlPath);
        }
        public IEnumerable<String> Targets { get; private set; }

        private void Read(string xmlPath)
        {
            XmlDocument doc=new XmlDocument();
            try
            {
                doc.Load(xmlPath);
                List<string> list=new List<string>();
                XmlElement root = doc.DocumentElement;
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    list.Add(root.ChildNodes[i].InnerText);
                }
                Targets = list;
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
