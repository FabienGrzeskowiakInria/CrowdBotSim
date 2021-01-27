using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;
using CrowdMP.Core;

/// <summary>
/// Law controlling the speed and orientation of an agent to reach a list of goals using unity nav mesh to navigate to the goal
/// </summary>
public class LawGoals : ControlLaw
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


    [XmlArray("Goals")]
    [XmlArrayItem("Goal")]
    public List<ConfigVect> goals;

    private Agent linkedAgent;
    private NavMeshAgent nav;
    private int currGoal = 0;
    private Vector3 oldPos;
    private Vector3 Noise;
    private NavMeshPath firstPath;
    private float approxRemainingDist;

    public LawGoals()
    {
        speedCurrent = 0;
        speedDefault = 1.33f;
        accelerationMax = 0.8f;
        reachedDist = 0.5f;
        angularSpeed = 360 * 100;
        isLooping = false;
        goals = new List<ConfigVect>();
        linkedAgent = null;
        approxRemainingDist = 0;
    }

    public void initialize(Agent a)
    {
        nav = a.gameObject.AddComponent<NavMeshAgent>();

        NavMeshHit hit = new NavMeshHit();

        foreach (ConfigVect g in goals)
        {
            NavMesh.SamplePosition(g.vect, out hit, 10, 1);
            g.vect = hit.position;
            //Debug.Log(hit.position);
        }

        NavMesh.SamplePosition(a.transform.position, out hit, 10, 1);
        nav.Warp(hit.position);
        a.transform.position = hit.position;
        oldPos = hit.position;
        //nav.Move(new Vector3(0, -1, 0));
        nav.updatePosition = false;
        nav.updateRotation = false;

        nav.autoBraking = false;
        nav.autoRepath = true;
        nav.autoTraverseOffMeshLink = true;

        nav.radius = 0.33f;
        nav.acceleration = accelerationMax;
        nav.angularSpeed = angularSpeed;
        nav.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        nav.speed = speedDefault;
        approxRemainingDist = 0;

        linkedAgent = a;
        //nextGoal();
        //nav.velocity = (nav.steeringTarget - nav.transform.position).normalized * speedCurrent;
        Noise = new Vector3(UnityEngine.Random.value * 5, UnityEngine.Random.value * 0, UnityEngine.Random.value * 5);

        firstPath = new NavMeshPath();
        NavMesh.CalculatePath(hit.position, goals[currGoal].vect, NavMesh.AllAreas, firstPath);
        currGoal = isLooping ? (currGoal + 1) % goals.Count : (currGoal + 1);
        if (!isLooping && goals.Count == currGoal)
            nav.autoBraking = true;
    }

    void nextGoal()
    {
        if (firstPath != null)
        {
            nav.SetPath(firstPath);
            firstPath = null;
        }
        if (nav.pathPending == true)
            return;
        if (goals.Count > currGoal)
        {

            //Debug.Log("SetDest => " + goals[currGoal].vect);
            //if ()
            //    Debug.Log("Destination set");
            //else
            //    Debug.Log("Destination Not Set");
            nav.SetDestination(goals[currGoal].vect);

            currGoal = isLooping ? (currGoal + 1) % goals.Count : (currGoal + 1);


            if (!isLooping && goals.Count == currGoal)
                nav.autoBraking = true;
        }
    }

    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);
        nav.Move(linkedAgent.transform.position - oldPos);
        oldPos = linkedAgent.transform.position;

        // Check goal
        if (float.IsInfinity(approxRemainingDist) || approxRemainingDist < reachedDist)
        {
            approxRemainingDist = nav.remainingDistance;
            if (approxRemainingDist < reachedDist)
                nextGoal();
        }



        Vector3 direction = nav.nextPosition - linkedAgent.Position;
        direction.y = 0;


        float newSpeed = direction.magnitude / deltaTime;
        if (newSpeed > speedDefault)
            newSpeed = speedDefault;

        /* Cannot control */
        if (speedCurrent < newSpeed)
            newSpeed = Math.Min(speedCurrent + deltaTime * accelerationMax, newSpeed);
        else
            newSpeed = Math.Max(speedCurrent - deltaTime * accelerationMax, newSpeed);



        //if (direction.magnitude == 0)
        //{
        //    direction = linkedAgent.transform.forward;
        //}

        translation.z = newSpeed * deltaTime;
        approxRemainingDist -= translation.z;

        rotation = direction;
        //Quaternion q = Quaternion.LookRotation(direction);
        //rotation = q.eulerAngles - linkedAgent.transform.rotation.eulerAngles;

        speedCurrent = newSpeed;


        nav.nextPosition = oldPos;
        return true;
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        if (rotation.sqrMagnitude!=0)
            a.transform.rotation = Quaternion.LookRotation(rotation);

        a.Translate(translation);
        return true;
    }

}
