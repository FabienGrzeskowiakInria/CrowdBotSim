using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
{

    public class VOGConfig : TrialControlSim
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

        public VOGConfigGroup group;

        public int getConfigId()
        {
            return id;
        }

        public ControlSim createControlSim(int id)
        {

            return new SimVOG(id);
        }

        public VOGConfig()
        {
            id = 0;
            neighborDist = 5;
            maxNeighbors = 3;
            timeHorizon = 5;
            timeHorizon = 2;
            radius = 0.33f;
            maxSpeed = 2;

            group = new VOGConfigGroup();
        }

    }

    [System.Serializable]
    public class VOGConfigGroup
    {
        [XmlAttribute]
        public int groupID;
        [XmlAttribute]
        public bool useFormation;
        [XmlAttribute]
        public int neighbourNum;
        [XmlAttribute]
        public float neighbourDist;
        [XmlAttribute]
        public float neighbourDetectionDist;
        [XmlAttribute]
        public float horizonTime;
        [XmlAttribute]
        public float weightPrefVel;
        [XmlAttribute]
        public float weightGroup;


        public VOGConfigGroup()
        {
            groupID = -1;
            useFormation=true;
            neighbourNum=2;
            neighbourDist=1f;
            neighbourDetectionDist=80;
            weightPrefVel=0.15f;
            weightGroup=0.20f;
            horizonTime=2f;
        }

        public VOGConfigGroup(VOGConfigGroup vogG)
        {
            groupID = vogG.groupID;
            useFormation = vogG.useFormation;
            neighbourNum = vogG.neighbourNum;
            neighbourDist = vogG.neighbourDist;
            neighbourDetectionDist = vogG.neighbourDetectionDist;
            weightPrefVel = vogG.weightPrefVel;
            weightGroup = vogG.weightGroup;
            horizonTime = vogG.horizonTime;
        }
    }
}
