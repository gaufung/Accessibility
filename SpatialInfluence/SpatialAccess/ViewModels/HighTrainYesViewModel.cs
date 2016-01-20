using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SpatialAccess.Models;
using SpatialAccess.Services.Algorithm;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.Raster;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    internal  class HighTrainYesViewModel:ViewModelBase
    {

        private static log4net.ILog _log =
           LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly string _cityFilePath;
        private readonly string _highTrainFilePath;

        private string _rasterFilePath;

        /// <summary>
        /// Dijkstra算法
        /// </summary>
        private DijkstraAlgorithm _dijkstra;

        private  RasterReader _rasterReader;

        private Dictionary<string, RasterOp> _highTrainStation; 
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

        public HighTrainYesViewModel(string cityFilePath,string highTrainFilePath)
        {
            _cityFilePath = cityFilePath;
            _highTrainFilePath = highTrainFilePath;
            Cities = new ObservableCollection<CalculatorCity>();
            _dijkstra = new DijkstraAlgorithm(NetWorkUtil.ReadRailway(_highTrainFilePath));
            _highTrainStation = new Dictionary<string, RasterOp>(_dijkstra.Count);
            InitializeControls();
            InitializeCommand();
        }
        private void InitializeCommand()
        {
            ChooseRasterCommand = new RelayCommand(ChooseRaster);
            ConfirmCommand = new RelayCommand(Confirm);
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
            if (string.IsNullOrEmpty(_rasterFilePath)) return;
            string folderPath = DialogHelper.OpenFolderDialog(true);
            if (!string.IsNullOrEmpty(folderPath))
            {
                string rasterWorkSpace = Path.GetDirectoryName(_rasterFilePath);
                _rasterReader = new RasterReader(rasterWorkSpace, RasterFileName + ".tif");
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
            ProgressWait wait = new ProgressWait("计算(已开通)空间可达性");
            Hashtable para = new Hashtable()
            {
                {"wait",wait},{"folderPath",folderPath},{"ret",false}
            };
            Thread t = new Thread(new ParameterizedThreadStart(Run));
            t.Start(para);
            wait.ShowDialog();
            t.Abort();
            return (bool)para["ret"];   

        }

        private void Run(object p)
        {
            Hashtable para = p as Hashtable;
            var wait = para["wait"] as ProgressWait;
            string folderPath = para["folderPath"].ToString();
            int totalCount = Cities.Count(item => item.IsSelected);
            int count = 0;
            try
            {
                wait.SetWaitCaption("计算高铁城市空间可达性");
                foreach (string city in _dijkstra.GetCityEnumerator())
                {
                    wait.SetProgress((double)count++/_dijkstra.Count);
                    _highTrainStation.Add(city, CalculationCity(city));
                }
                Dictionary<string,RasterOp> backup=new Dictionary<string, RasterOp>(_highTrainStation.Count);
                //backup
                foreach (var keyValue in _highTrainStation)
                {
                    backup.Add(keyValue.Key,new RasterOp(keyValue.Value));
                }
                //***********************************
                wait.SetProgress(0);
                wait.SetWaitCaption("计算高铁城市叠加效应");
                count = 0;
                foreach (string city in _dijkstra.GetCityEnumerator())
                {
                    wait.SetProgress((double)count++ / _dijkstra.Count);
                    float[] times = _dijkstra.Dijkstra(city);
                    foreach (var otherCity in _dijkstra.GetCityEnumerator())
                    {
                        if (city!=otherCity)
                        {
                            int cityIndex = _dijkstra.GetCityIndex(otherCity);
                            backup[otherCity].Overlay(item => item + times[cityIndex]);
                            _highTrainStation[city].Overlay(backup[otherCity],Math.Min);
                            backup[otherCity].Overlay(item => item - times[cityIndex]);
                        }
                    }
                  
                }
                //****************************************
                backup.Clear();
                //foreach (var keyValue in _highTrainStation)
                //{
                //    backup.Add(keyValue.Key, new RasterOp(keyValue.Value));
                //}
                wait.SetWaitCaption("计算所有城市空间可达性");
                wait.SetProgress(0);
                count = 0;
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double)count++ / totalCount);
                        RasterOp res;
                        if (_highTrainStation.ContainsKey(calculatorCity.Name))
                        {
                            res = _highTrainStation[calculatorCity.Name];
                        }
                        else
                        {
                            res = CalculationCity(calculatorCity.Name);
                            RasterOp back = res.Clone();
                            
                            foreach (var station in _highTrainStation)
                            {

                                //RasterOp op = back.Clone();
                                City city = Cities.First(item => item.Name == station.Key);
                                Postion pos = _rasterReader.Coordinate(city.XCoord, city.YCoord);
                                float timecost = (float)back.Read(pos.XIndex, pos.YIndex);
                                _highTrainStation[station.Key].Overlay(item=>item+timecost);
                                res.Overlay(_highTrainStation[station.Key],Math.Min);
                                _highTrainStation[station.Key].Overlay(item => item - timecost);
                            }
                        }
                        RasterWriter writer
                            = new RasterWriter(folderPath, calculatorCity.Name + "_高铁通车后", _rasterReader.RasterInfo);
                        res.WriteRaster(writer, "TIFF");
                    }

                }
                para["ret"] = true;
            }
            catch (Exception e)
            {
                _log.Error(e.Message+e.StackTrace);
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();
            }
           
        }

       

        /// <summary>
        /// 计算一个高铁城市的在高铁未通车的时间成本
        /// </summary>
        /// <returns>时间成本操作</returns>
        private RasterOp CalculationCity(string cityName)
        {
            RasterOp rasterOp=new RasterOp(_rasterReader);
            City city = Cities.First(item => item.Name == cityName);
            Postion pos = _rasterReader.Coordinate(city.XCoord,city.YCoord);
            return rasterOp.Calculator(pos);
        }
        /// <summary>
        ///初始化孔家
        /// </summary>
        private void InitializeControls()
        {
            try
            {
                var cities = NetWorkUtil.ReadCities(_cityFilePath);
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
                _log.Error(e.Message+e.StackTrace);
            }

        }   
    }
}
