using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CrowdMP.Core
    {
        public class ControlLawGen_LawCamControlEditor : ControlLawGen
        {
            [Header("Main Parameters")]
            public float lookSpeedH = 2f;
            public float lookSpeedV = 2f;
            public float zoomSpeed = 20;
            public float dragSpeed = 30f;


            public override CustomXmlSerializer<ControlLaw> createControlLaw()
            {
                LawCamControlEditor law = new LawCamControlEditor();
                law.lookSpeedH = lookSpeedH;
                law.lookSpeedV = lookSpeedV;
                law.zoomSpeed = zoomSpeed;
                law.dragSpeed = dragSpeed;

                return law;
            }
        }
    }