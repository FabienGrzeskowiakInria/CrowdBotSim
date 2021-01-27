using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace crowdbotsim
{
/// <summary>
/// The main manager controlling the Experiment flow
/// </summary>
public class CrowdBotSim_MainManager : MonoBehaviour, MainManager {

    public string configPath = @"/Configurations/config.xml";

    protected bool trialStarted;
    protected bool isEndXP;
    protected Player player;
    protected GameObject[] sceneObjectsModels;
    protected GameObject[] agentModels;
    protected GameObject[] playerModels;

    protected GameObject[] robotsModels;
    protected GameObject sceneGameObject;
    protected TrialManager currentTrialManager;

    public enum MainManagerState
    {
        idle,
        next,
        reset,
        previous,
        first,
        last,
        stop
    }

    public MainManagerState state = MainManagerState.idle;

    public void Awake()
    {
        UnityEngine.Random.InitState(454682);
        LoaderConfig.loadConfig(configPath);
    }

    // Use this for initialization
    public virtual void Start () {


        if (!initializeConstants())
        {
            ToolsDebug.logFatalError("Error on GUIManager constants initialisation");
            ToolsDebug.Quit();
        }

        // for (int i = 1; i < Display.displays.Length; i++)
        // {
        //     Display.displays[i].Activate();
        // }
        
        isEndXP = false;
        startTrial();
        startTransition();
    }

    /// <summary>
    /// Load and setup current trial
    /// </summary>
    public virtual void startTrial()
    {
        // Load trial file
        if (!LoaderConfig.LoadXP())
        {
            ToolsDebug.log("End of the trials", 2);
            isEndXP = true;
            //Application.Quit();
            return;
        }
        
        // Load the trial scene and unload the others
        string sceneMeshName = LoaderConfig.sceneName;
        foreach (GameObject currentGameObject in sceneObjectsModels)
        {
            /*Make every scene disapered exept the one given by the scenarioLoader*/
            if (currentGameObject.name == sceneMeshName)
            {
                currentGameObject.SetActive(true);

                if (currentGameObject.GetComponent<TrialManager>() == null)
                {
                    ToolsDebug.logFatalError("The trial need a Manager");
                    return;
                }


                sceneGameObject = currentGameObject;
                sceneGameObject.transform.position = LoaderConfig.scenePosition;
                sceneGameObject.transform.rotation = Quaternion.Euler(LoaderConfig.sceneRotation);
                break;
            }
        }

        if (sceneGameObject == null)
            ToolsDebug.logFatalError("Scene not found (" + sceneMeshName + ")");

        // Reset trial time
        ToolsTime.newLevel();

        // Initialize the trial manager
        currentTrialManager = sceneGameObject.GetComponent<CrowdBotSim_TrialManager>();
        if (currentTrialManager == null)
        {
            ToolsDebug.logFatalError("Trials " + LoaderConfig.xpCurrentTrial.ToString() + " does not have a manager");
            ToolsDebug.Quit();
        }
        currentTrialManager.initializeTrial(playerModels, agentModels);
        player = currentTrialManager.getPlayer();

        trialStarted = false;
        ToolsTime.tooglePause();
    }

    /// <summary>
    /// Method used to initialize scene constants
    /// </summary>
    /// <returns>True if constant are well initialized, otherwise False</returns>
    public virtual bool initializeConstants()
    {
        playerModels =GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in playerModels)
        {
            p.SetActive(false);
        }
        sceneObjectsModels = GameObject.FindGameObjectsWithTag("Stage");
        foreach (GameObject scene in sceneObjectsModels)
        {
            scene.SetActive(false);
        }

        agentModels = GameObject.FindGameObjectsWithTag("AgentModels");
        foreach (GameObject go in agentModels)
        {
            go.SetActive(false);
        }

        return true;
    }

    // Update is called once per frame
    void Update () {
        ToolsTime.updateTime();

        if (currentTrialManager == null)
            return;

        if (currentTrialManager.hasEnded())
            endTrial();
        else
            currentTrialManager.doStep();

    }

    // Handle key/button input in LateUpdate
    void LateUpdate()
    {

        if ((currentTrialManager==null || currentTrialManager.isReady())) //&& (ToolsInput.GetKeyDown(KeyCode.Space) || ToolsInput.GetButtonDown(ButtonCode.Fire1)))
        {
            if (!trialStarted)
            {
                trialStarted = true;
                currentTrialManager.startTrial();
            }

            // if (isEndXP)
                // ToolsDebug.Quit();

            // else
            //     transitionSign.SetActive(ToolsTime.tooglePause());
        }


        if (ToolsInput.GetKeyDown(KeyCode.N))
        {
            endTrial();
        }

        if (ToolsInput.GetKeyDown(KeyCode.P))
        {
            endTrial(-1);
        }

        if (ToolsInput.GetKeyDown(KeyCode.R))
        {
            endTrial(0);
        }

        if (ToolsInput.GetKeyDown(KeyCode.Escape))
        {
            ToolsDebug.Quit();
        }

        switch (state)
        {
            case MainManagerState.next:
                state = MainManagerState.idle;
                endTrial();
            break;

            case MainManagerState.previous:
                state = MainManagerState.idle;
                endTrial(-1);
            break;

            case MainManagerState.reset:
                state = MainManagerState.idle;
                endTrial(0);
            break;

            case MainManagerState.first:
                state = MainManagerState.idle;
                endTrial(-1*LoaderConfig.xpCurrentTrial);
            break;

            case MainManagerState.last:
                state = MainManagerState.idle;
                endTrial(LoaderConfig.xpMaxTrial);
            break;

            case MainManagerState.stop:
                ToolsDebug.Quit();
            break;

            default: //idle
            break;
        }
    }

    /// <summary>
    /// End the current trial change it
    /// </summary>
    public virtual void endTrial(int trialSwitch=1)
    {
        currentTrialManager.clear();
        sceneGameObject.gameObject.SetActive(false);
        sceneGameObject = null;
        currentTrialManager = null;
        LoaderConfig.ChangeTrial(trialSwitch);
        startTrial();
        startTransition();
    }

    /// <summary>
    /// Update and load transition screen
    /// </summary>
    public virtual void startTransition()
    {
        if (isEndXP)
        {
            player.gameObject.SetActive(true);
            return;
        }
    }
}

}