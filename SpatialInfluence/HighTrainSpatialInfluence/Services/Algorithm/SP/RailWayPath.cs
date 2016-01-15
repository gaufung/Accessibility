using System;

namespace HighTrainSpatialInfluence.Services.Algorithm.SP
{
    /// <summary>
    /// 高铁路线
    /// </summary>
    [Serializable]
    internal  sealed class RailwayPath
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
        /// 时间成本,单位为分钟
        /// </summary>
        public Double TimeCost { get; private set; }

        public RailwayPath(string startCity,string stopCity,double distance,double speed)
        {
            StartCity = startCity;
            StopCity = stopCity;
            Distance = distance;
            Speed = speed;
            TimeCost = ((Distance/1000)/Speed)*60;
        }
    }
}
