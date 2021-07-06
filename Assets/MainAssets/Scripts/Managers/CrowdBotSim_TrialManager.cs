
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;
using RosSharp.RosBridgeClient;
using System.Linq;

namespace crowdbotsim
{

    /// <summary>
    /// Regular trial manager with moving agents/player
    /// </summary>
    public class CrowdBotSim_TrialManager : MonoBehaviour, TrialManager
    {

        protected Player player;
        protected List<Robot> robots;
        protected List<Agent> agentsList;
        // protected Recorder rec;
        protected SimManager sims;

        List<RosConnector> ROS;
        List<Publisher> Ros_Publishers;

        List<TcpConnector> Tcp;
        List<TcpPublisher> TcpPublishers;

        private CrowdStampedPublisher crowdpub;

        private bool player_in_sim;
        // protected ToolsCamRecord camRecord;
        private List<bool> robot_in_sim;

        public GameObject obstaclesContainer;

        public GameObject In;
        public GameObject Out;


        public GameObject In2;
        public GameObject Out2;

        /// <summary>
        /// Reset trial
        /// </summary>
        public virtual void clear()
        {
            /* Cleaning old virtual humans */
            foreach (Agent currentAgent in agentsList)
            {
                //GameObject.DestroyImmediate(currentAgent.gameObject); // Dangerous function but mandatory since the GraphicPlayer can't wait the end of the current frame to store the activehumans (Start function) at the beginning of a level, so using Destroy instead will make it store old virtualhumans from the previous level that will be clean at the end of the frame.
                if (currentAgent != null) GameObject.Destroy(currentAgent.gameObject);
            }
            agentsList.Clear();

            sims.clear();
            player.clear();
            robots.Clear();

            foreach (MonoBehaviour p in Ros_Publishers) p.enabled = false;

            foreach (MonoBehaviour p in TcpPublishers) p.enabled = false;


            foreach (RosConnector r in ROS)
            {
                r.enabled = false;
                // r.start_scripts = true;
            }

            foreach (TcpConnector t in Tcp)
            {
                t.enabled = false;
                t.start_scripts = true;
            }

            robot_in_sim.Clear();

            GameObject[] my_robots = GameObject.FindGameObjectsWithTag("Robot");
            foreach (GameObject r in my_robots)
            {
                r.transform.Find("base_link").localPosition = new Vector3(0, 0, 0);
                r.transform.Find("base_link").localRotation = Quaternion.identity;

                DestroyImmediate(r.GetComponent<RegularRobot>());
                DestroyImmediate(r.GetComponent<UnityEngine.AI.NavMeshAgent>());
            }
            //player.gameObject.SetActive(false);
            // Destroy(player);
            // if (camRecord != null)
            // {
            //     Destroy(camRecord);
            //     camRecord = null;
            // }
            // if (rec != null)
            //     rec.clear();

        }

        public List<Agent> get_agents_list()
        {
            if (agentsList == null)
                agentsList = new List<Agent>();
            return agentsList;
        }

        private void lookForObst(Transform container, ref Obstacles obst)
        {
            foreach (Transform item in container.transform)
            {
                if (item.tag == "Wall")
                    obst.addWall(item.gameObject);
                else if (item.tag == "Pillar")
                    obst.addPillar(item.gameObject);
                else
                    lookForObst(item, ref obst);
            }
        }

        /// <summary>
        /// Initialize trial
        /// </summary>
        /// <param name="p">The player</param>
        /// <param name="agentModels">All the available agent models</param>
        /// <returns>True if the trial has been correctly intialized</returns>
        public virtual bool initializeTrial(GameObject[] playersModel, GameObject[] agentModels)
        {
            // Compute obstacles list
            Obstacles obst = new Obstacles();
            if (obstaclesContainer != null)
                lookForObst(obstaclesContainer.transform, ref obst);

            // Setup simulations for collision avoidance
            if (robots == null)
                robots = new List<Robot>();
            if (agentsList == null)
                agentsList = new List<Agent>();
            if (sims == null)
                sims = new SimManager();

            if (Ros_Publishers == null)
                Ros_Publishers = new List<Publisher>();
            if (ROS == null)
                ROS = new List<RosConnector>();

            if (Tcp == null)
                Tcp = new List<TcpConnector>();
            if (TcpPublishers == null)
                TcpPublishers = new List<TcpPublisher>();

            if (robot_in_sim == null)
                robot_in_sim = new List<bool>();

            sims.initSimulations(obst);

            // Setup the player
            foreach (GameObject p in playersModel)
            {

                if (p.name == LoaderConfig.playerInfo.mesh)
                {
                    player = LoaderConfig.playerInfo.createPlayerComponnent(p, 0);
                    p.SetActive(true);
                }
                else
                {
                    p.SetActive(false);
                }
            }

            if (player.GetType() == typeof(CamPlayer))
            {
                player_in_sim = ((CamPlayer)player).in_sim;
            }
            else
            {
                player_in_sim = true;
            }


            GameObject[] my_robots = GameObject.FindGameObjectsWithTag("Robot");
            foreach (GameObject r in my_robots)
            {
                r.SetActive(false);
            }

            // Create each agents from the trial.xml file
            uint i = 0;

            foreach (TrialRobot trial_r in LoaderConfig.robotsInfo)
            {
                ++i;
                foreach (GameObject r in my_robots)
                {
                    if (r.name == trial_r.mesh)
                    {
                        robots.Add(trial_r.createRobotComponnent(r, i));
                        r.SetActive(true);
                        robot_in_sim.Add(((TrialRegularRobot)trial_r).in_sim);
                    }
                }
            }

            foreach (TrialAgent a in LoaderConfig.agentsInfo)
            {
                ++i;
                GameObject currentAgentGameObject = null;

                foreach (GameObject currentAgentInModel in agentModels)
                {

                    if (currentAgentInModel.name == a.mesh)
                    {
                        currentAgentGameObject = (GameObject)GameObject.Instantiate(currentAgentInModel);
                        currentAgentGameObject.name = i.ToString() + " - " + currentAgentGameObject.name;

                        break;
                    }
                }

                if (currentAgentGameObject == null)
                {
                    ToolsDebug.logFatalError("Error, unknown mesh " + a.mesh);
                    Application.Quit();
                }
                currentAgentGameObject.SetActive(true);
                Agent currentAgent = a.createAgentComponnent(currentAgentGameObject, i);
                currentAgentGameObject.tag = "VirtualHumanActive";
                agentsList.Add(currentAgent);
            }

            foreach (GameObject r in my_robots)
            {
                if (r.activeSelf)
                {
                    try
                    {
                        ROS.Add(r.transform.Find("RosConnector").gameObject.GetComponent<RosConnector>());
                    }
                    catch (System.Exception)
                    {
                    }
                    try
                    {
                        Tcp.Add(r.transform.Find("TcpConnector").gameObject.GetComponent<TcpConnector>());
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }

            try
            {
                ROS.Add(GameObject.Find("RosConnector").gameObject.GetComponent<RosConnector>());
            }
            catch (System.Exception)
            {
            }
            try
            {
                Tcp.Add(GameObject.Find("TcpConnector").gameObject.GetComponent<TcpConnector>());
            }
            catch (System.Exception)
            {
            }

            foreach (RosConnector RosCon in ROS)
            {
                Ros_Publishers.AddRange(new List<Publisher>(RosCon.GetComponents<Publisher>()));
                foreach (Publisher script in Ros_Publishers)
                {
                    if (script.GetType() == typeof(TwistArrayStampedPublisher))
                    {
                        ((TwistArrayStampedPublisher)script).agents = new List<Agent>(agentsList);
                    }
                    if (script.GetType() == typeof(CrowdStampedPublisher))
                    {
                        crowdpub = (CrowdStampedPublisher)script;
                        crowdpub.agents = new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent[agentsList.Count];
                        for (int j = 0; j < agentsList.Count; j++)
                        {
                            crowdpub.agents[j] =
                                new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent(
                                    (int)(agentsList[j].id), agentsList[j].Position,
                                    Vector3.zero, Vector3.zero, agentsList[j].Position);
                        }
                    }
                }

                RosCon.enabled = true;
                // RosCon.start_scripts = true;
            }

            foreach (TcpConnector TcpCon in Tcp)
            {
                TcpPublishers.AddRange(new List<TcpPublisher>(TcpCon.GetComponents<TcpPublisher>()));
                foreach (TcpPublisher script in TcpPublishers)
                {
                    // TODO : no need TODO anymore thanks to CrowdTcpPublisher

                    // if(script.GetType() == typeof(TwistArrayStampedPublisher) )
                    // {
                    //     ((TwistArrayStampedPublisher)script).agents = new List<Agent>(agentsList);
                    // }
                    // if(script.GetType() == typeof(CrowdStampedPublisher) )
                    // {
                    //     crowdpub = (CrowdStampedPublisher)script;
                    //     crowdpub.agents = new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent[agentsList.Count];
                    //     for(int j = 0; j < agentsList.Count; j++)
                    //     {
                    //         crowdpub.agents[j] = 
                    //             new RosSharp.RosBridgeClient.CrowdStampedPublisher.CrowdBotAgent(
                    //                 (int)(agentsList[j].id), agentsList[j].Position, 
                    //                 Vector3.zero, Vector3.zero, agentsList[j].Position);
                    //     }
                    // }
                }

                TcpCon.enabled = true;
                TcpCon.start_scripts = true;
            }



            // Set the list of agent to watch in the recorder
            // rec=gameObject.GetComponent<Recorder>();
            // if (rec!=null)
            //     rec.initRecorder(player, agentsList);

            // TrialScreenRecorder screenRecorderInfos = LoaderConfig.screenRecorder;
            // if (screenRecorderInfos!=null)
            // {
            //     camRecord=Camera.main.gameObject.AddComponent<ToolsCamRecord>();
            //     camRecord.record = screenRecorderInfos.record;
            //     camRecord.timeToStart = screenRecorderInfos.timeToStart;               
            //     camRecord.timeToStop = screenRecorderInfos.timeToStop;               
            //     camRecord.framerate = screenRecorderInfos.framerate;                  
            //     camRecord.saveDir = screenRecorderInfos.saveDir;     
            // }

            return true;
        }


        /// <summary>
        /// Check the ending conditions of the trials
        /// </summary>
        /// <returns>True if the trial is over</returns>
        public virtual bool hasEnded()
        {
            bool isEnd = false;
            foreach (TrialEnding condition in LoaderConfig.sceneEndings)
            {
                float currentValue = 0;
                switch (condition.parameter)
                {
                    case TrialParam.time:
                        currentValue = ToolsTime.TrialTime;
                        break;
                    case TrialParam.x:
                        currentValue = robots[0].transform.position.x;
                        break;
                    case TrialParam.y:
                        currentValue = robots[0].transform.position.z;
                        break;
                }

                switch (condition.test)
                {
                    case TrialTest.greater:
                        isEnd = isEnd || currentValue > condition.value;
                        break;
                    case TrialTest.less:
                        isEnd = isEnd || currentValue < condition.value;
                        break;
                }
            }

            return isEnd;
        }

        /// <summary>
        /// Perform a step of the trial
        /// </summary>
        public virtual void doStep()
        {
            if (!player_in_sim)
                player.doStep();

            if (ToolsTime.DeltaTime == 0)
                return;

            List<Vector3> currPos = new List<Vector3>();

            // Do regular step
            if (player_in_sim)
            {
                currPos.Add(player.Position);
                player.doStep();
            }

            int robot_index = 0;
            foreach (Robot r in robots)
            {
                if (robot_in_sim[robot_index])
                {
                    Vector3 pos = r.transform.Find("base_link").position;
                    currPos.Add(pos);
                }
                r.doStep();
            }

            foreach (Agent a in agentsList)
            {
                currPos.Add(a.Position);
                a.doStep();
            }

            int start_index = 0;
            if (player_in_sim) start_index++; //player
            if (robots != null)
            {

                start_index += robot_in_sim.Where(b => b == true).Count();
            }

            if (crowdpub != null)
            {
                if (currPos.Count - start_index > 0) crowdpub.UpdateAgents(currPos.GetRange(start_index, currPos.Count - start_index), agentsList);
            }


            sims.doStep(ToolsTime.DeltaTime, currPos, player, agentsList, robots);

            if (In != null && Out != null)
            {
                foreach (Agent a in In.GetComponent<teleportTo>().get_agents_list())
                {
                    // a.transform.Translate(In.transform.position - Out.transform.position);
                    a.transform.position = new Vector3(
                        Out.transform.position.x - (In.transform.position.x - a.transform.position.x),
                        a.transform.position.y,
                        Out.transform.position.z - (In.transform.position.z - a.transform.position.z)
                    );
                }
            }

            if (In2 != null && Out2 != null)
            {
                foreach (Agent a in In2.GetComponent<teleportTo>().get_agents_list())
                {
                    // a.transform.Translate(In.transform.position - Out.transform.position);
                    a.transform.position = new Vector3(
                        Out2.transform.position.x - (In2.transform.position.x - a.transform.position.x),
                        a.transform.position.y,
                        Out2.transform.position.z - (In2.transform.position.z - a.transform.position.z)
                    );
                }
            }

            foreach (Publisher script in Ros_Publishers)
            {
                if (((MonoBehaviour)script).enabled == true)
                    script.UpdateMessage();
            }

        }

        // Use this for initialization
        void Start()
        {
            if (agentsList == null)
                agentsList = new List<Agent>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public Player getPlayer()
        {
            return player;

        }

        public virtual bool isReady()
        {
            return true;
        }
        public virtual void startTrial()
        {


        }


    }
}