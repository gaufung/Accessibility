using ESRI.ArcGIS.Geometry;

namespace HighTrainSpatialInfluence.Services.Raster
{
    internal sealed class RoadRaster:Raster
    {
        public RoadRaster(double cellSize, IEnvelope envelope, string rasterType) :
            base(cellSize, envelope, rasterType)
        {
            
        }
    }
}
