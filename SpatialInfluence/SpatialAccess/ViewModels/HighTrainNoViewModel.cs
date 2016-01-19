using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.Models;
using SpatialAccess.Services.Algorithm;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.Raster;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    class HighTrainNoViewModel:ViewModelBase
    {
        private readonly string _cityShapeFilePath;
        private string _rasterFilePath;

        #region 绑定属性
        private string _rasterFileName;

        public string RasterFileName
        {
            get { return _rasterFileName; }
            set
            {
                _rasterFileName = value;
                RaisePropertyChanged("RasterFileName");
            }
        }

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
        
        
        #endregion

        #region 绑定命令

        private void InitializeCommand()
        {
            ChooseRasterCommand = new RelayCommand(ChooseRaster);
            ConfirmCommand=new RelayCommand(Confirm);
        }

        public RelayCommand ChooseRasterCommand { get; set; }

        private void ChooseRaster()
        {
            _rasterFilePath = DialogHelper.OpenFile("tif", "选择成本栅格文件");
            if (!string.IsNullOrEmpty(_rasterFilePath))
            {
                RasterFileName = 
                    Path.GetFileNameWithoutExtension(_rasterFilePath);
            }
        }

        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (String.IsNullOrEmpty(_rasterFilePath)) return;
            var folderPath = DialogHelper.OpenFolderDialog(true);
            if (!String.IsNullOrEmpty(folderPath))
            {
                if (WriteCityRaster(folderPath))
                {
                    Messenger.Default.Send(new GenericMessage<string>("空间可达性计算成功"), "Message");
                }
                else
                {
                    Messenger.Default.Send(new GenericMessage<string>("空间可达性计算成功"), "Message");                    
                }
            }
        }

        private bool WriteCityRaster(string folderPath)
        {
            ProgressWait wait=new ProgressWait("计算(未开通)空间可达性");
            Hashtable para=new Hashtable()
            {
                {"wait",wait},{"folderPath",folderPath},{"ret",false}
            };
            Thread t=new Thread(new ParameterizedThreadStart(Run));
            t.Start(para);
            wait.ShowDialog();
            t.Abort();
            return (bool) para["ret"];
        }

        private void Run(object p)
        {
            Hashtable para= p as Hashtable;
            var wait = para["wait"] as ProgressWait;
            string folderPath = para["folderPath"].ToString();
            string rasterFolderPath = Path.GetDirectoryName(_rasterFilePath);
            try
            {
                RasterReader reader = new RasterReader(rasterFolderPath, RasterFileName + ".tif");
                int totalCount = Cities.Count(item => item.IsSelected);
                int count = 0;
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double) count++/totalCount);
                        Run(reader, calculatorCity, folderPath);
                        GC.Collect();
                    }
                }
                para["ret"] = true;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Messenger.Default.Send(new GenericMessage<string>("存在城市不在计算范围内"), "Exception");
                para["ret"] = false;
            }
            catch (Exception e)
            {
                //todo 写在日志文件中
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();
            }
            
        }

        private void Run(RasterReader reader, CalculatorCity city, string folderPath)
        {
            RasterOp rasterOp=new RasterOp(reader);
            Postion pos = reader.Coordinate(city.XCoord,city.YCoord);
            var result = rasterOp.Calculator(pos);
            RasterWriter writer=new RasterWriter(folderPath,city.Name+"_高铁未通车",reader.RasterInfo);
            result.WriteRaster(writer, "TIFF");
        }
        #endregion
        public HighTrainNoViewModel(string cityShapeFilePath)
        {
            _cityShapeFilePath = cityShapeFilePath;
            Cities = new ObservableCollection<CalculatorCity>();
            InitializeControls();
            InitializeCommand();
        }

        /// <summary>
        ///初始化孔家
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                var cities = NetWorkUtil.ReadCities(_cityShapeFilePath);
                foreach (var city in cities)
                {
                    Cities.Add(new CalculatorCity(city));
                }
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
    }
}
