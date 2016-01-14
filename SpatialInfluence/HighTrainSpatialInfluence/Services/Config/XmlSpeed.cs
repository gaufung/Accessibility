using System;
using System.Collections.Generic;
using System.Xml;

namespace HighTrainSpatialInfluence.Services.Config
{
    /// <summary>
    /// 从xml中解析出配置
    /// </summary>
    internal  class XmlSpeed :Speed
    {
        public override double this[string type]
        {
            get
            {
                if(!TypeSpeed.ContainsKey(type))
                    throw new ArgumentException(string.Format("不包含{0}类型的速度",type));
                return TypeSpeed[type];
            }
        }

        public XmlSpeed()
        {
            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"config\Speed.xml";
            Read(xmlPath);
        }

        private void Read(string xmlPath)
        {
            XmlDocument doc=new XmlDocument();          
            try
            {
                doc.Load(xmlPath);
                XmlElement root = doc.DocumentElement;
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    var typeSpeed = Parse(root.ChildNodes[i]);
                    TypeSpeed.Add(typeSpeed.Key,typeSpeed.Value);
                }
            }
            catch (NullReferenceException)
            {
                
            }           
        }
        /// <summary>
        /// 从一个xml结点获取type和speed
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private KeyValuePair<string, double> Parse(XmlNode node)
        {
            string type = node.ChildNodes[0].InnerText;
            double speed = Convert.ToDouble(node.ChildNodes[1].InnerText);
            return new KeyValuePair<string, double>(type,speed);
        }
    }
}
