using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Agent abstract class to create interactings agents in trials
/// </summary>
public abstract class Agent : MonoBehaviour {
    internal uint id;


    /// <summary>
    /// Get agent id
    /// </summary>
    /// <returns>the id</returns>
    public uint GetID()
    {
        return id;
    }

    /// <summary>
    /// Perform a step of the agent
    /// </summary>
    abstract public void doStep();

    /// <summary>
    /// Give the agent position 
    /// </summary>
    public virtual Vector3 Position {get {return gameObject.transform.position;}}

    /// <summary>
    /// Translate agent position
    /// </summary>
    public virtual void Translate(Vector3 translation) { gameObject.transform.Translate(translation); }

    /// <summary>
    /// Rotate agent position
    /// </summary>
    public virtual void Rotate(Vector3 rotation) { gameObject.transform.Rotate(rotation); }

    /// <summary>
    /// Overide agent goal by simulation results
    /// </summary>
    /// <param name="position">New position</param>
    /// <param name="speed">New speed</param>
    public virtual void simOverride(Vector3 position, Vector3 speed)
    {
        transform.position = position;

        if (speed.sqrMagnitude > 0.001)
        {
            Vector3 LookPos = position + speed;
            transform.LookAt(LookPos);
        }

    }
}


/// <summary>
/// Trial parameters concerning the player's input device (XML serializable)
/// </summary>
public abstract class TrialAgent
{
    [XmlAttribute]
    public string mesh;
    [XmlAttribute]
    public float radius=0.33f;

    public abstract Agent createAgentComponnent(GameObject agentObject, uint id);
    public abstract Vector3 getStartingPosition();
    public abstract TrialControlSim getControlSimInfo();
}