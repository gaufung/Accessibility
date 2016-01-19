using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using ESRI.ArcGIS.DataSourcesRaster;
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
    internal  class HighTrainYesViewModel:ViewModelBase
    {
        private readonly string _cityFilePath;
        private readonly string _highTrainFilePath;

        private string _rasterFilePath;

        /// <summary>
        /// Dijkstra算法
        /// </summary>
        private DijkstraAlgorithm _dijkstra;

        private RasterReader _rasterReader;

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
            //if (String.IsNullOrEmpty(_rasterFilePath)) return;
            //var folderPath = DialogHelper.OpenFolderDialog(true);
            //if (!String.IsNullOrEmpty(folderPath))
            //{
            //    if (WriteCityRaster(folderPath))
            //    {
            //        Messenger.Default.Send(new GenericMessage<string>("空间可达性计算成功"), "Message");
            //    }
            //    else
            //    {
            //        Messenger.Default.Send(new GenericMessage<string>("空间可达性计算成功"), "Message");
            //    }
            //}
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
                //foreach (var calculatorCity in Cities)
                //{
                //    if (calculatorCity.IsSelected)
                //    {
                //        wait.SetProgress((double) count++/totalCount);
                //        RasterOp res = CalculationCity(calculatorCity.Name);
                //        RasterOp backup = res.Clone();
                //        foreach (var highTrainCity in _dijkstra.GetCityEnumerator())
                //        {
                //            RasterOp op = backup.Clone();
                //            City city = Cities.First(item => item.Name == highTrainCity);
                //            Postion pos = _rasterReader.Coordinate(city.XCoord, city.YCoord);
                //            var timeCost = (float) backup.Read(pos.XIndex, pos.YIndex);
                //            RasterOp highTrainCityCost = CalutorHighTrainNetwork(highTrainCity);
                //            highTrainCityCost.Overlay(item => item + timeCost);
                //            op.Overlay(highTrainCityCost, Math.Min);
                //            res.Overlay(op, Math.Min);
                //        }
                //        RasterWriter writer
                //            = new RasterWriter(folderPath, calculatorCity.Name + "_高铁通车后", _rasterReader.RasterInfo);
                //        res.WriteRaster(writer, "TIFF");
                //    }
                //}
                wait.SetWaitCaption("计算高铁城市空间可达性");
                foreach (string city in _dijkstra.GetCityEnumerator())
                {
                    wait.SetProgress((double)count++/_dijkstra.Count);
                    _highTrainStation.Add(city, CalculationCity(city));
                }
                wait.SetProgress(0);
                wait.SetWaitCaption("计算高铁城市叠加效应");
                count = 0;
                foreach (string city in _dijkstra.GetCityEnumerator())
                {
                    wait.SetProgress((double)count++ / _dijkstra.Count);
                    RasterOp backup = _highTrainStation[city].Clone();
                    float[] times = _dijkstra.Dijkstra(city);
                    foreach (var otherCity in _dijkstra.GetCityEnumerator())
                    {
                        if (city!=otherCity)
                        {
                            RasterOp op = backup.Clone();
                            int cityIndex = _dijkstra.GetCityIndex(otherCity);
                            op.Overlay(item=>item+times[cityIndex]);
                            op.Overlay(_highTrainStation[otherCity],Math.Min);
                            _highTrainStation[city].Overlay(op,Math.Min);
                        }
                    }
                }
                wait.SetWaitCaption("计算所有城市空间可达性");
                wait.SetProgress(0);
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
                                RasterOp op = back.Clone();
                                City city = Cities.First(item => item.Name == station.Key);
                                Postion pos = _rasterReader.Coordinate(city.XCoord, city.YCoord);
                                float timecost =(float)op.Read(pos.XIndex, pos.YIndex);
                                op.Overlay(item => item + timecost);
                                op.Overlay(station.Value,Math.Min);
                                res.Overlay(op,Math.Min);
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
                //todo 写入日志文件中
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();
            }
           
        }

        /// <summary>
        /// 求解一个高铁站到达整个高铁网络中的时间成本
        /// </summary>
        /// <param name="cityName"></param>
        /// <returns></returns>
        private RasterOp CalutorHighTrainNetwork(string cityName)
        {
            float[] time = _dijkstra.Dijkstra(cityName);
            var cities = _dijkstra.GetCityEnumerator();
            RasterOp result = CalculationCity(cityName);
            RasterOp backup = result.Clone();
            foreach (string city in cities)
            {
                var operation = backup.Clone();
                int cityIndex = _dijkstra.GetCityIndex(city);
                operation.Overlay(item => item + time[cityIndex]);
                operation.Overlay(CalculationCity(city),Math.Min);
                result.Overlay(operation,Math.Min);
            }
            return result;
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
                //todo 写入log文件中
            }

        }   
    }
}
