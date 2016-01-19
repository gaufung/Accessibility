using System;
using ESRI.ArcGIS.Geometry;

namespace SpatialAccess.Models
{
    class CalculatorCity : City
    {
        public Boolean IsSelected { get; set; }
        public CalculatorCity(double xCoord, double yCoord, string name):base(xCoord,yCoord,name)
        {
            IsSelected = true;
        }

        public CalculatorCity(City city) : base(city.XCoord, city.YCoord, city.Name)
        {
            IsSelected = true;
        }

        public static implicit operator PointClass(CalculatorCity city)
        {
            return new PointClass(){X = city.XCoord,Y=city.YCoord};
        }
    }
}
