using System;
using System.Collections.Generic;
using UnityEngine;

namespace crowdbotsim
{
    public class CB_Agent
    {
        public int id_;
        public int maxNeighbors_;
        public float maxSpeed_;
        public float neighborDist_;
        private Vector3 position_;
        public float radius_;
        public float timeHorizon_;
        public float timeHorizonObst_;
        private Vector3 velocity_;
        public Vector3 pref_velocity_;

        public CB_Agent()
        {
            id_ = -1;
            maxNeighbors_ = 10;
            maxSpeed_ = 2.0f;
            neighborDist_ = 10.0f;
            position_ = Vector3.zero;
            radius_ = 0.3f;
            timeHorizon_ = 10.0f;
            timeHorizonObst_ = 10.0f;
            velocity_ = Vector3.zero;
            pref_velocity_ = Vector3.zero;
        }

        public void setVelocity(Vector3 v)
        { 
            velocity_ = Vector3.Lerp(getVelocity(),new Vector3(v.x,0,v.z),0.3f);
        }
        public void setPosition(Vector3 v)
        { 
            position_ = new Vector3(v.x,0,v.z);
        }

        public Vector3 getPosition(){ return new Vector3(position_.x, 0, position_.z); }
        public Vector3 getVelocity()
        { 
            Vector3 vel = new Vector3(velocity_.x, 0, velocity_.z);
            return vel;
        }
    }
}