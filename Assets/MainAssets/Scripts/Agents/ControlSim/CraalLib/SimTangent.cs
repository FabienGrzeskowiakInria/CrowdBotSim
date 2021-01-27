using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CrowdMP.Core
{

    public class SimTangent : ControlSim
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // EXPORT C++ FUNCTIONS FROM LIB
        [DllImport("CrowdSim")]
        private static extern IntPtr TModel_CreateSimObject();
        [DllImport("CrowdSim")]
        private static extern void TModel_DestroySimObject(IntPtr obj);

        [DllImport("CrowdSim")]
        private static extern void TModel_clear(IntPtr obj);
        [DllImport("CrowdSim")]
        private static extern void TModel_doStep(IntPtr obj, float deltaTime);

        [DllImport("CrowdSim")]
        private static extern void TModel_addObstacle(IntPtr obj, float s_startx, float s_starty, float s_endx, float s_endy);
        [DllImport("CrowdSim")]
        private static extern void TModel_addAgent(IntPtr obj, float s_x, float s_y, float speedComfort, float personalArea, float speedMax, float g_beta, float g_gamma, float _timeHorizon, uint _neighboursMaxCount);
        [DllImport("CrowdSim")]
        private static extern void TModel_addNonResponsiveAgent(IntPtr obj, float s_x, float s_y, float personalArea);
        [DllImport("CrowdSim")]
        private static extern void TModel_setPosition(IntPtr obj, int s_indPedestrian, float s_x, float s_y);
        [DllImport("CrowdSim")]
        private static extern void TModel_setVelocity(IntPtr obj, int s_indPedestrian, float s_x, float s_y);
        [DllImport("CrowdSim")]
        private static extern void TModel_setGoal(IntPtr obj, int s_indPedestrian, float s_x, float s_y);

        [DllImport("CrowdSim")]
        private static extern float TModel_getAgentPositionX(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float TModel_getAgentPositionY(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float TModel_getAgentSpeed(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float TModel_getAgentDirX(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float TModel_getAgentDirY(IntPtr obj, int s_indPedestrian);
        // EXPORT C++ FUNCTIONS FROM LIB
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------


        int ConfigId;
        IntPtr sim;

        public SimTangent(int id)
        {
            ConfigId = id;
            sim = TModel_CreateSimObject();
        }

        ~SimTangent()
        {
            TModel_DestroySimObject(sim);
        }

        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            TangentConfig Tinfos = (TangentConfig)infos;
            TModel_addAgent(sim, -position.x, position.z, Tinfos.speedComfort, Tinfos.personalArea, Tinfos.speedMax, Tinfos.g_beta, Tinfos.g_gamma, Tinfos.timeHorizon, Tinfos.maxNeighbors);
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            TModel_addNonResponsiveAgent(sim, -position.x, position.z, radius);
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                TModel_addNonResponsiveAgent(sim, -pillar.position.x, pillar.position.z, pillar.radius);
            }

            foreach (ObstWall wall in obst.Walls)
            {
                Vector3 center = (wall.A + wall.B + wall.C + wall.D) / 4;
                if (ObstWall.isClockwise(center, wall.A, wall.B) > 0)
                {
                    TModel_addObstacle(sim, -wall.A.x, wall.A.z, -wall.B.x, wall.B.z);
                    TModel_addObstacle(sim, -wall.B.x, wall.B.z, -wall.C.x, wall.C.z);
                    TModel_addObstacle(sim, -wall.C.x, wall.C.z, -wall.D.x, wall.D.z);
                    TModel_addObstacle(sim, -wall.D.x, wall.D.z, -wall.A.x, wall.A.z);
                }
                else
                {
                    TModel_addObstacle(sim, -wall.A.x, wall.A.z, -wall.D.x, wall.D.z);
                    TModel_addObstacle(sim, -wall.D.x, wall.D.z, -wall.C.x, wall.C.z);
                    TModel_addObstacle(sim, -wall.C.x, wall.C.z, -wall.B.x, wall.B.z);
                    TModel_addObstacle(sim, -wall.B.x, wall.B.z, -wall.A.x, wall.A.z);
                }
            }
        }

        public void clear()
        {
            TModel_clear(sim);
        }

        public void doStep(float deltaTime)
        {
            TModel_doStep(sim, deltaTime);
        }

        public Vector3 getAgentPos2d(int id)
        {
            return new Vector3(-TModel_getAgentPositionX(sim, id), 0, TModel_getAgentPositionY(sim, id));
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            return TModel_getAgentSpeed(sim, id) * new Vector3(-TModel_getAgentDirX(sim, id), 0, TModel_getAgentDirY(sim, id));
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            TModel_setPosition(sim, id, -position.x, position.z);
            TModel_setGoal(sim, id, -position.x - goal.x, position.z + goal.z);
        }
    }
}