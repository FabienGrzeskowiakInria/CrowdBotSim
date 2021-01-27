using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class AgentGen : MonoBehaviour
    {
        [Tooltip("Possible meshes for agents, will be selected randomly during spawn")]
        public GameObject[] meshes;
        [Tooltip("animOffset parameters of the agents (-1 for random)")]
        public float animOffset = -1f;
        [Tooltip("visualVariation parameters of the agents (-1 for random)")]
        public int visualVariation = -1;
        [Tooltip("heightVariation parameters of the agents (-1 for random)")]
        public float heightVariation = -1;
        [Tooltip("Radius of the agent")]
        public float radius = 0.33f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void defaultMeshes()
        {
            meshes = GameObject.FindGameObjectsWithTag("AgentModels");
        }

        public virtual AgentGen randDraw(GameObject agent)
        {
            AgentGen newAgentParam = agent.AddComponent<AgentGen>();

            newAgentParam.meshes = new GameObject[1];
            newAgentParam.meshes[0] = meshes[Random.Range(0,meshes.Length)];

            if (animOffset < 0)
                newAgentParam.animOffset = Random.Range(0f, 1f);
            else
                newAgentParam.animOffset = animOffset;

            if (visualVariation < 0)
                newAgentParam.visualVariation = Random.Range(0, 10);
            else
                newAgentParam.visualVariation = visualVariation;

            if (heightVariation < 0)
                newAgentParam.heightVariation = Random.Range(-0.05f, 0.05f);
            else
                newAgentParam.heightVariation = heightVariation;

            newAgentParam.radius = radius;

            return newAgentParam;
        }

        public virtual TrialAgent createAgent(int seedGroup)
        {
            TrialRegularAgent agent = new TrialRegularAgent();

            agent.mesh = meshes[Random.Range(0, meshes.Length)].name;

            if (animOffset < 0)
                agent.animationOffset = Random.Range(0f, 1f);
            else
                agent.animationOffset = animOffset;

            if (visualVariation < 0)
                agent.visualVariation = Random.Range(0, 10);
            else
                agent.visualVariation = visualVariation;

            agent.heightOffset = heightVariation;

            agent.radius = radius;


            agent.Position.vect = gameObject.transform.position;
            agent.Rotation.vect = gameObject.transform.rotation.eulerAngles;

            ControlLawGen lawGen = null;
            ControlLawGen[] tmpLG = gameObject.GetComponents<ControlLawGen>();
            foreach (ControlLawGen lg in tmpLG)
                if (lg.enabled)
                {
                    lawGen = lg;
                    break;
                }
            if (lawGen != null)
                agent.xmlControlLaw = lawGen.createControlLaw();

            ControlSimGen simGen = null;
            ControlSimGen[] tmpSG = gameObject.GetComponents<ControlSimGen>();
            foreach (ControlSimGen sg in tmpSG)
                if (sg.enabled)
                {
                    simGen = sg;
                }
            if (simGen != null)
                agent.xmlControlSim = simGen.createControlSim(seedGroup);

            return agent;
        }
    }
}
