using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HighTrainSpatialInfluence.Services.Common;
using HighTrainSpatialInfluence.Services.Config;
using HighTrainSpatialInfluence.Services.ShapeFile;
using HighTrainSpatialInfluence.Views;
using log4net;

namespace HighTrainSpatialInfluence.ViewModels
{
    internal class MainWindowViewModel:ViewModelBase
    {
        #region Log 日志
        private static ILog _log;

        static MainWindowViewModel()
        {
            _log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
        }
        #endregion

        #region 地图控件
        private readonly AxMapControl _axMapControl;

      
        #endregion

        #region 私有成员(shape文件)

        /// <summary>
        /// 土地利用    
        /// </summary>
        private string _landUseFilePath;
        /// <summary>
        /// 交通路网文件
        /// </summary>
        private string _trafficRoadFilePath;
        /// <summary>
        /// 高铁文件
        /// </summary>
        private string _highTrainFilePath;
        /// <summary>
        /// 城市群文件
        /// </summary>
        private string _citiesFilePath;

        #endregion

        #region shape文件操作

        private ShapeOp _shapeOp;

        private Dictionary<string, int> _mapIndex; 
        #endregion
        public MainWindowViewModel(AxMapControl axMapControl)
        {
            _axMapControl = axMapControl;
            InitializeCommand();
            _shapeOp=new ShapeOp();
            _mapIndex=new Dictionary<string, int>(4);
        }


        #region 命令

        private void InitializeCommand()
        {
            OpenLandUseCommand=new RelayCommand(OpenLandUse);
            OpenTrafficRoadCommand = new RelayCommand(OpenTrafficRoad);
            OpenHighTrainCommand = new RelayCommand(OpenHighTrain);
            OpenCitiesCommand = new RelayCommand(OpenCities);
            RasterTimeCostCommand = new RelayCommand(RasterTimeCost);
        }

        public RelayCommand OpenLandUseCommand { get; set; }

        private void OpenLandUse()
        {
            DialogHelper dialogHelper=new DialogHelper("shp");
            var tempFilePath = dialogHelper.OpenFile("选择土地利用类型文件");
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                _landUseFilePath = tempFilePath;
                _shapeOp.FilePath = _landUseFilePath;
                LoadMap("LandUse", esriGeometryType.esriGeometryPolygon);
            }
           
        }

        public RelayCommand OpenTrafficRoadCommand { get; set; }

        private void OpenTrafficRoad()
        {
            DialogHelper dialogHelper = new DialogHelper("shp");
            var tempFilePath = dialogHelper.OpenFile("请选择交通路网信息");
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                _trafficRoadFilePath = tempFilePath;
                _shapeOp.FilePath = _trafficRoadFilePath;
                LoadMap("TrafficRoad", esriGeometryType.esriGeometryPolyline);
            }
        }
        public RelayCommand OpenHighTrainCommand { get; set; }

        private void OpenHighTrain()
        {
            DialogHelper dialogHelper = new DialogHelper("shp");
            var tempFilePath = dialogHelper.OpenFile("请选择高铁文件");
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                _highTrainFilePath = tempFilePath;
                _shapeOp.FilePath = _highTrainFilePath;
                LoadMap("HighTrain", esriGeometryType.esriGeometryPolyline);
            }
        }
        public RelayCommand OpenCitiesCommand { get; set; }

        private void OpenCities()
        {
            DialogHelper dialogHelper = new DialogHelper("shp");
            var tempFilePath = dialogHelper.OpenFile("请选择城市文件");
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                _citiesFilePath = tempFilePath;
                _shapeOp.FilePath = _citiesFilePath;
                LoadMap("Cities", esriGeometryType.esriGeometryPoint);
            }
        }

        public RelayCommand RasterTimeCostCommand { get; set; }

        private void RasterTimeCost()
        {
            Speed speed = new XmlSpeed();
            RasterTimeCostViewModel vm = new RasterTimeCostViewModel(_landUseFilePath, _trafficRoadFilePath, speed);
            RasterTimeCostView view = new RasterTimeCostView(vm);
            view.ShowDialog();
        }
        #endregion

        #region 加载图层
        /// <summary>
        /// 加载图层
        /// </summary>
        /// <param name="layerType"></param>
        /// <param name="targetType">打开目标要素类型</param>
        private bool LoadMap(string layerType, esriGeometryType targetType)
        {
            try
            {
                //如果存在,删除
                if (_mapIndex.ContainsKey(layerType))
                {
                    var pLayer = _axMapControl.Map.Layer[_mapIndex[layerType]];
                    _axMapControl.Map.DeleteLayer(pLayer);
                    _mapIndex.Remove(layerType);
                }
                //添加
                var pFeatureClass = _shapeOp.OpenFeatureClass();
                if (!pFeatureClass.GeometryTypeCheck(targetType))
                {
                    throw new Exception("打开要素类型不正确");
                }
                _axMapControl.Map.AddLayer(new FeatureLayerClass()
                {
                    FeatureClass = pFeatureClass
                });
                _mapIndex.Add(layerType, _axMapControl.Map.LayerCount - 1);
                return true;
            }
            catch (Exception e)
            {
                _log.Error(e.ToString());
                IfLoadFailed(layerType);
                MessageBox.Show(e.ToString());
            }
            return false;
        }

        private void IfLoadFailed(string layerType)
        {
            switch (layerType)
            {
                case "LandUse":
                    _landUseFilePath = string.Empty;
                    break;
                case "TrafficRoad":
                    _trafficRoadFilePath = string.Empty;
                    break;
                case "HighTrain":
                    _highTrainFilePath = string.Empty;
                    break;
                default:
                    _citiesFilePath = string.Empty;
                    break;
            }
        }
        #endregion

       
    }
}
