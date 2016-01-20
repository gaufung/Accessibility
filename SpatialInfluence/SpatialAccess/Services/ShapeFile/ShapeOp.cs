using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using SpatialAccess.Services.Common;

namespace SpatialAccess.Services.ShapeFile
{
    /// <summary>
    /// shape文件操作
    /// </summary>
    internal sealed class ShapeOp
    {
        public string FilePath { get; set; }
        public ShapeOp(string filePath)
        {
            FilePath = filePath;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IFeatureClass OpenFeatureClass()
        {
            string path2Workspace = Path.GetDirectoryName(FilePath);
            string shapefileName = Path.GetFileNameWithoutExtension(FilePath);
            IWorkspaceFactory pWorkspaceFactroy = new ShapefileWorkspaceFactoryClass();
            IWorkspace pws = pWorkspaceFactroy.OpenFromFile(path2Workspace, 0);
            IFeatureWorkspace pFeatureWorkspace = pws as IFeatureWorkspace;
            return pFeatureWorkspace != null ? 
                pFeatureWorkspace.OpenFeatureClass(shapefileName) : null;
        }
        public  void AddField(IFeatureClass pFeatureClass,string fieldName,esriFieldType fieldType)
        {
            //如果存在不必添加字段，直接返回
            if (pFeatureClass.FieldExistCheck(fieldName)) return;
            var pField = new FieldClass();
            var pFieldEdit = (IFieldEdit)pField;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.Type_2 = fieldType;  
            pFeatureClass.AddField(pFieldEdit);
        }

        public Dictionary<string,object> FindValue(IFeatureClass pFeatureClass, string keyName, string targetName)
        {
            //int keyFieldIndex = pFeatureClass.Fields.FindField(keyField);
            //int targetFieldIndex = pFeatureClass.Fields.FindField(targetField);
            //IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            //IFeature pFeature;
            //while ((pFeature = pFeatureCursor.NextFeature()) != null)
            //{
            //    if (pFeature.Value[keyFieldIndex].ToString() == keyValue) break;

            //}
            //Marshal.ReleaseComObject(pFeatureCursor);
            //return pFeature == null ? null : pFeature.Value[targetFieldIndex];
            int keyFieldIndex = pFeatureClass.Fields.FindField(keyName);
            int targetFieldIndex = pFeatureClass.Fields.FindField(targetName);
            Dictionary<string,object> dic=new Dictionary<string, object>(pFeatureClass.FeatureCount(null));
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            IFeature pFeature;
            while ((pFeature = pFeatureCursor.NextFeature()) != null)
            {
                if (!dic.ContainsKey(pFeature.Value[keyFieldIndex].ToString()))
                {
                    dic.Add(pFeature.Value[keyFieldIndex].ToString(), pFeature.Value[targetFieldIndex]);
                    
                }
            }
            Marshal.ReleaseComObject(pFeatureCursor);
            return dic;
        }
    }
}
