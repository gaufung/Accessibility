using System;
using System.Windows;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using HighTrainSpatialInfluence.Services;
using HighTrainSpatialInfluence.Services.Config;
using HighTrainSpatialInfluence.Services.Raster;
using HighTrainSpatialInfluence.Services.ShapeFile;

namespace HighTrainSpatialInfluence
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();    
            Test();
        }

        private void Test()
        {
            //Speed speed=new XmlSpeed();
            ////栅格化土地利用
            //ShapeReader reader=new ShapeReader(speed,1000,"LandType");
            //IFeatureClass pFeatureClass = reader.OpenFromFile(@"C:\Users\gaufung\Desktop\空间可达性项目\Data\土地利用.shp");
            //IEnvelope pEnvelope = ((IGeoDataset) pFeatureClass).Extent;
            //Raster rater=new LandRaster(1000,pEnvelope,"TIFF");
            //rater.Convert(pFeatureClass,"TimeCost",@"D:\temp1","土地");
            //reader = new ShapeReader(speed, 1000, "Grade"); 
            //IFeatureClass pRoadFeatureClass = reader.OpenFromFile(@"C:\Users\gaufung\Desktop\空间可达性项目\Data\合并公路.shp");
            //rater.Convert(pRoadFeatureClass,"TimeCost",@"D:\temp2","公路");
            string workspace = @"D:\temp1";
            string rasterName = @"土地.tif";
            RasterReaderWriter readerAndWriter=new RasterReaderWriter(workspace,rasterName);
            readerAndWriter.Write(0,0,12);
            Console.WriteLine(readerAndWriter.Read(0,0));
        }
    }
}
