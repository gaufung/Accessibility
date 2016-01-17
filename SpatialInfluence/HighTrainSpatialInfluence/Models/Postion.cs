using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HighTrainSpatialInfluence.Model
{
    internal class Postion
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }

        public Postion()
        {
            
        }

        public Postion(int xIndex, int yIndex)
        {
            XIndex = xIndex;
            YIndex = yIndex;
        }

        #region 八个方向
        public Postion LeftTop()
        {
            return new Postion(XIndex - 1, YIndex - 1);
        }

        public Postion Top()
        {
            return new Postion(XIndex, YIndex - 1);
        }

        public Postion RightTop()
        {
            return new Postion(XIndex + 1, YIndex - 1);
        }

        public Postion Right()
        {
            return new Postion(XIndex + 1, YIndex);
        }

        public Postion RightButtom()
        {
            return new Postion(XIndex + 1, YIndex + 1);
        }

        public Postion Buttom()
        {
            return new Postion(XIndex, YIndex + 1);
        }

        public Postion LeftButtom()
        {
            return new Postion(XIndex - 1, YIndex + 1);
        }

        public Postion Left()
        {
            return new Postion(XIndex - 1, YIndex);
        }
        #endregion

        public override bool Equals(object obj)
        {
            Postion pos = (Postion) obj;
            if (pos!=null)
            {
                return pos.XIndex == XIndex && pos.YIndex == YIndex;
            }
            return false;
        }
    }
}
