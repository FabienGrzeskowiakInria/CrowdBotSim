using System;
using System.Xml.Serialization;
using UnityEngine;

//LawControllerSpeedSideStep : ControlLaw
namespace CrowdMP.Core
{
    public class ControlLawGen_LawControllerSpeedSideStep : ControlLawGen
    {
        public float speedCurrent = 0.0f;
        public float speedDefault = 1.33f;
        public float accelerationMax = 0.8f;
        public float sideSpeed = 0.5f;
        public float timeBeforeControl = 0;
        public float speedVariation = 0.5f;


        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawControllerSpeedSideStep law = new LawControllerSpeedSideStep();
            law.speedCurrent = speedCurrent;
            law.speedDefault = speedDefault;
            law.accelerationMax = accelerationMax;
            law.sideSpeed = sideSpeed;
            law.timeBeforeControl = timeBeforeControl;
            law.speedVariation = speedVariation;


            return law;
        }
    }
}