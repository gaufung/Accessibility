using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SpatialAccess.Models;
using SpatialAccess.Services.Raster;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    class NetworkEfficiencyViewModel:IndexBaseViewModel
    {
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

        private Dictionary<string, Postion> _cityPos; 
        public NetworkEfficiencyViewModel(string cityFilePath) : base(cityFilePath)
        {
            Speed = 300;
            _cityPos=new Dictionary<string, Postion>();
        }

        private void InitPos()
        {
            foreach (var calculatorCity in Cities)
            {
                if (calculatorCity.IsSelected)
                {
            //            int xIndex = (int)((point.X - rasterProps.Extent.XMin)/rasterProps.MeanCellSize().X);
            //int yIndex = (int) ((rasterProps.Extent.YMax - point.Y)/rasterProps.MeanCellSize().Y);
                    int xIndex = (int) ((calculatorCity.XCoord - Info.OriginPoint.X)/Info.XCellSize);
                    int yIndex=Info.Height-(int)((calculatorCity.YCoord - Info.OriginPoint.Y)/Info.YCellSize);
                    _cityPos.Add(calculatorCity.Name,new Postion(xIndex,yIndex));
                }
            }
        }
        protected override bool Run(string folderPath)
        {
            InitPos();
            ProgressWait wait = new ProgressWait("计算相对可达性");
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
            var folderPath = para["folderPath"].ToString();
            int totalCount = Cities.Count(item => item.IsSelected);
            int count = 0;
            RasterOp.Reset();
            try
            {
                float sumM = SumM();
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double)count++ / totalCount);
                        Overlay(RasterOp, HighTrainNoFolderPath, calculatorCity.Name + "_高铁未通车", calculatorCity.Name);
                    }
                }
                RasterOp.Overlay(item => item / sumM);
                Wirte(RasterOp, folderPath, "相对可达性_未通车");
                GC.Collect();

                //*********************************************
                count = 0;
                wait.SetWaitCaption("计算高铁通车后");
                RasterOp.Reset();
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double)count++ / totalCount);
                        Overlay(RasterOp, HighTrainYesFolderPath, calculatorCity.Name + "_高铁通车后", calculatorCity.Name);
                    }
                }
                RasterOp.Overlay(item => item / sumM);
                Wirte(RasterOp, folderPath, "相对可达性_通车后");
                GC.Collect();
                para["ret"] = true;
            }
            catch (Exception)
            {
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();
            }
        }

        private void Overlay(RasterOp basic, string rasterFolderPath, string fileName, string cityName)
        {
            RasterReader reader = new RasterReader(rasterFolderPath, fileName + ".tif");
            RasterOp op = new RasterOp(reader);
            Overlay(basic,op,cityName);
        }
        private void Overlay(RasterOp basic, RasterOp city,string cityName)
        {
            for (int i = 0; i < basic.Width; i++)
            {
                for (int j = 0; j < basic.Height; j++)
                {
                    if (basic.Read(i, j).HasValue)
                    {
                        
                        var timecost = (float) city.Read(i, j);                       
                        float shorestTimeCost = ShortestTimeCost(i, j, _cityPos[cityName]);
                        if (Math.Abs(shorestTimeCost) < 10e-5)
                        {
                            continue;
                        }
                        basic.Write(i, j, (float)basic.Read(i, j) + timecost/shorestTimeCost * CityValues[cityName]);                                                }
                }
            }
        }
        /// <summary>
        /// 获取某一个城市到某个网格点的直线距离所需的是时间
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="cityName"></param>
        /// <returns></returns>
        public float ShortestTimeCost(int i, int j,Postion cityPos)
        {
            double x = Math.Abs(i - cityPos.XIndex)*Info.XCellSize;
            double y = Math.Abs(j - cityPos.YIndex)*Info.YCellSize;
            double distance = Math.Sqrt(x*x + y*y)/1000;
            return (float)(distance/Speed*60);

        }
    }
}
