#if (UNITY_EDITOR) 
using UnityEngine;
using System.Collections;
using UnityEditor;
namespace CrowdMP.Core
{

    /// <summary>
    /// Editor code to add button to the MainManager script object
    /// </summary>
    [CustomEditor(typeof(Spawn))]
    public class SpawnButtons : Editor
    {

        public override void OnInspectorGUI()
        {
            Spawn myTarget = (Spawn)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Spawn"))
            {
                Undo.RecordObject(target, "Spawn");
                EditorUtility.SetDirty(target);
                myTarget.spawn();
            }
        }
    }

    /// <summary>
    /// Editor code to add button to the MainManager script object
    /// </summary>
    [CustomEditor(typeof(TrialGen))]
    public class TrialGenButtons : Editor
    {

        public override void OnInspectorGUI()
        {
            TrialGen myTarget = (TrialGen)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Generation"))
            {
                myTarget.generate();
            }
            if (GUILayout.Button("SpawnAll"))
            {
                myTarget.spawn();
            }
            if (GUILayout.Button("ClearAll"))
            {
                myTarget.clear();
            }
        }
    }

    [CustomEditor(typeof(AgentGen))]
    public class AgentGenButton : Editor
    {

        public override void OnInspectorGUI()
        {
            AgentGen myTarget = (AgentGen)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Get meshes"))
            {
                Undo.RecordObject(target, "AgentGen");
                EditorUtility.SetDirty(target);
                myTarget.defaultMeshes();
            }
        }
    }
}
#endif
