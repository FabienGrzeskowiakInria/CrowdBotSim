using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class SimVBM : ControlSim
    {
        int ConfigId;
        VBM.Simulator sim;

        public SimVBM(int id)
        {
            ConfigId = id;
            sim = new VBM.Simulator();
        }

        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            VBMConfig VBMinfos = (VBMConfig)infos;
            sim.addAgent(ToolsGeneral.convert(position), VBMinfos.radius, Vector2.zero, VBMinfos.neighborAgentDist, VBMinfos.neighborWallDist, VBMinfos.sigTtca, VBMinfos.sigDca, VBMinfos.sigSpeed, VBMinfos.sigAngle);
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            sim.addNonResponsiveAgent(ToolsGeneral.convert(position), radius);
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                sim.addNonResponsiveAgent(ToolsGeneral.convert(pillar.position),pillar.radius);
            }

            foreach (ObstWall wall in obst.Walls)
            {
                Vector2[] wallPoints = new Vector2[4];
                wallPoints[0] = ToolsGeneral.convert(wall.A);
                wallPoints[1] = ToolsGeneral.convert(wall.B);
                wallPoints[2] = ToolsGeneral.convert(wall.C);
                wallPoints[3] = ToolsGeneral.convert(wall.D);

                sim.addWall(wallPoints);
            }
        }

        public void clear()
        {
            sim.Clear();
        }

        public void doStep(float deltaTime)
        {
            sim.doStep(deltaTime);
        }

        public Vector3 getAgentPos2d(int id)
        {
            return ToolsGeneral.convert(sim.getAgentPosition(id));
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            return ToolsGeneral.convert(sim.getAgentVelocity(id));
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            sim.setAgentPosition(id, ToolsGeneral.convert(position));
            sim.setAgentPrefVelocity(id, ToolsGeneral.convert(goal));
        }
    }
}
