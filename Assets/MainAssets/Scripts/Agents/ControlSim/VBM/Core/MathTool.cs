using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VBM
{
    public class MathTool
    {
        public static float crossProduct(Vector2 v1, Vector2 v2)
        {
            return (v1.x * v2.y) - (v1.y * v2.x);
        }


        public static Vector2 Intersection(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
        {
            float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

            if (tmp == 0)
            {
                // No solution!
                found = false;
                return Vector2.zero;
            }

            float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

            found = true;

            return new Vector2(
                B1.x + (B2.x - B1.x) * mu,
                B1.y + (B2.y - B1.y) * mu
            );
        }

        public static Vector2 RotateV2(Vector2 aPoint, float aRadian)
        {
            float s = Mathf.Sin(aRadian);
            float c = Mathf.Cos(aRadian);
            return new Vector2(aPoint.x * c - aPoint.y * s, aPoint.y * c + aPoint.x * s);
        }
    }
}
