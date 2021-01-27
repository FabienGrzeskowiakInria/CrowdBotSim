using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

public class RobotGen : MonoBehaviour
{
    [Tooltip("Robot Gameobject")]
    public GameObject robot_go;

    [Tooltip("Perceived radius of the robot during the trial")]
    public float robotRadius = 1.0f;
    [Tooltip("The control law used in the trial")]
    public ControlLawGen lawGen;

    [Tooltip("The control law used in the trial")]
    public ControlSimGen lawSim;

    [Tooltip("Is the robot part of the crowd simulation (aka avoided by the crowd)")]
    public bool in_sim = true;

    public virtual TrialRobot createRobot()
    {
        TrialRegularRobot robot = new TrialRegularRobot();
        if(robot_go == null) return robot;
        robot.mesh = robot_go.name;
        robot.radius = robotRadius;
        robot.Position.vect = gameObject.transform.position;
        robot.Rotation.vect = gameObject.transform.rotation.eulerAngles;

        robot.in_sim = in_sim;

        // ControlLawGen lawGen = gameObject.GetComponent<ControlLawGen>();
        if (lawGen!=null)
            robot.xmlControlLaw = lawGen.createControlLaw();
        else
            robot.xmlControlLaw = new LawFake();

        if (lawSim!=null)
            robot.xmlControlSim = lawSim.createControlSim(0);
        else
            robot.xmlControlSim = null;

        return robot;
    }
}
