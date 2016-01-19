using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SpatialAccess.Services.Algorithm;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.Config;
using SpatialAccess.Services.Raster;
using SpatialAccess.Services.ShapeFile;
using SpatialAccess.Views;
using Path = System.IO.Path;

namespace SpatialAccess.ViewModels
{
    class RasterTimeCostViewModel:ViewModelBase
    {
        private readonly string _landUseFilePath;
        private readonly string _trafficRoadFilePath;

        private XmlSpeed _speed;
        private ShapeOp _shapeOp;
        public RasterTimeCostViewModel(string landuse, string traffice)
        {
            _landUseFilePath = landuse;
            _trafficRoadFilePath = traffice;
            _shapeOp=new ShapeOp(string.Empty);
            FillShapeName();
            InitSpeed();
            InitializeCommand();
        }

        /// <summary>
        /// 读取速度配置
        /// </summary>
        private void InitSpeed()
        {
            try
            {
                _speed = new XmlSpeed();
            }
            catch (FileNotFoundException e)
            {
                Messenger.Default.Send(new GenericMessage<string>(e.Message), "Exception");
            }
            catch (FileFormatException e)
            {
                Messenger.Default.Send(new GenericMessage<string>(e.Message), "Exception");
            }
            catch (Exception e)
            {
                //todo 写到配置日志文件中
            }


        }

        private void FillShapeName()
        {
            LandUse = Path.GetFileNameWithoutExtension(_landUseFilePath);
            TrafficRoad = Path.GetFileNameWithoutExtension(_trafficRoadFilePath);
        }

        #region 绑定属性
        private String _landUse;

        public String LandUse
        {
            get { return _landUse; }
            set
            {
                _landUse = value;
                RaisePropertyChanged("LandUse");
            }
        }

        private String _trafficRoad;

        public String TrafficRoad
        {
            get { return _trafficRoad; }
            set
            {
                _trafficRoad = value;
                RaisePropertyChanged("TrafficRoad");
            }
        }

        private Double _cellSize;

        public Double CellSize
        {
            get { return _cellSize; }
            set
            {
                _cellSize = value;
                RaisePropertyChanged("CellSize");
            }
        }

        private String _timeCostName;

        public String TimeCostName
        {
            get { return _timeCostName; }
            set
            {
                _timeCostName = value;
                RaisePropertyChanged("TimeCostName");
            }
        }
        #endregion

        #region 绑定命令

        private void InitializeCommand()
        {
            LandTimeCostCommand = new RelayCommand(LandTimeCost);
            RoadTimeCostCommand=new RelayCommand(RoadTimeCost);
            ConfirmCommand=new RelayCommand(Confirm);
        }

        /// <summary>
        /// 先前检查
        /// </summary>
        /// <returns></returns>
        private Boolean PreCheck()
        {
            return TimeCostName != LandUse && TimeCostName != TrafficRoad
                   && CellSize > 0&&_speed!=null;
        }
        private void AddFields(string shapeFilePath, string fieldName, esriFieldType type)
        {
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            if (pFeatureClass.Fields.FindField(fieldName) == -1)
            {
                _shapeOp.AddField(pFeatureClass, fieldName, type);
            }
        }
        public RelayCommand LandTimeCostCommand { get; set; }

        private void LandTimeCost()
        {
            if (!PreCheck())
            {
                Messenger.Default.Send(new GenericMessage<string>("参数配置错误"), "ArgumentError");
                return;
            }
            AddFields(_landUseFilePath, "TimeCost", esriFieldType.esriFieldTypeDouble);
            if (SetTimeCost(_landUseFilePath, "LandType", "TimeCost"))
            {
                Messenger.Default.Send(new GenericMessage<string>("时间成本设置成功"), "Message");    
            }
            else
            {
                Messenger.Default.Send(new GenericMessage<string>("时间成本设置失败"), "Message");
            }

        }

        public RelayCommand RoadTimeCostCommand { get; set; }

        private void RoadTimeCost()
        {
            if (!PreCheck())
            {
                Messenger.Default.Send(new GenericMessage<string>("参数配置错误"), "ArgumentError");
                return;
            }
            AddFields(_trafficRoadFilePath, "TimeCost", esriFieldType.esriFieldTypeDouble);
            SetTimeCost(_trafficRoadFilePath, "Grade", "TimeCost");
        }

        private Boolean FieldsCheck(string shapeFilePath, params string[] targetField)
        {
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            return pFeatureClass.FieldExistCheck( targetField);
        }
        private bool SetTimeCost(string shapeFilePath, string typeField, string targetField)
        {
            if (!FieldsCheck(shapeFilePath, typeField,targetField))
            {
                Messenger.Default.Send(new GenericMessage<string>(string.Format("{0}shape文件缺少{1}和{2}",shapeFilePath,typeField,targetField))
                    , "ArgumentError");
                return false;
            }
            var wait = new ProgressWait();
            Hashtable para = new Hashtable()
            {
                {"shapeFilePath",shapeFilePath},{"typeField",typeField},{"targetField",targetField}
                ,{"wait",wait},{"ret",false}
            };
            Thread t = new Thread(new ParameterizedThreadStart(TimeCostRun));
            t.Start(para);
            wait.ShowDialog();
            t.Abort();
            return (bool)para["ret"];
        }
        private void TimeCostRun(object p)
        {
            Hashtable para = p as Hashtable;
            string shapeFilePath = para["shapeFilePath"].ToString();
            string typeField = para["typeField"].ToString();
            string targetField = para["targetField"].ToString();
            var wait = para["wait"] as ProgressWait;
            _shapeOp.FilePath = shapeFilePath;
            IFeatureClass pFeatureClass = _shapeOp.OpenFeatureClass();
            IDataset pDataset = pFeatureClass as IDataset;
            string landType = string.Empty;
            try
            {
                IWorkspaceEdit pWorkspaceEdit = pDataset.Workspace as IWorkspaceEdit;
                IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
                pWorkspaceEdit.StartEditing(true);
                pWorkspaceEdit.StartEditOperation();
                IFeature pFeature;
                int count = 0;
                int totalCount = pFeatureClass.FeatureCount(null);
                while (count != totalCount && (pFeature = pFeatureCursor.NextFeature()) != null)
                {
                    count++;
                    wait.SetProgress((double) count/totalCount);
                    landType = pFeature.Value[pFeature.Fields.FindField(typeField)].ToString();
                    pFeature.Value[pFeature.Fields.FindField(targetField)] = ((CellSize)/1000)/_speed[landType]*60;
                    pFeature.Store();
                }
                wait.SetWaitCaption("正在存储数据");
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
                Marshal.ReleaseComObject(pFeatureCursor);
                para["ret"] = true;
            }
            catch (KeyNotFoundException e)
            {
                Messenger.Default.Send(new GenericMessage<string>(
                    string.Format("{0}的速度缺失", landType)
                    , "Exception"));
                para["ret"] = false;
            }
            catch (Exception e)
            {
                //todo 写到日志文件中
                para["ret"] = false;
            }
            finally
            {
                wait.CloseWait();  
            }      
        }   
        #endregion

        #region 生成栅格命令
        public RelayCommand ConfirmCommand { get; set; }

        private void Confirm()
        {
            if (!PreCheck())
            {
                Messenger.Default.Send(new GenericMessage<string>("参数配置错误"), "ArgumentError");
                return;
            }
            if (!FieldsCheck(_landUseFilePath, "TimeCost") || !FieldsCheck(_trafficRoadFilePath, "TimeCost"))
            {
                Messenger.Default.Send(new GenericMessage<string>(string.Format("shape文件缺少{0}字段", "TimeCost"))
                    , "ArgumentError");
                return ;
            }
            var folderPath = DialogHelper.OpenFolderDialog(true);
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (!WriteRaster(folderPath))
                {
                    Messenger.Default.Send(new GenericMessage<string>("时间成本生成失败"), "Message");  
                    return;
                }
                Messenger.Default.Send(new GenericMessage<string>("时间成本生成成功"), "Message");
                GC.Collect();
            }
        }
        private bool WriteRaster(string folderPath)
        {
            var wait = new ContinuousWait("生成时间成本栅格");
            Hashtable para = new Hashtable()
            {
                {"folderPath", folderPath},
                {"wait", wait},
                {"ret", false}
            };
            Thread t = new Thread(new ParameterizedThreadStart(RasterRun));
            t.Start(para);
            wait.ShowDialog();
            t.Abort();
            return (bool)para["ret"];
        }
        private void RasterRun(object p)
        {
            Hashtable para = p as Hashtable;
            string folderPath = para["folderPath"].ToString();
            var wait = para["wait"] as ContinuousWait;
            bool result = true;
            result &= TimeCost2Raster(folderPath);
            
            result &= WriteTimeCostRaster(folderPath);
            GC.Collect();
            para["ret"] = result;
            wait.CloseWait();
        }

        private bool TimeCost2Raster(string folderPath)
        {
            try
            {
                _shapeOp.FilePath = _landUseFilePath;
                IFeatureClass pLandFeatureClass = _shapeOp.OpenFeatureClass();
                _shapeOp.FilePath = _trafficRoadFilePath;
                IFeatureClass pTrafficFeatureClass = _shapeOp.OpenFeatureClass();
                IEnvelope pEnvelope = ((IGeoDataset)pLandFeatureClass).Extent;
                var raster = new RasterConvertion(CellSize, pEnvelope, "TIFF");
                raster.Convert(pLandFeatureClass, "TimeCost", folderPath, LandUse);
                raster.Convert(pTrafficFeatureClass, "TimeCost", folderPath, TrafficRoad);
                return true;
            }
            catch (Exception e)
            {
               //todo 写到日志文件
            }
            return false;
        }
        private bool WriteTimeCostRaster(string folderPath)
        {
            try
            {
                //读取两个文件
                var landReader = new RasterReader(folderPath, LandUse + ".tif");
                var roadReader = new RasterReader(folderPath, TrafficRoad + ".tif");
                RasterOp landOp = new RasterOp(landReader);
                RasterOp roadOp = new RasterOp(roadReader);
                //取最小值
                landOp.Overlay(roadOp,RasterAlgorithm.MinPixel);
                ////写入文件
                RasterWriter writer = new RasterWriter(folderPath, TimeCostName, landReader.RasterInfo);
                landOp.WriteRaster(writer,"TIFF");
                return true;
            }
            catch (Exception e)
            {
                //todo 写到日志文件
            }
            return false;
        }
        #endregion
    }
}
