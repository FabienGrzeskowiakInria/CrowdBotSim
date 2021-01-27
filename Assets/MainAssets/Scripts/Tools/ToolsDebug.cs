using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Control debug messages (Can be modified to save debug in specific location)
/// </summary>
public static class ToolsDebug {
    
    /// <summary>
    /// Log a debug msg
    /// </summary>
    /// <param name="s"> the debug msg </param>
    /// <param name="lvl"> importance of the message (0=error, 1=warning, 2=important info, 3=regular info)</param>
    static public void log(string s, int lvl=3)
    {
        if (LoaderConfig.debugLvl+1 > lvl)
            Debug.Log("[" + lvl + "] => " + ToolsTime.AbsoluteTime+ " : " + s);
    }

    /// <summary>
    /// Log warning message (Does not break everything but might change the result)
    /// </summary>
    /// <param name="s"> the debug msg </param>
    static public void logWarning(string s)
    {
        if (LoaderConfig.debugLvl > 0) // at least 1
            Debug.LogWarning("[1] => " + ToolsTime.AbsoluteTime + " : " + s);
    }

    /// <summary>
    /// Log error message
    /// </summary>
    /// <param name="s"> the error message </param>
    static public void logError(string s)
    {
        if (LoaderConfig.debugLvl > -1) // at least 0
            Debug.LogError("[0] => " + ToolsTime.AbsoluteTime + " : " + s);
    }

    /// <summary>
    /// Log very bad error message then quit the application (Show messages on Camera in Unity as application does not quit)
    /// </summary>
    /// <param name="s"> the error message </param>
    static public void logFatalError(string s)
    {
        createMessage("ERROR:\n" + s, Color.red);
        ToolsTime.pauseAndResumeGame(true);
        Debug.LogError("[FATAL] => " + ToolsTime.AbsoluteTime + " : " + s);
        ToolsDebug.Quit();
    }

    /// <summary>
    /// Create floating text for error messages in Unity Debug
    /// </summary>
    /// <param name="text"> The message to show </param>
    /// <param name="color"> The color used to write the message </param>
    static private void createMessage(string text, Color color)
    {
        ToolsDebug.logError(text);
        GameObject newText = new GameObject(text.Replace(" ", "-"), typeof(RectTransform));
        TextMesh newTextComp = newText.AddComponent<TextMesh>();
        //newText.AddComponent<CanvasRenderer>();
        //Text newText = transform.gameObject.AddComponent<Text>();
        newTextComp.text = text;
        //newTextComp.font = new Font("Arial");
        newTextComp.fontStyle = FontStyle.Bold;
        newTextComp.color = color;
        newTextComp.anchor = TextAnchor.MiddleCenter;
        newTextComp.alignment = TextAlignment.Center;
        newTextComp.fontSize = 10;
        
        newText.transform.SetParent(Camera.main.transform);
        newText.transform.position = Camera.main.transform.position;
        newText.transform.Translate(Camera.main.transform.forward * 10);
        newText.transform.LookAt(Camera.main.transform);
        newText.transform.Rotate(Camera.main.transform.up, 180);
    }

    static public void Quit(){
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
