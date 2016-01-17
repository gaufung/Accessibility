using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HighTrainSpatialInfluence.Models
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
    }
}
