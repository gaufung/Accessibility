using System;
using System.Diagnostics;
using System.IO;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Object = System.Object;

namespace HighTrainSpatialInfluence.Services.Raster
{
    internal  class Raster
    {
        protected Object CellSize;
        protected Object ExtentEnvelope;
        protected readonly String RasterType;
        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="cellSize">栅格大小</param>
        /// <param name="envelope">拓展矩形</param>
        /// <param name="rasterType">栅格文件的类型</param>
        public Raster(double cellSize,IEnvelope envelope,string rasterType)
        {
            Debug.Assert(cellSize>=0);
            Debug.Assert(envelope!=null);
            CellSize = cellSize;
            ExtentEnvelope = envelope;
            RasterType = rasterType;
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="pFeatureClass">要素类</param>
        /// <param name="fieldName">栅格采用的字段</param>
        /// <param name="rasterWorkSpace">输出栅格的工作空间</param>
        /// <param name="newRasterName">新的栅格名称</param>
        public void Convert(IFeatureClass pFeatureClass,string fieldName,string rasterWorkSpace,string newRasterName)
        {
            DeleteWorkspace(rasterWorkSpace, newRasterName);
            IFeatureClassDescriptor featureClassDescriptor = new FeatureClassDescriptorClass();
            featureClassDescriptor.Create(pFeatureClass, null, fieldName);
            IGeoDataset geoDataset = (IGeoDataset)featureClassDescriptor;
            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();           
            IWorkspace workspace = workspaceFactory.OpenFromFile(rasterWorkSpace, 0);
            IConversionOp conversionOp = new RasterConversionOpClass();
            IRasterAnalysisEnvironment rasterAnalysisEnvironment = (IRasterAnalysisEnvironment)conversionOp;
            rasterAnalysisEnvironment.OutWorkspace = workspace;

       
            //set cell size
            rasterAnalysisEnvironment.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref CellSize);
            //set output extent  
            object objectMissing = Type.Missing;
            rasterAnalysisEnvironment.SetExtent(esriRasterEnvSettingEnum.esriRasterEnvValue, ref ExtentEnvelope, ref objectMissing);
            //set output spatial reference 
            rasterAnalysisEnvironment.OutSpatialReference = ((IGeoDataset)pFeatureClass).SpatialReference;
            //convertion
            conversionOp.ToRasterDataset(geoDataset, RasterType, workspace, newRasterName);   
           
        }

        /// <summary>
        /// 删除文件下的文件
        /// </summary>
        /// <param name="rasterWorkSpace">工作空间文件夹</param>
        /// <param name="rasterName"></param>
        private void DeleteWorkspace(string rasterWorkSpace,string rasterName)
        {
            int dotIndex = rasterName.IndexOf(".");
            if (dotIndex != -1) rasterName = rasterName.Substring(0, dotIndex + 1);
            try
            {
                DirectoryInfo info = new DirectoryInfo(rasterWorkSpace);
                foreach (var fileInfo in info.GetFiles(rasterName))
                {
                    fileInfo.Delete();
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }
    }
}
