using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Control the animation from LocomotionFinal
/// </summary>
public class AnimationController : MonoBehaviour
{
    
    public Vector3 velocity;
    private Vector3 _oldPosition;
    private Animator _objectAnimator;
    private Agent _agent;
    private uint agentID;
    private Vector3 _oldVelocity;
    private Quaternion _oldRotation;
    private string _oldState = "Idle";
    private float _timeIdle=0;
    public string _defaultIdleAnimation = "Talking";

    public bool IsIdle { get { return _timeIdle > 1.0f; } }

    // Use this for initialization
    void Start()
    {
        if (_objectAnimator != null)
            return;

        _agent = GetComponent<Agent>();
        if(_agent != null) agentID = _agent.GetID();
        else agentID = 0;
        _oldPosition = this.transform.position;
        _objectAnimator = this.GetComponent<Animator>();
        _objectAnimator.applyRootMotion = false;
        _objectAnimator.speed = 0;
        _objectAnimator.SetFloat("Speed", 0);
        _objectAnimator.SetFloat("AngularSpeed", 0);
        _objectAnimator.SetInteger("AnimationType", UnityEngine.Random.Range(1,4));  // 1, 2 or 3

        if(ToolsTime.useExternal()){
            _objectAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _objectAnimator.enabled = false;
        }
    }

    public void temp()
    {
        //idleAnimationID = ((TrialAgent) LoaderConfig.agentsInfo[(int)agentID - 2]).idleAnimationID;
        //_objectAnimator.SetFloat("IdleAnimationID", idleAnimationID);
        //applauseAnimationID = ((TrialMultiControlLawAgent) LoaderConfig.agentsInfo[(int)agentID - 2]).applauseAnimationID;
        //lookAroundAnimationID = ((TrialMultiControlLawAgent) LoaderConfig.agentsInfo[(int)agentID - 2]).lookAroundAnimationID;
        //_objectAnimator.SetFloat("ApplauseAnimationID", applauseAnimationID);
        //_objectAnimator.SetFloat("LookAroundAnimationID", lookAroundAnimationID);
    }

    public void setAnimationType(int type)
    {
        if (_objectAnimator == null)
            Start();
        _objectAnimator.SetInteger("AnimationType", type);
    }

    public void setAnimOffset(float offset)
    {
        if (_objectAnimator == null)
            Start();
        _objectAnimator.SetFloat("CycleOffset", offset);
    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (ToolsTime.DeltaTime != 0)
        {
            _objectAnimator.speed = 1;
            //If object is ready and the time isn't in pause, play the animation as fast as the virtual human speed.
            float animationSpeed;
            Vector3 position = _oldPosition - this.transform.position;
            float angle = Vector3.SignedAngle(_oldVelocity, this.transform.forward, this.transform.up);
            float angularSpeed = angle / ToolsTime.DeltaTime;


            animationSpeed = position.magnitude / ToolsTime.DeltaTime / 1.4f;

            velocity = -1 * position / ToolsTime.DeltaTime;

            if (animationSpeed > 0.01f)
            {
              _timeIdle = 0.0f;
              if (_oldState != "Walk")
              {
                _oldState = "Walk";
              }
            }
            else
            {
              // wait before entering idle state
              _timeIdle += ToolsTime.DeltaTime;
            }
            if (IsIdle)
            {
              animationSpeed = 0.0f;
              if (_oldState != _defaultIdleAnimation)
              {
                // print("Switching to Idle Animation");
                _oldState = _defaultIdleAnimation;
                _objectAnimator.SetInteger("AnimationType", UnityEngine.Random.Range(1,4));  // 1, 2 or 3
                _objectAnimator.Play("Transition");
              }
            }
            _objectAnimator.SetFloat("Speed", animationSpeed);
            _objectAnimator.SetFloat("AngularSpeed", angularSpeed, 100f, ToolsTime.DeltaTime);
            _objectAnimator.SetBool("One-Eighty", false);
            _objectAnimator.SetFloat("TurnAngle", 0);

            if (IsIdle)
            {
                if (Mathf.Abs(angle) >= 45)
                {
                    _objectAnimator.SetBool("One-Eighty", true);
                    _objectAnimator.SetFloat("TurnAngle", angle);
                }
            }



        }
        else
        {
            // if the time is in pause, pause the animation.
            _objectAnimator.speed = 0;
        }

        _oldPosition = this.transform.position;
        _oldRotation = this.transform.rotation;
        _oldVelocity = this.transform.forward;

        if(ToolsTime.useExternal()){
            _objectAnimator.Update(ToolsTime.DeltaTime);
        }
    }
}


