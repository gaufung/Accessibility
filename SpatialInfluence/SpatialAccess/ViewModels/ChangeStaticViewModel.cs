using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.Raster;
using SpatialAccess.Views;

namespace SpatialAccess.ViewModels
{
    class ChangeStaticViewModel:ViewModelBase
    {
        private String _rasterName;

        public String RasterName
        {
            get { return _rasterName; }
            set
            {
                _rasterName = value;
                RaisePropertyChanged("RasterName");
            }
        }

        private string _preRasterFilePath;
        private string _aftRasterFilePath;

        public RelayCommand SelectedPreCommand { get; set; }

        private void SelectedPre()
        {
            _preRasterFilePath = DialogHelper.OpenFile("tif");
        }

        public RelayCommand SelectedAftCommand { get; set; }

        private void SelectedAft()
        {
            _aftRasterFilePath = DialogHelper.OpenFile("tif");
        }
        public ChangeStaticViewModel()
        {
            SelectedAftCommand = new RelayCommand(SelectedAft);
            SelectedPreCommand = new RelayCommand(SelectedPre);
            ConfirmCommand=new RelayCommand(Confirm);
        }

        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (string.IsNullOrEmpty(_rasterName)||string.IsNullOrEmpty(_aftRasterFilePath)
                ||string.IsNullOrEmpty(_preRasterFilePath))
            {
                Messenger.Default.Send(new GenericMessage<string>("参数为设置"), "Message"); 
            }
            var folderPath = DialogHelper.OpenFolderDialog();
            if (string.IsNullOrEmpty(folderPath)) return;
            Messenger.Default.Send(
                Write(folderPath) ? new GenericMessage<string>("变化率计算成功") : new GenericMessage<string>("变化率计算失败"), "Message");
        }

        private bool Write(string  folderPath)
        {
            ContinuousWait wait=new ContinuousWait("计算转换率");
            Hashtable para=new Hashtable()
            {
                {"wait",wait},{"folderPath",folderPath},{"ret",false}
            };
            Thread t=new Thread(new ParameterizedThreadStart(Run));
            t.Start(para);
            wait.ShowDialog();
            t.Abort();
            return (bool)para["ret"];
        }

        private void Run(object p)
        {
            Hashtable para=p as Hashtable;
            ContinuousWait wait = para["wait"] as ContinuousWait;
            var folderPath = para["folderPath"].ToString();
            try
            {
                string preWorkSpace = Path.GetDirectoryName(_preRasterFilePath);
                string preFileName = Path.GetFileNameWithoutExtension(_preRasterFilePath);
                string aftWorkSpace = Path.GetDirectoryName(_aftRasterFilePath);
                string aftFileName = Path.GetFileNameWithoutExtension(_aftRasterFilePath);
                RasterReader preReader = new RasterReader(preWorkSpace, preFileName + ".tif");
                RasterReader aftReader = new RasterReader(aftWorkSpace, aftFileName + ".tif");
                RasterOp preOp = new RasterOp(preReader);
                RasterOp aftOp = new RasterOp(aftReader);
                RasterOp res = preOp.Clone();
                res.Reset();
                for (int i = 0; i < preOp.Width; i++)
                {
                    for (int j = 0; j < preOp.Height; j++)
                    {
                        if (preOp.Read(i, j).HasValue)
                        {
                            float orgin = (float) preOp.Read(i, j);
                            float now = (float) aftOp.Read(i, j);
                            if (Math.Abs(orgin) > 10e-5)
                            {
                                float rate = (now - orgin)/orgin;
                                res.Write(i, j, rate);
                            }
                        }
                    }
                }
                RasterWriter writer=new RasterWriter(folderPath,RasterName,preReader.RasterInfo);
                res.WriteRaster(writer,"TIFF");
                para["ret"] = true;
            }
            catch (Exception e)
            {
                //todo log
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();
            }
        }
    }
}
