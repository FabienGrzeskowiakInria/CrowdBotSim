using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Regular trial manager with moving agents/player
/// </summary>
public class RegularTrial : MonoBehaviour, TrialManager {

    protected Player player;
    protected List<Agent> agentsList;
    protected Recorder rec;
    protected SimManager sims;
    protected ToolsCamRecord camRecord;

    public GameObject obstaclesContainer;

    /// <summary>
    /// Reset trial
    /// </summary>
    public virtual void clear()
    {
        /* Cleaning old virtual humans */
        foreach (Agent currentAgent in agentsList)
        {
            //GameObject.DestroyImmediate(currentAgent.gameObject); // Dangerous function but mandatory since the GraphicPlayer can't wait the end of the current frame to store the activehumans (Start function) at the beginning of a level, so using Destroy instead will make it store old virtualhumans from the previous level that will be clean at the end of the frame.
            GameObject.Destroy(currentAgent.gameObject);
        }
        agentsList.Clear();

        sims.clear();
        player.clear();
        //player.gameObject.SetActive(false);
        Destroy(player);
        if (camRecord != null)
        {
            Destroy(camRecord);
            camRecord = null;
        }
        if (rec != null)
            rec.clear();

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
        if (agentsList == null)
            agentsList = new List<Agent>();
        if (sims==null)
            sims = new SimManager();
        sims.initSimulations(obst);

        // Setup the player
        foreach (GameObject p in playersModel)
        {

            if (p.name == LoaderConfig.playerInfo.mesh)
            {
                player = LoaderConfig.playerInfo.createPlayerComponnent(p, 0);
                p.SetActive(true);
            } else
            {
                p.SetActive(false);
            }
        }

        // Create each agents from the trial.xml file
        uint i = 0;
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
            Agent currentAgent = a.createAgentComponnent(currentAgentGameObject, i + 1);
            currentAgentGameObject.tag = "VirtualHumanActive";
            agentsList.Add(currentAgent);
        }

        // Set the list of agent to watch in the recorder
        rec=gameObject.GetComponent<Recorder>();
        if (rec!=null)
            rec.initRecorder(player, agentsList);

        TrialScreenRecorder screenRecorderInfos = LoaderConfig.screenRecorder;
        if (screenRecorderInfos!=null)
        {
            camRecord=Camera.main.gameObject.AddComponent<ToolsCamRecord>();
            camRecord.record = screenRecorderInfos.record;
            camRecord.timeToStart = screenRecorderInfos.timeToStart;               
            camRecord.timeToStop = screenRecorderInfos.timeToStop;               
            camRecord.framerate = screenRecorderInfos.framerate;                  
            camRecord.saveDir = screenRecorderInfos.saveDir;     
        }

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
            float currentValue=0;
            switch (condition.parameter)
            {
                case TrialParam.time:
                    currentValue = ToolsTime.TrialTime;
                    break;
                case TrialParam.x:
                    currentValue = -player.transform.position.x;
                    break;
                case TrialParam.y:
                    currentValue = player.transform.position.z;
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
        if (ToolsTime.DeltaTime == 0)
            return;

        List<Vector3> currPos = new List<Vector3>();

        // Do regular step
        currPos.Add(player.Position);
        player.doStep();

        foreach (Agent a in agentsList)
        {
            currPos.Add(a.Position);
            a.doStep();
        }

        sims.doStep(ToolsTime.DeltaTime, currPos, player, agentsList);
    }

    ///// <summary>
    ///// Method used to initialize the VirtualHumans
    ///// </summary>
    ///// <returns>True if the VirtualHumans are well initialized, otherwise False</returns>
    //private bool initializeAgents()
    //{

    //    if (agentsList == null)
    //    {
    //        agentsList = new List<Agent>();
    //    }
    //    else
    //    {
    //        /* Cleaning old virtual humans */
    //        foreach (Agent currentAgent in agentsList)
    //        {
    //            GameObject.DestroyImmediate(currentAgent.gameObject); // Dangerous function but mandatory since the GraphicPlayer can't wait the end of the current frame to store the activehumans (Start function) at the beginning of a level, so using Destroy instead will make it store old virtualhumans from the previous level that will be clean at the end of the frame.
    //        }
    //        agentsList.Clear();
    //    }


    //    int i = 0;
    //    foreach (TrialAgent a in LoaderConfig.agentsInfo)
    //    {
    //        ++i;
    //        GameObject currentAgentGameObject = null;

    //        foreach (GameObject currentAgentInModel in agentModels)
    //        {

    //            if (currentAgentInModel.name == a.mesh)
    //            {
    //                currentAgentGameObject = (GameObject)GameObject.Instantiate(currentAgentInModel);
    //                currentAgentGameObject.name = i.ToString() + " - " + currentAgentGameObject.name;
    //                break;
    //            }
    //        }

    //        if (currentAgentGameObject == null)
    //        {
    //            ToolsDebug.logFatalError("Error, unknown mesh " + a.mesh);
    //            Application.Quit();
    //        }
    //        Agent currentAgent = currentAgentGameObject.GetComponent<Agent>();
    //        currentAgent.initAgent(a);
    //        currentAgent.gameObject.SetActive(true);
    //        currentAgent.gameObject.tag = "VirtualHumanActive";

    //    }




    //    //    virtualHumanListForSimulationWatcher.Add(currentVirtualHumanGameObject);
    //    //    VirtualHuman.GameObject = currentVirtualHumanGameObject;
    //    //    VirtualHuman.GraphicObject = VirtualHuman.GameObject.GetComponent<GraphicVirtualHuman>();
    //    //    VirtualHuman.Behavior = controlLawGenerator.setControlModule(VirtualHuman.BehaviorXelement, currentVirtualHumanGameObject);

    //    //    VirtualHuman.GameObject.SetActive(true);
    //    //    VirtualHuman.GraphicObject.SetPositionAndRotation(VirtualHuman.PositionAndRotation);
    //    //    VirtualHuman.GraphicObject.AnimationOffset = VirtualHuman.AnimationOffset;
    //    //    VirtualHuman.GraphicObject.idleAnimationIndex = VirtualHuman.IdleAnimationIdex;
    //    //    VirtualHuman.GraphicObject.IdColor = TableColor[i - 1];
    //    //    InvertColorDict[TableColor[i - 1]] = i - 1;
    //    //    VirtualHuman.GameObject.tag = "VirtualHumanActive";



    //    //    /* Set RVO and other responsive systems */
    //    //    if (VirtualHuman.Behavior.getBehaviorType() == BehaviorType.responsive)
    //    //    {
    //    //        _isRVOUsed = true;
    //    //        RVOManager.addAgent(currentVirtualHumanGameObject);
    //    //    }
    //    //    else if (VirtualHuman.Behavior.getBehaviorType() == BehaviorType.unresponsive)
    //    //    {
    //    //        RVOManager.addBlindAgent(currentVirtualHumanGameObject);
    //    //    }
    //    //}

    //    //if (currentXPManager != null)
    //    //    virtualHumanListForSimulationWatcher.AddRange(currentXPManager.getRecordedObjects());

    //    //this.GetComponent<simulationWatcher>().VirtualHumans = virtualHumanListForSimulationWatcher;
    //    //this.GetComponent<simulationWatcher>().InvertColorDict = InvertColorDict;
    //    //this.GetComponent<simulationWatcher>().InitDictGaze();

    //    return true;
    //}

    // Use this for initialization
    void Start () {
        if (agentsList==null)
            agentsList = new List<Agent>();
    }

    // Update is called once per frame
    void Update ()
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
