using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

public class RosSimConfig : TrialControlSim
{
    [XmlAttribute("SimulationID")]
    public int id;
    [XmlAttribute]
    public float neighborDist;
    [XmlAttribute]
    public int maxNeighbors;
    [XmlAttribute]
    public float timeHorizon;
    [XmlAttribute]
    public float timeHorizonObst;
    [XmlAttribute]
    public float radius;
    [XmlAttribute]
    public float maxSpeed;

    public int getConfigId()
    {
        return id;
    }

    public ControlSim createControlSim(int id)
    {
        
        return new RosSim(id);
    }

    public RosSimConfig()
    {
        id = 0;
    }

}
