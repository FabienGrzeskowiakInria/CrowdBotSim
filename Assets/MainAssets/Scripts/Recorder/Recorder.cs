using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data recorder interface to define class that save data from experimental trials
/// </summary>
public interface Recorder
{
    /// <summary>
    /// Initialize the recorder
    /// </summary>
    /// <param name="agentList"> The list of agents to watch </param>
    void initRecorder(Player p, List<Agent> agentList);

    /// <summary>
    /// Reset the recorder
    /// </summary>
    void clear();
}
