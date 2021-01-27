using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class CB_Agent : MonoBehaviour {
    internal uint id;
    public float radius=0.33f;
    // ControlLaw movementController;
    public int visualVariation;
    public float animationOffset;
    public float heightOffset;
    public Vector3 Position;
    public Vector3 Rotation;
    // public ControlLaw controlLaw { get { return xmlControlLaw == null ? new LawFake() : xmlControlLaw.Data; } }
    // public TrialControlSim controlSim { get { return xmlControlSim == null ? null : xmlControlSim.Data; } }

    /// <summary>
    /// Get agent id
    /// </summary>
    /// <returns>the id</returns>
    public uint GetID()
    {
        return id;
    }

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
    public void simOverride(Vector3 position, Vector3 speed)
    {
        transform.position = position;

        if (speed.sqrMagnitude > 0.001)
        {
            Vector3 LookPos = position + speed;
            transform.LookAt(LookPos);
        }
    }

    /// <summary>
    /// Perform a step of the agent
    /// </summary>
    public void doStep()
    {

        /* Check new player coordinates */
        // Vector3 translation;
        // Vector3 rotation;
        // movementController.computeGlobalMvt(ToolsTime.DeltaTime, out translation, out rotation);

        /*Update player coordinates */
        // movementController.applyMvt(this, translation, rotation);
    }

    public CB_Agent()
    {
        visualVariation = 0;
        animationOffset = 0;
        heightOffset = 0;

        Position = new Vector3();
        Rotation = new Vector3();
    }

    // override public TrialControlSim getControlSimInfo()
    // {
    //     return controlSim;
    // }

    public CB_Agent createAgentComponnent(GameObject agentObject, uint id)
    {
        CB_Agent a = agentObject.AddComponent<CB_Agent>();
        a.id = id;

        agentObject.transform.position = Position;
        agentObject.transform.rotation = Quaternion.Euler(Rotation);

        agentObject.transform.localScale = new Vector3(1, 1 + heightOffset, 1);
        float yOffset = (1.7f * heightOffset) / 2;
        Vector3 tmp = agentObject.transform.position;
        tmp.y += yOffset;
        agentObject.transform.position = tmp;

        // // Init control law
        // a.movementController = controlLaw;
        // controlLaw.initialize(a);

        AnimationController ac = agentObject.GetComponent<AnimationController>();
        if (ac != null)
            ac.setAnimOffset(animationOffset);

        return a;
    }

    public Vector3 getStartingPosition()
    {
        return Position;
    }
}