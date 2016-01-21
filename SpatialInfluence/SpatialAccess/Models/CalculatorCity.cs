using System;
using ESRI.ArcGIS.Geometry;

namespace SpatialAccess.Models
{
    class CalculatorCity : City
    {
        public Boolean IsSelected { get; set; }

        public CalculatorCity(City city) : base()
        {
            XCoord = city.XCoord;
            YCoord = city.YCoord;
            Name = city.Name;
            CityType = city.CityType;
            if (CityType == CityType.HighStationAndCity || CityType == CityType.OnlyCity)
            {
                IsSelected = true;
            }
            else
            {
                IsSelected = false;
            }

        }

        public static implicit operator PointClass(CalculatorCity city)
        {
            return new PointClass(){X = city.XCoord,Y=city.YCoord};
        }
    }
}
