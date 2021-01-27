using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class LawFake : ControlLaw
{
    private Agent linkedAgent;

    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0 ,0);
        return true;
    }

    public void initialize(Agent a)
    {
        linkedAgent = a;
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        return true;
    }
}
