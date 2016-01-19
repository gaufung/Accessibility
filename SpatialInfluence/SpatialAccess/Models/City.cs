using System;

namespace SpatialAccess.Models
{
    /// <summary>
    /// 城市结点
    /// </summary>
    [Serializable]
    internal  class City
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public Double XCoord { get; private set; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public Double YCoord { get; private set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public String Name { get;  set; }

        #region 城市的人口，经济等等相关属性
        
        #endregion

        public City(double xCoord, double yCoord, string name)
        {
            XCoord = xCoord;
            YCoord = yCoord;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;
            return Name.Equals(((City) obj).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
