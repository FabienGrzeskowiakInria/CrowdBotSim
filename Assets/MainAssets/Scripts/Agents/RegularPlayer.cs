using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Player agent that can be control using different input devices
/// </summary>
public class RegularPlayer : Player
{

    internal GameObject headObject;
    internal ControlLaw playerController;

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
        playerController = null;
    }

    // Use this for initialization
    void Start()
    {
        if (headObject == null)
        {
            initObjectLink();
        }
    }

    public virtual void initObjectLink()
    {
        foreach (Transform go in gameObject.GetComponentsInChildren<Transform>())
        {
            if (go.tag == "HeadPlayer")
            {
                headObject = go.gameObject;
                go.position = transform.position + new Vector3(0, LoaderConfig.xpUserHeight, 0);
#if MIDDLEVR
            }
            if (go.name == "HeadNode")
            {
                HeadNode= go.gameObject;
            } else if (go.name == "HandNode")
            {
                HandNode = go.gameObject;
            }
#else
                break;
            }
#endif
            }
        }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Compute agent behavior for a simulation step
    /// </summary>
    public override void doStep()
    {
        // No behavior
        if (playerController == null)
            return;

        // Check for new state
        Vector3 translation;
        Vector3 rotation;

        playerController.computeGlobalMvt(ToolsTime.DeltaTime, out translation, out rotation);

        playerController.applyMvt(this, translation, rotation);
    }

    public override GameObject getHeadObject()
    {
        return headObject;
    }
}

/// <summary>
/// Trial parameters concerning the player (XML serializable)
/// </summary>
public class TrialRegularPlayer : TrialPlayer
{
    public ConfigVect Position;
    public ConfigVect Rotation;

    [XmlElement("controlLaw")]
    public CustomXmlSerializer<ControlLaw> xmlControlLaw;

    [XmlElement("controlSim")]
    public CustomXmlSerializer<TrialControlSim> xmlControlSim;


    [XmlIgnore]
    public ControlLaw controlLaw { get { return xmlControlLaw.Data; } }
    [XmlIgnore]
    public TrialControlSim controlSim { get { return xmlControlSim == null ? null : xmlControlSim.Data; } }

    public TrialRegularPlayer()
    {
        mesh = "RegPlayer";
        Position = new ConfigVect();
        Rotation = new ConfigVect();

        xmlControlLaw = new LawFake();
        xmlControlSim = null;
    }

    public override Player createPlayerComponnent(GameObject agentObject, uint id)
    {
        RegularPlayer a = agentObject.AddComponent<RegularPlayer>();
        a.id = id;
        // Init starting state
        agentObject.transform.position = Position.vect;
        agentObject.transform.rotation = Quaternion.Euler(Rotation.vect);

        // Set the head at the user height
        if (a.headObject == null)
        {
            a.initObjectLink();
        }

        // Init control law
        a.playerController = controlLaw;
        controlLaw.initialize(a);

        return a;
    }

    public override Agent createAgentComponnent(GameObject agentObject, uint id)
    {
        return createPlayerComponnent(agentObject, id);
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
