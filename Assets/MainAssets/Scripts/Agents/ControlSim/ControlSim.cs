using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    /// <summary>
    /// Interface to create new simulation controller
    /// </summary>
    public interface ControlSim
    {
        /// <summary>
        /// Reset the controller
        /// </summary>
        void clear();

        /// <summary>
        /// Perform a step of the simulation
        /// </summary>
        /// <param name="deltaTime">Time step since last step</param>
        void doStep(float deltaTime);

        /// <summary>
        /// Add obstacles in the simulations
        /// </summary>
        /// <param name="obst">List of all the obstacles present in the environment</param>
        void addObstacles(Obstacles obst);

        /// <summary>
        /// Add an agent controlled by the simulation
        /// </summary>
        /// <param name="position">Starting position of the agent</param>
        /// <param name="infos">Simulation parameters of the agent</param>
        void addAgent(Vector3 position, TrialControlSim infos);

        /// <summary>
        /// Add an agent that is not controlled by the simulation
        /// </summary>
        /// <param name="position">Starting position of the agent</param>
        void addNonResponsiveAgent(Vector3 position, float radius);

        /// <summary>
        /// Update agent state from external control
        /// </summary>
        /// <param name="id">Number of the agent</param>
        /// <param name="position">New position</param>
        /// <param name="goal">New goal</param>
        void updateAgentState(int id, Vector3 position, Vector3 goal);

        /// <summary>
        /// Get the agent position from the simulation
        /// </summary>
        /// <param name="id">Id of the agent</param>
        /// <returns>The agent position from the simulation</returns>
        Vector3 getAgentPos2d(int id);
        /// <summary>
        /// Get the agent speed from the simulation
        /// </summary>
        /// <param name="id">Id of the agent</param>
        /// <returns>The agent speed as a vector from the simulation</returns>
        Vector3 getAgentSpeed2d(int id);

        /// <summary>
        /// Get the id of the simulation given by the trial parameters
        /// </summary>
        /// <returns>Id of the simulation</returns>
        int getConfigId();
    }
}
