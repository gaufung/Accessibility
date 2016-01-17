using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using HighTrainSpatialInfluence.Model;

namespace HighTrainSpatialInfluence.Services.Algorithm
{
    internal class RasterCost
    {
        /// <summary>
        /// 两个栅格数据图层叠加,如果两个在相应的位置都有值的话，去最小值添加到基础图层中
        /// </summary>
        /// <param name="basicRaster">基础栅格图层</param>
        /// <param name="overlayRaster">用来叠加的图层</param>
        public static void Overlay(float?[,] basicRaster, float?[,] overlayRaster)
        {
            Debug.Assert(basicRaster.GetLength(0)==overlayRaster.GetLength(0)
                &&basicRaster.GetLength(1)==overlayRaster.GetLength(1));
            for (int i = 0; i < basicRaster.GetLength(0); i++)
            {
                for (int j = 0; j < basicRaster.GetLength(1); j++)
                {
                    if (basicRaster[i, j].HasValue)
                    {
                        if (overlayRaster[i, j].HasValue)
                        {
                            basicRaster[i, j] = Math.Min((float) basicRaster[i, j], (float) overlayRaster[i, j]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basicRaster"></param>
        /// <param name="value"></param>
        public static void Overlay(float?[,] basicRaster, float value)
        {
            for (int i = 0; i < basicRaster.GetLength(0); i++)
            {
                for (int j = 0; j < basicRaster.GetLength(1); j++)
                {
                    if (basicRaster[i, j].HasValue)
                    {

                        basicRaster[i, j] = (float) basicRaster[i, j]+value;
                    }
                }
            }
        }

        #region 计算时间费用

        public static float?[,] Calculator(float?[,] timeCostRaster, Postion startPoistion)
        {
            if(!timeCostRaster[startPoistion.XIndex,startPoistion.YIndex].HasValue)
                 throw new ArgumentException("初始位置不含有时间成本，无法计算");
            CellCost[,] cost=new CellCost[timeCostRaster.GetLength(0),timeCostRaster.GetLength(1)];
            for (int i = 0; i < cost.GetLength(0); i++)
            {
                for(int j=0;j<cost.GetLength(1);j++)
                    cost[i,j]=new CellCost(){Cost = 0,HasValue = false,Visited = false};
            }
            //start
            Queue<Postion> queue=new Queue<Postion>();
            cost[startPoistion.XIndex, startPoistion.YIndex].HasValue = true;
            cost[startPoistion.XIndex, startPoistion.YIndex].Visited = true;
            cost[startPoistion.XIndex, startPoistion.YIndex].Cost = 0.0;
            queue.Enqueue(startPoistion);         
            try
            {
                while (queue.Count != 0)
                {
                    Postion pos = queue.Dequeue();
                    var postions = Sourrond(timeCostRaster, cost, pos).ToArray();
                    foreach (var postion in postions)
                    {                       
                            double relativeCost = ((float)timeCostRaster[pos.XIndex, pos.YIndex]
                                                   + (float)timeCostRaster[postion.XIndex, postion.YIndex]) * 0.5;
                            //如果是对角线问题
                            if (pos.XIndex != postion.XIndex && pos.YIndex != postion.YIndex)
                                relativeCost *= Math.Sqrt(2);
                            cost[postion.XIndex, postion.YIndex].Visited = true;
                            cost[postion.XIndex, postion.YIndex].HasValue = true;
                            cost[postion.XIndex, postion.YIndex].Cost 
                                = (cost[pos.XIndex, pos.YIndex].Cost + relativeCost);
                            queue.Enqueue(postion);                      
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //返回处理后的矩阵
            float?[,] res=new float?[cost.GetLength(0),cost.GetLength(1)];
            for (int i = 0; i < res.GetLength(0); i++)
            {
                for (int j = 0; j < res.GetLength(1); j++)
                {
                    if (cost[i, j].HasValue)
                    {
                        res[i, j] = (float)cost[i, j].Cost;
                    }
                    else
                    {
                        res[i, j] = null;
                    }
                }
            }
            return res;
        }

        private static IEnumerable<Postion> Sourrond(float?[,] timeCostRaster,CellCost[,] cost, Postion postion)
        {

            if (IsValid(timeCostRaster, postion.LeftTop(), cost))
                yield return postion.LeftTop();
            if (IsValid(timeCostRaster, postion.Top(), cost))
                yield return postion.Top();
            if (IsValid(timeCostRaster, postion.RightTop(), cost))
                yield return postion.RightTop();
            if (IsValid(timeCostRaster, postion.Right(), cost))
                yield return postion.Right();
            if (IsValid(timeCostRaster, postion.RightButtom(), cost))
                yield return postion.RightButtom();
            if (IsValid(timeCostRaster, postion.Buttom(), cost))
                yield return postion.Buttom();
            if (IsValid(timeCostRaster, postion.LeftButtom(), cost))
                yield return postion.LeftButtom();
            if (IsValid(timeCostRaster, postion.Left(), cost))
                yield return postion.Left();
        }

        private static Boolean IsValid(float?[,] timeCostRaster, Postion postion, CellCost[,] cost)
        {
            return postion.XIndex >= 0 && postion.XIndex < timeCostRaster.GetLength(0)
                   && postion.YIndex >= 0 && postion.YIndex < timeCostRaster.GetLength(1)
                   && timeCostRaster[postion.XIndex, postion.YIndex].HasValue
                   && !cost[postion.XIndex,postion.YIndex].Visited;
        }
        #endregion
    }
}
