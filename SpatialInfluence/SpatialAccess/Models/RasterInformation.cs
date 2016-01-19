using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;

namespace SpatialAccess.Models
{
    internal class RasterInformation
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public Double XCellSize { get; set; }
        public Double YCellSize { get; set; }
        public IPoint OriginPoint { get; set; }
        public ISpatialReference SpatialReference { get; set; }
    }
}
