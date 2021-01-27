using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class SimVOG : ControlSim
    {
        int ConfigId;
        VOG.Simulator sim;

        public SimVOG(int id)
        {
            ConfigId = id;
            sim = new VOG.Simulator(0, false);
            sim.simulateGroup = true;
        }

        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            VOGConfig VOGinfos = (VOGConfig)infos;
            VOG.Agent a = new VOG.Agent(position);
            a.agentID = sim.GetAgents().Count;
            a.MaxSpeed = VOGinfos.maxSpeed;
            a.NeighbourDist = VOGinfos.neighborDist;
            a.AgentTimeHorizon = VOGinfos.timeHorizon;
            a.ObstacleTimeHorizon = VOGinfos.timeHorizonObst;
            a.Height = 1.7f;
            a.Radius = VOGinfos.radius;
            a.MaxNeighbours = VOGinfos.maxNeighbors;
            
            a.groupID = VOGinfos.group.groupID;
            a.formConstraint = VOGinfos.group.useFormation;
            a.groupNeighbourNum = VOGinfos.group.neighbourNum;
            a.neighbourRadius = VOGinfos.group.neighbourDist;
            a.groupNeighbourDist = VOGinfos.group.neighbourDetectionDist;
            a.weightForDesiredVel = VOGinfos.group.weightPrefVel;
            a.weightForGroup = VOGinfos.group.weightGroup;
            a.groupHorizonTime = VOGinfos.group.horizonTime;

            sim.AddAgent(a);
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            VOG.Agent a = new VOG.Agent(position);

            a.agentID = sim.GetAgents().Count;
            a.groupID = -1;
            a.MaxSpeed = 2;
            a.NeighbourDist = 0;
            a.AgentTimeHorizon = 5;
            a.ObstacleTimeHorizon = 5;
            a.Height = 1.7f;
            a.Radius = radius;
            a.MaxNeighbours = 0;

            sim.AddAgent(a);
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                addNonResponsiveAgent(pillar.position, pillar.radius);
            }

            foreach (ObstWall wall in obst.Walls)
            {
                List<RVO.Vector2> poly = new List<RVO.Vector2>();

                Vector3 center = (wall.A + wall.B + wall.C + wall.D) / 4;
                if (ObstWall.isClockwise(center, wall.A, wall.B) > 0)
                {
                    sim.AddObstacle(wall.A, wall.B, 2);
                    sim.AddObstacle(wall.B, wall.C, 2);
                    sim.AddObstacle(wall.C, wall.D, 2);
                    sim.AddObstacle(wall.D, wall.A, 2);
                }
                else
                {
                    sim.AddObstacle(wall.A, wall.D, 2);
                    sim.AddObstacle(wall.D, wall.C, 2);
                    sim.AddObstacle(wall.C, wall.B, 2);
                    sim.AddObstacle(wall.B, wall.A, 2);
                }
            }
        }

        public void clear()
        {
            sim = new VOG.Simulator(0, false);
        }

        public void doStep(float deltaTime)
        {
            sim.Update(deltaTime);
        }

        public Vector3 getAgentPos2d(int id)
        {
            List<VOG.Agent> aList = sim.GetAgents();

            Vector3 pos = aList[id].InterpolatedPosition;
            pos.y = 0;

            return pos;
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            List<VOG.Agent> aList = sim.GetAgents();

            Vector3 vel = aList[id].newVelocity;
            vel.y = 0;

            return vel;
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            List<VOG.Agent> aList = sim.GetAgents();

            aList[id].Teleport(position);
            aList[id].DesiredVelocity = goal;
        }
    }
}
