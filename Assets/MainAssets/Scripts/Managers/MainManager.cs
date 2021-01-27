using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main manager controlling the Experiment flow
/// </summary>
public interface MainManager {
    /// <summary>
    /// Load and setup current trial
    /// </summary>
    void startTrial();

    /// <summary>
    /// Method used to initialize scene constants
    /// </summary>
    /// <returns>True if constant are well initialized, otherwise False</returns>
    bool initializeConstants();

    /// <summary>
    /// End the current trial change it
    /// </summary>
    void endTrial(int trialSwitch);

    /// <summary>
    /// Update and load transition screen
    /// </summary>
    void startTransition();
}
