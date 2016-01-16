using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using HighTrainSpatialInfluence.Model;
using HighTrainSpatialInfluence.Services;
using HighTrainSpatialInfluence.Services.Algorithm;
using HighTrainSpatialInfluence.Services.Algorithm.SP;
using HighTrainSpatialInfluence.Services.Config;
using HighTrainSpatialInfluence.Services.Raster;
using HighTrainSpatialInfluence.Services.ShapeFile;
using HighTrainSpatialInfluence.ViewModels;
using FileStream = ESRI.ArcGIS.esriSystem.FileStream;

namespace HighTrainSpatialInfluence
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AxMapControl _mainMapControl;
        public MainWindow()
        {
            InitializeComponent();    
            CreateEngineControls();
            var mainVm=new MainWindowViewModel(_mainMapControl);
            DataContext = mainVm;
        }

        private void CreateEngineControls()
        {
            _mainMapControl=new AxMapControl
            {
                Dock = DockStyle.None,
                BackColor = System.Drawing.Color.AliceBlue
            };
            MainFormsHost.Child = _mainMapControl;
        }
        private void Test()
        {
            //Speed speed = new XmlSpeed();
            ////栅格化土地利用
            //ShapeOp op = new ShapeOp(speed, 1000, "LandType");
            //IFeatureClass pFeatureClass = op.OpenFromFile(@"C:\Users\gaufung\Desktop\空间可达性项目\Data\土地利用.shp");
            //IEnvelope pEnvelope = ((IGeoDataset)pFeatureClass).Extent;
            //Raster rater = new LandRaster(1000, pEnvelope, "TIFF");
            //rater.Convert(pFeatureClass, "TimeCost", @"D:\temp1", "土地.tif");
            //op = new ShapeOp(speed, 1000, "Grade");
            //IFeatureClass pRoadFeatureClass = op.OpenFromFile(@"C:\Users\gaufung\Desktop\空间可达性项目\Data\合并公路.shp");
            //rater.Convert(pRoadFeatureClass, "TimeCost", @"D:\temp1", "公路");
            ////string workspace = @"D:\temp1";
            ////string rasterName = @"土地.tif";
            ////RasterReader readerAndWriter = new RasterReader(workspace, rasterName);
            ////MessageBox.Show(readerAndWriter.Read(0, 0).ToString());
        }

        private void TestOverlay()
        {
            string workspace = @"D:\temp1";
            //string rasterName = @"土地.tif";
            //RasterReader readerAnd = new RasterReader(workspace, rasterName);
            //var rasterLand = readerAnd.Convert2Matrix();
            //readerAnd=new RasterReader(workspace,@"公路.tif");
            //var rasterRoad = readerAnd.Convert2Matrix();
            //RasterCost.Overlay(rasterLand, rasterRoad);
            //RasterWriter writer = new RasterWriter(@"D:\temp1", "整个.tif");
            //writer.OriginPoint = readerAnd.OriginPoint;
            //writer.CellSizeX = readerAnd.XCellSize;
            //writer.CellSizeY = readerAnd.YCellSize;
            //writer.SpatialReference = readerAnd.SpatialReference;
            //writer.Write(rasterLand,"TIFF");
           // Print(raster,"原始数据");
            //int width = readerAnd.Width;
            //int height = readerAnd.Height;
            //var res = RasterCost.Calculator(raster, new Postion() {XIndex = width/2, YIndex = height/2});
          //  Print(res, "新数据");
            //RasterWriter writer = new RasterWriter(@"D:\temp2", "可达性.tif");
            //writer.OriginPoint = readerAnd.OriginPoint;
            //writer.CellSizeX = readerAnd.XCellSize;
            //writer.CellSizeY = readerAnd.YCellSize;
            //writer.SpatialReference = readerAnd.SpatialReference;
            //writer.NoDataValue = readerAnd.NoDataValue;
            //writer.Write(res, "TIFF");
        }

        private void TestArrive()
        {
            //string workspace = @"D:\temp1";
            //string rasterName = @"整个.tif";
            //RasterReader readerAnd = new RasterReader(workspace, rasterName);
            //var rasterLand = readerAnd.Convert2Matrix();
            //int width = readerAnd.Width;
            //int height = readerAnd.Height;
            //var res = RasterCost.Calculator(rasterLand, new Postion() { XIndex = width / 2, YIndex = height / 2 });
            //RasterWriter writer = new RasterWriter(@"D:\temp1", "可达性.tif");
            //writer.OriginPoint = readerAnd.OriginPoint;
            //writer.CellSizeX = readerAnd.XCellSize;
            //writer.CellSizeY = readerAnd.YCellSize;
            //writer.SpatialReference = readerAnd.SpatialReference;
            //writer.Write(res, "TIFF");
        }
        private void CreatePolygon()
        {
        //    Speed speed = new XmlSpeed();
        //    //栅格化土地利用
        //    ShapeOp op = new ShapeOp(speed, 1000, "LandType");
        //    IFeatureClass pFeatureClass = op.OpenFromFile(@"C:\Users\gaufung\Desktop\空间可达性项目\Data\Land.shp");
        //    Ring ring = new RingClass();
        //    object missing = Type.Missing;
        //    IPoint pt = new PointClass();
        //    pt.PutCoords(0,0);
        //    ring.AddPoint(pt,ref missing,ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(5000, 0);
        //    ring.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(5000, 4000);
        //    ring.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(0, 4000);
        //    ring.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(0, 0);
        //    ring.AddPoint(pt, ref missing, ref missing);
        //    IGeometryCollection pointPolygon = new PolygonClass();
        //    pointPolygon.AddGeometry(ring as IGeometry, ref missing, ref missing);
        //    IPolygon polyGonGeo = pointPolygon as IPolygon;
        //    IFeature pFeature = pFeatureClass.CreateFeature();
        //    pFeature.Shape = polyGonGeo;
        //    pFeature.Store();

        //    //another
        //    Ring ring2 = new RingClass();
        //    pt = new PointClass();
        //    pt.PutCoords(5000, 0);
        //    ring2.AddPoint(pt,ref missing,ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(6000, 0);
        //    ring2.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(6000, 1000);
        //    ring2.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(5000,1000);
        //    ring2.AddPoint(pt, ref missing, ref missing);
        //    pt = new PointClass();
        //    pt.PutCoords(5000, 0);
        //    ring2.AddPoint(pt, ref missing, ref missing);
        //    pointPolygon = new PolygonClass();
        //    pointPolygon.AddGeometry(ring2 as IGeometry, ref missing, ref missing);
        //    IPolygon polyGonGeo2 = pointPolygon as IPolygon;
        //    IFeature pFeature2 = pFeatureClass.CreateFeature();
        //    pFeature2.Shape = polyGonGeo2;
        //    pFeature2.Store();
        //}

        //private void Print(float?[,] raster,string name)
        //{
        //    string fileName = @"D:\数据" + name;
        //    using (System.IO.FileStream fs=new System.IO.FileStream(fileName,FileMode.OpenOrCreate,FileAccess.ReadWrite))
        //    {
        //        StreamWriter sw=new StreamWriter(fs);
        //        for (int i = 0; i < raster.GetLength(1); i++)
        //        {
        //            string line=string.Empty;
        //            for (int j = 0; j < raster.GetLength(0); j++)
        //            {
        //                line += raster[j, i].ToString() + "\t";
        //            }
        //            sw.WriteLine(line);
        //            sw.Flush();
        //        }
        //        sw.Close();
        //        fs.Close();
        //    }

        }

        public void TestRailWay()
        {
            string shapeFile = @"C:\Users\gaufung\Desktop\空间可达性项目\Data\高铁.shp";
            var result = NetWorkUtil.ReadRailway(shapeFile).ToList();
            DijkstraAlgorithm algorithm=new DijkstraAlgorithm(result);
            algorithm.Dijkstra("广州");
        }
    }
}
