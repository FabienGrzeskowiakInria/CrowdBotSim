using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class Obstacles
    {

        List<ObstCylinder> pillars;
        List<ObstWall> walls;

        public Obstacles()
        {
            pillars = new List<ObstCylinder>();
            walls = new List<ObstWall>();
        }

        public void addPillar(Vector3 center, float radius)
        {
            pillars.Add(new ObstCylinder(center, radius));
        }

        public void addPillar(GameObject pillar)
        {
            float diameter = pillar.transform.lossyScale.z;
            if (pillar.transform.lossyScale.z < pillar.transform.lossyScale.x)
                diameter = pillar.transform.lossyScale.x;
            addPillar(pillar.transform.position, diameter / 2);
        }

        public void addWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            walls.Add(new ObstWall(p1, p2, p3, p4));
        }

        public void addWall(GameObject wall)
        {
            Vector3 center = wall.transform.position;
            Vector3 p1 = center + wall.transform.forward * wall.transform.lossyScale.z / 2 - wall.transform.right * wall.transform.lossyScale.x / 2;
            Vector3 p2 = center + wall.transform.forward * wall.transform.lossyScale.z / 2 + wall.transform.right * wall.transform.lossyScale.x / 2;
            Vector3 p3 = center - wall.transform.forward * wall.transform.lossyScale.z / 2 + wall.transform.right * wall.transform.lossyScale.x / 2;
            Vector3 p4 = center - wall.transform.forward * wall.transform.lossyScale.z / 2 - wall.transform.right * wall.transform.lossyScale.x / 2;

            walls.Add(new ObstWall(p1, p2, p3, p4));
        }

        public List<ObstCylinder> Pillars { get { return pillars; } }
        public List<ObstWall> Walls { get { return walls; } }
    }

    public class ObstCylinder
    {
        public Vector3 position;
        public float radius;

        public ObstCylinder(Vector3 pos, float r)
        {
            position = pos;
            radius = r;
        }
    }

    public class ObstWall
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;

        public ObstWall(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            A = p1;
            B = p2;
            C = p3;
            D = p4;
        }

        /// <summary>
        /// Check clockwise order between two vector
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>   1 if first comes before second in clockwise order.
        ///             -1 if second comes before first.
        ///             0 if the points are identical.</returns>
        public static float isClockwise(Vector3 center, Vector3 v1, Vector3 v2)
        {
            if (v1 == v2)
                return 0;

            Vector3 OA = (v1 - center);
            Vector3 OB = (v2 - center);
            float det = -OA.x * OB.z + OB.x * OA.z;

            return det;




            //det = (a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y)
        }
    }
}