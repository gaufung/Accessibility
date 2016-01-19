using System;
using System.Collections.Generic;
using System.Linq;
using SpatialAccess.Models;

namespace SpatialAccess.Services.Algorithm
{
    /// <summary>
    /// Dijkstra算法求解最短路径
    /// </summary>
    internal sealed class DijkstraAlgorithm
    {
        private IEnumerable<HighTrainPath> _railways;

        private Dictionary<string, int> _cities;

        private float[,] _netWork;

        private const float NonConnect = -1;
        private const double Tolerance = 10e-5;

        public int Count { get; private set; }
        public DijkstraAlgorithm(IEnumerable<HighTrainPath> railways)
        {
            _railways = railways;
            _cities = new Dictionary<string, int>();
            InitializeName();
            InitalizeNet();
            Count = _cities.Count;
        }


        #region 初始化
        /// <summary>
        /// 初始化整个工作区间
        /// </summary>
        private void InitializeName()
        {
            int count = 0;            
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
            _netWork = new float[_cities.Count, _cities.Count];
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

        private float[] Dijkstra(int orig)
        {
            int n = _cities.Count;
            float[] shortest = new float[n];
            Boolean[] visited=new bool[n];
            shortest[orig] = 0;
            visited[orig] = true;
            for (int count = 0; count !=n-1; count++)
            {
                int k = -1;
                float dmin = NonConnect;
                for (int i = 0; i < n; i++)
                {
                    if (!visited[i]&&_netWork[orig,i]!=NonConnect)
                    {
                        if (dmin == NonConnect || dmin > _netWork[orig, i])
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
                        float callen = dmin + _netWork[k, i];
                        if (_netWork[orig, i] == NonConnect || _netWork[orig, i] > callen)
                            _netWork[orig, i] = callen;
                    }
                }

            }            
            return shortest;
        }

        public float[] Dijkstra(string name)
        {
            if (_cities.ContainsKey(name))
            {
                return Dijkstra(_cities[name]);
            }
            throw new KeyNotFoundException("网络中不包含目标城市");
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
