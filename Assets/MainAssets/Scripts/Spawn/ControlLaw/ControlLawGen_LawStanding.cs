using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// public class LawStanding : ControlLaw
namespace CrowdMP.Core
{
    public class ControlLawGen_LawStanding : ControlLawGen
    {
        [Header("Main Parameters")]
        public int animationType = 0; // 0 - No animation | 1 - idle | 2 - talking | 3 - applause | 4 - look around
        public Vector3 LookAt= new Vector3();

        [System.Serializable]
        public class SpawnerParams
        {
            public int[] animationTypes;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;


        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawStanding law = new LawStanding();
            law.animationType = animationType;
            law.LookAt.vect = LookAt;


            return law;
        }

        public override ControlLawGen randDraw(GameObject agent, int id = 0)
        {
            ControlLawGen_LawStanding law = agent.AddComponent<ControlLawGen_LawStanding>();

            if (randomness.animationTypes.Length > 0)
                law.animationType = randomness.animationTypes[Random.Range(0, randomness.animationTypes.Length)];
            else
                law.animationType = animationType;

            law.LookAt = LookAt;
            return law;
        }

        public override ControlLawGen randDraw(GameObject agent, ControlLawGen groupTemplate, int id = 0)
        {
            ControlLawGen_LawStanding nl = (ControlLawGen_LawStanding)randDraw(agent, id);
            nl.LookAt = ((ControlLawGen_LawStanding)groupTemplate).LookAt;

            return nl;
        }
    }
}