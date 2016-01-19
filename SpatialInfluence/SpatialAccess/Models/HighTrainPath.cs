using System;

namespace SpatialAccess.Models
{
    /// <summary>
    /// 高铁路线
    /// <remarks>高铁连接城市类
    /// </remarks>
    /// </summary>
    [Serializable]
    internal  sealed class HighTrainPath
    {
        /// <summary>
        /// 起点城市名称
        /// </summary>
        public String StartCity { get; set; }
        /// <summary>
        /// 终点城市名称
        /// </summary>
        public String StopCity { get; set; }
        /// <summary>
        /// 距离，单位为米
        /// </summary>
        public Double Distance { get; set; }
        /// <summary>
        /// 速度，单位为km/h
        /// </summary>
        public Double Speed { get; set; }

        /// <summary>
        /// 时间成本,默认使用mi/1k/(km/h)*60=>时间
        /// </summary>
        public  Double TimeCost {
            get
            {
                if (Calcu == null)
                {
                    Calcu = (dis, speed) => (dis)/1000/speed*60;
                }
                return Calcu(Distance, Speed);
            }
        }

        /// <summary>
        /// 计算时间公式
        /// </summary>
        public Func<double, double, double> Calcu{ get; set; } 

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="startCity">起始城市</param>
        /// <param name="stopCity">结束城市</param>
        /// <param name="distance">距离</param>
        /// <param name="speed">速度</param>
        /// <param name="func">时间消耗计算函数</param>
        public HighTrainPath(string startCity,string stopCity,double distance,double speed,Func<double, double, double> func=null)
        {
            StartCity = startCity;
            StopCity = stopCity;
            Distance = distance;
            Speed = speed;
            Calcu = func;
        }
    }
}
