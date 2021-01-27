using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Convert the player input device's data to text for recording
/// </summary>
public class DataDeviceState : RecorderData
{

    public string getAgentData(Agent a)
    {
        return "";
    }

    public string getAgentHeader(Agent a)
    {
        return "";
    }

    public string getOtherData()
    {
        string dataText =   /* Store value of VAxis and Haxis */
                            LoaderConfig.RecDataSeparator +
                            ToolsInput.getAxisValue(ToolsAxis.Vertical).ToString().Replace(".", LoaderConfig.RecDecimalSeparator) +
                            LoaderConfig.RecDataSeparator +
                            ToolsInput.getAxisValue(ToolsAxis.Horizontal).ToString().Replace(".", LoaderConfig.RecDecimalSeparator);
        return dataText;
    }

    public string getOtherHeader()
    {
        string dataText =   /* Store value of VAxis and Haxis */
                            LoaderConfig.RecDataSeparator +
                            "Input Vertical Axis" +
                            LoaderConfig.RecDataSeparator +
                            "Input Horizontal Axis";
        return dataText;
    }

    public string getPlayerData(Player p)
    {
        return "";
    }

    public string getPlayerHeader(Player p)
    {
        return "";
    }

    public void initialize()
    {
        
    }
}
