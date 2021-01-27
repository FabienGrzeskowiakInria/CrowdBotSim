using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;
using crowdbotsim;

public class RosSim : ControlSim
{
    int ConfigId;
    Simulator sim;

    public RosSim(int id)
    {
        ConfigId = id;
        sim = new Simulator();
    }

    public void addAgent(Vector3 position, TrialControlSim infos)
    {
        RosSimConfig ConfigInfos = (RosSimConfig)infos;
        sim.addAgent(position, ConfigInfos.neighborDist, ConfigInfos.maxNeighbors, ConfigInfos.timeHorizon, ConfigInfos.timeHorizonObst, ConfigInfos.radius, ConfigInfos.maxSpeed, new Vector3(0,0,0));
    }

    public void addNonResponsiveAgent(Vector3 position, float radius)
    {
        sim.addAgent(position, 0, 0, 0, 0, radius, 5, new Vector3(0,0,0));
    }

    public void addObstacles(Obstacles obst)
    {
        foreach (ObstCylinder pillar in obst.Pillars)
        {
            sim.addAgent(pillar.position, 0, 0, 0, 0, pillar.radius, 5, new Vector3(0,0,0));
        }

        foreach (ObstWall wall in obst.Walls)
        {
            sim.addWall(wall);
        }
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
