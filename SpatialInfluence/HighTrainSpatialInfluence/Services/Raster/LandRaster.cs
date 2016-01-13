using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace HighTrainSpatialInfluence.Services.Raster
{
    internal sealed class LandRaster
    {
        /// <summary>
        /// 栅格的大小
        /// </summary>
        private Double _cellSize;

        public LandRaster(double cellSize)
        {
            _cellSize = cellSize;
        }

        public IGeoDataset Raster(IFeatureClass pFeatureClass,String raterWorkSpace)
        {
            IGeoDataset geoDataset = (IGeoDataset) pFeatureClass;
            ISpatialReference spatialReference = geoDataset.SpatialReference;
            //create a rasterMaker operator      
            return null;
        }
    }
}
