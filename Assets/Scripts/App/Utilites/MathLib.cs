using UnityEngine;

namespace Loom.ZombieBattleground.Helpers
{
    public struct IntVector2
    {
        public int X, Y;

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

        public float X, Y, Z;

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
    }

    public class MathLib
    {
        public static Vector3 FloatVector3ToVector3(FloatVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
        {
            Vector2 diference = vec2 - vec1;
            float sign = vec2.x > vec1.x ? -1.0f : 1.0f;
            return (180 + Vector2.Angle(Vector2.up, diference)) * sign;
        }
    }
}
