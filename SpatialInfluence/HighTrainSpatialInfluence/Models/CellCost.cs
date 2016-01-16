using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTrainSpatialInfluence.Model
{
    internal class CellCost
    {
        public Boolean Visited { get; set; }
        public Boolean HasValue { get; set; }
        public Double Cost { get; set; }

        public CellCost()
        {
            Visited = false;
            HasValue = false;
            Cost = 0.0;
        }

    }
}
