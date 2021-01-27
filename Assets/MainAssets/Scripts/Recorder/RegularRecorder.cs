using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Recorder recording basic data (Spatial data of agents and player as well as player inputs)
/// </summary>
public class RegularRecorder : MonoBehaviour, Recorder {

    private Player player;
    private List<Agent> agents;
    private ToolsOutput recordingObject;
    private GameObject playerCam;


    /// <summary>
    /// Reset the recorder
    /// </summary>
    public void clear()
    {
        if (recordingObject!=null)
            recordingObject = null;
        if (agents != null)
        {
            agents.Clear();
            agents = null;
        }

    }

    /// <summary>
    /// Initialize the recorder
    /// </summary>
    /// <param name="agentList"> The list of agents to watch </param>
    public void initRecorder(Player p,List<Agent> agentList)
    {
        player = p;
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");

        // Create data files
        string filePrototype = LoaderConfig.sceneOutputFile;
        if (filePrototype != "")
        {

            string path = LoaderConfig.dataPath + @"/" + LoaderConfig.RecFolder + @"\" + filePrototype;
            path = path.Replace("{USER}", LoaderConfig.xpCurrentUser.ToString());
            path = path.Replace("{ITT}", LoaderConfig.xpCurrentTrial.ToString());

            recordingObject = new ToolsOutput(path);
        }
        else
        {
            recordingObject = null;
            return;
        }

        // Save agents list
        agents = agentList;
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Create a string containing all the recorded player data
    /// </summary>
    /// <returns>player's data as a string</returns>
    public string getPlayerData()
    {
        string dataText =   /* Positon PlayerObject */
                            player.gameObject.transform.position.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            player.gameObject.transform.position.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            player.gameObject.transform.position.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +

                            /* Position Camera */
                            playerCam.transform.position.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            playerCam.transform.position.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            playerCam.transform.position.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +

                            /* Rotation Camera */
                            playerCam.transform.eulerAngles.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            playerCam.transform.eulerAngles.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            playerCam.transform.eulerAngles.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +

                            /* Store value of VAxis and Haxis */
                            ToolsInput.getAxisValue(ToolsAxis.Vertical).ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            ToolsInput.getAxisValue(ToolsAxis.Horizontal).ToString().Replace(".", LoaderConfig.RecDecimalSeparator);


        return dataText;
    }

    /// <summary>
    /// Create a string containing all the recorded data of the agents
    /// </summary>
    /// <returns>agents' data as a string</returns>
    public string getAgentsData()
    {
        string dataText = "";
        /* Store position virtual human */
        foreach (Agent a in agents)
        {
            dataText = dataText + a.gameObject.transform.position.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                                  LoaderConfig.RecDataSeparator +
                                  a.gameObject.transform.position.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                                  LoaderConfig.RecDataSeparator +
                                  a.gameObject.transform.position.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                                  LoaderConfig.RecDataSeparator;
        }

        return dataText.TrimEnd(LoaderConfig.RecDataSeparator.ToCharArray());
    }

    /// <summary>
    /// Save data during LateUpdate when all movement as be done (done during the update)
    /// </summary>
    void LateUpdate()
    {
        if (ToolsTime.DeltaTime != 0 && recordingObject!=null)
        {
            string pData = getPlayerData();
            string aData = getAgentsData();

            string data = ToolsTime.TrialTime.ToString();
            if (pData.Length > 0)
                data = data + LoaderConfig.RecDataSeparator + pData;
            if (aData.Length>0)
                data = data + LoaderConfig.RecDataSeparator + aData;

            recordingObject.writeLine(data);
        }
    }
}
