/* Crowd Simulator Engine
** Copyright (C) 2018 - Inria Rennes - Rainbow - Julien Pettre
**
** This program is free software; you can redistribute it and/or
** modify it under the terms of the GNU General Public License
** as published by the Free Software Foundation; either version 2
** of the License, or (at your option) any later version.
**
** This program is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** You should have received a copy of the GNU General Public License
** along with this program; if not, write to the Free Software
** Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
**
** Authors: Julien Bruneau, Tristan Le Bouffant
**
** Contact: crowd_group@inria.fr
*/
using UnityEngine;

/// <summary>
/// Control camera recording by taking screenshot at specific framerate
/// </summary>
public class ToolsCamRecord : MonoBehaviour
{

    #region attributes
    private int imageIncrement = 0;             // NB images already save for incrementing files name
    public bool record = true;                  // Is recording or not

    public float timeToStart = 0;               // Time when recording shall start
    public float timeToStop = 60;               // Time when recording shall stop
    public int framerate = 25;                  // Framerate at which screenshot are taken
    public string saveDir = "Img/capture/";     // Directory where to save all the pictures
    #endregion

    /// <summary>
    /// Initialize the recording
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// Change the recording framerate
    /// </summary>
    /// <param name="rate">new framerate</param>
    public void ChangeFramerate(int rate)
    {
        framerate = rate;
    }

    /// <summary>
    /// Create screenshot during recording time
    /// </summary>
    void Update()
    {
        if (record && !ToolsTime.isInPause && !(ToolsTime.TrialTime < timeToStart))
        {
            if (Time.captureFramerate == 0)
                Time.captureFramerate = framerate;

            if (ToolsTime.TrialTime > timeToStop)
            {
                record = false;
                Time.captureFramerate = 0;
                Debug.Log("record stopped !");
                Application.Quit();
                return;
            }
            ScreenCapture.CaptureScreenshot(LoaderConfig.dataPath + saveDir + imageIncrement.ToString("D" + 5) + ".png");
            imageIncrement++;
        }
    }

    //void OnGUI()
    //{
    //	Event e = Event.current;

    //	if ((EventType.KeyDown == e.type) 
    //	    && (Event.current.keyCode != KeyCode.None)) { // we need to avoid the "empty key pressed" which is detected
    //		KeyboardEvents(e.keyCode);
    //	}
    //}

    //void KeyboardEvents(KeyCode keyCode)
    //{
    //	if (KeyCode.Space == keyCode)
    //	{
    //		record = !record;
    //		if(record)
    //		{
    //			Time.captureFramerate = framerate;
    //		}
    //	}
    //}	
}
