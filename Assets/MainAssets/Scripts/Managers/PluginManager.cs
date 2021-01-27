using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PluginManager : MonoBehaviour
{

    /// <summary>
    /// Load the parameters and setup everything or deactivate everything if plugin not used
    /// </summary>
    /// <returns>If the plugin has been loaded or deactivated</returns>
    abstract public bool LoadPlugin();

    abstract public void UnloadPlugin();

}
