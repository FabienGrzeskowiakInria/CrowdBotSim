using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{

    public class SimRVO : ControlSim
    {
        int ConfigId;
        RVO.Simulator sim;

        public SimRVO(int id)
        {
            ConfigId = id;
            sim = RVO.Simulator.Instance;
        }

        public SimRVO(int id, float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst, float radius, float maxSpeed)
        {
            ConfigId = id;
            sim = RVO.Simulator.Instance;
            sim.setAgentDefaults(neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, new RVO.Vector2(0, 0));
        }

        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            RVOconfig RVOinfos = (RVOconfig)infos;
            sim.addAgent(position, RVOinfos.neighborDist, RVOinfos.maxNeighbors, RVOinfos.timeHorizon, RVOinfos.timeHorizonObst, RVOinfos.radius, RVOinfos.maxSpeed, new RVO.Vector2(0, 0));
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            sim.addAgent(position, 0, 0, 0, 0, radius, 5, new RVO.Vector2(0, 0));
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                sim.addAgent(pillar.position, 0, 0, 0, 0, pillar.radius, 5, new RVO.Vector2(0, 0));
            }

            foreach (ObstWall wall in obst.Walls)
            {
                List<RVO.Vector2> poly = new List<RVO.Vector2>();

                Vector3 center = (wall.A + wall.B + wall.C + wall.D) / 4;
                if (ObstWall.isClockwise(center, wall.A, wall.B) > 0)
                {
                    poly.Add(wall.A);
                    poly.Add(wall.B);
                    poly.Add(wall.C);
                    poly.Add(wall.D);

                }
                else
                {
                    poly.Add(wall.A);
                    poly.Add(wall.D);
                    poly.Add(wall.C);
                    poly.Add(wall.B);

                }

                sim.addObstacle(poly);
            }

            sim.processObstacles();
        }

        public void clear()
        {
            sim.Clear();
        }

        public void doStep(float deltaTime)
        {
            sim.setTimeStep(deltaTime);
            sim.doStep();
        }

        public Vector3 getAgentPos2d(int id)
        {
            return sim.getAgentPosition(id);
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            return sim.getAgentVelocity(id);
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            sim.setAgentPosition(id, position);
            sim.setAgentPrefVelocity(id, goal);
        }
    }
}