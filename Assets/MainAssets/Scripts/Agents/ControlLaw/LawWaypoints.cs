using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;
using CrowdMP.Core;

/// <summary>
/// Law controlling the speed and orientation of an agent to reach a list of waypoints one after the other
/// </summary>
public class LawWaypoints : ControlLaw
{
    [XmlAttribute]
    public float speedCurrent;
    [XmlAttribute]
    public float speedDefault;
    [XmlAttribute]
    public float accelerationMax;
    [XmlAttribute]
    public float angularSpeed;
    [XmlAttribute]
    public float reachedDist;
    [XmlAttribute]
    public bool isLooping;


    [XmlArray("Waypoints")]
    [XmlArrayItem("Waypoint")]
    public List<ConfigVect> goals;

    private int currGoal=0;
    private Agent linkedAgent;

    public LawWaypoints()
    {
        speedCurrent = 0;
        speedDefault = 1.33f;
        accelerationMax = 0.8f;
        reachedDist = 5.0f;
        angularSpeed = 360*100;
        isLooping = false;
        goals = new List<ConfigVect>();
    }

    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);

        // Check goal
        Vector3 direction;
        if (currGoal >= goals.Count)
        {
            direction = new Vector3(0, 0, 0);
        }
        else
        {
            

            direction = goals[currGoal].vect - linkedAgent.Position;
            direction.y = 0;
            if (direction.magnitude < reachedDist)
                currGoal = isLooping ? (currGoal + 1) % goals.Count : (currGoal + 1);

            direction = direction.magnitude* (Quaternion.RotateTowards(linkedAgent.transform.rotation, Quaternion.LookRotation(direction), angularSpeed * deltaTime)*Vector3.forward);

        }

        float newSpeed= direction.magnitude/ deltaTime;
        if (newSpeed > speedDefault)
            newSpeed = speedDefault;

        /* Cannot control */
        if (speedCurrent < newSpeed)
            newSpeed = Math.Min(speedCurrent + deltaTime * accelerationMax, newSpeed);
        else
            newSpeed = Math.Max(speedCurrent - deltaTime * accelerationMax, newSpeed);



        if (direction.magnitude==0)
        {
            direction = linkedAgent.transform.forward;
        }

        translation.z = newSpeed * deltaTime;

        rotation = direction;
        //Quaternion q = Quaternion.LookRotation(direction);
        //rotation = q.eulerAngles - linkedAgent.transform.rotation.eulerAngles;

        //Quaternion relative = q * Quaternion.Inverse(linkedAgent.transform.rotation);
        //rotation = relative.eulerAngles;

        speedCurrent = newSpeed;

        return true;
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        if (translation.sqrMagnitude != 0)
            a.transform.rotation = Quaternion.LookRotation(rotation);

        a.Translate(translation);
        return true;
    }

    public void initialize(Agent a)
    {
        linkedAgent = a;
    }
}
