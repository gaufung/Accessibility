using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HighTrainSpatialInfluence.Model;
using HighTrainSpatialInfluence.Models;
using HighTrainSpatialInfluence.Services.Algorithm.SP;
using HighTrainSpatialInfluence.Services.Common;
using HighTrainSpatialInfluence.Services.Raster;
using HighTrainSpatialInfluence.Services.ShapeFile;

namespace HighTrainSpatialInfluence.ViewModels
{
    internal class NetworkEfficiencyViewModel:ViewModelBase
    {
        #region 私有成员变量

        private string _citiesFilePath;
        private string _rasterFilePath;
        private ShapeOp _shapeOp;
        #endregion  

        #region 绑定属性
        private ObservableCollection<CalculatorCity> _cities;

        public ObservableCollection<CalculatorCity> Cities
        {
            get { return _cities; }
            set
            {
                _cities = value;
                RaisePropertyChanged("Cities");
            }
        }

        private CalculatorCity _selectedCity;

        public CalculatorCity SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                RaisePropertyChanged("SelectedCity");
            }
        }

        private IEnumerable<String> _indexes;

        public IEnumerable<String> Indexes
        {
            get { return _indexes; }
            set
            {
                _indexes = value;
                RaisePropertyChanged("Indexes");
            }
        }

        private String _selectedIndex;

        public String SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        private Double _speed;

        public Double Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                RaisePropertyChanged("Speed");
            }
        }
        
        #endregion

        public NetworkEfficiencyViewModel(string citiesFilePath)
        {
            _citiesFilePath = citiesFilePath;
            _shapeOp = new ShapeOp(_citiesFilePath);
            Cities = new ObservableCollection<CalculatorCity>();
            InitializeControls();
            InitializeCommand();
        }
        private void InitializeControls()
        {
            Indexes = _shapeOp.Fields();
            var cities = NetWorkUtil.ReadCities(_citiesFilePath);
            foreach (var city in cities)
            {
                Cities.Add(new CalculatorCity(city));
            }
        }
        private void InitializeCommand()
        {
            ChooseRasterCommand = new RelayCommand(ChooseRaster);
            ConfirmCommand = new RelayCommand(Confirm);
        }
        #region 绑定命令

        public RelayCommand ChooseRasterCommand { get; set; }

        private void ChooseRaster()
        {
            DialogHelper dialogHelper = new DialogHelper("tif");
            _rasterFilePath = dialogHelper.OpenFile("选择该城市的时间成本文件");
        }

        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (!PreCheck()) return;
            string folder = System.IO.Path.GetDirectoryName(_rasterFilePath);
            string rasterName = System.IO.Path.GetFileNameWithoutExtension(_rasterFilePath);
            RasterReader reader = new RasterReader(folder, rasterName + ".tif");
            var dic = CitiesValue();
            double numerator = 0;
            double denominator = 0;
            foreach (var city in Cities)
            {
                if (city.IsSelected)
                {
                    Postion pos = reader.Coordinate(city.XCoord, city.YCoord);
                    double distance = Math.Sqrt(Math.Pow(SelectedCity.XCoord - city.XCoord, 2) +
                                                Math.Pow(SelectedCity.YCoord - city.YCoord, 2));
                    var readValue = reader.Read(pos.XIndex, pos.YIndex);
                    if (readValue != null)
                    {
                        if (Math.Abs(distance) < 10e-5)
                        {
                            numerator += 0;
                        }
                        else
                        {
                            float timecost =(float)((distance/1000)/Speed*60);
                            numerator += dic[city.Name]*(float) readValue/timecost;
                        }                      
                        denominator += dic[city.Name];
                    }

                }
            }
            MessageBox.Show(string.Format("城市：{0}的区位可达性为:{1}", SelectedCity.Name, numerator / denominator));

        }

        private Dictionary<string, double> CitiesValue()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>(Cities.Count(item => item.IsSelected == true) + 1);
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            int nameIndex = pFeatureClass.Fields.FindField("Name");
            int valueIndex = pFeatureClass.Fields.FindField(SelectedIndex);
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            IFeature pFeature;
            while ((pFeature = pFeatureCursor.NextFeature()) != null)
            {
                if (Cities.Any(item => item.IsSelected && item.Name == pFeature.Value[nameIndex].ToString()))
                {
                    dic.Add(pFeature.Value[nameIndex].ToString(), Convert.ToDouble(pFeature.Value[valueIndex]));
                }
            }
            Marshal.ReleaseComObject(pFeatureCursor);
            return dic;
        }

        private bool PreCheck()
        {
            if (SelectedCity == null)
            {
                MessageBox.Show("选择城市");
                return false;
            }
            if (string.IsNullOrEmpty(_rasterFilePath))
            {
                MessageBox.Show("请选择计算栅格");
                return false;
            }
            if (string.IsNullOrEmpty(SelectedIndex))
            {
                MessageBox.Show("请选择计算指标");
                return false;
            }
            if (Speed <= 0)
            {
                MessageBox.Show("请输入直线时速");
                return false;
            }
            return true;
        }
        #endregion
    }
}
