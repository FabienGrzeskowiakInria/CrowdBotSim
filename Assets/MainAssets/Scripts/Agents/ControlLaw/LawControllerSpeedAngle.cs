using System;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Law controlling speed and rotation of an agent from a device input (Made for player)
/// </summary>
public class LawControllerSpeedAngle : ControlLaw
{
    [XmlAttribute]
    public float speedCurrent;
    [XmlAttribute]
    public float speedDefault;
    [XmlAttribute]
    public float speedVariation;

    [XmlAttribute]
    public float angularSpeed;
    [XmlAttribute]
    public float accelerationMax;
    [XmlAttribute]
    public float timeBeforeControl;

    public LawControllerSpeedAngle()
    {
        speedCurrent = 0.0f;
        speedDefault = 1.33f;
        accelerationMax = 0.8f;
        angularSpeed = 30f;
        timeBeforeControl = 0;
        speedVariation = 0.5f;
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="speed">Speed.</param>
    /// <param name="acceleration">Acceleration.</param>
    /// <param name="angularSpeed">Angular speed.</param>
    /// <param name="timeBeforeControl">Time before control.</param>
    /// <param name="speedOffset">Speed offset.</param>
    public LawControllerSpeedAngle(float speed, float acceleration, float angular_Speed, float timeBC, float speedOffset)
    {
        speedCurrent = 0;

        speedDefault = speed;
        accelerationMax = acceleration;
        angularSpeed = angular_Speed;
        timeBeforeControl = timeBC;

        speedVariation = speedOffset;
    }



	public bool computeGlobalMvt(       float deltaTime,
                                    out Vector3 translation,
                                    out Vector3 rotation)
    {
        translation = new Vector3(0, 0, 0);
        rotation = new Vector3(0, 0, 0);

        float newSpeed = speedCurrent;
        if (timeBeforeControl > 0)
        {
            /* Cannot control */
            if (speedCurrent < speedDefault)
                newSpeed = Math.Min(speedCurrent + deltaTime * accelerationMax, speedDefault);
            else
                newSpeed = Math.Max(speedCurrent - deltaTime * accelerationMax, speedDefault);
            translation.z = newSpeed * deltaTime;
            timeBeforeControl = timeBeforeControl - deltaTime;
        }
        else
        {
            /* Can control */

            float desiredSpeed = ToolsInput.getAxisValue(ToolsAxis.Vertical) * speedVariation + speedDefault;
            if (speedCurrent < desiredSpeed)
                newSpeed = Math.Min(speedCurrent + deltaTime * accelerationMax, desiredSpeed);
            else
                newSpeed = Math.Max(speedCurrent - deltaTime * accelerationMax, desiredSpeed);


            translation.z = newSpeed * deltaTime;
            rotation.y = angularSpeed * deltaTime * ToolsInput.getAxisValue(ToolsAxis.Horizontal);
        }

        speedCurrent = newSpeed;

        return true;
    }

    public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
    {
        a.Rotate(rotation);
        a.Translate(translation);
        return true;
    }

    public void initialize(Agent a)
    {
    }
}
