﻿using System;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;

namespace HighTrainSpatialInfluence.Services.Raster
{
    internal sealed class RasterWriter
    {
        private string _workSpace;
        private string _fileName;

        public IRasterInfo2 RasterInfo { get; set; }
        ////public string Format { get; set; }
        ///// <summary>
        ///// the lower left corner of the raster.
        ///// </summary>
        //public IPoint OriginPoint { get; set; }

        ///// <summary>
        ///// X网格的宽度
        ///// </summary>
        //public Double CellSizeX { get; set; }
        ///// <summary>
        ///// Y网格的宽度
        ///// </summary>
        //public Double CellSizeY { get; set; }
      

        ///// <summary>
        ///// Bands 
        ///// </summary>
        //public int NumBands { get; set; }

        //public Int32 Width { get;  private set; }
        //public Int32 Height { get; private set; }

        /// <summary>
        /// Spatail Reference
        /// </summary>
       // public ISpatialReference SpatialReference { get; set; }

        public RasterWriter(string workSpace, string fileName, RasterInformation rasterInfo)
        {
            _workSpace = workSpace;
            _fileName = fileName;
            RasterInfo = rasterInfo;
            //NumBands = 1;
        }

        public void Write(float?[,] rasterValue, string format)
        {

            IRasterWorkspace2 rasterWs = OpenRasterWorkspace();
            if (rasterWs==null)
            {
                throw new NullReferenceException("栅格文件打开失败");
            }
            IRasterDataset rasterDataset = rasterWs.CreateRasterDataset(_fileName,
            format, RasterInfo.Origin, RasterInfo.Width, RasterInfo.Height, 
                RasterInfo.CellSize.X, RasterInfo.CellSize.Y, RasterInfo.BandCount, rstPixelType.PT_FLOAT,
            RasterInfo.SpatialReference, true);
            IRasterBandCollection rasterBands = (IRasterBandCollection)rasterDataset;
            var rasterBand = rasterBands.Item(0);
            var rasterProps = (IRasterProps)rasterBand;
            //Set NoData if necessary. For a multiband image, NoData value needs to be set for each band.
            rasterProps.NoDataValue = -1;
            //Create a raster from the dataset.
            IRaster raster = rasterDataset.CreateDefaultRaster();

            //Create a pixel block.
            IPnt blocksize = new PntClass();
            blocksize.SetCoords(RasterInfo.Width, RasterInfo.Height);
            IPixelBlock3 pixelblock = raster.CreatePixelBlock(blocksize) as IPixelBlock3;
            //Populate some pixel values to the pixel block.
            var pixels = (Array)pixelblock.get_PixelData(0);
            for (int i = 0; i < RasterInfo.Width; i++)
                for (int j = 0; j < RasterInfo.Height; j++)
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

        private  IRasterWorkspace2 OpenRasterWorkspace()
        {           
           IWorkspaceFactory workspaceFact = new RasterWorkspaceFactoryClass();
           return workspaceFact.OpenFromFile(_workSpace, 0) as IRasterWorkspace2;         
        }

    }
}
