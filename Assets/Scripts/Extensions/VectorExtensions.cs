using UnityEngine;
using UnityEngine.Animations;

namespace Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 SetAxis(this Vector3 v, Axis axis, float value)
        {
            switch (axis)
            {
                case Axis.X: return new Vector3(value, v.y, v.z);
                case Axis.Y: return new Vector3(v.x, value, v.z);
                case Axis.Z: return new Vector3(v.x, v.y, value);
            }

            return v;
        }
    }
}