using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using SpatialAccess.Services.Raster;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    internal class LocationAccessibilityViewModel:IndexBaseViewModel
    {
        private static log4net.ILog _log =
           LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);   
        public LocationAccessibilityViewModel(string cityFilePath) : base(cityFilePath)
        {
        }

     
        protected override bool Run(string folderPath)
        {
            ProgressWait wait = new ProgressWait("区位可达性");
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
            Hashtable para=p as Hashtable;
            var wait = para["wait"] as ProgressWait;
            var folderPath = para["folderPath"].ToString();
            int totalCount = Cities.Count(item => item.IsSelected);
            int count = 0;
            wait.SetWaitCaption("计算高铁未通车");
            RasterOp.Reset();
            try
            {
                float sumM = SumM();
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double) count++/totalCount);
                        Overlay(RasterOp, HighTrainNoFolderPath, calculatorCity.Name + "_高铁未通车", calculatorCity.Name);
                    }
                }
                RasterOp.Overlay(item => item / sumM);
                Wirte(RasterOp, folderPath, "区位可达性_未通车");
                GC.Collect();

                //*********************************************
                count = 0;
                wait.SetWaitCaption("计算高铁通车后");
                RasterOp.Reset();
                foreach (var calculatorCity in Cities)
                {
                    if (calculatorCity.IsSelected)
                    {
                        wait.SetProgress((double) count++/totalCount);
                        Overlay(RasterOp, HighTrainYesFolderPath, calculatorCity.Name + "_高铁通车后", calculatorCity.Name);
                    }
                }
                RasterOp.Overlay(item => item / sumM);
                Wirte(RasterOp, folderPath, "区位可达性_通车后");
                GC.Collect();
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

        //private void Wirte(RasterOp op, string foldePath, string name)
        //{
        //    RasterWriter writer = new RasterWriter(foldePath, name,Info);
        //    op.WriteRaster(writer,"TIFF");
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basic"></param>
        /// <param name="rasterFolderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="cityName"></param>
        private void Overlay(RasterOp basic, string rasterFolderPath,string fileName,string cityName)
        {
            RasterReader reader = new RasterReader(rasterFolderPath, fileName + ".tif");
            RasterOp op=new RasterOp(reader);
            Overlay(basic, op, CityValues[cityName]);
        }
        /// <summary>
        /// 计算一个城市对其中的影响
        /// </summary>
        /// <param name="basic"></param>
        /// <param name="city"></param>
        /// <param name="value"></param>
        private void Overlay(RasterOp basic, RasterOp city,float value)
        {
            for (int i = 0; i < basic.Width; i++)
            {
                for (int j = 0; j < basic.Height; j++)
                {
                    if (basic.Read(i,j).HasValue)
                    {
                        var timeCost = (float)city.Read(i, j);
                        basic.Write(i,j,(float)basic.Read(i,j)+timeCost*value);
                    }
                }
            }
        }
    }
}
