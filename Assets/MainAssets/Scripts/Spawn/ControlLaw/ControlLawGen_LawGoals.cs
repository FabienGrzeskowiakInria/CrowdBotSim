﻿using UnityEngine;


//    public class LawGoals : ControlLaw
namespace CrowdMP.Core
{
    public class ControlLawGen_LawGoals : ControlLawGen
    {
        public enum GoalType
        {
            FixOrder,
            RandOrder,
            SequentialOrder
        }

        [Header("Main Parameters")]
        public float speedCurrent = 0;
        public float speedDefault = 1.33f;
        public float accelerationMax = 0.8f;
        public float angularSpeed = 360 * 100;
        public float reachedDist = 0.5f;
        public bool isLooping = false;

        public Vector3[] goals;

        [System.Serializable]
        public class SpawnerParams
        {
            public float speedCurrentOffset = 0f;
            public float speedDefaultOffset = 0f;
            public float accelerationMaxOffset = 0f;
            public float angularSpeedOffset = 0f;

            public int numGoals; 
            public Goal[] goalAreas;
            public GoalType goalSelection = GoalType.FixOrder;

        }
        [Header("Spawner Parameters")]
        public SpawnerParams randomness;


        public override CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawGoals law = new LawGoals();
            law.speedCurrent = speedCurrent;
            law.speedDefault = speedDefault;
            law.accelerationMax = accelerationMax;
            law.angularSpeed = angularSpeed;
            law.reachedDist = reachedDist;
            law.isLooping = isLooping;

            foreach (Vector3 v in goals)
                law.goals.Add(new ConfigVect(v));


            return law;
        }

        public override ControlLawGen randDraw(GameObject agent, int id = 0)
        {
            ControlLawGen_LawGoals law = agent.AddComponent<ControlLawGen_LawGoals>();

            law.speedCurrent = speedCurrent + Random.Range(-randomness.speedCurrentOffset, randomness.speedCurrentOffset);
            law.speedDefault = speedDefault + Random.Range(-randomness.speedDefaultOffset, randomness.speedDefaultOffset);
            law.accelerationMax = accelerationMax + Random.Range(-randomness.accelerationMaxOffset, randomness.accelerationMaxOffset);
            law.angularSpeed = angularSpeed + Random.Range(-randomness.angularSpeedOffset, randomness.angularSpeedOffset);
            law.reachedDist = reachedDist;
            law.isLooping = isLooping;

            if (randomness.numGoals > 0)
            {
                law.goals = new Vector3[randomness.numGoals];
                switch (randomness.goalSelection)
                {
                    case GoalType.RandOrder:
                        {

                            for (int i = 0; i < randomness.numGoals; ++i)
                            {
                                int randomSelection = Random.Range(0, randomness.goalAreas.Length);
                                law.goals[i] = randomness.goalAreas[randomSelection].randPosInArea();
                            }
                            break;
                        }
                    case GoalType.SequentialOrder:
                        {
                            Goal[] sequence = new Goal[randomness.numGoals];

                            int randomSelection = Random.Range(0, randomness.goalAreas.Length);
                            sequence[0] = randomness.goalAreas[randomSelection];
                            law.goals[0] = sequence[0].randPosInArea();

                            for (int i = 1; i < randomness.numGoals; ++i)
                            {
                                sequence[i] = sequence[i - 1].getNextPoint(sequence[Mathf.Max(0, i - 2)]);
                                law.goals[i] = sequence[i].randPosInArea();
                            }
                            break;
                        }
                    case GoalType.FixOrder:
                    default:
                        {

                            for (int i = 0; i < randomness.numGoals; ++i)
                            {
                                int randomSelection = i % randomness.goalAreas.Length;
                                law.goals[i] = randomness.goalAreas[randomSelection].randPosInArea();

                            }
                            break;
                        }
                }
            }

            return law;
        }

        public Vector3 randPosInArea(GameObject area)
        {
            Vector3 newVec = area.transform.position
                    + area.transform.right * area.transform.lossyScale.x * Random.Range(-0.5f, 0.5f) * 10f
                    + area.transform.forward * area.transform.lossyScale.z * Random.Range(-0.5f, 0.5f) * 10f;

            return newVec;
        }

        public override ControlLawGen randDraw(GameObject agent, ControlLawGen groupTemplate, int id = 0)
        {
            ControlLawGen_LawGoals newLaw = (ControlLawGen_LawGoals)randDraw(agent, id);
            newLaw.goals = ((ControlLawGen_LawGoals)groupTemplate).goals;

            return newLaw;
        }
    }
}
