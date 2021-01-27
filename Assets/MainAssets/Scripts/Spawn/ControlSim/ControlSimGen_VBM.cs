using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen_VBM : ControlSimGen
    {
        [Header("Main Parameters")]
        public int id = 0;
        public float radius = 0.33f;
        public float neighborAgentDist = 5;
        public float neighborWallDist = 10;

        public float sigTtca = 1.8f;
        public float sigDca = 0.3f;
        public float sigSpeed = 3.3f;
        public float sigAngle = 2.0f;

        [System.Serializable]
        public class SpawnerParams
        {
            public float radiusOffset = 0;
            public float neighborAgentDistOffset = 0;
            public float neighborWallDistOffset = 0;

            public float sigTtcaOffset = 0;
            public float sigDcaOffset = 0;
            public float sigSpeedOffset = 0;
            public float sigAngleOffset = 0;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;


        public override CustomXmlSerializer<TrialControlSim> createControlSim(int GroupSeed)
        {
            VBMConfig sim = new VBMConfig();
            sim.id = id;
            sim.radius = radius;
            sim.neighborAgentDist = neighborAgentDist;
            sim.neighborWallDist = neighborWallDist;

            sim.sigTtca = sigTtca;
            sim.sigDca = sigDca;
            sim.sigSpeed = sigSpeed;
            sim.sigAngle = sigAngle;

            return sim;
        }

        public override ControlSimGen randDraw(GameObject agent, int id = 0, int groupID = 0)
        {
            ControlSimGen_VBM csg = agent.AddComponent<ControlSimGen_VBM>();


            csg.id = id;
            csg.radius = radius + Random.Range(-randomness.radiusOffset, randomness.radiusOffset);
            csg.neighborAgentDist = neighborAgentDist + Random.Range(-randomness.neighborAgentDistOffset, randomness.neighborAgentDistOffset);
            csg.neighborWallDist = neighborWallDist + Random.Range(-randomness.neighborWallDistOffset, randomness.neighborWallDistOffset);

            csg.sigTtca = sigTtca + Random.Range(-randomness.sigTtcaOffset, randomness.sigTtcaOffset);
            csg.sigDca = sigDca + Random.Range(-randomness.sigDcaOffset, randomness.sigDcaOffset);
            csg.sigSpeed = sigSpeed + Random.Range(-randomness.sigSpeedOffset, randomness.sigSpeedOffset);
            csg.sigAngle = sigAngle + Random.Range(-randomness.sigAngleOffset, randomness.sigAngleOffset);

            return csg;
        }

    }
}
