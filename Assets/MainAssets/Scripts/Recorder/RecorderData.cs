using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class to define classes converting data to text for the ConfigurableRecorder
/// </summary>
public interface RecorderData{

    /// <summary>
    /// initialize the data recorder
    /// </summary>
    void initialize();
    /// <summary>
    /// Return data to recorded about the player (String start with delimiter, end without)
    /// </summary>
    /// <param name="p">String containing the player data in CSV format</param>
    /// <returns></returns>
    string getPlayerData(Player p);
    /// <summary>
    /// Return data to recorded about a give agent (String start with delimiter, end without)
    /// </summary>
    /// <param name="a">agent which data are return</param>
    /// <returns>String containing the given agent data in CSV format</returns>
    string getAgentData(Agent a);
    /// <summary>
    /// Return data to recorded about other thing than player or agent (String start with delimiter, end without)
    /// </summary>
    /// <returns>String containing the data in CSV format</returns>
    string getOtherData();
    /// <summary>
    /// Return header for the recorded data about the player (String start with delimiter, end without)
    /// </summary>
    /// <returns>String containing the recorded data list for the player in CSV format</returns>
    string getPlayerHeader(Player p);
    /// <summary>
    /// Return header for the recorded data about the agents (String start with delimiter, end without)
    /// </summary>
    /// <returns>String containing the recorded data list for the agents in CSV format</returns>
    string getAgentHeader(Agent a);
    /// <summary>
    /// Return header for the recorded data that is neither about player nor agents (String start with delimiter, end without)
    /// </summary>
    /// <returns>String containing the recorded data list, for everything neither player nor agents related, in CSV format</returns>
    string getOtherHeader();
}
