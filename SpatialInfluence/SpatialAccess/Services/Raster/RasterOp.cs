using System;
using System.Collections.Generic;
using SpatialAccess.Models;
using Wintellect.PowerCollections;

namespace SpatialAccess.Services.Raster
{
    /// <summary>
    /// 栅格操作
    /// </summary>
    internal  class RasterOp
    {
        private readonly float?[,] _raster;

        #region 属性

        public int Width
        {
            get { return _raster.GetLength(0); }
        }

        public int Height
        {
            get { return _raster.GetLength(1); }
        }

        #endregion

        #region 构造函数
        public RasterOp(int width, int height)
        {
            _raster = new float?[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    _raster = null;
                }
            }
        }

        public RasterOp(float?[,] raster)
        {
            _raster = raster;
        }

        public RasterOp(RasterReader reader)
        {
            if (reader == null) throw new ArgumentNullException("栅格文件读取为null");
            _raster = reader.Convert2Matrix();
        }

        public RasterOp(RasterOp op)
        {
            _raster = new float?[op.Width, op.Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    _raster[i, j] = op.Read(i, j);
                }
            }
        }
        #endregion

        #region 获取数据

        public float? Read(int width, int height)
        {
            if (!Valid(width, height)) 
                throw new ArgumentOutOfRangeException("操作范围无效");
            return _raster[width, height];
        }

        public void Reset()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (_raster[i,j].HasValue)
                    {
                        _raster[i, j] = 0.0F;
                    }
                }
            }
        }

        public void Write(int width, int height, float? value)
        {
            if (!Valid(width,height))
                throw new ArgumentOutOfRangeException("操作范围无效");
            _raster[width, height] = value;
        }

        public bool Valid(int width, int height)
        {
            return width >= 0 && width < _raster.GetLength(0)
                   && height >= 0 && height < _raster.GetLength(1);
        }

        public bool ValueValid(int width, int height)
        {
            return Valid(width,height)&&Read(width,height).HasValue;
        }
        #endregion

        public void Overlay(RasterOp raster,Func<float,float,float> func)
        {
            if (Width!=raster.Width||Height!=raster.Height)
            {
                throw new ArgumentException("栅格大小不一致，无法叠加操作");
            }
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Read(i,j).HasValue&&raster.Read(i,j).HasValue)
                    {
                        float basic =Read(i, j).GetValueOrDefault();
                        float overlay =raster.Read(i, j).GetValueOrDefault();
                        Write(i,j,func(basic,overlay));
                    }
                }
            }
        }
        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="func"></param>
        public void Overlay(Func<float,float> func)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Read(i, j).HasValue)
                    {
                        float basic = Read(i, j).GetValueOrDefault();
                        Write(i, j, func(basic));
                    }
                }
            }
        }

        /// <summary>
        /// 将内容写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format"></param>
        public void WriteRaster(RasterWriter writer,string format)
        {
            writer.Write(_raster,format);
        }

        public RasterOp Calculator(Postion startPostion)
        {
            if (!Read(startPostion.XIndex,startPostion.YIndex).HasValue)
                throw new ArgumentOutOfRangeException("起始位置不含有数据");
            RasterPositionValue[,] cost=new RasterPositionValue[Width,Height];
            //初始化操作
            InitializeValues(cost);
            InitializeStart(startPostion,cost);
            //使用orderbag,是的每次取出的最小值
            OrderedBag<RasterPositionValue> bag=new OrderedBag<RasterPositionValue>();
            bag.Add(cost[startPostion.XIndex,startPostion.YIndex]);
            while (bag.Count!=0)
            {
                RasterPositionValue pos = bag.RemoveFirst();
                var postions = Sourround(cost, pos);
                foreach (var postion in postions)
                {
                    double relativeCost = Read(postion.XIndex, postion.YIndex).GetValueOrDefault() * 0.5 +
                                         Read(postion.XIndex, postion.YIndex).GetValueOrDefault()*0.5;
                    if (pos.XIndex!=postion.XIndex&&pos.YIndex!=postion.YIndex)
                    {
                        relativeCost *= Math.Sqrt(2);
                    }
                    cost[postion.XIndex, postion.YIndex].Visited = true;
                    cost[postion.XIndex, postion.YIndex].HasValue = true;
                    cost[postion.XIndex, postion.YIndex].RasterValue = (float) relativeCost
                        +cost[pos.XIndex,pos.YIndex].RasterValue;
                    bag.Add(cost[postion.XIndex, postion.YIndex]);
                }
            }
            return Result(cost);
        }

        private RasterOp Result(RasterPositionValue[,] cost)
        {
            float?[,] raster = new float?[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (cost[i, j].Visited)
                    {
                        raster[i, j] = cost[i, j].RasterValue;
                    }
                    else
                    {
                        raster[i, j] = null;
                    }
                }
            }
            return new RasterOp(raster);
        }

        public IEnumerable<Postion> Sourround(RasterPositionValue[,] cost,
            Postion postion)
        {
            if (VisistedValid(cost, postion.LeftTop()))
                yield return postion.LeftTop();
            if (VisistedValid(cost, postion.Top()))
                yield return postion.Top();
            if (VisistedValid(cost, postion.RightTop()))
                yield return postion.RightTop();
            if (VisistedValid(cost, postion.Right()))
                yield return postion.Right();
            if (VisistedValid(cost, postion.RightButtom()))
                yield return postion.RightButtom();
            if (VisistedValid(cost, postion.Buttom()))
                yield return postion.Buttom();
            if (VisistedValid(cost, postion.LeftButtom()))
                yield return postion.LeftButtom();
            if (VisistedValid(cost, postion.Left()))
                yield return postion.Left();
        }

        private bool VisistedValid(RasterPositionValue[,] cost,Postion pos)
        {
            //return ValueValid(pos.XIndex, pos.YIndex)
            //    &&!cost[pos.XIndex,pos.YIndex].Visited;    
            return pos.XIndex >= 0 && pos.XIndex < Width
                   && pos.YIndex >= 0 && pos.YIndex < Height
                   && _raster[pos.XIndex, pos.YIndex].HasValue
                   && cost[pos.XIndex, pos.YIndex].Visited == false;
        }

        private  void InitializeStart(Postion startPostion,RasterPositionValue[,] cost)
        {
            //startPostion.HasValue = true;
            //startPostion.RasterValue = 0;
            //startPostion.Visited = true;
            cost[startPostion.XIndex, startPostion.YIndex].HasValue = true;
            cost[startPostion.XIndex, startPostion.YIndex].Visited = true;
            cost[startPostion.XIndex, startPostion.YIndex].RasterValue = 0;
        }

        private void InitializeValues(RasterPositionValue[,] cost)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    cost[i, j] = new RasterPositionValue()
                    {
                        HasValue = false,
                        Visited = false,
                        RasterValue = 0,
                        XIndex = i,
                        YIndex = j
                    };
                }
            }
        }

        public RasterOp Clone()
        {
            float?[,] raster=new float?[Width,Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    raster[i, j] = Read(i, j);
                }
            }
            return new RasterOp(raster);
        }
    }
}
