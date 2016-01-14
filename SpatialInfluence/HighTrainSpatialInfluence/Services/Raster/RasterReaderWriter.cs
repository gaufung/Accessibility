using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;

namespace HighTrainSpatialInfluence.Services.Raster
{
    /*
     * 在打开栅格数据集时，
     * 如果数据格式为是ESRI GRID，那么OpenRasterDataset（）方法的参数为栅格要素集的名称，
     * 如果数据格式为TIFF格式，那么该方法的参数为完整的文件名，即要加上.tif扩展名，
     * 例如OpenRasterDataset("hillshade.tif")。下面代码为打开GRID格式的栅格数据：
     */
    /*
     * 栅格的图像的坐标显示
     * (0,0)---------->x
     * |
     * |
     * |
     * |
     * |
     * y
     * 
     */
    internal sealed class RasterReaderWriter
    {

        #region 私有成员
        /// <summary>
        /// 栅格文件所在的文件夹
        /// </summary>
        private string _rasterWorkSapce;
        /// <summary>
        /// 栅格文件名
        /// </summary>
        private string _rasterName;

        /// <summary>
        /// IRasterDataset 对象
        /// </summary>
        private IRasterDataset _pDataset;
        #endregion

        #region 公开的成员包含了栅格的高度和宽度

        public int Height { get; private set; }
        public int Width { get; private set; }

        #endregion


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rasterWorkSapce">栅格文件夹路径</param>
        /// <param name="rasterName">栅格文件,要求有.tif文件名</param>
        public RasterReaderWriter(string rasterWorkSapce, string rasterName)
        {
            _rasterWorkSapce = rasterWorkSapce;
            _rasterName = rasterName;
            Open();
        }

        /// <summary>
        /// 获取该栅格图像的相关数据
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term>
        /// 初始化_pDataset
        /// </term>
        /// <item>
        /// 获取栅格的Height和Width
        /// </item>
        /// </item>
        /// </list>
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        private void Open()
        {
            //Open
            IWorkspaceFactory pRFactory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace2 rasterWorkspace = pRFactory.OpenFromFile(_rasterWorkSapce, 0) as IRasterWorkspace2;
            if (rasterWorkspace == null)
                throw new ArgumentException("栅格文件无法打开");
            _pDataset= rasterWorkspace.OpenRasterDataset(_rasterName);
            //set height and width
            IRasterProps pRasterProps = (IRasterProps)GetRaster();
            Height = pRasterProps.Height;
            Width = pRasterProps.Width;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workSpaceName"></param>
        /// <param name="rasterName"></param>
        public void CopyTo(string workSpaceName, string rasterName)
        {
            IWorkspaceFactory pFactory=new RasterWorkspaceFactoryClass();
            IWorkspace pWorkspace = pFactory.OpenFromFile(workSpaceName, 0);
            if (_pDataset != null)
            {
                _pDataset.Copy(rasterName, pWorkspace);
            }         
        }

        public object Read(int xIndex, int yIndex)
        {
            Debug.Assert(xIndex<Width);
            Debug.Assert(yIndex<Height);
            IRaster pRaster = GetRaster();
            IPnt pnt = new PntClass();
            pnt.SetCoords(xIndex, yIndex);
            IPnt pntSize=new PntClass();
            pntSize.SetCoords(1,1);
            IPixelBlock pixelBlock = pRaster.CreatePixelBlock(pntSize);
            pRaster.Read(pnt,pixelBlock);          
            object obj = pixelBlock.GetVal(0, 0, 0);
            return obj;
        }

        public void Write(int xIndex, int yIndex,object value)
        {
            IPnt blockSize=new PntClass();
            blockSize.SetCoords(Width,Height);
            IRaster pRaster = GetRaster();
            IPixelBlock3 pixelBlock=pRaster.CreatePixelBlock(blockSize) as IPixelBlock3;
            
            //Populate some pixel values to the Pixel block
            Array pixels = (Array) pixelBlock.get_PixelData(0);
            pixels.SetValue(value, xIndex, yIndex);
            pixelBlock.PixelData[0]=pixels;
            //define the location that the upper left corner of the pixel block is to write
            IPnt upperLeft = new PntClass();
            upperLeft.SetCoords(0,0);
            IRasterEdit rasterEdit = (IRasterEdit) pRaster;
            rasterEdit.Write(upperLeft, (IPixelBlock)pixelBlock);
            Marshal.ReleaseComObject(rasterEdit);
        }

        private IRaster GetRaster()
        {
            return _pDataset.CreateDefaultRaster();
        }

    }
}
