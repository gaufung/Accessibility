using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using HighTrainSpatialInfluence.Model;

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
    internal sealed class RasterReader
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

        public double XCellSize { get; private set; }
        public double YCellSize { get; private set; }
        public IPoint OriginPoint { get; private set; }
        public ISpatialReference SpatialReference { get; private set; }

        public object NoDataValue { get; private set; }
        #endregion


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="rasterWorkSapce">栅格文件夹路径</param>
        /// <param name="rasterName">栅格文件,要求有.tif文件名</param>
        public RasterReader(string rasterWorkSapce, string rasterName)
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
            XCellSize = pRasterProps.MeanCellSize().X;
            YCellSize = pRasterProps.MeanCellSize().Y;
            OriginPoint = new PointClass
            {
                X = pRasterProps.Extent.XMin,
                Y = pRasterProps.Extent.YMin
            };
            NoDataValue = pRasterProps.NoDataValue;
            SpatialReference = ((IGeoDataset) _pDataset).SpatialReference;
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
        
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="xIndex">水平方向的序号</param>
        /// <param name="yIndex">垂直方向的序号</param>
        /// <returns>获取的值，如果为null,表明该位置没有数据</returns>
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


        /// <summary>
        /// 获取IRaster接口
        /// </summary>
        /// <returns></returns>
        private IRaster GetRaster()
        {
            return _pDataset.CreateDefaultRaster();
        }

        /// <summary>
        /// 为了方便计算，栅格数据转换成二维矩阵
        /// </summary>
        /// <returns></returns>
        public float?[,] Convert2Matrix()
        {
            float?[,] raster = new float?[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    object res = Read(i, j);
                    if (res == null)
                    {
                        raster[i,j] = null;
                    }
                    else
                    {
                       // raster[i, j] = System.Convert.ToSingle(res) < 0 ? null : System.Convert.ToSingle(res);
                        Single value = System.Convert.ToSingle(res);
                        if (value < 0)
                        {
                            raster[i, j] = null;
                        }
                        else
                        {
                            raster[i, j] = value;
                        }
                    }
                }
            }
            return raster;
        }


        //获取某个位置相对于栅格位置的坐标
        public RasterPositionValue Coordinate(IPoint point)
        {
            IRasterProps rasterProps = (IRasterProps) GetRaster();
            if (!Contains(rasterProps.Extent.Envelope, point))
                return null;
            int xIndex = (int)((point.X - rasterProps.Extent.XMin)/rasterProps.MeanCellSize().X);
            int yIndex = (int) ((rasterProps.Extent.YMax - point.Y)/rasterProps.MeanCellSize().Y);
            return new RasterPositionValue()
            {
                RasterValue = Read(xIndex,yIndex),
                XIndex = xIndex,
                YIndex = yIndex
            };
        }
        public RasterPositionValue Coordinate(double x,double y)
        {
            return Coordinate(new PointClass() {X = x, Y = y});
        }
        private Boolean Contains(IEnvelope pEnvelope, IPoint point)
        {
            return point.X <= pEnvelope.XMax && point.X >= pEnvelope.XMin
                   && point.Y >= pEnvelope.YMin && point.Y <= pEnvelope.YMax;
        }

    }
}
