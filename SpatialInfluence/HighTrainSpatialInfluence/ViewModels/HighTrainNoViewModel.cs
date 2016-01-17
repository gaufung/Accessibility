using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HighTrainSpatialInfluence.Model;
using HighTrainSpatialInfluence.Services.Algorithm;
using HighTrainSpatialInfluence.Services.Algorithm.SP;
using HighTrainSpatialInfluence.Services.Common;
using HighTrainSpatialInfluence.Services.Raster;

namespace HighTrainSpatialInfluence.ViewModels
{
    internal class HighTrainNoViewModel:ViewModelBase
    {
        #region 私有成员

        private string _cityShapeFilePath;
        private string _rasterFilePath;
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
                RasterTimeCost = System.IO.Path.GetFileNameWithoutExtension(_rasterFilePath);
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
                string rasterWorkSpace = System.IO.Path.GetDirectoryName(_rasterFilePath);
                RasterReader reader = new RasterReader(rasterWorkSpace, RasterTimeCost+".tif");
                var raster = reader.Convert2Matrix();
                Postion pos =
                    reader.Coordinate(new PointClass() {X = SelectedCity.XCoord, Y = SelectedCity.YCoord});
                if (pos==null)
                {
                    MessageBox.Show("选择的城市不再范围内");
                    return;
                }
                var res=RasterCost.Calculator(raster, pos);
                RasterWriter writer=new RasterWriter(folderPath,AccessName+".tif");
                writer.OriginPoint = reader.OriginPoint;
                writer.CellSizeX = reader.XCellSize;
                writer.CellSizeY = reader.YCellSize;
                writer.SpatialReference = reader.SpatialReference;
                writer.Write(res, "TIFF");
                MessageBox.Show("生成成功");
                GC.Collect();

            }

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

        public HighTrainNoViewModel(string cityShapeFilePath)
        {
            _cityShapeFilePath = cityShapeFilePath;
            Cities = new ObservableCollection<City>();
            Cities = NetWorkUtil.ReadCities(_cityShapeFilePath);
            ChooseTimeRasterCommand = new RelayCommand(ChooseTimeRater);
            ConfirmCommand=new RelayCommand(Confirm);
        }
        
    }
}
