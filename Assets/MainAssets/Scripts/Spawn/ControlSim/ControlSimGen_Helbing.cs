using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen_Helbing : ControlSimGen
    {
        [Header("Main Parameters")]
        public int id = 0;
        public float radius = 0.33f;
        public float neighborDist = 10.0f;
        public bool doBoids = false;

        [System.Serializable]
        public class SpawnerParams
        {
            public float neighborDistOffset = 0;
            public float radiusOffset = 0;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;

        public override CustomXmlSerializer<TrialControlSim> createControlSim(int GroupSeed)
        {
            HelbingConfig sim = new HelbingConfig();
            sim.id = id;
            sim.neighborDist = neighborDist;
            sim.radius = radius;
            sim.doBoids = doBoids;

            return sim;
        }

        public override ControlSimGen randDraw(GameObject agent, int id = 0, int groupID = 0)
        {
            ControlSimGen_Helbing csg = agent.AddComponent<ControlSimGen_Helbing>();


            csg.id = id;
            csg.neighborDist = neighborDist + Random.Range(-randomness.neighborDistOffset, randomness.neighborDistOffset);
            csg.radius = radius + Random.Range(-randomness.radiusOffset, randomness.radiusOffset);
            csg.doBoids = doBoids;

            return csg;
        }
    }
}
