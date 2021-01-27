using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

/// <summary>
/// Configurable recorder using DataRecorder to load a list of data to record from the trial parameters
/// </summary>
public class ConfigurableRecorder : MonoBehaviour, Recorder {

    private Player player;
    private List<Agent> agents;
    private ToolsOutput recordingObject;
    //private GameObject playerCam;
    private List<RecorderData> dataList;

    /// <summary>
    /// Reset recorder
    /// </summary>
    public void clear()
    {
        if (recordingObject != null)
            recordingObject = null;
        if (agents != null)
        {
            agents.Clear();
            agents = null;
        }
        dataList.Clear();

    }

    /// <summary>
    /// Initialize recorder with an agent list to look after
    /// </summary>
    /// <param name="agentList"> The agents from which to save data </param>
    public void initRecorder(Player p, List<Agent> agentList)
    {
        player = p;
        //playerCam = GameObject.FindGameObjectWithTag("MainCamera");


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

        // Save Data list
        if (dataList==null)
            dataList = new List<RecorderData>();
        foreach (CustomXmlSerializer<RecorderData> d in LoaderConfig.recordedDataList)
        {
            d.Data.initialize();
            dataList.Add(d.Data);
        }

        if (LoaderConfig.RecHeaders)
        {
            // Print headers
            string dataLine = "Time";

            // Player data
            foreach (RecorderData d in dataList)
                dataLine = dataLine + d.getPlayerHeader(player);

            // Player data
            foreach (Agent a in agents)
                foreach (RecorderData d in dataList)
                    dataLine = dataLine + d.getAgentHeader(a);

            // Other data
            foreach (RecorderData d in dataList)
                dataLine = dataLine + d.getOtherData();

            recordingObject.writeLine(dataLine);
        }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Data are recorded during Late Update when all agent's movements are over (movements are done during update)
    /// </summary>
    void LateUpdate()
    {
        if (ToolsTime.DeltaTime != 0 && recordingObject != null)
        {
            string dataLine = ToolsTime.TrialTime.ToString();

            // Player data
            foreach (RecorderData d in dataList)
                dataLine = dataLine + d.getPlayerData(player);

            // Player data
            foreach (Agent a in agents)
                foreach (RecorderData d in dataList)
                    dataLine = dataLine + d.getAgentData(a);

            // Other data
            foreach (RecorderData d in dataList)
                dataLine = dataLine + d.getOtherData();

            recordingObject.writeLine(dataLine);
        }
    }
}
