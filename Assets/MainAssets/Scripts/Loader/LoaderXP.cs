using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace CrowdMP.Core
{

    /// <summary>
    /// Load or create trials files, contains all the parameters of the current trial and the trials list
    /// </summary>
    public class LoaderXP
    {

        private List<string> trialsList;

        public Trial currentTrial;
        public int maxTrial { get { return trialsList.Count; } }

        public LoaderXP()
        {
            // -------------
            // LOAD XP ORDER
            string orderFilePath = LoaderConfig.dataPath + @"/" + LoaderConfig.xpOrderFile.Replace("{USER}", LoaderConfig.xpCurrentUser.ToString());
            orderFilePath = orderFilePath.Replace("{TWO}", (LoaderConfig.xpCurrentUser % 2).ToString());

            if (File.Exists(orderFilePath) == true)
            {
                // If the file exist, store it's content and go to the first scene
                string[] sourceFileContentArray = File.ReadAllLines(orderFilePath);
                trialsList = sourceFileContentArray.ToList();

                ToolsDebug.log("Loaded scenario file : " + orderFilePath + ". Got " + trialsList.Count + " files");
            }
            else
            {
                ToolsDebug.logFatalError("End of program while trying to reach user " + LoaderConfig.xpCurrentUser.ToString() + " (File " + orderFilePath + " doesn't exist");
                Application.Quit();
            }
            // LOAD XP ORDER
            // -------------
        }

        public LoaderXP(Trial xp)
        {
            // -------------
            // LOAD XP ORDER
            currentTrial = xp;
            // LOAD XP ORDER
            // -------------
        }

        /// <summary>
        /// Load current trial's parameters
        /// </summary>
        /// <returns>False if no more trial</returns>
        public bool loadXP()
        {
            if (LoaderConfig.xpCurrentTrial < 0)
                LoaderConfig.xpCurrentTrial = 0;
            if (trialsList != null && LoaderConfig.xpCurrentTrial < trialsList.Count)
            {
                string filePath = LoaderConfig.dataPath + trialsList[LoaderConfig.xpCurrentTrial];

                if (File.Exists(filePath) == true)
                {
                    currentTrial = (Trial)LoaderXML.LoadXML<Trial>(filePath);
                    return true;
                }
                else
                {
                    ToolsDebug.logError("Error, file " + filePath + " doesn't exist (found on line " + LoaderConfig.xpCurrentTrial + " of " + LoaderConfig.xpOrderFile + ").");
                }
            }
            return false;
        }

        /// <summary>
        /// Create a template trial file
        /// </summary>
        public static void CreateDefaultScenarioFile()
        {
            Trial defaultTrial = new Trial();

            //GazeReplayconfig parameters = new GazeReplayconfig();
            //parameters.rightEye = new eyeConfig();
            //defaultTrial.config = parameters;

            TrialRegularAgent tmp = new TrialRegularAgent();
            LawStanding tmpLawMoveStraight = new LawStanding();
            tmp.xmlControlLaw = tmpLawMoveStraight;
            tmp.xmlControlSim = new CustomXmlSerializer<TrialControlSim>();
            tmp.xmlControlSim.Data = new VOGConfig();
            defaultTrial.agents.Add(tmp);

            tmp = new TrialRegularAgent();
            LawFileData tmpLawWaypoint = new LawFileData();
            //tmpLawWaypoint.goals.Add(new ConfigVect());
            tmp.xmlControlLaw = tmpLawWaypoint;
            defaultTrial.agents.Add(tmp);
            defaultTrial.scene.endings.Add(new TrialEnding());
            defaultTrial.screenRecorder = new TrialScreenRecorder();

            defaultTrial.recordDataList = new List<CustomXmlSerializer<RecorderData>>();
            defaultTrial.recordDataList.Add(new DataUnitySpatial());
            defaultTrial.recordDataList.Add(new DataDeviceState());


            LoaderXML.CreateXML<Trial>(LoaderConfig.dataPath + @"/TrialTemplate.xml", defaultTrial);
        }
    }

    /// <summary>
    /// Trial parameters (XML serializable)
    /// </summary>
    [System.Serializable]
    public class Trial
    {
        public TrialScene scene;
        public CustomXmlSerializer<TrialConfig> config;

        [XmlElement("player")]
        public CustomXmlSerializer<TrialPlayer> player;

        [XmlArray("robots")]
        [XmlArrayItem("robot")]
        public List<CustomXmlSerializer<TrialRobot>> robots;

        public TrialScreenRecorder screenRecorder;
        [XmlArray("SavedDataList")]
        [XmlArrayItem("data", type: typeof(CustomXmlSerializer<RecorderData>))]
        public List<CustomXmlSerializer<RecorderData>> recordDataList;

        [XmlArray("agents")]
        [XmlArrayItem("agent")]
        public List<CustomXmlSerializer<TrialAgent>> agents;

        [XmlArray("obstacles")]
        [XmlArrayItem("obstacle")]
        public List<CustomXmlSerializer<TrialObstacle>> obstacles;

        public Trial()
        {
            scene = new TrialScene();
            player = new TrialRegularPlayer();
            screenRecorder = null;

            recordDataList = null;
            robots = new List<CustomXmlSerializer<TrialRobot>>();
            agents = new List<CustomXmlSerializer<TrialAgent>>();
            obstacles = new List<CustomXmlSerializer<TrialObstacle>>();
        }
    }

    #region Scene
    /// <summary>
    /// Trial parameters concerning the scene (XML serializable)
    /// </summary>
    [System.Serializable]
    public class TrialScene
    {
        [XmlAttribute]
        public string meshName;

        public ConfigVect Position;
        public ConfigVect Rotation;

        public string recordingFile;

        [XmlArray("endingConditions")]
        [XmlArrayItem("condition")]

        public List<TrialEnding> endings;

        public TrialScene()
        {
            meshName = "Kerlann";
            Position = new ConfigVect();
            Rotation = new ConfigVect();
            recordingFile = "Output_{USER}_{ITT}.csv";

            endings = new List<TrialEnding>();
            endings.Add(new TrialEnding());
        }

    }

    /// <summary>
    /// Trial parameters concerning the ending condition (XML serializable)
    /// </summary>
    [System.Serializable]
    public class TrialEnding
    {
        [XmlAttribute]
        public TrialParam parameter;
        [XmlAttribute]
        public TrialTest test;
        [XmlAttribute]
        public float value;

        public TrialEnding()
        {
            parameter = TrialParam.time;
            test = TrialTest.less;
            // test = TrialTest.greater;
            // value = 60;
            value = 0;
        }

    }

    /// <summary>
    /// Enum the trials variables
    /// </summary>
    [System.Serializable]
    public enum TrialParam
    {
        time,
        x,
        y
    }

    /// <summary>
    /// Enum test that can be perform on trial variables
    /// </summary>
    [System.Serializable]
    public enum TrialTest
    {
        greater,
        less
    }
    #endregion

    public class TrialScreenRecorder
    {
        [XmlAttribute]
        public bool record;                  // Is recording or not
        [XmlAttribute]
        public float timeToStart;               // Time when recording shall start
        [XmlAttribute]
        public float timeToStop;               // Time when recording shall stop
        [XmlAttribute]
        public int framerate;                  // Framerate at which screenshot are taken

        public string saveDir;     // Directory where to save all the pictures

        public TrialScreenRecorder()
        {
            record = false;
            timeToStart = 0;
            timeToStop = 0;
            framerate = 25;
            saveDir = "./Output/";
        }
    }

    #region Agents
    /// <summary>
    /// Interface for the trial parameters concerning an agent control law (XML serializable)
    /// </summary>
    public interface TrialControlSim
    {
        int getConfigId();
        ControlSim createControlSim(int id);
    }
    #endregion

    /// <summary>
    /// Vector3 equivalent for XML serialization
    /// </summary>
    [System.Serializable]
    public class ConfigVect
    {
        [XmlAttribute]
        public float x;
        [XmlAttribute]
        public float y;
        [XmlAttribute]
        public float z;

        public ConfigVect()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public ConfigVect(float naturalX, float naturalY, float naturalZ)
        {
            x = naturalX;
            y = naturalY;
            z = naturalZ;
        }

        public ConfigVect(Vector3 v)
        {
            x = -v.x;
            y = v.z;
            z = v.y;
        }


        [XmlIgnoreAttribute]
        public Vector3 vect
        {
            get { return new Vector3(-x, z, y); }
            set { x = -value.x; y = value.z; z = value.y; }
        }

    }
}