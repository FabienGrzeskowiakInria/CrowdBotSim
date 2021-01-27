using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Player agent that can be control using different input devices
/// </summary>

public abstract class Robot : Agent {
    public abstract void clear();
    
}

/// <summary>
/// Trial parameters concerning the player (XML serializable)
/// </summary>
public abstract class TrialRobot : TrialAgent
{
    public abstract Robot createRobotComponnent(GameObject agentObject, uint id);
}
