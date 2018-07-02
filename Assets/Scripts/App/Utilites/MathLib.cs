// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB.Helpers
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

    public struct FloatVector3
    {
        public static FloatVector3 one = new FloatVector3(1, 1, 1);
        public static FloatVector3 zero = new FloatVector3(0, 0, 0);

        public float x,
                     y,
                     z;

        public FloatVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public FloatVector3(float general)
        {
            this.x = general;
            this.y = general;
            this.z = general;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}; z:{2}", x, y, z);
        }
    }

    public class MathLib
    {

        public static Vector3 FloatVector3ToVector3(FloatVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

    }
}