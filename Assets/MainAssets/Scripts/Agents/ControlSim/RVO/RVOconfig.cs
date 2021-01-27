using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{

    public class RVOconfig : TrialControlSim
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

            return new SimRVO(id);
        }

        public RVOconfig()
        {
            id = 0;
            neighborDist = 5;
            maxNeighbors = 3;
            timeHorizon = 5;
            timeHorizon = 2;
            radius = 0.33f;
            maxSpeed = 2;

        }

    }
}