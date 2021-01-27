using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{

    public class TangentConfig : TrialControlSim
    {
        [XmlAttribute("SimulationID")]
        public int id;
        [XmlAttribute]
        public float speedComfort;
        [XmlAttribute]
        public float personalArea;
        [XmlAttribute]
        public float speedMax;
        [XmlAttribute]
        public float g_beta;
        [XmlAttribute]
        public float g_gamma;
        [XmlAttribute]
        public float timeHorizon;
        [XmlAttribute]
        public uint maxNeighbors;

        public int getConfigId()
        {
            return id;
        }

        public ControlSim createControlSim(int id)
        {

            return new SimTangent(id);
        }

        public TangentConfig()
        {
            id = 0;
            speedComfort = 1.3f;
            personalArea = 0.33f;
            speedMax = 2.0f;
            g_beta = 0.5f;
            g_gamma = 0.25f;
            timeHorizon = 15;
            maxNeighbors = 15;
        }
    }
}