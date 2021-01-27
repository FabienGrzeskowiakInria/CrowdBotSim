using System;
using UnityEngine;

/// <summary>
/// ControlLaw interface to create classes controlling agent's goals
/// </summary>
public interface ControlLaw
{

    void initialize(Agent a);
    bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation);
    bool applyMvt(Agent a, Vector3 translation, Vector3 rotation);

}
