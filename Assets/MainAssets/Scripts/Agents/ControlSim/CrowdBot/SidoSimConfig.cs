using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

public class SidoSimConfig : TrialControlSim
{
    [XmlAttribute("SimulationID")]
    public int id;

    [XmlAttribute]
    public float maxSpeed;

    public int getConfigId()
    {
        return id;
    }

    public ControlSim createControlSim(int id)
    {
        return new SimRVO(id);
    }

    public SidoSimConfig()
    {
        id = 0;
        maxSpeed = 0.8f;

    }

}
