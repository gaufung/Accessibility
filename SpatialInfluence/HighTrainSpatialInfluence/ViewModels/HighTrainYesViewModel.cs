using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HighTrainSpatialInfluence.Model;
using HighTrainSpatialInfluence.Services.Algorithm;
using HighTrainSpatialInfluence.Services.Algorithm.SP;
using HighTrainSpatialInfluence.Services.Common;
using HighTrainSpatialInfluence.Services.Raster;
using Path = System.IO.Path;

namespace HighTrainSpatialInfluence.ViewModels
{
    /// <summary>
    /// 通高铁后时间成本
    /// </summary>
    internal class HighTrainYesViewModel:ViewModelBase
    {
        #region 私有成员

        /// <summary>
        /// 城市shape文件
        /// </summary>
        private string _cityShapeFilePath;
        /// <summary>
        /// 高铁城市文件
        /// </summary>
        private string _highTrainShapeFilePath;
        /// <summary>
        /// 基础栅格文件
        /// </summary>
        private string _rasterFilePath;

        /// <summary>
        /// Dijkstra算法
        /// </summary>
        private DijkstraAlgorithm _dijkstra;

        private RasterReader reader;
        #endregion

        #region 绑定属性
        private String _rasterTimeCost;

        public String RasterTimeCost
        {
            get { return _rasterTimeCost; }
            set
            {
                _rasterTimeCost = value;
                RaisePropertyChanged("RasterTimeCost");
            }
        }
        private IEnumerable<City> _cities;

        public IEnumerable<City> Cities
        {
            get { return _cities; }
            set
            {
                _cities = value;
                RaisePropertyChanged("Cities");
            }
        }
        private City _selectedCity;

        public City SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                RaisePropertyChanged("SelectedCity");
            }
        }

        private String _accessName;

        public String AccessName
        {
            get { return _accessName; }
            set
            {
                _accessName = value;
                RaisePropertyChanged("AccessName");
            }
        }   
        #endregion

        #region 绑定命令
        
        public RelayCommand ChooseTimeRasterCommand { get; set; }

        private void ChooseTimeRater()
        {
            DialogHelper dialogHelper=new DialogHelper("tif");
            var tempFilePath = dialogHelper.OpenFile("选择时间成本栅格文件");
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                _rasterFilePath = tempFilePath;
                RasterTimeCost = Path.GetFileNameWithoutExtension(_rasterFilePath);
            }
        }

        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (!PreCheck())return;
            DialogHelper dialogHelper = new DialogHelper();
            var folderPath = dialogHelper.OpenFolderDialog(true);
            if (!string.IsNullOrEmpty(folderPath))
            {
                string rasterWorkSpace = Path.GetDirectoryName(_rasterFilePath);
                reader = new RasterReader(rasterWorkSpace, RasterTimeCost + ".tif");
                var raster = reader.Convert2Matrix();
                Postion pos =
                   reader.Coordinate(new PointClass() { X = SelectedCity.XCoord, Y = SelectedCity.YCoord });
                if (pos == null)
                {
                    MessageBox.Show("选择的城市不再范围内");
                    return;
                }   
                //如果没有高铁的时间成本
                var res = RasterCost.Calculator(raster, pos);
                CalculateWithHighTrain(raster, res);
                RasterWriter writer = new RasterWriter(folderPath, AccessName + ".tif");
                writer.OriginPoint = reader.OriginPoint;
                writer.CellSizeX = reader.XCellSize;
                writer.CellSizeY = reader.YCellSize;
                writer.SpatialReference = reader.SpatialReference;
                writer.Write(res, "TIFF");
                MessageBox.Show("生成成功");
                GC.Collect();   
            }
        }

        /// <summary>
        /// 计算城市在拥有高铁后的空间可达性
        /// </summary>
        /// <param name="raster"></param>
        /// <param name="res"></param>
        /// <param name="name"></param>
        private void CalculateWithHighTrain(float?[,] raster, float?[,] res)
        {
            var highTrainCities = _dijkstra.GetCityEnumerator().ToArray();
            foreach (var highTrainCity in highTrainCities)
            {
                double[] cost = _dijkstra.Dijkstra(highTrainCity);
                Postion sourcePos = HighTrainStationPos(highTrainCity);
                var sourceTimeCost = HighTrainStationAccess(raster, sourcePos);
                for (int i = 0; i < highTrainCities.Length; i++)
                {
                    var pos = HighTrainStationPos(highTrainCities[i]);
                    var tempRes = HighTrainStationAccess(raster, pos);
                    RasterCost.Overlay(tempRes,(float)cost[i]);
                    RasterCost.Overlay(sourceTimeCost, tempRes);
                }
                float toHighTrainCost = (float)res[sourcePos.XIndex, sourcePos.YIndex];
                RasterCost.Overlay(sourceTimeCost,toHighTrainCost);
                RasterCost.Overlay(res, sourceTimeCost);
            }
        }

        /// <summary>
        /// 获取高铁城市在网格中的位置
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        private Postion HighTrainStationPos(string stationName)
        {
            City city = _cities.First(item => item.Name == stationName);
            return 
                 reader.Coordinate(new PointClass() { X = city.XCoord, Y = city.YCoord });
        }
        /// <summary>
        /// 获取高铁城市的通达性
        /// </summary>
        /// <param name="raster"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private float?[,] HighTrainStationAccess(float?[,] raster, Postion pos)
        {          
            return RasterCost.Calculator(raster, pos);
        }

        private Boolean PreCheck()
        {
            if (string.IsNullOrEmpty(RasterTimeCost))
            {
                MessageBox.Show("请选择时间成本栅格");
                return false;
            }
            if (SelectedCity == null)
            {
                MessageBox.Show("请选择计算的城市");
                return false;
            }
            if (String.IsNullOrEmpty(AccessName))
            {
                MessageBox.Show("请输入可达性文件名称");
                return false;
            }
            return true;
        }   
        #endregion

        public HighTrainYesViewModel(string cityShapeFilePath,string highTrainShapeFilePath)
        {
            _cityShapeFilePath = cityShapeFilePath;
            _highTrainShapeFilePath = highTrainShapeFilePath;
            Cities = new ObservableCollection<City>();
            Cities = NetWorkUtil.ReadCities(_cityShapeFilePath);
            _dijkstra = new DijkstraAlgorithm(NetWorkUtil.ReadRailway(highTrainShapeFilePath));
            ChooseTimeRasterCommand = new RelayCommand(ChooseTimeRater);
            ConfirmCommand = new RelayCommand(Confirm);
        }
    }
}
