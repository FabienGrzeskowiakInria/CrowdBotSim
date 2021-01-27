using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Control Experiment and Trials time (can be modified for multi-platform and time synchronization) 
/// </summary>
public static class ToolsTime {

    private static float deltaTime;
    private static float realDeltaTime;
    private static float absoluteTime;
    private static float trialTime;
    private static float timeSinceTrialStarted;
    private static bool bPause;
    private static bool use_external;

    private static RosSharp.RosBridgeClient.ClockSubscriber ros_clock;
    private static crowdbotsim.TcpClockSubscriber tcp_clock;

    public static bool useExternal() {return use_external; }
    static ToolsTime() {
        realDeltaTime = 0;
        deltaTime = 0;
        absoluteTime = 0;
        trialTime = 0;
        timeSinceTrialStarted = 0;
        bPause = false;

        GameObject clock = GameObject.FindGameObjectWithTag("Clock");

        if(clock != null){
            tcp_clock = clock.GetComponent<crowdbotsim.TcpClockSubscriber>();
            ros_clock = clock.GetComponent<RosSharp.RosBridgeClient.ClockSubscriber>();
        }
        
        use_external = (ros_clock != null) || (tcp_clock != null);
        if(use_external)
        {
            Physics.autoSimulation = false;
        }
        Debug.Log("Use external Clock : " + use_external);
    }

    public static bool  isInPause               { get { return bPause; } }
    public static float AbsoluteTime            { get { return absoluteTime; } }
    public static float TrialTime               { get { return trialTime; } }
    public static float TimeSinceTrialStarted   { get { return timeSinceTrialStarted; } }
    public static float DeltaTime               { get { return deltaTime; } }
    public static float RealDeltaTime           { get { return realDeltaTime; } }



    public static void updateTime()
    {
        if(use_external)
        {
            float dt = realDeltaTime;
            if(tcp_clock != null)
            {
                dt = tcp_clock.DeltaTime;
            }
            else if(ros_clock != null)
            {
                dt = ros_clock.DeltaTime;
            }

            realDeltaTime = dt;

            if(realDeltaTime > 0)
            {
                for(int i = 0; i < realDeltaTime/0.01f; i++)
                    Physics.Simulate(0.01f);
            }
        }
        else 
            realDeltaTime = Time.deltaTime;
        
        timeSinceTrialStarted += realDeltaTime;
        if(use_external)
        {
            if(tcp_clock != null)
            {
                absoluteTime = tcp_clock.Time;
            }
            else if(ros_clock != null)
            {
                absoluteTime = ros_clock.Time;
            }
        }
        else
        {
            absoluteTime += realDeltaTime;
        }

        if (bPause)
            deltaTime = 0;
        else
        {
            trialTime += realDeltaTime;
            deltaTime = realDeltaTime;
        }
    }

    public static void pauseAndResumeGame(bool stop)
    {
        bPause = stop;
    }

    public static bool tooglePause()
    {
        pauseAndResumeGame(!bPause);
        if(bPause) ToolsDebug.log("Pause");
        else ToolsDebug.log("Unpause");
        return bPause;
    }


    public static void newLevel()
    {
        deltaTime = 0;
        trialTime = 0;
        timeSinceTrialStarted = 0;
        bPause = true;
    }

}
