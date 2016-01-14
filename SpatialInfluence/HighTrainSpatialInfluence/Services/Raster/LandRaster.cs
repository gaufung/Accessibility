using ESRI.ArcGIS.Geometry;

namespace HighTrainSpatialInfluence.Services.Raster
{
    /// <summary>
    /// 将土地利用的Polygon对象栅格化
    /// </summary>
    internal sealed class LandRaster:Raster
    {
        public LandRaster(double cellSize, IEnvelope envelope, string rasterType):
            base(cellSize,envelope,rasterType)
        {
            
        }
    }
}
