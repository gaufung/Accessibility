using System.Collections.Generic;

namespace HighTrainSpatialInfluence.Services.Config
{
    /// <summary>
    /// 不同类型的速度 其中单位 km/h
    /// </summary>
    internal abstract class Speed
    {
        private readonly Dictionary<string, double> _typeSpeed;

        protected Dictionary<string, double> TypeSpeed
        {
            get { return _typeSpeed; }
        }

        public abstract double this[string type] { get; }

        protected Speed()
        {
            _typeSpeed = new Dictionary<string, double>();
        }
    }
}
