using System;
using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR) 
using UnityEditor;
#endif
using UnityEngine;

namespace CrowdMP.Core
{
    public class TrialGen : MonoBehaviour {

        [Tooltip("Path of the generated XML file")]
        public string path = "./trial.xml"; //

        [Tooltip("Scene to use for the trial")]
        public GameObject stage = null;

        [Tooltip("Name of the output file to save data during the trial")]
        public string recordingFile = "Output_{USER}_{ITT}.csv";

        [Tooltip("The player parameters")]
        public GameObject playerGenerator;

        [Tooltip("Spawners containing the agents to include in the trial")]
        public Spawn[] spawners;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        // Use this for initialization
        void Start() {
            //gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update() {

        }

        public void spawn()
        {
            foreach (Spawn s in spawners)
            {
#if (UNITY_EDITOR)
                Undo.RecordObject(s,"Spawn");
                EditorUtility.SetDirty(s);
#endif
                s.spawn();
            }
        }

        public void clear()
        {
            foreach (Spawn s in spawners)
            {
#if (UNITY_EDITOR)
                Undo.RecordObject(s, "ClearSpawn");
                EditorUtility.SetDirty(s);
#endif
                s.clearAll();
            }
        }

        public void generate()
        {
            if (stage == null)
            {
                ToolsDebug.logError("No scene selected");
                return;
            }


            Trial newTrial = new Trial();
            // SCENE PARAMETERS
            newTrial.scene.meshName = stage.name;
            newTrial.scene.Position.vect = stage.transform.position;
            newTrial.scene.Rotation.vect = stage.transform.rotation.eulerAngles;
            newTrial.scene.recordingFile = recordingFile;
            // PLAYER PARAMETERS
            newTrial.player = playerGenerator.GetComponent<PlayerGen>().createPlayer();

            // ROBOT PARAMETERS
            RobotGen[] robots = gameObject.GetComponentsInChildren<RobotGen>();            
            foreach(RobotGen r in robots)
            {
                newTrial.robots.Add(r.createRobot());
            }

            int seedGroup = 0;
            foreach (Spawn s in spawners)
            {
                AgentGen[] agents = s.getAllAgent();
                foreach (AgentGen a in agents)
                {
                    if (a != null)
                        newTrial.agents.Add(a.createAgent(seedGroup));
                }
                seedGroup += agents.Length;

            }

              // obstacles
            foreach(GameObject o_go in GameObject.FindGameObjectsWithTag("Pillar"))
            {
                TrialObstacle obstacle = new TrialObstacle();
                Transform o = o_go.transform;
                obstacle.points.Add(new TrialObstacle.point(o.position.x, o.position.z, o.localScale.x));
                newTrial.obstacles.Add(obstacle);
            }

            foreach(GameObject o_go in GameObject.FindGameObjectsWithTag("Wall"))
            {
                TrialObstacle obstacle = new TrialObstacle();
                Transform o = o_go.transform;
                float x1 = o.position.x - o.localScale.x / 2.0f * Mathf.Cos(o.eulerAngles.y * Mathf.Deg2Rad);
                float y1 = o.position.z - o.localScale.x / 2.0f * Mathf.Cos(o.eulerAngles.y * Mathf.Deg2Rad);
                float x2 = o.position.x + o.localScale.x / 2.0f * Mathf.Cos(o.eulerAngles.y * Mathf.Deg2Rad);
                float y2 = o.position.z + o.localScale.x / 2.0f * Mathf.Cos(o.eulerAngles.y * Mathf.Deg2Rad);
                obstacle.lines.Add(new TrialObstacle.line(x1, y1, x2, y2, o.localScale.z));
                newTrial.obstacles.Add(obstacle);
            }



            LoaderXML.CreateXML<Trial>(path, newTrial);
        }
    }
}
