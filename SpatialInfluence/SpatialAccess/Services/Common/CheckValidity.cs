using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SpatialAccess.Services.Common
{
    public static class CheckValidity
    {

        /// <summary>
        /// 判断要素类的字段是否存在
        /// </summary>
        /// <param name="pFeatureClass">要素类</param>
        /// <param name="fields">字段集合</param>
        /// <returns>是否存在</returns>
        public static bool FieldExistCheck(this IFeatureClass pFeatureClass, params string[] fields)
        {
            return 
                fields.All(field => pFeatureClass.FindField(field) != -1);
        }

        /// <summary>
        /// 判断要素类的字段是否存在
        /// </summary>
        /// <param name="pFeatureClass">要素类</param>
        /// <param name="fields">字段集合</param>
        /// <returns>是否存在</returns>
        public static bool FieldExistCheck(this IFeatureClass pFeatureClass, IEnumerable<string> fields)
        {
            return pFeatureClass.FieldExistCheck(fields.ToArray());
        }
        /// <summary>
        /// 要素类型检查，判断是否符合要求
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool GeometryTypeCheck(this IFeatureClass pFeatureClass, esriGeometryType targetType)
        {
            return pFeatureClass.ShapeType == targetType;
        }

        /// <summary>
        /// 检查所给的字段不为空
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static bool NotNullCheck(this IFeatureClass pFeatureClass, params string[] fields)
        {
            if (!pFeatureClass.FieldExistCheck(fields)) return false;
            var fieldsIndex = pFeatureClass.FieldsIndex(fields);
            IFeatureCursor pFeatureCursor = pFeatureClass.Search(null, false);
            IFeature pFeature;
            bool flag = true;
            var indices = fieldsIndex as int[] ?? fieldsIndex.ToArray();
            while ((pFeature = pFeatureCursor.NextFeature()) != null)
            {
                if (!indices.Any(index => pFeature.Value[index] == null)) continue;
                flag = false;
                break;
            }
            Marshal.ReleaseComObject(pFeatureCursor);
            return flag;
        }

        private static IEnumerable<int> FieldsIndex(this IFeatureClass pFeatureClass, 
            params string[] fields)
        {
            return 
                fields.Select(field => pFeatureClass.Fields.FindField(field));
        }
        /// <summary>
        /// 检查某个字段是否符合类型
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="fieldName"></param>
        /// <param name="targetFieldType"></param>
        /// <returns></returns>
        public static bool FieldTypeCheck(this IFeatureClass pFeatureClass, string fieldName,
           esriFieldType targetFieldType)
        {
            var type = pFeatureClass.Fields.Field[pFeatureClass.Fields.FindField(fieldName)].Type;
            return  type == targetFieldType;
        }

        /// <summary>
        /// 获取所有数据类型为数值int,long,float,double等等
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <returns></returns>
        public static IEnumerable<string> NumbericFieldsName(this IFeatureClass pFeatureClass)
        {
            for (int i = 0; i < pFeatureClass.Fields.FieldCount; i++)
            {
                var fieldType = pFeatureClass.Fields.Field[i].Type;
                if (fieldType == esriFieldType.esriFieldTypeDouble
                    || fieldType == esriFieldType.esriFieldTypeInteger
                    ||fieldType==esriFieldType.esriFieldTypeSingle
                    ||fieldType==esriFieldType.esriFieldTypeSmallInteger)
                {
                    yield return pFeatureClass.Fields.Field[i].Name;
                }
            }
        }

        public static IEnumerable<string> FieldsName(this IFeatureClass pFeatureClass)
        {
            for (int i = 0; i < pFeatureClass.Fields.FieldCount; i++)
            {                
                yield return pFeatureClass.Fields.Field[i].Name;
            }
        }
    }
}
