using System;
using ESRI.ArcGIS.Geometry;

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
        public Double XCoord { get;  set; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public Double YCoord { get;  set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public String Name { get;  set; }

        public CityType CityType { get; set; }

        #region 城市的人口，经济等等相关属性
        
        #endregion

        public City()
        {
            
        }
        public City(double xCoord, double yCoord, string name,string cityType)
        {
            XCoord = xCoord;
            YCoord = yCoord;
            Name = name;
            CityType=Convert(cityType);
        }

        private CityType Convert(string cityType)
        {
            if (cityType=="是")return CityType.HighStation;
            if(cityType=="否")return CityType.OnlyCity;
            if(cityType=="同时")return CityType.HighStationAndCity;
            throw new ArgumentOutOfRangeException("城市类型不在范围内");
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
        /// <summary>
        /// 隐式装换
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public static implicit operator PointClass(City city)
        {
            return new PointClass(){X =city.XCoord,Y=city.YCoord };
        }
    }

    internal enum CityType
    {
        HighStation,
        OnlyCity,
        HighStationAndCity  
    }
}
