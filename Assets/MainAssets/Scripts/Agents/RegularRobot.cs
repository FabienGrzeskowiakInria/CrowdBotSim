using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;
using crowdbotsim;
using RosSharp.RosBridgeClient;

/// <summary>
/// Player agent that can be control using different input devices
/// </summary>
public class RegularRobot : Robot
{
    internal ControlLaw robotController;

    private Transform publishedTransform;

    private Transform base_link;
    
    //void OnDisable()
    //{
    //    Debug.Log("PrintOnDisable: script was disabled => " + this.gameObject );
    //}

    //void OnEnable()
    //{
    //    Debug.Log("PrintOnEnable: script was enabled => " + this.gameObject);
    //}

    /// <summary>
    /// Reset the player
    /// </summary>
    public override void clear()
    {
        robotController = null;
    }

    // Use this for initialization
    void Start()
    {
        publishedTransform = transform.Find("PublishedTransform");
        base_link = transform.Find("base_link");
    }

    /// <summary>
    /// Compute agent behavior for a simulation step
    /// </summary>
    public override void doStep()
    {
        // No behavior
        if (robotController == null)
            return;

        // Check for new state
        Vector3 translation;
        Vector3 rotation;

        robotController.computeGlobalMvt(ToolsTime.DeltaTime, out translation, out rotation);

        if(publishedTransform != null)
        {
            publishedTransform.localPosition = translation/ToolsTime.DeltaTime;
            publishedTransform.localRotation = Quaternion.FromToRotation(base_link.rotation * Vector3.forward,rotation);
        }

    }

    public override void simOverride(Vector3 position, Vector3 speed)
    {    
        // Vector3 translation = position - base_link.position;

        // if(publishedTransform != null)
        // {
        //     publishedTransform.localPosition = translation;
        //     publishedTransform.localRotation = Quaternion.FromToRotation(base_link.rotation * Vector3.forward,rotation);
        // }
        // publishedTransform.localPosition = speed;
        // if (speed.sqrMagnitude > 0.001)
        // {
        //     Vector3 LookPos = position + speed;
        //     publishedTransform.localPosition = translation/ToolsTime.DeltaTime;
        //     publishedTransform.localRotation = Quaternion.FromToRotation(base_link.rotation * Vector3.forward,rotation);
        //     publishedTransform.LookAt(LookPos);
        // }
       
    }

}

/// <summary>
/// Trial parameters concerning the player (XML serializable)
/// </summary>
public class TrialRegularRobot : TrialRobot
{
    public ConfigVect Position;
    public ConfigVect Rotation;

    [XmlElement("controlLaw")]
    public CustomXmlSerializer<ControlLaw> xmlControlLaw;

    [XmlElement("controlSim")]
    public CustomXmlSerializer<TrialControlSim> xmlControlSim;

    [XmlAttribute]
    public bool in_sim = true;

    [XmlIgnore]
    public ControlLaw controlLaw { get { return xmlControlLaw.Data; } }
    [XmlIgnore]
    public TrialControlSim controlSim { get { return xmlControlSim == null ? null : xmlControlSim.Data; } }

    public TrialRegularRobot()
    {
        Position = new ConfigVect();
        Rotation = new ConfigVect();
        xmlControlLaw = new LawFake();
        xmlControlSim = null;
    }

    public override Robot createRobotComponnent(GameObject agentObject, uint id)
    {
        RegularRobot a = agentObject.AddComponent<RegularRobot>();
        a.id = id;
        // Init starting state
        agentObject.transform.position = Position.vect;
        agentObject.transform.rotation = Quaternion.Euler(Rotation.vect);


        // Init control law
        a.robotController = controlLaw;
        controlLaw.initialize(a);

        return a;
    }

    public override Agent createAgentComponnent(GameObject agentObject, uint id)
    {
        return createRobotComponnent(agentObject, id);
    }

    public override Vector3 getStartingPosition()
    {
        return Position.vect;
    }

    public override TrialControlSim getControlSimInfo()
    {
        return controlSim;
    }
}
