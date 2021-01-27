using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class Spawn : MonoBehaviour
    {
        private const int MAXLOOPTRY = 1000;

        [Tooltip("The agent generator to use when spawning agents")]
        public AgentGen agentsGenerator;
        [Tooltip("Radius used to spawn agent apart from one another")]
        public float agentSpawnRadius = 0.33f;
        [Tooltip("The number of agents to spawn")]
        public int numAgent;
        [Tooltip("The area in which to spawn agents")]
        public GameObject spawningArea;
        [Tooltip("The offset used for the Y coordinate when spawing agents (So the area can be put over the ground to be easily seen)")]
        public float elevationOffset = -0.1f;

        [Tooltip("Random orientation or choosen?")]
        public bool RandomRotation = true;
        public float Orientation = 0;

        [System.Serializable]
        public class GroupDistrib
        {
            [Tooltip("Frequence factor of current group size")]
            public int freqFactor = -1;
            [Tooltip("Thje size of the group")]
            public int groupSize = -1;
        }
        [Tooltip("Distribution of the different group size to spawn")]
        public GroupDistrib[] groupDistrib;
        [Tooltip("List of spawned agent (Modify only if you know what you are doing)")]
        public AgentGen[] agents;

        // Use this for initialization
        void Start()
        {


        }

        // Update is called once per frame
        void Update()
        {

        }


        public int getGroupID()
        {
            if (groupDistrib.Length>0)
            {
                int maxDraw = 0;
                foreach (GroupDistrib g in groupDistrib)
                {
                    maxDraw += g.freqFactor;
                }
                int draw = Random.Range(0, maxDraw);

                for (int i = 0; i < groupDistrib.Length; ++i)
                {
                    draw -= groupDistrib[i].freqFactor;
                    if (draw<0)
                    {
                        return i;
                    }
                }
            }

            return groupDistrib.Length-1;
        }

        private int[] drawGroups()
        {
            int[] numGroup = new int[groupDistrib.Length];
            for (int i=0;i< groupDistrib.Length; ++i)
            {
                for (int j=i+1; j < groupDistrib.Length; j++)
                {
                    if (groupDistrib[i].groupSize < groupDistrib[j].groupSize)
                    {
                        int tmp1= groupDistrib[i].groupSize;
                        int tmp2= groupDistrib[i].freqFactor;
                        groupDistrib[i].groupSize= groupDistrib[j].groupSize;
                        groupDistrib[i].freqFactor= groupDistrib[j].freqFactor;
                        groupDistrib[j].groupSize= tmp1;
                        groupDistrib[j].freqFactor= tmp2;
                    }
                }
            }

            int numSpawned = 0;
            while (numSpawned < numAgent)
            {
                int selection = getGroupID();

                int test = 0;
                while (groupDistrib[selection].groupSize+ numSpawned> numAgent && test < MAXLOOPTRY)
                {
                    selection = getGroupID();
                    test++;
                }
                if (test < MAXLOOPTRY) {
                    numSpawned += groupDistrib[selection].groupSize;
                    numGroup[selection] += 1;
                } else
                {
                    break;
                }

            }

            return numGroup;
        }

        public Vector3[] spawnGroup(int size, Vector3 offset)
        {
            Vector3[] groupPos = new Vector3[size];
            float distSqr = agentSpawnRadius * agentSpawnRadius;

            for (int i = 0; i < size; ++i)
            {

                int test = 0;
                bool isGood = false;
                float radius = agentSpawnRadius;
                do
                {
                    test++;
                    isGood = true;
                    groupPos[i] = offset + Random.insideUnitSphere * radius;
                    groupPos[i].y = offset.y;

                    for (int j = 0; j < i; ++j)
                    {
                        if ((groupPos[i] - groupPos[j]).sqrMagnitude < distSqr)
                        {
                            isGood = false;
                            break;
                        }
                    }
                    radius += agentSpawnRadius / 5;
                } while (!isGood && test < MAXLOOPTRY);
            }

            return groupPos;
        }

        public void spawn()
        {

            clearAll();
            agents = new AgentGen[numAgent];
            ControlLawGen lawGen=null;
            ControlLawGen[] tmpLG = gameObject.GetComponents<ControlLawGen>();
            foreach (ControlLawGen lg in tmpLG)
                if (lg.enabled)
                {
                    lawGen = lg;
                    break;
                }

            ControlSimGen simGen = null;
            ControlSimGen[] tmpSG = gameObject.GetComponents<ControlSimGen>();
            foreach (ControlSimGen sg in tmpSG)
                if (sg.enabled)
                {
                    simGen = sg;
                    break;
                }



            if (groupDistrib.Length==0)
            {
                groupDistrib = new GroupDistrib[1];
                groupDistrib[0] = new GroupDistrib();
                groupDistrib[0].freqFactor = 1;
                groupDistrib[0].groupSize = 1;
            }

            float distSqr = agentSpawnRadius * agentSpawnRadius;
            int[] numGroup = drawGroups();
            int numSpawnedAgent = 0;

            int groupId=0;
            for (int g=0; g< numGroup.Length; ++g)
            {
                for (int i=0; i< numGroup[g]; i++)
                {
                    bool isGood = false;
                    int test = 0;

                    Vector3[] newGroupPos;
                    // Random draw position for groups till no collision
                    do
                    {
                        test++;
                        isGood = true;
                        Vector3 newVec = spawningArea.transform.position + new Vector3(0, elevationOffset, 0)
                         + spawningArea.transform.right * spawningArea.transform.lossyScale.x * Random.Range(-0.5f, 0.5f) * 10f
                         + spawningArea.transform.forward * spawningArea.transform.lossyScale.z * Random.Range(-0.5f, 0.5f) * 10f;
                        newGroupPos = spawnGroup(groupDistrib[g].groupSize, newVec);

                        // Check for collision
                        for (int j = 0; j < numSpawnedAgent; ++j)
                        {
                            for (int h = 0; h < newGroupPos.Length; h++)
                            {
                                if ((newGroupPos[h] - agents[j].transform.position).sqrMagnitude < distSqr)
                                {
                                    isGood = false;
                                    break;
                                }
                            }
                        }

                    } while (!isGood && test < MAXLOOPTRY);

                    // Spawn the group
                    if (isGood)
                    {
                        int tmpGroupID = -1;
                        if (newGroupPos.Length > 1)
                        {
                            tmpGroupID = groupId;
                            groupId++;
                        }
                        Color tmpColor = new Color(Random.value, Random.value, Random.value, 1);
                        ControlLawGen groupTemplate = new ControlLawGen();
                        for (int j=0;j< newGroupPos.Length; j++)
                        {
                            GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            currentGO.name = "agentTemplate" + numSpawnedAgent;
                            currentGO.transform.position = newGroupPos[j];
                            currentGO.transform.SetParent(gameObject.transform);
                            currentGO.transform.localScale = new Vector3(agentSpawnRadius / currentGO.transform.lossyScale.x, 1 / currentGO.transform.lossyScale.y, agentSpawnRadius / currentGO.transform.lossyScale.z);
                            
                            if(RandomRotation)
                                currentGO.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                            else
                                currentGO.transform.rotation = Quaternion.Euler(0, Orientation, 0);

                            // -----------------
                            // SET AGENT'S COLOR
                            SkinnedMeshRenderer tmpRenderer = currentGO.GetComponentInChildren<SkinnedMeshRenderer>();
                            if (tmpRenderer==null)
                            {
                                tmpRenderer=currentGO.AddComponent<SkinnedMeshRenderer>();
                            }
                            Material tmpMaterial = new Material(Shader.Find("Legacy Shaders/Diffuse"));
                            tmpMaterial.color = tmpColor;
                            tmpRenderer.material = tmpMaterial;


                            agents[numSpawnedAgent] = agentsGenerator.randDraw(currentGO);

                            if (lawGen != null)
                            {
                                if (j==0)
                                    groupTemplate=lawGen.randDraw(currentGO);
                                else
                                    lawGen.randDraw(currentGO, groupTemplate);
                            }
                            if (simGen != null)
                            {
                                simGen.randDraw(currentGO, 0, tmpGroupID);
                            }
                            numSpawnedAgent++;
                        }
                    }
                    else // Skip these group size, not able to spawn anymore
                    {
                        break;
                    }

                }



            }


            //float distSqr = agentRadius * agentRadius;

            //int i = 0;
            //while (i < numAgent)
            //{
            //    int groupSize = getGroupSize();

            //    for (int g = i; g < i + groupSize; g++)
            //    {
            //        Vector3 newVec;

            //        bool isGood = false;
            //        int test = 0;
            //        do
            //        {
            //            test++;
            //            isGood = true;
            //            newVec = gameObject.transform.position
            //             + gameObject.transform.right * gameObject.transform.lossyScale.x * Random.Range(-0.5f, 0.5f) * 10f
            //             + gameObject.transform.forward * gameObject.transform.lossyScale.z * Random.Range(-0.5f, 0.5f) * 10f;

            //            for (int j = 0; j < i; ++j)
            //            {
            //                if ((newVec - agents[j].transform.position).sqrMagnitude < distSqr)
            //                {
            //                    isGood = false;
            //                    break;
            //                }
            //            }
            //        } while (!isGood && test < MAXLOOPTRY);

            //        if (test == MAXLOOPTRY)
            //            break;

            //        GameObject currentGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            //        currentGO.transform.position = newVec;
            //        currentGO.transform.SetParent(gameObject.transform.parent);
            //        currentGO.transform.localScale = new Vector3(agentRadius / currentGO.transform.lossyScale.x, 1 / currentGO.transform.lossyScale.y, agentRadius / currentGO.transform.lossyScale.z);
            //        Vector3 toto = -currentGO.transform.position;
            //        currentGO.transform.Translate(toto);

            //        currentGO.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            //        agents[i] = agentsGenerator.randDraw(currentGO);

            //        if (lawGen != null)
            //        {
            //            lawGen.randDraw(currentGO);
            //        }
            //        if (simGen != null)
            //        {
            //            simGen.randDraw(currentGO);
            //        }
            //    }

            //    i += groupSize;
            //}
        }

        public AgentGen[] getAllAgent()
        {
            return agents;
        }

        public void clearAll()
        {
            if (agents == null)
                return;

            foreach (AgentGen ag in agents)
            {
                if (ag!=null)
                    DestroyImmediate(ag.gameObject);
            }
        }
    }
}
