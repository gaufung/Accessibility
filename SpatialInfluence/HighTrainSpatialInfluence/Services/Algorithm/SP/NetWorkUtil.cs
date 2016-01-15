using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using HighTrainSpatialInfluence.Services.ShapeFile;

namespace HighTrainSpatialInfluence.Services.Algorithm.SP
{
    internal class NetWorkUtil
    {
        private static ShapeOp _shapeOp;
        public static IEnumerable<RailwayPath> ReadRailway(string filePath)
        {
            _shapeOp=new ShapeOp(filePath);
            var pFeatureClass = _shapeOp.OpenFeatureClass();
            if (!_shapeOp.IsFiledsExist(pFeatureClass, "Speed", "起点", "终点"))
                throw new ArgumentException("部分字段不存在");
            IFeatureCursor pFeaureCursor=pFeatureClass.Search(null, false);
            IFeature pFeature;
            int speedIndex = pFeatureClass.Fields.FindField("Speed");
            int startIndex = pFeatureClass.Fields.FindField("起点");
            int stopIndex = pFeatureClass.Fields.FindField("终点");
            while ((pFeature=pFeaureCursor.NextFeature())!=null)
            {
                var speed = Convert.ToDouble(pFeature.Value[speedIndex]);
                var start = Convert.ToString(pFeature.Value[startIndex]);
                var stop = Convert.ToString(pFeature.Value[stopIndex]);
                IPolyline polyline = pFeature.Shape as IPolyline;
                var distance = polyline.Length;
                yield return new RailwayPath(start,stop,distance,speed);
            }
            Marshal.ReleaseComObject(pFeaureCursor);
        }

        public static IEnumerable<City> ReadCities(string filePath)
        {
            _shapeOp=new ShapeOp(filePath);
            var pFeatureClass = _shapeOp.OpenFeatureClass();
            if (!_shapeOp.IsFiledsExist(pFeatureClass, "Name"))
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
