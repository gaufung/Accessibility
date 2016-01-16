using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HighTrainSpatialInfluence.Services.Algorithm;
using HighTrainSpatialInfluence.Services.Common;
using HighTrainSpatialInfluence.Services.Config;
using HighTrainSpatialInfluence.Services.Raster;
using HighTrainSpatialInfluence.Services.ShapeFile;
using log4net;
using Path = System.IO.Path;

namespace HighTrainSpatialInfluence.ViewModels
{
    internal class RasterTimeCostViewModel : ViewModelBase
    {

        #region Log 日志

        private static ILog _log;

        static RasterTimeCostViewModel()
        {
            _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        #endregion

        #region 私有成员变量

        private readonly string _landUseFilePath;
        private readonly string _trafficRoadFilePath;

        private Speed _speed;
        private ShapeOp _shapeOp;

        #endregion


        #region 绑定属性

        private String _landUse;

        public String LandUse
        {
            get { return _landUse; }
            set
            {
                _landUse = value;
                RaisePropertyChanged("LandUse");
            }
        }

        private String _trafficRoad;

        public String TrafficRoad
        {
            get { return _trafficRoad; }
            set
            {
                _trafficRoad = value;
                RaisePropertyChanged("TrafficRoad");
            }
        }

        private Double _cellSize;

        public Double CellSize
        {
            get { return _cellSize; }
            set
            {
                _cellSize = value;
                RaisePropertyChanged("CellSize");
            }
        }

        private String _timeCostName;

        public String TimeCostName
        {
            get { return _timeCostName; }
            set
            {
                _timeCostName = value;
                RaisePropertyChanged("TimeCostName");
            }
        }


        #endregion

        //构造函数
        public RasterTimeCostViewModel(string landuse, string traffice, Speed speed)
        {
            _landUseFilePath = landuse;
            _trafficRoadFilePath = traffice;
            _speed = speed;
            _shapeOp = new ShapeOp();
            FillShapeName();
            ConfirmCommand = new RelayCommand(Confirm);
        }

        private void FillShapeName()
        {
            LandUse = Path.GetFileNameWithoutExtension(_landUseFilePath);
            TrafficRoad = Path.GetFileNameWithoutExtension(_trafficRoadFilePath);
        }

        #region 绑定命令

        public RelayCommand ConfirmCommand { get; set; }

        /// <summary>
        /// 开始栅格化操作
        /// <remarks>需要做好一下工作</remarks>
        /// <list type="Bullet">
        /// <item>设置生成文件名与两个shape文件不能同名</item>
        /// <item>检查是否有合法字段</item>
        /// <item>设置时间消耗字段</item>
        /// <item>检查类型类型字段中是否存在对应的值</item>
        /// <item>栅格化土地利用文件，并获取工作空间</item>
        /// <item>栅格路网文件</item>
        /// <item>两者叠加分析,取最小值</item>
        /// <item>写栅格文件</item>
        /// </list>
        /// </summary>
        private void Confirm()
        {
            if (!PreCheck())
            {
                MessageBox.Show("参数设置错误，请检查！");
                return;
            }
            if (!FieldCheck(_landUseFilePath, "LandType") || !FieldCheck(_trafficRoadFilePath, "Grade"))
            {
                MessageBox.Show("文件缺少字段,请检查");
                return;
            }
            AddFields(_landUseFilePath, "TimeCost", esriFieldType.esriFieldTypeDouble);
            AddFields(_trafficRoadFilePath, "TimeCost", esriFieldType.esriFieldTypeDouble);
            SetTimeCost(_landUseFilePath, "LandType", "TimeCost");
            SetTimeCost(_trafficRoadFilePath, "Grade", "TimeCost");
            DialogHelper dialogHelper = new DialogHelper();
            var folderPath = dialogHelper.OpenFolderDialog(true);
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (!Convert2Raster(folderPath))
                {
                    MessageBox.Show("时间成本栅格失败");
                }
                if (!WriteTimeCostRaster(folderPath))
                {
                    MessageBox.Show("栅格叠加失败");
                }
                MessageBox.Show("栅格叠加成功");
                GC.Collect();
            }
        }

        /// <summary>
        /// 先前检查
        /// </summary>
        /// <returns></returns>
        private Boolean PreCheck()
        {
            return TimeCostName != LandUse && TimeCostName != TrafficRoad
                   && CellSize > 0;
        }

        /// <summary>
        /// 采用hard code
        /// </summary>
        /// <returns></returns>
        private Boolean FieldCheck(string shapeFilePath, string fieldName)
        {
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            return pFeatureClass.FieldExistCheck(fieldName);
        }

        private void AddFields(string shapeFilePath, string fieldName, esriFieldType type)
        {
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            if (pFeatureClass.Fields.FindField(fieldName) == -1)
            {
                _shapeOp.AddField(pFeatureClass, fieldName, type);
            }
        }

        private void SetTimeCost(string shapeFilePath, string typeField, string targetField)
        {
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            IDataset pDataset = pFeatureClass as IDataset;
            IWorkspaceEdit pWorkspaceEdit = pDataset.Workspace as IWorkspaceEdit;
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            pWorkspaceEdit.StartEditing(true);
            pWorkspaceEdit.StartEditOperation();
            IFeature pFeature;
            while ((pFeature = pFeatureCursor.NextFeature()) != null)
            {
                string landType = pFeature.Value[pFeature.Fields.FindField(typeField)].ToString();
                pFeature.Value[pFeature.Fields.FindField(targetField)] = ((CellSize)/1000)/_speed[landType]*60;
                pFeature.Store();
            }
            Marshal.ReleaseComObject(pFeatureCursor);
            pWorkspaceEdit.StopEditOperation();
            pWorkspaceEdit.StopEditing(true);
        }

        #endregion

        private bool Convert2Raster(string folderPath)
        {
            try
            {
                _shapeOp.FilePath = _landUseFilePath;
                IFeatureClass pLandFeatureClass = _shapeOp.OpenFeatureClass();
                _shapeOp.FilePath = _trafficRoadFilePath;
                IFeatureClass pTrafficFeatureClass = _shapeOp.OpenFeatureClass();
                IEnvelope pEnvelope = ((IGeoDataset) pLandFeatureClass).Extent;
                var raster = new Raster(CellSize, pEnvelope, "TIFF");
                raster.Convert(pLandFeatureClass, "TimeCost", folderPath, LandUse);
                raster.Convert(pTrafficFeatureClass, "TimeCost", folderPath, TrafficRoad);
                return true;
            }
            catch (Exception e)
            {

                _log.Error(e.ToString());
            }
            return false;

        }

        private bool WriteTimeCostRaster(string folderPath)
        {
            try
            {
                //读取两个文件
                var landReader = new RasterReader(folderPath, LandUse + ".tif");
                var roadReader = new RasterReader(folderPath, TrafficRoad + ".tif");
                var landRaster = landReader.Convert2Matrix();
                var roadRaster = roadReader.Convert2Matrix();
               // Print(landRaster, "ditu");
                //Print(roadRaster, "xianlu");
                //叠加操作
                RasterCost.Overlay(landRaster, roadRaster);
               // Print(landRaster, "diejiahou");
                //写入文件
                RasterWriter writer = new RasterWriter(folderPath, TimeCostName + ".tif")
                {
                    OriginPoint = landReader.OriginPoint,
                    CellSizeX = landReader.XCellSize,
                    CellSizeY = landReader.YCellSize,
                    SpatialReference = landReader.SpatialReference
                };
                writer.Write(landRaster, "TIFF");
                return true;
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
            }
            return false;
        }

        private void Print(float?[,] raster, string name)
        {
            string fileName = @"D:\数据" + name;
            using (
                System.IO.FileStream fs = new System.IO.FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite)
                )
            {
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0; i < raster.GetLength(1); i++)
                {
                    string line = string.Empty;
                    for (int j = 0; j < raster.GetLength(0); j++)
                    {
                        line += raster[j, i].ToString() + "\t";
                    }
                    sw.WriteLine(line);
                    sw.Flush();
                }
                sw.Close();
                fs.Close();
            }
        }
    }
}
