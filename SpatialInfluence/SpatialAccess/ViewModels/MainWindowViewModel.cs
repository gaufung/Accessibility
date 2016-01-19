using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.ShapeFile;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    internal class MainWindowViewModel:ViewModelBase
    {
        /// <summary>
        /// 地图控件
        /// </summary>
        private readonly AxMapControl _axMapControl;

        private readonly ShapeOp _shapeOp;
        private Dictionary<string, int> _mapIndex; 

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
        public MainWindowViewModel(AxMapControl axMapControl)
        {
            _axMapControl = axMapControl;
            _shapeOp=new ShapeOp(string.Empty);
            _mapIndex=new Dictionary<string, int>(5);
            InitializeCommand();
        }
        #region 绑定命令

        private void InitializeCommand()
        {
            OpenLandUseCommand=new RelayCommand(OpenLandUse);
            OpenTrafficRoadCommand = new RelayCommand(OpenTrafficRoad);
            OpenHighTrainCommand = new RelayCommand(OpenHighTrain);
            OpenCitiesCommand=new RelayCommand(OpenCities);
            RasterTimeCostCommand = new RelayCommand(RasterTimeCost);
            HighTrainNoCommand = new RelayCommand(HighTrainNo);
            HighTrainYesCommand = new RelayCommand(HighTrainYes);
        }
        public RelayCommand OpenLandUseCommand { get; set; }
        private void OpenLandUse()
        {
            var tempFilePath = DialogHelper.OpenFile("shp", "选择土地利用类型文件");
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
            //DialogHelper dialogHelper = new DialogHelper("shp");
            //var tempFilePath = dialogHelper.OpenFile("请选择交通路网信息");
            var tempFilePath = DialogHelper.OpenFile("shp", "选择交通路网信息");
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
            //DialogHelper dialogHelper = new DialogHelper("shp");
            //var tempFilePath = dialogHelper.OpenFile("请选择高铁文件");
            var tempFilePath = DialogHelper.OpenFile("shp", "选择高铁文件");
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
            //DialogHelper dialogHelper = new DialogHelper("shp");
            //var tempFilePath = dialogHelper.OpenFile("请选择城市文件");
            var tempFilePath = DialogHelper.OpenFile("shp", "选择城市文件");
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

            if (!String.IsNullOrEmpty(_landUseFilePath) && !string.IsNullOrEmpty(_trafficRoadFilePath))
            {
                RasterTimeCostViewModel vm = new RasterTimeCostViewModel(_landUseFilePath, _trafficRoadFilePath);
                RasterTimeCostView view = new RasterTimeCostView(vm);
                view.ShowDialog();
            }
            
        }
        public RelayCommand HighTrainNoCommand { get; set; }

        private void HighTrainNo()
        {
            if (!string.IsNullOrEmpty(_citiesFilePath))
            {
                HighTrainNoView view = new HighTrainNoView(new HighTrainNoViewModel(_citiesFilePath));
                view.ShowDialog();
            }
        }
        public RelayCommand HighTrainYesCommand { get; set; }

        private void HighTrainYes()
        {

            if (!string.IsNullOrEmpty(_citiesFilePath)&&!string.IsNullOrEmpty(_highTrainFilePath))
            {
                var view = new HighTrainYesView(new HighTrainYesViewModel(_citiesFilePath,_highTrainFilePath));
                view.ShowDialog();
            }
        }
        #endregion

        private void LoadMap(string layerType, esriGeometryType targetType)
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
            }
            catch (Exception e)
            {
                Messenger.Default.Send(new GenericMessage<string>(e.Message),"Exception");
            }
        }   
    }
}
