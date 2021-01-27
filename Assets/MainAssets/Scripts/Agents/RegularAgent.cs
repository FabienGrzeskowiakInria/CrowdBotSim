using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Regular agent following a controlLaw and a simulation
/// </summary>
public class RegularAgent : Agent
{
    internal ControlLaw movementController;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Perform a step of the agent
    /// </summary>
    public override void doStep()
    {

        /* Check new player coordinates */
        Vector3 translation;
        Vector3 rotation;
        movementController.computeGlobalMvt(ToolsTime.DeltaTime, out translation, out rotation);

        /*Update player coordinates */
        movementController.applyMvt(this, translation, rotation);
    }
}


/// <summary>
/// Trial parameters concerning the player's input device (XML serializable)
/// </summary>
public class TrialRegularAgent : TrialAgent
{
    [XmlAttribute]
    public int visualVariation;
    [XmlAttribute]
    public float animationOffset;
    [XmlAttribute]
    public float heightOffset;

    public ConfigVect Position;
    public ConfigVect Rotation;

    [XmlElement("controlLaw")]
    public CustomXmlSerializer<ControlLaw> xmlControlLaw;
    [XmlElement("controlSim")]
    public CustomXmlSerializer<TrialControlSim> xmlControlSim;

    [XmlIgnore]
    public ControlLaw controlLaw { get { return xmlControlLaw == null ? new LawFake() : xmlControlLaw.Data; } }
    [XmlIgnore]
    public TrialControlSim controlSim { get { return xmlControlSim == null ? null : xmlControlSim.Data; } }

    public TrialRegularAgent()
    {
        mesh = "m002";
        visualVariation = 0;
        animationOffset = 0;
        heightOffset = 0;

        Position = new ConfigVect();
        Rotation = new ConfigVect();

        xmlControlLaw = null;
        xmlControlSim = null;
    }

    override public TrialControlSim getControlSimInfo()
    {
        return controlSim;
    }

    override public Agent createAgentComponnent(GameObject agentObject, uint id)
    {
        RegularAgent a =agentObject.AddComponent<RegularAgent>();
        a.id = id;

        agentObject.transform.position = Position.vect;
        agentObject.transform.rotation = Quaternion.Euler(Rotation.vect);

        agentObject.transform.localScale = new Vector3(1, 1 + heightOffset, 1);
        float yOffset = (1.7f * heightOffset) / 2;
        Vector3 tmp = agentObject.transform.position;
        tmp.y += yOffset;
        agentObject.transform.position = tmp;

        CapsuleCollider tmpCap = agentObject.GetComponent<CapsuleCollider>();
        if (tmpCap!=null)
        {
            tmp = tmpCap.center;
            tmp.y -= yOffset;
            tmpCap.center = tmp;
        }

        // Init control law
        a.movementController = controlLaw;
        controlLaw.initialize(a);

        AnimationController ac = agentObject.GetComponent<AnimationController>();
        if (ac != null)
            ac.setAnimOffset(animationOffset);

        //TextureVariation tv = agentObject.GetComponentInChildren<TextureVariation>();
        //if (tv != null)
        //    tv.SetVariationId(visualVariation);

        return a;
    }

    override public Vector3 getStartingPosition()
    {
        return Position.vect;
    }
}