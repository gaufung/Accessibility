using System;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SpatialAccess.Services.Common;
using Object = System.Object;

namespace SpatialAccess.Services.Raster
{
    internal  class RasterConvertion
    {
        private Object _cellSize;
        private Object _extentEnvelope;
        private readonly String _rasterType;
        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="cellSize">栅格大小</param>
        /// <param name="envelope">拓展矩形</param>
        /// <param name="rasterType">栅格文件的类型</param>
        public RasterConvertion(double cellSize,IEnvelope envelope,string rasterType)
        {
            _cellSize = cellSize;
            _extentEnvelope = envelope;
            _rasterType = rasterType;
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="pFeatureClass">要素类</param>
        /// <param name="fieldName">栅格采用的字段</param>
        /// <param name="rasterWorkSpace">输出栅格的工作空间</param>
        /// <param name="newRasterName">新的栅格名称</param>
        public void Convert(IFeatureClass pFeatureClass,
            string fieldName,
            string rasterWorkSpace,
            string newRasterName)
        {
            FileHelper.DeleteFile(rasterWorkSpace, newRasterName, ".tif", ".tfw",".tif.aux");
            IFeatureClassDescriptor featureClassDescriptor = new FeatureClassDescriptorClass();
            featureClassDescriptor.Create(pFeatureClass, null, fieldName);
            IGeoDataset geoDataset = (IGeoDataset)featureClassDescriptor;
            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();           
            IWorkspace workspace = workspaceFactory.OpenFromFile(rasterWorkSpace, 0);
            IConversionOp conversionOp = new RasterConversionOpClass();
            IRasterAnalysisEnvironment rasterAnalysisEnvironment = (IRasterAnalysisEnvironment)conversionOp;
            rasterAnalysisEnvironment.OutWorkspace = workspace;

       
            //set cell size
            rasterAnalysisEnvironment.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref _cellSize);
            //set output extent  
            object objectMissing = Type.Missing;
            rasterAnalysisEnvironment.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref _extentEnvelope, ref objectMissing);
            //set output spatial reference 
            rasterAnalysisEnvironment.OutSpatialReference = ((IGeoDataset)pFeatureClass).SpatialReference;
            //convertion
            conversionOp.ToRasterDataset(geoDataset, _rasterType, workspace, newRasterName);              
        }    
    }
}
