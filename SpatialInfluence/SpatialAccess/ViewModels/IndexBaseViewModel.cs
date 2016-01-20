using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.Models;
using SpatialAccess.Services.Algorithm;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.Raster;
using SpatialAccess.Services.ShapeFile;

namespace SpatialAccess.ViewModels
{
    internal class IndexBaseViewModel:ViewModelBase
    {
        protected string HighTrainNoFolderPath;
        protected string HighTrainYesFolderPath;

        protected readonly string CityFilePath;

        protected RasterInformation Info;
        protected RasterOp RasterOp;

        protected readonly Dictionary<string, float> CityValues; 


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
        private IEnumerable<string> _indexes;

        public IEnumerable<string> Indexes
        {
            get { return _indexes; }
            set
            {
                _indexes = value; 
                RaisePropertyChanged("Indexes");
            }
        }
        private string _selectedIndex;

        public string SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }
        
        #endregion

        public IndexBaseViewModel(string cityFilePath)
        {
            CityFilePath = cityFilePath;
            CityValues=new Dictionary<string, float>();
            InitControls();
            InitCommand();
        }

        private void InitControls()
        {
            Cities = new ObservableCollection<CalculatorCity>();
            try
            {
                var cities = NetWorkUtil.ReadCities(CityFilePath);
                foreach (var city in cities)
                {
                    Cities.Add(new CalculatorCity(city));
                }
                ShapeOp shapeOp=new ShapeOp(CityFilePath);
                Indexes = shapeOp.OpenFeatureClass().NumbericFieldsName();
            }
            catch (ArgumentException e)
            {
                Messenger.Default.Send(new GenericMessage<string>(e.Message), "Exception");
            }
            catch (Exception e)
            {
                //todo 写入log文件中
            }
        }

        #region 命令

        private void InitCommand()
        {
            ChooseHighTrainYesCommand = new RelayCommand(ChooseHighTrainYes);
            ConfirmCommand=new RelayCommand(Confirm);
            ChooseHighTrainNoCommand = new RelayCommand(ChooseHighTrainNo);
        }
        public RelayCommand ChooseHighTrainNoCommand { get; set; }

        private void ChooseHighTrainNo()
        {
            HighTrainNoFolderPath = DialogHelper.OpenFolderDialog(false);
        }

        public RelayCommand ChooseHighTrainYesCommand { get; set; }

        private void ChooseHighTrainYes()
        {
            HighTrainYesFolderPath = DialogHelper.OpenFolderDialog(false);
        }

        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (string.IsNullOrEmpty(HighTrainYesFolderPath)||
                string.IsNullOrEmpty(HighTrainNoFolderPath)
                ||string.IsNullOrEmpty(SelectedIndex))
            {
                Messenger.Default.Send(new GenericMessage<string>("选择相关参数"), "Message");
                return;
            }
            var folderPath = DialogHelper.OpenFolderDialog(true);
            if (string.IsNullOrEmpty(folderPath)) return;
            if (Cities.Count(item => item.IsSelected) == 0) return;
            if (!FileExist())
            {
                Messenger.Default.Send(new GenericMessage<string>("文件缺失"), "Message");
                return;
            }
            Init();
            InitDic();
            
            Messenger.Default.Send(
                Run(folderPath) ? new GenericMessage<string>("指标计算成功") : new GenericMessage<string>("指标计算失败"), "Message");
        }

        private void InitDic()
        {
            ShapeOp shapeOp = new ShapeOp(CityFilePath);
            IFeatureClass pFeatureClass = shapeOp.OpenFeatureClass();
            var res = shapeOp.FindValue(pFeatureClass, "Name", SelectedIndex);
            foreach (var re in res)
            {
                CityValues.Add(re.Key,Convert.ToSingle(re.Value));
            }
            //foreach (var calculatorCity in Cities)
            //{
            //    if (calculatorCity.IsSelected)
            //    {
            //        CityValues.Add(calculatorCity.Name,(float)shapeOp.FindValue(pFeatureClass,"Name",
            //            calculatorCity.Name,SelectedIndex));
            //    }
            //}
        }
        private void Init()
        {
            var firstCity = Cities.First(item => item.IsSelected);
            RasterReader reader = new RasterReader(HighTrainNoFolderPath, firstCity.Name + "_高铁未通车" + ".tif");
            Info = reader.RasterInfo;
            RasterOp = new RasterOp(reader);   
            RasterOp.Reset();
        }

        private bool FileExist()
        {
            foreach (var calculatorCity in Cities)
            {
                if (calculatorCity.IsSelected)
                {
                    string filePath = HighTrainNoFolderPath + @"\" + calculatorCity.Name + "_高铁未通车.tif";
                    if (!File.Exists(filePath)) return false;
                    filePath = HighTrainNoFolderPath + @"\" + calculatorCity.Name + "_高铁未通车.tfw";
                    if (!File.Exists(filePath)) return false;
                    filePath = HighTrainYesFolderPath + @"\" + calculatorCity.Name + "_高铁通车后.tif";
                    if (!File.Exists(filePath)) return false;
                    filePath = HighTrainYesFolderPath + @"\" + calculatorCity.Name + "_高铁通车后.tfw";
                    if (!File.Exists(filePath)) return false;
                }
            }
            return true;
        }
        protected virtual bool Run(string folderPath)
        {
            return true;
        }

        protected float SumM()
        {
            return CityValues.Sum(item => item.Value);
        }
        protected void Wirte(RasterOp op, string foldePath, string name)
        {
            RasterWriter writer = new RasterWriter(foldePath, name, Info);
            op.WriteRaster(writer, "TIFF");
        }
        #endregion
    }
}
