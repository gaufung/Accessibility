using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace HighTrainSpatialInfluence.Services.Raster
{
    internal sealed class RasterWriter
    {
        private string _workSpace;
        private string _fileName;

        //public string Format { get; set; }
        /// <summary>
        /// the lower left corner of the raster.
        /// </summary>
        public IPoint OriginPoint { get; set; }

        /// <summary>
        /// X网格的宽度
        /// </summary>
        public Double CellSizeX { get; set; }
        /// <summary>
        /// Y网格的宽度
        /// </summary>
        public Double CellSizeY { get; set; }
        public object NoDataValue { get; set; }

        /// <summary>
        /// Bands 
        /// </summary>
        public int NumBands { get; set; }

        public Int32 Width { get;  private set; }
        public Int32 Height { get; private set; }

        /// <summary>
        /// Spatail Reference
        /// </summary>
        public ISpatialReference SpatialReference { get; set; }

        public RasterWriter(string workSpace, string fileName)
        {
            _workSpace = workSpace;
            _fileName = fileName;
            NumBands = 1;
        }

        public void Write(float?[,] rasterValue, string format)
        {
            Width = rasterValue.GetLength(0);
            Height = rasterValue.GetLength(1);
            IRasterWorkspace2 rasterWs = OpenRasterWorkspace();
            if(SpatialReference==null) SpatialReference=new UnknownCoordinateSystemClass();
            try
            {
                IRasterDataset rasterDataset = rasterWs.CreateRasterDataset(_fileName,
                format, OriginPoint, Width, Height, CellSizeX, CellSizeY, NumBands, rstPixelType.PT_FLOAT,
                SpatialReference, true);
                IRasterBandCollection rasterBands = (IRasterBandCollection)rasterDataset;
                var rasterBand = rasterBands.Item(0);
                var rasterProps = (IRasterProps)rasterBand;
                //Set NoData if necessary. For a multiband image, NoData value needs to be set for each band.
                rasterProps.NoDataValue = -1;
                //Create a raster from the dataset.
                IRaster raster = rasterDataset.CreateDefaultRaster();

                //Create a pixel block.
                IPnt blocksize = new PntClass();
                blocksize.SetCoords(Width, Height);
                IPixelBlock3 pixelblock = raster.CreatePixelBlock(blocksize) as IPixelBlock3;
                //Populate some pixel values to the pixel block.
                var pixels = (Array)pixelblock.get_PixelData(0);
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        if (rasterValue[i, j].HasValue)
                            pixels.SetValue((float)rasterValue[i, j], i, j);
                        else
                        {
                            pixels.SetValue(-1, i, j);
                        }

                pixelblock.set_PixelData(0, pixels);

                //Define the location that the upper left corner of the pixel block is to write.
                IPnt upperLeft = new PntClass();
                upperLeft.SetCoords(0, 0);

                //Write the pixel block.
                IRasterEdit rasterEdit = (IRasterEdit)raster;
                rasterEdit.Write(upperLeft, (IPixelBlock)pixelblock);

                //Release rasterEdit explicitly.
                Marshal.ReleaseComObject(rasterEdit);
            }
            catch (Exception ex)
            {
                throw;
            }
            

        }

        private  IRasterWorkspace2 OpenRasterWorkspace()
        {
           
            try
            {
                IWorkspaceFactory workspaceFact = new RasterWorkspaceFactoryClass();
                return workspaceFact.OpenFromFile(_workSpace, 0) as IRasterWorkspace2;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

    }
}
