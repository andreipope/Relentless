using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB.Helpers
{
    public struct IntVector2
    {
        public int x,
                   y;

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}", x, y);
        }
    }
}