#if (UNITY_EDITOR) 
using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Editor code to add button to the MainManager script object
/// </summary>
[CustomEditor(typeof(MainManager))]
public class ManagerCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Create Template Config Files"))
        {
            LoaderConfig.CreateDefaultConfigFile();
        }
    }
}
#endif