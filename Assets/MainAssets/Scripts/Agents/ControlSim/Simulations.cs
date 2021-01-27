using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Class managing all the simulations used during the trial
/// </summary>
public class SimManager {

    public List<ControlSim> simulators;
    List<int> agentToSim;

    /// <summary>
    /// Initialize the class
    /// </summary>
    public SimManager()
    {
        simulators = new List<ControlSim>();
        agentToSim = new List<int>();
    }

    /// <summary>
    /// Reset the manager
    /// </summary>
    public void clear()
    {
        foreach (ControlSim sim in simulators)
            sim.clear();
        simulators.Clear();
        agentToSim.Clear();
    }

    /// <summary>
    /// Initialize all the simulations needed for the Trial using the LoaderConfig
    /// </summary>
    public void initSimulations(Obstacles obst)
    {
        // Create simulators
        
        if(((TrialCamPlayer)LoaderConfig.playerInfo).in_sim)
            agentToSim.Add(getInternalId(LoaderConfig.playerInfo.getControlSimInfo()));

        foreach (TrialRobot r in LoaderConfig.robotsInfo)
        {
            if (( (TrialRegularRobot)r).in_sim )
                agentToSim.Add(getInternalId(r.getControlSimInfo()));
        }

        foreach (TrialAgent a in LoaderConfig.agentsInfo)
        {
            agentToSim.Add(getInternalId(a.getControlSimInfo()));
        }

        // Fill simulators with agents and Obstacles
        int internalID = 0;

        foreach (ControlSim sim in simulators)
        {
            // Agents
            int agent = 0;

            if(LoaderConfig.playerInfo.GetType() == typeof(TrialCamPlayer))
            {
                if(((TrialCamPlayer)LoaderConfig.playerInfo).in_sim)
                {
                    if (agentToSim[agent] == internalID)
                    {
                        sim.addAgent(LoaderConfig.playerInfo.getStartingPosition(), LoaderConfig.playerInfo.getControlSimInfo());
                    }
                    else
                    {
                        sim.addNonResponsiveAgent(LoaderConfig.playerInfo.getStartingPosition(), LoaderConfig.playerInfo.radius);
                    }
                }
                else
                    agent = -1;
            }
            else
            {
                sim.addNonResponsiveAgent(LoaderConfig.playerInfo.getStartingPosition(), LoaderConfig.playerInfo.radius);
            }

            foreach (TrialRobot r in LoaderConfig.robotsInfo)
            {
                if (( (TrialRegularRobot)r).in_sim )
                {
                    ++agent;
                    // sim.addNonResponsiveAgent(r.getStartingPosition(), r.radius);

                    if (agentToSim[agent] == internalID)
                    {
                        sim.addAgent(r.getStartingPosition(), r.getControlSimInfo());
                    }
                    else
                    {
                        sim.addNonResponsiveAgent(r.getStartingPosition(), r.radius);
                    }

                }
            
            }

            foreach (TrialAgent a in LoaderConfig.agentsInfo)
            {
                ++agent;

                if (agentToSim[agent] == internalID)
                {
                    sim.addAgent(a.getStartingPosition(), a.getControlSimInfo());
                }
                else
                {
                    sim.addNonResponsiveAgent(a.getStartingPosition(), a.radius);
                }
            }

            
            // Obstacles
            sim.addObstacles(obst);

            // update id
            ++internalID;
        }
    }

    public void addNewAgent(Agent a, float radius, TrialControlSim infos)
    {
        // Fill simulators with agents and Obstacles
        int internalID = 0;
        agentToSim.Add(getInternalId(infos));
        foreach (ControlSim sim in simulators)
        {
            if (agentToSim[agentToSim.Count-1] == internalID)
            {
                sim.addAgent(a.transform.position, infos);
            }
            else
            {
                sim.addNonResponsiveAgent(a.transform.position, radius);
            }
            // update id
            ++internalID;
        }
    }

    /// <summary>
    /// Check the config id of a simulation and get the internal one if already created, create a new simulations otherwise
    /// </summary>
    /// <param name="info">Trial parameters of the simulation</param>
    /// <returns>Internal id</returns>
    private int getInternalId(TrialControlSim info)
    {
        int id = -1;
        if (info == null)
        {
            return id;
        }

        foreach (ControlSim sim in simulators) {
            if (sim.getConfigId() == info.getConfigId())
            {
                id = info.getConfigId();
                break;
            }
        }
        if (id<0)
        {
            id = info.getConfigId();
            simulators.Add(info.createControlSim(id));
        }

        return id;
    }

    /// <summary>
    /// Update all the simulations with XP state, perform a simulation step and ovveride controlled agent state
    /// </summary>
    /// <param name="deltaTime">Time since last step</param>
    /// <param name="posList">List of current position fo the agents</param>
    /// <param name="playerGoalState">The player state he is trying to reach</param>
    /// <param name="agentsGoalState">The agent states they are trying to reach</param>
    public void doStep(float deltaTime, List<Vector3> posList, Agent playerGoalState, List<Agent> agentsGoalState, List<Robot> robots = null)
    {
        int i = 0;
        int in_sim_id = 0; 
        bool player_in_sim = false;

        List<bool> robot_in_sim = new List<bool>();
        int index_robot = 0;

        foreach(TrialRobot rinfo in LoaderConfig.robotsInfo)
        {
            robot_in_sim.Add( ((TrialRegularRobot)rinfo).in_sim );
        }

        // Update simulators state
        foreach (ControlSim sim in simulators)
        {
            i = 0;
            if(LoaderConfig.playerInfo.GetType() == typeof(TrialCamPlayer))
            {
                if(((TrialCamPlayer)LoaderConfig.playerInfo).in_sim)
                {
                    sim.updateAgentState(i, posList[i], (playerGoalState.Position- posList[i])/deltaTime);
                    player_in_sim = true;
                }
                else
                {
                    i = -1;
                }
            }
            else
            {
                sim.updateAgentState(i, posList[i], Vector3.zero);
            }


            foreach(Robot robot in robots)
            {
                if(robot_in_sim[index_robot])
                {
                    ++i;
                    sim.updateAgentState(i, robot.transform.Find("base_link").position, Vector3.zero);
                }
                index_robot++;
            }
            
            foreach (Agent a in agentsGoalState)
            {
                ++i;
                ++in_sim_id;

                sim.updateAgentState(i, posList[i], (a.Position - posList[i]) / deltaTime);
            }

            sim.doStep(deltaTime);
        }


        i = 0;
        int simId = agentToSim[i];

        if(LoaderConfig.playerInfo.GetType() == typeof(TrialCamPlayer))
        {
            if(player_in_sim)
            {
                if (simId >= 0)
                    playerGoalState.simOverride(simulators[simId].getAgentPos2d(i), simulators[simId].getAgentSpeed2d(i));
            }
            else
            {
                i = -1;
            }
        }

        index_robot = 0;
        foreach(Agent robot in robots)
        {
            if(robot_in_sim != null)
            {
                if(robot_in_sim[index_robot])
                {
                    ++i;
                    simId = agentToSim[i];
                    if (simId >= 0)
                        robot.simOverride(simulators[simId].getAgentPos2d(i), simulators[simId].getAgentSpeed2d(i));
                }
                index_robot++; 
            }
        }

        foreach (Agent a in agentsGoalState)
        {
            ++i;
            simId = agentToSim[i];
            if (simId >= 0)
            {
                a.simOverride(simulators[simId].getAgentPos2d(i) + new Vector3(0, a.transform.position.y, 0), simulators[simId].getAgentSpeed2d(i));
            }



        }
    }
}
