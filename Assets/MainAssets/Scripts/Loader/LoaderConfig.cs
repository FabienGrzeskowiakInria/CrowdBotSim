using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CrowdMP.Core;

/// <summary>
/// Load or create config/trials files, contains all the config and current trial parameters
/// </summary>
static class LoaderConfig
{
    #region staticAttributs
    static public string dataPath;

    static private Config data;
    static private LoaderXP trialData;
    private static string configFileName;
    #endregion

    static LoaderConfig()
    {
        string pathPlayer = Application.dataPath;
        int lastIndex = pathPlayer.LastIndexOf('/');
        dataPath = pathPlayer.Remove(lastIndex, pathPlayer.Length - lastIndex);

        //configFileName = dataPath + @"/Config.xml";

        data = null;
        //loadConfig(configFileName);

        //if (data == null)  
        //{
        //    CreateDefaultConfigFile();
        //    ToolsDebug.logFatalError("No config file, see folder configTemplate.xml");
        //    Application.Quit();
        //}

        //trialData = new LoaderXP();
    }

    public static void loadConfig(string path)
    {
        configFileName = dataPath + path;
        char[] arr = configFileName.ToCharArray();
        // arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c) || c == '/' || c == '.')));
        configFileName = new string(arr);

        if (File.Exists(configFileName))
        {
            data = (Config)LoaderXML.LoadXML<Config>(configFileName);
            trialData = new LoaderXP();
            if (data == null)
                ToolsDebug.logFatalError("Configuration file exists but XML reader failed to load it");
            else
                ToolsDebug.log("Loaded configuration");
        }
        else
        {
            ToolsDebug.logFatalError("Configuration file " + configFileName + " doesn't exist");
        }
    }

    /// <summary>
    /// Create template of the config file
    /// </summary>
    public static void CreateDefaultConfigFile()
    {
        Config defaultData = new Config();

        //defaultData.addOnList.Add(new CustomXmlSerializer<ConfigExtra>(new ConfigFove()));

        LoaderXML.CreateXML<Config>(dataPath + @"/ConfigTemplate.xml", defaultData);

        LoaderXP.CreateDefaultScenarioFile();
    }


    /// <summary>
    /// Load parameters for current trial
    /// </summary>
    /// <returns>Return false if no more trial to load</returns>
    public static bool LoadXP()
    {
        return trialData.loadXP();
    }

    /// <summary>
    /// Goes to the next trial file
    /// </summary>
    public static void NextTrial()
    {
        xpCurrentTrial++;
    }

    /// <summary>
    /// Goes to the previous trial file
    /// </summary>
    public static void ChangeTrial(int inc)
    {
        xpCurrentTrial += inc;
    }
    /// <summary>
    /// Goes to the previous trial file
    /// </summary>
    public static void PreviousTrial()
    {
        xpCurrentTrial--;
    }


    #region get_set
    // User
    public static int xpCurrentUser { get { return data.experience.userID; } }
    public static int xpCurrentTrial
    {
        get { return data.experience.trial; }
        set { data.experience.trial = value; }
    }
    public static int xpMaxTrial { get { return trialData.maxTrial; } }
    public static string xpOrderFile { get { return data.experience.xpFiles; } }

    // Log
    public static int debugLvl { get { return data == null ? 3 : data.log.debugLvl; } }

    // Current Trial - Scene
    public static string sceneName { get { return trialData.currentTrial.scene.meshName; } }
    public static TrialConfig sceneConfig { get { return trialData.currentTrial.config.Data; } }
    public static Vector3 scenePosition { get { return trialData.currentTrial.scene.Position.vect; } }
    public static Vector3 sceneRotation { get { return trialData.currentTrial.scene.Rotation.vect; } }
    public static string sceneOutputFile { get { return trialData.currentTrial.scene.recordingFile; } }
    public static List<TrialEnding> sceneEndings { get { return trialData.currentTrial.scene.endings; } }
    // Current Trial - Player
    public static TrialPlayer playerInfo { get { return trialData.currentTrial.player; } }
    public static List<CustomXmlSerializer<TrialRobot>> robotsInfo { get { return trialData.currentTrial.robots; } }

    //public static Vector3   playerPosition          { get { return trialData.currentTrial.player.Position.vect; } }
    //public static Vector3   playerRotation          { get { return trialData.currentTrial.player.Rotation.vect; } }
    //public static TrialDevice playerDevice          { get { return trialData.currentTrial.player.device; } }
    //public static ControlLaw playerControlLaw       { get { return trialData.currentTrial.player.controlLaw; } }
    //public static TrialControlSim playerControlSim  { get { return trialData.currentTrial.player.controlSim; } }
    // Current Trial - Agent
    public static List<CustomXmlSerializer<TrialAgent>> agentsInfo { get { return trialData.currentTrial.agents; } }
    // Record data
    public static TrialScreenRecorder screenRecorder { get { return trialData.currentTrial.screenRecorder; } }
    // ADD ON
    public static List<CustomXmlSerializer<ConfigExtra>> addons { get { return data.addOnList; } }
    #endregion
}

/// <summary>
/// Config parameters (XML serializable)
/// </summary>
public class Config
{
    public ConfigUser experience;
    public ConfigLog log;

    [XmlArray("AddOns")]
    [XmlArrayItem("AddOn", type: typeof(CustomXmlSerializer<ConfigExtra>))]
    public List<CustomXmlSerializer<ConfigExtra>> addOnList;


    public Config()
    {
        experience = new ConfigUser();
        log = new ConfigLog();
        addOnList = new List<CustomXmlSerializer<ConfigExtra>>();
    }

}

/// <summary>
/// Config parameters concerning the users (XML serializable)
/// </summary>
public class ConfigUser
{
    [XmlAttribute("currentUser")]
    public int userID;
    [XmlAttribute("startingTrial")]
    public int trial;

    [XmlElement("sourceFileExperiment")]
    public string xpFiles;

    public ConfigUser()
    {
        userID = 0;
        trial = 0;
        xpFiles = "./Scenario/Test/FileOrder{USER}.csv";
    }
}

/// <summary>
/// Config parameters concerning the logging process (XML serializable)
/// </summary>
public class ConfigLog
{
    [XmlAttribute]
    public int debugLvl;

    public ConfigLog()
    {
        debugLvl = 0;
    }
}


