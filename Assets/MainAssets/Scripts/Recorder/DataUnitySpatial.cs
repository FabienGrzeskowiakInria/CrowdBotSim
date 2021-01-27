using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Convert agent spatial data to text for recording
/// </summary>
public class DataUnitySpatial : RecorderData
{
    public string getAgentData(Agent a)
    {
        string dataText = 
                        LoaderConfig.RecDataSeparator + 
                        a.gameObject.transform.position.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                        LoaderConfig.RecDataSeparator +
                        a.gameObject.transform.position.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                        LoaderConfig.RecDataSeparator +
                        a.gameObject.transform.position.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator);
        return dataText;
    }

    public string getAgentHeader(Agent a)
    {
        string dataText =   LoaderConfig.RecDataSeparator +
                            "Agent " + a.GetID() + " Position X" +
                            LoaderConfig.RecDataSeparator +
                            "Agent " + a.GetID() + " Position Y" +
                            LoaderConfig.RecDataSeparator +
                            "Agent " + a.GetID() + " Position Z";
        return dataText;
    }

    public string getOtherData()
    {
        return "";
    }

    public string getOtherHeader()
    {
        return "";
    }

    public string getPlayerData(Player p)
    {
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        string dataText =   /* Positon PlayerObject */
                           LoaderConfig.RecDataSeparator +
                           p.gameObject.transform.position.x.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                           LoaderConfig.RecDataSeparator +
                           p.gameObject.transform.position.y.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                           LoaderConfig.RecDataSeparator +
                           p.gameObject.transform.position.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
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
                           playerCam.transform.eulerAngles.z.ToString().Replace(".", LoaderConfig.RecDecimalSeparator);

        return dataText;
    }

    public string getPlayerHeader(Player p)
    {
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        string dataText =   /* Positon PlayerObject */
                           LoaderConfig.RecDataSeparator +
                           "Player Object Position X" +
                           LoaderConfig.RecDataSeparator +
                           "Player Object Position Y" +
                           LoaderConfig.RecDataSeparator +
                           "Player Object Position Z" +

                           /* Position Camera */
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Position X" +
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Position Y" +
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Position Z" +

                           /* Rotation Camera */
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Rotation X" +
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Rotation Y" +
                           LoaderConfig.RecDataSeparator +
                           "Player Camera Rotation Z";

        return dataText;
    }

    public void initialize()
    {

    }
}
