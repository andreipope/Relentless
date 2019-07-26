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

    public struct FloatVector2
    {
        public readonly static FloatVector2 One = new FloatVector2(1, 1);
        public readonly static FloatVector2 Zero = new FloatVector2(0, 0);

        [JsonProperty("x")]
        public float X;

        [JsonProperty("y")]
        public float Y;

        [JsonConstructor]
        public FloatVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public FloatVector2(float general)
        {
            X = general;
            Y = general;
        }

        public override string ToString()
        {
            return $"x:{X}; y:{Y}";
        }

        public static explicit operator Vector2(FloatVector2 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static explicit operator FloatVector2(Vector2 vector)
        {
            return new FloatVector2(vector.x, vector.y);
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
