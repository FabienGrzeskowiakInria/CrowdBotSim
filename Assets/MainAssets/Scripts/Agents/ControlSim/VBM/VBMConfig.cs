using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{
    public class VBMConfig : TrialControlSim
    {
        [XmlAttribute("SimulationID")]
        public int id;
        [XmlAttribute]
        public float radius;
        [XmlAttribute]
        public float neighborAgentDist;
        [XmlAttribute]
        public float neighborWallDist;
        [XmlAttribute]
        public float sigTtca = 1.8f;
        [XmlAttribute]
        public float sigDca = 0.3f;
        [XmlAttribute]
        public float sigSpeed = 3.3f;
        [XmlAttribute]
        public float sigAngle = 2.0f;

        public int getConfigId()
        {
            return id;
        }

        public ControlSim createControlSim(int id)
        {
            return new SimVBM(id);
        }

        public VBMConfig()
        {
            id = 0;
            radius = 0.33f;
            neighborAgentDist = 5;
            neighborWallDist = 10;
            sigTtca = 1.8f;
            sigDca = 0.3f;
            sigSpeed = 3.3f;
            sigAngle = 2.0f;
        }
    }
}
