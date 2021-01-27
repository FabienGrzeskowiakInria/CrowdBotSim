using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using CrowdMP.Core;

public class LawStanding : ControlLaw
{

    [XmlAttribute]
    public int animationType; // 0 - No animation | 1 - idle | 2 - talking | 3 - applause | 4 - look around

    public ConfigVect LookAt = new ConfigVect();

    private Agent linkedAgent;
    private Vector3 oldPos;


    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = LookAt.vect;

        //float speed = (linkedAgent.transform.position - oldPos).magnitude / deltaTime;

        //if (speed < 0.0001)
        //{
        //    float angle = Vector3.SignedAngle(linkedAgent.transform.forward, LookAt.vect - linkedAgent.transform.position, linkedAgent.transform.up);
        //    rotation = new Vector3(0, Mathf.Min(angle, 180 * deltaTime), 0);
        //}
        //oldPos = linkedAgent.transform.position;
        return true;
    }

    public void initialize(Agent a)
    {
        linkedAgent = a;
        //oldPos = a.transform.position;

        AnimationController ac = a.gameObject.GetComponentInChildren<AnimationController>();
        if(ac != null) ac.setAnimationType(animationType);
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        a.transform.LookAt(rotation);
        return true;
    }
}
