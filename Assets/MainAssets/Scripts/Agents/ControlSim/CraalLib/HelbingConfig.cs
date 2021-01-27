using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{

    public class HelbingConfig : TrialControlSim
    {

        [XmlAttribute("SimulationID")]
        public int id;
        [XmlAttribute]
        public float radius;
        [XmlAttribute]
        public float neighborDist;
        [XmlAttribute]
        public bool doBoids;

        public int getConfigId()
        {
            return id;
        }

        public ControlSim createControlSim(int id)
        {

            return new SimHelbing(id, doBoids);
        }

        public HelbingConfig()
        {
            id = 0;
            radius = 0.33f;
            neighborDist = 10.0f;
            doBoids = false;
        }
    }
}
