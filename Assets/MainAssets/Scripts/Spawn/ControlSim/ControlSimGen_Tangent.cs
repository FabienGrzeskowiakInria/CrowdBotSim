using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen_Tangent : ControlSimGen
    {
        [Header("Main Parameters")]
        public int id = 0;
        public float speedComfort = 1.3f;
        public float personalArea = 0.33f;
        public float speedMax = 2.0f;
        public float g_beta = 0.5f;
        public float g_gamma = 0.25f;
        public float timeHorizon = 15;
        public uint maxNeighbors = 15;

        [System.Serializable]
        public class SpawnerParams
        {
            public float speedComfortOffset = 0;
            public int personalAreaOffset = 0;
            public float speedMaxOffset = 0;
            public float g_betaOffset = 0;
            public float g_gammaOffset = 0;
            public float timeHorizonOffset = 0;
            public int maxNeighborsOffset = 0;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;

        public override CustomXmlSerializer<TrialControlSim> createControlSim(int GroupSeed)
        {
            TangentConfig sim = new TangentConfig();
            sim.id = id;
            sim.speedComfort = speedComfort;
            sim.personalArea = personalArea;
            sim.speedMax = speedMax;
            sim.g_beta = g_beta;
            sim.g_gamma = g_gamma;
            sim.timeHorizon = timeHorizon;
            sim.maxNeighbors = maxNeighbors;

            return sim;
        }

        public override ControlSimGen randDraw(GameObject agent, int id = 0, int groupID = 0)
        {
            ControlSimGen_Tangent csg = agent.AddComponent<ControlSimGen_Tangent>();


            csg.id = id;
            csg.speedComfort = speedComfort + Random.Range(-randomness.speedComfortOffset, randomness.speedComfortOffset);
            csg.personalArea = personalArea + Random.Range(-randomness.personalAreaOffset, randomness.personalAreaOffset);
            csg.speedMax = speedMax + Random.Range(-randomness.speedMaxOffset, randomness.speedMaxOffset);
            csg.g_beta = g_beta + Random.Range(-randomness.g_betaOffset, randomness.g_betaOffset);
            csg.g_gamma = g_gamma + Random.Range(-randomness.g_gammaOffset, randomness.g_gammaOffset);
            csg.timeHorizon = timeHorizon + Random.Range(-randomness.timeHorizonOffset, randomness.timeHorizonOffset);
            csg.maxNeighbors = maxNeighbors + (uint)Random.Range(-randomness.maxNeighborsOffset, randomness.maxNeighborsOffset);

            return csg;
        }
    }
}
