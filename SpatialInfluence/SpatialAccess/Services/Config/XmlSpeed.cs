using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpatialAccess.Services.Config
{
    /// <summary>
    /// 从xml中解析出配置
    /// </summary>
    internal  class XmlSpeed 
    {
        private readonly Dictionary<string, double> _typeSpeed;
        public  double this[string type] {
            get
            {
                if (!_typeSpeed.ContainsKey(type))
                    throw new ArgumentException(string.Format("不包含{0}类型的速度", type));
                return _typeSpeed[type];
            }
        }

        public XmlSpeed()
        {
            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"config\Speed.xml";
            _typeSpeed=new Dictionary<string, double>();
            Read(xmlPath);
        }
        private void Read(string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException("配置文件不存在");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlElement root = doc.DocumentElement;
            if (root==null)
            {
                throw new FileFormatException("配置文件格式不正确");
            }
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                var typeSpeed = Parse(root.ChildNodes[i]);
                _typeSpeed.Add(typeSpeed.Key, typeSpeed.Value);
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
            return new KeyValuePair<string, double>(type, speed);
        }
    }
}
