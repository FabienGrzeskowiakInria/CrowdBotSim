using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CrowdMP.Core
{

    public class SimHelbing : ControlSim
    {
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // EXPORT C++ FUNCTIONS FROM LIB
        [DllImport("CrowdSim")]
        private static extern IntPtr HModel_CreateSimObject();
        [DllImport("CrowdSim")]
        private static extern void HModel_DestroySimObject(IntPtr obj);

        [DllImport("CrowdSim")]
        private static extern void HModel_doStep(IntPtr obj, float deltaTime);
        [DllImport("CrowdSim")]
        private static extern void HModel_SetBoids(IntPtr obj, bool doBoids);

        [DllImport("CrowdSim")]
        private static extern void HModel_addObstacle(IntPtr obj, float s_startx, float s_starty, float s_endx, float s_endy);
        [DllImport("CrowdSim")]
        private static extern void HModel_addAgent(IntPtr obj, float pos_x, float pos_y, float vel_x, float vel_y, float radius, float neighborDist);
        [DllImport("CrowdSim")]
        private static extern void HModel_addNonResponsiveAgent(IntPtr obj, float pos_x, float pos_y, float vel_x, float vel_y, float radius);
        [DllImport("CrowdSim")]
        private static extern void HModel_setPosition(IntPtr obj, int s_indPedestrian, float s_x, float s_y);
        [DllImport("CrowdSim")]
        private static extern void HModel_setVelocity(IntPtr obj, int s_indPedestrian, float s_x, float s_y);
        [DllImport("CrowdSim")]
        private static extern void HModel_setGoalVel(IntPtr obj, int s_indPedestrian, float s_x, float s_y);

        [DllImport("CrowdSim")]
        private static extern float HModel_getAgentPositionX(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float HModel_getAgentPositionY(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float HModel_getAgentVelX(IntPtr obj, int s_indPedestrian);
        [DllImport("CrowdSim")]
        private static extern float HModel_getAgentVelY(IntPtr obj, int s_indPedestrian);
        // EXPORT C++ FUNCTIONS FROM LIB
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------


        int ConfigId;
        IntPtr sim;

        public SimHelbing(int id, bool doBoids)
        {
            ConfigId = id;
            sim = HModel_CreateSimObject();
            HModel_SetBoids(sim, doBoids);
        }

        ~SimHelbing()
        {
            HModel_DestroySimObject(sim);
        }

        public void addAgent(Vector3 position, TrialControlSim infos)
        {
            HelbingConfig Hinfos = (HelbingConfig)infos;
            HModel_addAgent(sim, -position.x, position.z, 0, 0, Hinfos.radius, Hinfos.neighborDist);
        }

        public void addNonResponsiveAgent(Vector3 position, float radius)
        {
            HModel_addAgent(sim, -position.x, position.z, 0, 0, radius, 0);
        }

        public void addObstacles(Obstacles obst)
        {
            foreach (ObstCylinder pillar in obst.Pillars)
            {
                HModel_addNonResponsiveAgent(sim, -pillar.position.x, pillar.position.z, 0, 0, pillar.radius);
            }

            foreach (ObstWall wall in obst.Walls)
            {
                Vector3 center = (wall.A + wall.B + wall.C + wall.D) / 4;
                if (ObstWall.isClockwise(center, wall.A, wall.B) > 0)
                {
                    HModel_addObstacle(sim, -wall.A.x, wall.A.z, -wall.B.x, wall.B.z);
                    HModel_addObstacle(sim, -wall.B.x, wall.B.z, -wall.C.x, wall.C.z);
                    HModel_addObstacle(sim, -wall.C.x, wall.C.z, -wall.D.x, wall.D.z);
                    HModel_addObstacle(sim, -wall.D.x, wall.D.z, -wall.A.x, wall.A.z);
                }
                else
                {
                    HModel_addObstacle(sim, -wall.A.x, wall.A.z, -wall.D.x, wall.D.z);
                    HModel_addObstacle(sim, -wall.D.x, wall.D.z, -wall.C.x, wall.C.z);
                    HModel_addObstacle(sim, -wall.C.x, wall.C.z, -wall.B.x, wall.B.z);
                    HModel_addObstacle(sim, -wall.B.x, wall.B.z, -wall.A.x, wall.A.z);
                }
            }
        }

        public void clear()
        {
            HModel_DestroySimObject(sim);
            sim = HModel_CreateSimObject();
        }

        public void doStep(float deltaTime)
        {
            HModel_doStep(sim, deltaTime);
        }

        public Vector3 getAgentPos2d(int id)
        {
            return new Vector3(-HModel_getAgentPositionX(sim, id), 0, HModel_getAgentPositionY(sim, id));
        }

        public Vector3 getAgentSpeed2d(int id)
        {
            return new Vector3(-HModel_getAgentVelX(sim, id), 0, HModel_getAgentVelY(sim, id));
        }

        public int getConfigId()
        {
            return ConfigId;
        }

        public void updateAgentState(int id, Vector3 position, Vector3 goal)
        {
            HModel_setPosition(sim, id, -position.x, position.z);
            //HModel_setVelocity(sim, id, -goal.x, goal.z);
            HModel_setGoalVel(sim, id, -goal.x, goal.z);
        }
    }
}