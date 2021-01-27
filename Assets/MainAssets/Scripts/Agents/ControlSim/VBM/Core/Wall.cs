using UnityEngine;

namespace VBM
{
    public class Wall
    {
        public int id;

        private Vector2[] corners;
        private Vector2 u;
        private Vector2 v;

        public Wall(Vector2[] points, int wallNumber)
        {
            if (points.Length!=4)
            {
                throw (new System.ArgumentException("The number of points does not correspond to a rectangle", "points"));
            }
            id = wallNumber;

            corners = new Vector2[4];
            corners[0] = points[0];
            corners[1] = points[0];
            corners[2] = points[0];
            corners[3] = points[0];

            for (int j=1; j<4; ++j)
            {
                Vector2 p = points[j];
                //    SOUTH   --     SOUTH         WEST
                if (p.y < corners[0].y || (p.y == corners[0].y && p.x < corners[0].x))
                {
                    corners[0] = p;
                }
                //    EAST  --    EAST       SOUTH
                if (p.x> corners[1].x || (p.x== corners[1].x && p.y< corners[1].y))
                {
                    corners[1] = p;
                }
                //    NORTH   --    NORTH          EAST
                if (p.y > corners[2].y || (p.y == corners[2].y && p.x > corners[2].x))
                {
                    corners[2] = p;
                }
                //     WEST   --     WEST         NORTH
                if (p.x < corners[3].x || (p.x == corners[3].x && p.y > corners[3].y))
                {
                    corners[3] = p;
                }
            }

            u = (corners[1] - corners[0]).normalized;
            v = (corners[3] - corners[0]).normalized;
        }

        public Vector2 getPoint(int i)
        {
            return corners[(i+4) % 4];
        }

        /// <summary>
        /// return the normalized vector from the origin point to the next one
        /// </summary>
        /// <param name="originPoint">Starting point of the vector</param>
        /// <returns></returns>
        public Vector2 getDirVector(int originPoint)
        {
            switch (originPoint%4)
            {
                case 0:
                    return u;
                case 1:
                    return v;
                case 2:
                    return -u;
                case 3:
                    return -v;
            }
            return Vector2.zero;
        }

        /// <summary>
        /// return the normal vector from the origin point to the next one
        /// </summary>
        /// <param name="originPoint">Starting point of the vector</param>
        /// <returns></returns>
        public Vector2 getNormal(int originPoint)
        {
            switch (originPoint % 4)
            {
                case 0:
                    return -v;
                case 1:
                    return u;
                case 2:
                    return v;
                case 3:
                    return -u;
            }
            return Vector2.zero;
        }


    }
}
