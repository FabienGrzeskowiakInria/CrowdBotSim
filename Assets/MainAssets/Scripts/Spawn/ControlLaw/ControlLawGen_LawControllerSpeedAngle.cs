using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlLawGen_LawControllerSpeedAngle : ControlLawGen
    {
        public float speedCurrent = 0.0f;
        public float speedDefault = 1.33f;
        public float accelerationMax = 0.8f;
        public float angularSpeed = 30f;
        public float timeBeforeControl = 0;
        public float speedVariation = 0.5f;


        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawControllerSpeedAngle law = new LawControllerSpeedAngle();
            law.speedCurrent = speedCurrent;
            law.speedDefault = speedDefault;
            law.accelerationMax = accelerationMax;
            law.angularSpeed = angularSpeed;
            law.timeBeforeControl = timeBeforeControl;
            law.speedVariation = speedVariation;


            return law;
        }
    }
}
