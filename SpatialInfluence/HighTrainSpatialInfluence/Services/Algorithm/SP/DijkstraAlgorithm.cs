using System;
using System.Collections.Generic;
using System.Linq;
using HighTrainSpatialInfluence.Models;

namespace HighTrainSpatialInfluence.Services.Algorithm.SP
{
    /// <summary>
    /// Dijkstra算法求解最短路径
    /// </summary>
    internal sealed class DijkstraAlgorithm
    {
        private IEnumerable<RailwayPath> _railways;

        private Dictionary<string, int> _cities;

        private double[,] _netWork;

        private const Double NonConnect = -1;
        private const Double Tolerance = 10e-5;
        public DijkstraAlgorithm(IEnumerable<RailwayPath> railways)
        {
            _railways = railways;
            _cities = new Dictionary<string, int>();
            InitializeName();
            InitalizeNet();
        }


        #region 初始化
        /// <summary>
        /// 初始化整个工作区间
        /// </summary>
        private void InitializeName()
        {
            int count = 0;            
            //HashSet<string> set=new HashSet<string>();
            //foreach (var way in _railways)
            //{
            //    set.Add(way.StartCity);
            //    set.Add(way.StopCity);
            //}

            //foreach (var name in set)
            //{
            //    _cities.Add(name,count);
            //    count++;
            //}
            //for (int i = 0; i < _railways.Count; i++)
            //{
            //    string startName = _railways[i].StartCity;
            //    string stopName = _railways[i].StopCity;
            //    if(!_cities.ContainsKey(startName))
            //        _cities.Add(startName, count++);
            //    if(!_cities.ContainsKey(stopName))
            //        _cities.Add(stopName, count++);
            //}

            foreach (var railway in _railways)
            {
                string startName = railway.StartCity;
                string stopName = railway.StopCity;
                if (!_cities.ContainsKey(startName))
                    _cities.Add(startName, count++);
                if (!_cities.ContainsKey(stopName))
                    _cities.Add(stopName, count++);
            }
        }
        
        /// <summary>
        /// 初始化网络
        /// </summary>
        private void InitalizeNet()
        {
            _netWork = new double[_cities.Count, _cities.Count];
            for (int i = 0; i < _netWork.GetLength(0); i++)
            {
                for (int j = 0; j < _netWork.GetLength(1); j++)
                {
                    //同一个点，之间的路径为0
                    if (i == j)
                    {
                        _netWork[i, j] = 0;
                    }
                    //不同的点，先设置不通
                    else
                    {
                        _netWork[i, j] = NonConnect;
                    }
                }
            }
            //设置网络
            foreach (var way in _railways)
            {
                int startIndex = _cities[way.StartCity];
                int stopIndex = _cities[way.StopCity];
                _netWork[startIndex, stopIndex] = way.TimeCost;
                _netWork[stopIndex, startIndex] = way.TimeCost;
            }
        }
        #endregion

        #region Dijkstra算法

        private double[] Dijkstra(int orig)
        {
            int n = _cities.Count;
            double[] shortest=new double[n];
            Boolean[] visited=new bool[n];
            shortest[orig] = 0;
            visited[orig] = true;
            for (int count = 0; count !=n-1; count++)
            {
                int k = -1;
                double dmin = NonConnect;
                for (int i = 0; i < n; i++)
                {
                    if (!visited[i]&&_netWork[orig,i]!=NonConnect)
                    {
                        if (dmin==-1||dmin>_netWork[orig,i])
                        {
                            dmin = _netWork[orig, i];
                            k = i;
                        }
                    }
                }
                // 正确的图生成的矩阵不可能出现K ==-1的情况
                if (k == -1) return null;
                shortest[k] = dmin;
                visited[k] = true;
                // 以k为中间点，修正从原点到未访问各点的距离
                for (int i = 0; i < n; i++)
                {
                    if (!visited[i] && _netWork[k, i] != NonConnect)
                    {
                        double callen = dmin + _netWork[k, i];
                        if (_netWork[orig, i] == NonConnect || _netWork[orig, i] > callen)
                            _netWork[orig, i] = callen;
                    }
                }

            }
            //for (int i = 0; i < n; i++)
            //{
            //    shortest[i] = Double.MaxValue;
            //    visited[i] = false;
            //}
            //初始化
            //shortest[orig] = 0;
            //visited[orig] = true;
            //for (int count = 0; count < n-1; count++)
            //{
            //    int minIndex = -1;
            //    double minCost =Double.MaxValue;
            //    for (int i = 0; i < n; i++)
            //    {
            //        //获取最小值并且计算获取下一个orig
            //        if (!visited[i] && IsConnect(orig,i))
            //        {
            //            shortest[i] = Math.Min(shortest[i], shortest[orig] + _netWork[orig, i]);
            //            if (shortest[i] < minCost)
            //            {
            //                minCost = shortest[i];
            //                minIndex = i;
            //            }
            //        }
            //    }
            //    if (minIndex != -1)
            //    {
            //        orig = minIndex; 
            //        visited[minIndex] = true;
            //    }
            //    else
            //    {
            //        //int minIndex = -1;
            //        //double minCost = Double.MaxValue;
            //    }
            //}
            return shortest;
        }

        public double[] Dijkstra(string name)
        {
            if (_cities.ContainsKey(name))
            {
                return Dijkstra(_cities[name]);
            }
            throw new ArgumentOutOfRangeException("网络中不包含目标城市");
        }

        public IEnumerable<string> GetCityEnumerator()
        {
            return _cities.Select(city => city.Key);
        }

        public int GetCityIndex(string name)
        {
            return _cities.ContainsKey(name) ? _cities[name] : -1;
        }

        private Boolean IsConnect(int start, int stop)
        {
            if (start==stop)
            {
                return false;
            }
            return Math.Abs(_netWork[start, stop] - NonConnect) > Tolerance;
        }
        #endregion
        
    }
}
