using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen_VOG : ControlSimGen
    {
        [Header("Main Parameters")]
        public int id = 0;
        public float neighborDist = 5;
        public int maxNeighbors = 3;
        public float timeHorizon = 5;
        public float timeHorizonObst = 2;
        public float radius = 0.33f;
        public float maxSpeed = 2;

        public VOGConfigGroup group;

        [System.Serializable]
        public class SpawnerParams
        {
            public float neighborDistOffset = 0;
            public int maxNeighborsOffset = 0;
            public float timeHorizonOffset = 0;
            public float timeHorizonObstOffset = 0;
            public float radiusOffset = 0;
            public float maxSpeedOffset = 0;
        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;

        public override CustomXmlSerializer<TrialControlSim> createControlSim(int groupSeed)
        {
            VOGConfig sim = new VOGConfig();
            sim.id = id;
            sim.neighborDist = neighborDist;
            sim.maxNeighbors = maxNeighbors;
            sim.timeHorizon = timeHorizon;
            sim.timeHorizonObst = timeHorizonObst;
            sim.radius = radius;
            sim.maxSpeed = maxSpeed;

            sim.group = new VOGConfigGroup(group);
            if (sim.group.groupID>-1)
                sim.group.groupID += groupSeed;

            return sim;
        }

        public override ControlSimGen randDraw(GameObject agent, int simID = 0, int groupID = 0)
        {
            ControlSimGen_VOG csg = agent.AddComponent<ControlSimGen_VOG>();


            csg.id = simID;
            csg.neighborDist = neighborDist + Random.Range(-randomness.neighborDistOffset, randomness.neighborDistOffset);
            csg.maxNeighbors = maxNeighbors + Random.Range(-randomness.maxNeighborsOffset, randomness.maxNeighborsOffset);
            csg.timeHorizon = timeHorizon + Random.Range(-randomness.timeHorizonOffset, randomness.timeHorizonOffset);
            csg.timeHorizonObst = timeHorizonObst + Random.Range(-randomness.timeHorizonObstOffset, randomness.timeHorizonObstOffset);
            csg.radius = radius + Random.Range(-randomness.radiusOffset, randomness.radiusOffset);
            csg.maxSpeed = maxSpeed + Random.Range(-randomness.maxSpeedOffset, randomness.maxSpeedOffset);

            csg.group = new VOGConfigGroup();
            csg.group.groupID = groupID;

            return csg;
        }
    }
}
