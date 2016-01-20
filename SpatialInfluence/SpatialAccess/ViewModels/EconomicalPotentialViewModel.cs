using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ESRI.ArcGIS.Carto;
using SpatialAccess.Services.Raster;
using SpatialAccess.Services.ShapeFile;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    class EconomicalPotentialViewModel:IndexBaseViewModel
    {
        public EconomicalPotentialViewModel(string cityFilePath) : base(cityFilePath)
        {
        }

        private String _selectedPopIndex;

        public String SelectedPopIndex
        {
            get { return _selectedPopIndex; }
            set
            {
                _selectedPopIndex = value; 
                RaisePropertyChanged("SelectedPopIndex");
            }
        }

        private Dictionary<string, float> _popDic;

        private void InitPop()
        {
            _popDic=new Dictionary<string, float>();
            if (String.IsNullOrEmpty(_selectedPopIndex)) return;
            ShapeOp op=new ShapeOp(CityFilePath);
            var pFeatureClass = op.OpenFeatureClass();
            var res = op.FindValue(pFeatureClass,"Name", SelectedPopIndex);
            foreach (var re in res)
            {
                _popDic.Add(re.Key,Convert.ToSingle(re.Value));
            }
        }
        protected override bool Run(string folderPath)
        {
            InitPop();
            ProgressWait wait = new ProgressWait("计算经济潜能");
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
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double)count++ / totalCount);
                        Overlay(RasterOp, HighTrainNoFolderPath, calculatorCity.Name + "_高铁未通车", calculatorCity.Name);
                    }
                }
                RasterOp.Overlay(item => item);
                Wirte(RasterOp, folderPath, "经济潜能_未通车");
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
                RasterOp.Overlay(item => item);
                Wirte(RasterOp, folderPath, "经济潜能_通车后");
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
            Overlay(basic, op, cityName);
        }
        private void Overlay(RasterOp basic, RasterOp city, string cityName)
        {
            for (int i = 0; i < basic.Width; i++)
            {
                for (int j = 0; j < basic.Height; j++)
                {
                    if (basic.Read(i, j).HasValue)
                    {
                        float timeCost = (float)city.Read(i, j);
                        if (Math.Abs(timeCost) < 10e-5)
                        {
                            if (_popDic.ContainsKey(cityName))
                            {
                                float tt = (float)(3*Math.Log10(_popDic[cityName]*10));
                                basic.Write(i,j,(float)basic.Read(i,j)+CityValues[cityName]/tt);
                            }
                        }
                        else
                        {
                            basic.Write(i, j, (float)basic.Read(i, j) + CityValues[cityName] / timeCost);
                        }
                    }
                }
            }
        }   
    }
}
