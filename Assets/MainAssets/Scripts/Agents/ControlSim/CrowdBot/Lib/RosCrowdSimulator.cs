using System;
using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

namespace crowdbotsim
{
    public class Simulator
    {
        List<CB_Agent> agents_;
        List<ObstWall> walls_;

        float timeStep_ = 0.01f;

        // private float sigDca_ = 0.3f;
        // private float sigDca_ = 0.3f;
        // private float sigTtca_ = 0.8f;
        // float sigDist_ = 0.05f;
        // private float coefficient_ = 10.0f;
        // private float coefficientSpeed_ = 0.01f;
        // private float coefficientDist_ = 1.0f;
        // private float alpha = 0.12f;
        // private float eps = 0.001f;

        private float sigTtca_ = 0.8f;
        private float sigDca_ = 0.3f;
        float sigDist_ = 1.5f;
        private float coefficient_ = 15.0f;
        private float coefficientSpeed_ = 0.0f;
        private float coefficientDist_ = 1.0f;
        private float alpha = 0.1f;
        private float eps = 0.001f;

        public Simulator()
        {        
            agents_ = new List<CB_Agent>();
            walls_ = new List<ObstWall>();
        }

        public int addAgent(Vector3 position, float neighborDist, int maxNeighbors, float timeHorizon, float timeHorizonObst, float radius, float maxSpeed, Vector3 velocity)
        {
            CB_Agent agent = new CB_Agent();
            agent.id_ = agents_.Count;
            agent.maxNeighbors_ = maxNeighbors;
            agent.maxSpeed_ = maxSpeed;
            agent.neighborDist_ = neighborDist;
            agent.setPosition(position);
            agent.radius_ = radius;
            agent.timeHorizon_ = timeHorizon;
            agent.timeHorizonObst_ = timeHorizonObst;
            agent.setVelocity(velocity);
            agents_.Add(agent);

            return agent.id_;
        }
        
        public void Clear()
        {
            agents_.Clear();
        }

        public void setTimeStep(float deltaTime)
        {
            timeStep_ = deltaTime;
        }

        public void doStep()
        {
            Vector3[] new_vels = new Vector3[agents_.Count];
            //Do the magic here
            foreach(CB_Agent a in agents_)
            {
                Vector3 Gradient = new Vector3();
                Vector3 Vnorm = a.getVelocity().normalized;

	            Matrix2x2 R = new Matrix2x2(new Vector3(Vnorm.z, -Vnorm.x), Vnorm);

                float TotalCost = 0;
            	float GradTh = 0;
            	float GradS = 0;
                Vector3 GradDist = Vector3.zero;
                Vector3 GradWalls = Vector3.zero;
    
                //find the neighbors
                List<int> neighbors_id = FindNeighbors(a.id_);

             	int NumAgentsVisible = 0;

                // for each agent of the neighbourhood
                foreach(int n_id in neighbors_id)
                {
                    // Compute relative velocity and relative position
                    Vector3 relPos = getAgentPosition(n_id) - a.getPosition();
                    float radius_diff = agents_[n_id].radius_ + a.radius_;
                    
                    relPos = relPos.magnitude > radius_diff ? relPos - relPos.normalized * radius_diff : relPos.normalized * eps;

                    Vector3 relVelocity = getAgentVelocity(n_id) - a.getVelocity();


                    //Only see agents in front of the direction of motion
                    if (Vector3.Dot(relPos, a.getVelocity()) < 0) {
                        continue;
                    }

                    GradDist += coefficientDist_ * relPos / (2*sigDist_*sigDist_) * Mathf.Exp(-0.5f*(relPos.magnitude/sigDist_)*(relPos.magnitude/sigDist_)) * a.getVelocity().sqrMagnitude;

                    float ttca = 0;
                    float dca = 0;
             		Vector3 vdca = new Vector3(0,0,0);

                    // there is adaptation only if relative velocity is not zero
                    if (relVelocity.sqrMagnitude > eps)
                    {
                        // computing ttc and dca
                        ttca = -Vector3.Dot(relPos, relVelocity) / relVelocity.sqrMagnitude;
                        vdca = (relPos + ttca * relVelocity);
                        dca = vdca.magnitude;
                        if (ttca < 0) continue;

                        // saving the total cost
                        float cost = Cost(ttca,dca);

                        // cost = cost * (coefficient_ / relPos.sqrMagnitude); //simulate num of pixels of an agent in screen
                        cost = cost * (coefficient_ / relPos.magnitude); //simulate num of pixels of an agent in screen
                        TotalCost += cost;
            
                        // computing gradients of the cost regarding the speed and velocity of the main agent
                        Vector3 velRot = new Vector3(a.getVelocity().z,0,-a.getVelocity().x);
                        float gradTtcaAngle = -Vector3.Dot(relPos + 2*ttca*relVelocity,velRot) / (Vector3.Dot(relVelocity,relVelocity));
                        float gradTtcaSpeed = Vector3.Dot(relPos + 2*ttca*relVelocity,a.getVelocity().normalized) / (Vector3.Dot(relVelocity,relVelocity));

                        float gradDcaAngle, gradDcaSpeed;
                        if(Mathf.Abs(dca) > eps){
                            gradDcaAngle = Vector3.Dot(vdca, gradTtcaAngle*relVelocity + ttca*velRot) / dca;
                            gradDcaSpeed = Vector3.Dot(vdca, gradTtcaSpeed*relVelocity - ttca * (a.getVelocity().normalized))/ dca;
                        }
                        else { 
                            gradDcaAngle = 0;
                            gradDcaSpeed = 0;
                        }

                        float gradCSpeed = -cost * ( gradTtcaSpeed * (ttca/(sigTtca_*sigTtca_)) +  gradDcaSpeed * (dca/(sigDca_*sigDca_)));
                        float gradCAngle = -cost * ( gradTtcaAngle * (ttca/(sigTtca_*sigTtca_)) + gradDcaAngle * (dca/(sigDca_*sigDca_)));

                        gradCSpeed += 0.5f*UnityEngine.Random.Range(-1.0f,1.0f);
                        gradCAngle += 1.0f*UnityEngine.Random.Range(-1.0f,1.0f);
                        gradCSpeed *= coefficientSpeed_;
                        gradCAngle *= coefficient_;

                        GradTh += gradCAngle;
                        GradS += gradCSpeed;

                        ++NumAgentsVisible;
                    }
                    // else{
                    //     GradS = coefficientSpeed_*UnityEngine.Random.Range(-1.0f,1.0f);
                    //     GradTh = coefficient_*UnityEngine.Random.Range(-1.0f,1.0f);
                    // }
                }
                foreach(ObstWall wall in walls_)
                {
                    //TODO: better

                    //find the longest part
                    Vector3 L1 = wall.B - wall.A;
                    Vector3 L2 = wall.D - wall.A;

                    Vector3 AB = (L1.magnitude > L2.magnitude) ? L1 : L2;
                    Vector3 AP = a.getPosition() - wall.A;
                    Vector3 AM = Vector3.Dot(AP,AB)*AB.normalized;
                    Vector3 PM = AP - AM;
                    
                    // 100 factor to boost it
                    GradWalls = PM.normalized * Mathf.Exp(-0.5f*(PM.sqrMagnitude))*100.0f;
                }
                if(Mathf.Abs(GradS) > a.getVelocity().magnitude || a.getVelocity().magnitude < eps) GradS = 0;
                // GradTh *= coefficient_;
                // GradS *= coefficient_; 
                // GradS = Mathf.Clamp(GradS,-0.5f,0.5f); 
                // GradS = 0; 
                // GradTh = Mathf.Clamp(GradTh, -0.5f, 0.5f);
        
                // Gradient = -1 * R.product(new Vector3(Mathf.Sin(GradTh), 0, 1 - Mathf.Cos(GradTh))*a.getVelocity().magnitude + new Vector3(0,0,GradS));
                // Gradient = a.getVelocity().normalized - Quaternion.AngleAxis(GradTh, Vector3.up) * a.getVelocity().normalized * (1 + GradS);
                // Gradient = -(a.getVelocity().normalized - Quaternion.AngleAxis(GradTh, Vector3.up) * a.getVelocity().normalized);
                // Gradient = -(a.getVelocity().normalized - Quaternion.AngleAxis(GradTh, Vector3.up) * a.getVelocity().normalized*(1+GradS)) - GradDist - GradWalls;
                Gradient = -(a.getVelocity().normalized - Quaternion.AngleAxis(GradTh, Vector3.up) * a.getVelocity().normalized);
                Gradient -= a.getVelocity()*GradS;
                Gradient -= GradDist;
                if (NumAgentsVisible > 0) {
                    Gradient = Gradient / (float)NumAgentsVisible;
                    TotalCost = TotalCost / (float)NumAgentsVisible;
                }


                Vector3 new_delta_vel = new Vector3();

                //TODO Debug this

                // float CostAngle = -0.5f*Vector3.Angle(a.pref_velocity_, a.velocity_)*Mathf.Exp(-0.5f*( Mathf.Pow(Vector3.Angle(a.pref_velocity_, a.velocity_),2)));
                // float CostSpeed = -0.5f*(a.velocity_.magnitude - a.pref_velocity_.magnitude)*Mathf.Exp(-0.5f*( Mathf.Pow(a.velocity_.magnitude - a.pref_velocity_.magnitude,2)));

                // Vector3 CostMov = new Vector3(CostAngle, 0, CostSpeed);

                // if(a.velocity_.magnitude > 0){
                //     // new_vel = Quaternion.AngleAxis(CostMov.x, Vector3.up) * (a.velocity_.normalized * (a.velocity_.magnitude + CostMov.z));
                //     new_vel = Quaternion.LookRotation(a.pref_velocity_, Vector3.up) * (a.velocity_.normalized * (a.velocity_.magnitude + CostMov.z));
                // }
                // else{
                //     new_vel = Quaternion.LookRotation(a.pref_velocity_, Vector3.up) * (a.pref_velocity_.normalized * CostMov.z);
                // }

                //new_delta_vel = (a.pref_velocity_ - a.velocity_) + Gradient;
                new_delta_vel = alpha * (a.pref_velocity_ - a.getVelocity()) + (1-alpha) * Gradient;
                // if(new_delta_vel.magnitude >= 0.8f)
                // {
                //     new_delta_vel = new_delta_vel.normalized * 0.8f;
                // }
                new_vels[a.id_] = a.getVelocity() + new_delta_vel;
                if(new_vels[a.id_].magnitude > 2.0f){
                    new_vels[a.id_] = new_vels[a.id_].normalized*2.0f;
                }
                // a.setVelocity(new_vels[a.id_]);
                // a.setPosition(a.getPosition() + a.getVelocity() * timeStep_);
                // Debug.DrawLine(a.getPosition(), a.getPosition() + Gradient*10.0f);
            }
            
            foreach(CB_Agent a in agents_)
            {
                a.setVelocity(new_vels[a.id_]);
                a.setPosition(a.getPosition() + a.getVelocity() * timeStep_);
            }
        }

        private struct Matrix2x2 {
            float a11_;
            float a12_;
            float a21_;
            float a22_;
            public Matrix2x2(Vector3 v1, Vector3 v2) {
                a11_ = v1.x;
                a21_ = v1.z;
                a12_ = v2.x;
                a22_ = v2.z;
            }
            public Vector3 product(Vector3 v)
            {
                return new Vector3(a11_*v.x + a12_*v.z, 0, a21_*v.x + a22_*v.z);
            }
        }

        float Cost(float ttca, float dca){
            return Mathf.Exp(-0.5f*( (ttca/sigTtca_)*(ttca/sigTtca_) + (dca/sigDca_)*(dca/sigDca_) ) );
        }

        public Vector3 getAgentPosition(int id)
        {
            return agents_[id].getPosition();
        }
        public Vector3 getAgentVelocity(int id)
        {
            return agents_[id].getVelocity();
        }
        public void setAgentPosition(int id, Vector3 position)
        {
            agents_[id].setPosition(position);

        }
        public void setAgentPrefVelocity(int id, Vector3 goal)
        {
            agents_[id].pref_velocity_ = goal;
        }

        private List<int> FindNeighbors(int a_id)
        {
            //TODO: KDTREE
            
            List<int> n = new List<int>();
            if(agents_[a_id].maxNeighbors_ == 0) return n;
            for(int i = 0; i < agents_.Count; i++)
            {
                if(n.Count >= agents_[a_id].maxNeighbors_) 
                    return n;
                if(i == a_id) 
                    continue;
                if(Vector3.Distance(getAgentPosition(a_id), getAgentPosition(i)) < agents_[a_id].neighborDist_)
                {
                    n.Add(i);                
                }
            }
            return n;
        }

        public void addWall(ObstWall wall)
        {
            walls_.Add(wall);
        }
    }
}
