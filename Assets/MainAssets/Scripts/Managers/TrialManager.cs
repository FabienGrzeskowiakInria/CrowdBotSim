using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trial Manager interface to create classes that control a trial
/// </summary>
public interface TrialManager {

    /// <summary>
    /// Initialize trial
    /// </summary>
    /// <param name="p">The player</param>
    /// <param name="agentModels">All the available agent models</param>
    /// <returns>True if the trial has been correctly intialized</returns>
    bool initializeTrial(GameObject[] Player, GameObject[] agentModels);
    /// <summary>
    /// Check the ending conditions of the trials
    /// </summary>
    /// <returns>True if the trial is over</returns>
    bool hasEnded();
    /// <summary>
    /// Reset trial
    /// </summary>
    void clear();
    /// <summary>
    /// Perform a step of the trial
    /// </summary>
    void doStep();
    /// <summary>
    /// Return the player of the trial
    /// </summary>
    Player getPlayer();

    bool isReady();
    void startTrial();
}

/// <summary>
/// Trial parameters concerning the player's input device (XML serializable)
/// </summary>
public interface TrialConfig
{
}
