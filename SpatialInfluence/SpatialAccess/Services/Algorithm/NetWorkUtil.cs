using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SpatialAccess.Models;
using SpatialAccess.Services.Common;
using SpatialAccess.Services.ShapeFile;

namespace SpatialAccess.Services.Algorithm
{
    internal class NetWorkUtil
    {
        private static ShapeOp _shapeOp;
        public static IEnumerable<HighTrainPath> ReadRailway(string filePath)
        {
            _shapeOp=new ShapeOp(filePath);
            var pFeatureClass = _shapeOp.OpenFeatureClass();
            if (!pFeatureClass.FieldExistCheck( "Speed", "起点", "终点"))
                throw new ArgumentException("部分字段不存在");
            IFeatureCursor pFeaureCursor=pFeatureClass.Search(null, false);
            IFeature pFeature;
            int speedIndex = pFeatureClass.Fields.FindField("Speed");
            int startIndex = pFeatureClass.Fields.FindField("起点");
            int stopIndex = pFeatureClass.Fields.FindField("终点");
            while ((pFeature=pFeaureCursor.NextFeature())!=null)
            {
                var speed = Convert.ToSingle(pFeature.Value[speedIndex]);
                var start = Convert.ToString(pFeature.Value[startIndex]);
                var stop = Convert.ToString(pFeature.Value[stopIndex]);
                IPolyline polyline = pFeature.Shape as IPolyline;
                var distance = (float)polyline.Length;
                yield return new HighTrainPath(start, stop, distance, speed);
            }
            Marshal.ReleaseComObject(pFeaureCursor);
        }

        public static IEnumerable<City> ReadCities(string filePath)
        {
            _shapeOp=new ShapeOp(filePath);
            var pFeatureClass = _shapeOp.OpenFeatureClass();
            if (!pFeatureClass.FieldExistCheck("Name"))
                throw new ArgumentException("部分字段不存在");
            IFeatureCursor pFeaureCursor = pFeatureClass.Search(null, false);
            IFeature pFeature;
            int nameIndex = pFeatureClass.Fields.FindField("Name");
            while ((pFeature = pFeaureCursor.NextFeature()) != null)
            {
                var name = Convert.ToString(pFeature.Value[nameIndex]);
                IPoint point = pFeature.Shape as IPoint;
                yield return new City(point.X, point.Y, name);
            }
            Marshal.ReleaseComObject(pFeaureCursor);
        }
    }
}
