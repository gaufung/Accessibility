using System;

namespace SpatialAccess.Models
{
    internal sealed class RasterPositionValue : Postion, IComparable<RasterPositionValue>
    {
     
        public float RasterValue { get; set; }


        public int CompareTo(RasterPositionValue other)
        {
            if (RasterValue>other.RasterValue)
            {
                return 1;
            }
            if (RasterValue < other.RasterValue)
            {
                return -1;
            }
            return 0;
        }
    }
}
