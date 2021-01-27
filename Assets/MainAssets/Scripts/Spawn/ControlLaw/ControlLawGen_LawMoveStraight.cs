using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlLawGen_LawMoveStraight : ControlLawGen
    {
        [Header("Main Parameters")]
        public float speedCurrent = 0f;
        public float speedDefault = 1.33f;
        public float accelerationMax = 0.8f;

        [System.Serializable]
        public class SpawnerParams
        {
            public float speedCurrentOffset = 0f;
            public float speedDefaultOffset = 0f;
            public float accelerationMaxOffset = 0f;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;


        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawMoveStraight law = new LawMoveStraight();
            law.speedCurrent = speedCurrent;
            law.speedDefault = speedDefault;
            law.accelerationMax = accelerationMax;


            return law;
        }

        public override ControlLawGen randDraw(GameObject agent, int id = 0)
        {
            ControlLawGen_LawMoveStraight law = agent.AddComponent<ControlLawGen_LawMoveStraight>();

            law.speedCurrent = speedCurrent + Random.Range(-randomness.speedCurrentOffset, randomness.speedCurrentOffset);
            law.speedDefault = speedDefault + Random.Range(-randomness.speedDefaultOffset, randomness.speedDefaultOffset);
            law.accelerationMax = accelerationMax + Random.Range(-randomness.accelerationMaxOffset, randomness.accelerationMaxOffset);

            return law;
        }
    }
}
