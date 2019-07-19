using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Loom.ZombieBattleground.Helpers
{
    public struct IntVector2
    {
        [JsonProperty("x")]
        public int X;

        [JsonProperty("y")]
        public int Y;

        [JsonConstructor]
        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}", X, Y);
        }
    }

    public struct FloatVector3
    {
        public static FloatVector3 One = new FloatVector3(1, 1, 1);

        public static FloatVector3 Zero = new FloatVector3(0, 0, 0);

        [JsonProperty("x")]
        public float X;

        [JsonProperty("y")]
        public float Y;

        [JsonProperty("z")]
        public float Z;

        [JsonConstructor]
        public FloatVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public FloatVector3(float general)
        {
            X = general;
            Y = general;
            Z = general;
        }

        public override string ToString()
        {
            return string.Format("x:{0}; y:{1}; z:{2}", X, Y, Z);
        }

        public static explicit operator Vector3(FloatVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }

    public static class MathUtility
    {
        public static int Repeat(int value, int length)
        {
            if (length < 1)
                throw new ArgumentOutOfRangeException(nameof(length));

            while (value < 0) {
                value += length;
            }

            while (value >= length) {
                value -= length;
            }

            return value;
        }
    }
}
