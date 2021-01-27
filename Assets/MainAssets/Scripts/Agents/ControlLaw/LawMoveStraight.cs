using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Law controlling the speed of an agent to keep moving forward
/// </summary>
public class LawMoveStraight : ControlLaw
{
    [XmlAttribute]
    public float speedCurrent;
    [XmlAttribute]
    public float speedDefault;
    [XmlAttribute]
    public float accelerationMax;

    public LawMoveStraight()
    {
        speedCurrent = 1.33f;
        speedDefault = 1.33f;
        accelerationMax = 0.8f;
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="speed">speed of the forward movement</param>
    /// <param name="acceleration">acceleration use to reach the speed</param>
    public LawMoveStraight(float speed, float acceleration)
    {
        speedCurrent = 0;
        speedDefault = speed;
        accelerationMax = acceleration;

    }

    public bool computeGlobalMvt(float deltaTime, out Vector3 translation, out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);
        float newSpeed = speedCurrent;

        /* Cannot control */
        if (speedCurrent < speedDefault)
            newSpeed = Math.Min(speedCurrent + deltaTime * accelerationMax, speedDefault);
        else
            newSpeed = Math.Max(speedCurrent - deltaTime * accelerationMax, speedDefault);
        translation.z = newSpeed * deltaTime;
        speedCurrent = newSpeed;

        return true;
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        //a.Rotate(rotation);
        a.Translate(translation);
        return true;
    }


    public void initialize(Agent a)
    {

    }
}
