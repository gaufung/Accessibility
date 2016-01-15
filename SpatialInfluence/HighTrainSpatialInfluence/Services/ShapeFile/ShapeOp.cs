using System;
using System.IO;
using System.Linq;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;

namespace HighTrainSpatialInfluence.Services.ShapeFile
{
    internal sealed class ShapeOp
    {

        //#region Log 日志
        //private static ILog _log;

        //static ShapeOp()
        //{
        //    _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //}
        //#endregion

        //// #region Constructor
        ///// <summary>
        ///// 不同种类的速度
        ///// </summary>
        //private Speed _speed;
        ///// <summary>
        ///// 划分的话的单元格宽度,其中单位为km
        ///// </summary>
        //private Double _cellLength;

        //public string TypeFieldName { get; set; }

        ///// <summary>
        ///// Constructor
        ///// </summary>
        ///// <param name="speed">速度km/h</param>
        ///// <param name="cellLength">方格的大小，单位为m</param>
        ///// <param name="typeFieldName">要素类型的字段名称</param>
        //public ShapeOp(Speed speed,Double cellLength,String typeFieldName)
        //{
        //    _speed = speed;
        //    _cellLength = cellLength/1000;
        //    TypeFieldName = typeFieldName;
        //}
        //#endregion

        //public IFeatureClass OpenFromFile(string filePath)
        //{
        //    string path2Workspace = RailwayPath.GetDirectoryName(filePath);
        //    string shapefileName = RailwayPath.GetFileNameWithoutExtension(filePath);
        //    IWorkspaceFactory pWorkspaceFactroy = new ShapefileWorkspaceFactoryClass();
        //    IWorkspace pws = pWorkspaceFactroy.OpenFromFile(path2Workspace, 0);
        //    IFeatureWorkspace pFeatureWorkspace = pws as IFeatureWorkspace;
        //    IFeatureClass pFeatureClass = pFeatureWorkspace.OpenFeatureClass(shapefileName);
        //    AddField(pFeatureClass);
        //    if (!IsFiledsExist(pFeatureClass, TypeFieldName))
        //        throw new ArgumentException("要素类中不包含字段");
        //    SetTimeCost(pFeatureClass);
        //    return pFeatureClass;
        //}

        ///// <summary>
        ///// 添加字段
        ///// </summary>
        ///// <param name="pFeatureClass"></param>
        //private void AddField(IFeatureClass pFeatureClass)
        //{
        //    if (IsFiledsExist(pFeatureClass, "TimeCost")) return;
        //    var pField = new FieldClass();
        //    var pFieldEdit = (IFieldEdit)pField;
        //    pFieldEdit.Name_2 = "TimeCost";
        //    pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
        //    pFeatureClass.AddField(pFieldEdit);
        //}

        ///// <summary>
        ///// 判断要素类的某个字段是否存在
        ///// </summary>
        ///// <param name="pFeatureClass"></param>
        ///// <param name="fieldName"></param>
        ///// <returns></returns>
        //private Boolean IsFiledsExist(IFeatureClass pFeatureClass, string fieldName)
        //{
        //    return pFeatureClass.Fields.FindField(fieldName) != -1;
        //}

        //private void SetTimeCost(IFeatureClass pFeatureClass)
        //{
        //    IDataset pDataset = pFeatureClass as IDataset;
        //    IWorkspaceEdit pWorkspaceEdit = pDataset.Workspace as IWorkspaceEdit;
        //    IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
        //    pWorkspaceEdit.StartEditing(true);
        //    pWorkspaceEdit.StartEditOperation();
        //    IFeature pFeature;
        //    while ((pFeature = pFeatureCursor.NextFeature()) != null)
        //    {
        //        string landType = pFeature.Value[pFeature.Fields.FindField(TypeFieldName)].ToString();
        //        pFeature.Value[pFeature.Fields.FindField("TimeCost")] = (_cellLength) / _speed[landType] * 60;
        //        pFeature.Store();
        //    }
        //    Marshal.ReleaseComObject(pFeatureCursor);
        //    pWorkspaceEdit.StopEditOperation();
        //    pWorkspaceEdit.StopEditing(true);                 
        //}

        private readonly string _filePath;
        public ShapeOp(string filePath)
        {
            _filePath = filePath;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IFeatureClass OpenFeatureClass()
        {
            string path2Workspace = Path.GetDirectoryName(_filePath);
            string shapefileName = Path.GetFileNameWithoutExtension(_filePath);
            IWorkspaceFactory pWorkspaceFactroy = new ShapefileWorkspaceFactoryClass();
            IWorkspace pws = pWorkspaceFactroy.OpenFromFile(path2Workspace, 0);
            IFeatureWorkspace pFeatureWorkspace = pws as IFeatureWorkspace;
            return pFeatureWorkspace != null ? 
                pFeatureWorkspace.OpenFeatureClass(shapefileName) : null;
        }
        public Boolean IsFiledsExist(IFeatureClass pFeatureClass, params string[] fieldNames)
        {
            
            return 
                fieldNames.All(fn => pFeatureClass.Fields.FindField(fn) != -1);
        }

        public  void AddField(IFeatureClass pFeatureClass,string fieldName,esriFieldType fieldType)
        {
            if (IsFiledsExist(pFeatureClass, fieldName)) return;
            var pField = new FieldClass();
            var pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.Type_2 = fieldType;  
            pFeatureClass.AddField(pFieldEdit);
        }
    }
}
