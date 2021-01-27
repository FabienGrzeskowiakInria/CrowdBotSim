//#define VBM_DEBUG
//#define VBM_SAMPLING


using System.Collections.Generic;
using UnityEngine;
using CrowdMP.Core;

namespace VBM
{
    public class Agent
    {
        // Agent parameters
        public int id_;
        public bool isActive= true;
        public float radius_;

        // Simulation restriction
        public float neighborsAgentDist;
        public float neighborsWallDist;

        // Dutra parameters
        private float sigTtca_ = 1.8f;
        private float sigDca_ = 0.3f;
        private float sigSpeed_ = 3.3f;
        private float sigAngle_ = 2.0f;
        private float eps = 0.001f;

        // Dutra Constant Parameter
        private float maxAcceleration = 0.8f;
        private float maxAngularSpeed = Mathf.PI / 4;

        // Fake vision field parameters
        public float FoV = 150; // Filed of view in degree
        public int Res = 256;   // Resolution of the 2d field of view

        // Sim data
        private Vector2 position_;
        private Vector2 velocity_;
        private Vector2 prefVelocity_;
        private Vector2 dir_;
        private float speed_;
#if VBM_SAMPLING
        private Vector2 OptimalVel_;
#endif


        public PixelPoint[] visionField;


        public Agent()
        {
            id_ = -1;
            radius_ = 0.33f;

            position_ = Vector2.zero;
            velocity_ = Vector2.zero;
            prefVelocity_ = Vector2.zero;
            dir_ = Vector2.right;
#if VBM_SAMPLING
            OptimalVel_ = Vector2.zero;
#endif

            // generate visionField
            float from = Mathf.Deg2Rad * (-FoV / 2);
            float to = Mathf.Deg2Rad * (FoV / 2);
            visionField = new PixelPoint[Res];

            Vector2 forward = new Vector2(1, 0); 
            for (int i=0;i<Res;i++)
            {
                float angle = from + i * FoV / (Res - 1);
                Vector2 v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float crossValue = MathTool.crossProduct(forward, v);
                visionField[i] = new PixelPoint(angle, crossValue);
            }

        }

        public Agent(float sigTtca, float sigDca, float sigSpeed, float sigAngle)
        {
            id_ = -1;
            radius_ = 0.33f;

            position_ = Vector2.zero;
            velocity_ = Vector2.zero;
            dir_ = Vector2.right;

            sigTtca_ = sigTtca;
            sigDca_ = sigDca;
            sigSpeed_ = sigSpeed;
            sigAngle_ = sigAngle;
#if VBM_SAMPLING
            OptimalVel_ = Vector2.zero;
#endif

            // generate visionField
            float from = Mathf.Deg2Rad * (-FoV / 2);
            float to = Mathf.Deg2Rad * (FoV / 2);
            visionField = new PixelPoint[Res];

            Vector2 forward = new Vector2(1, 0);
            for (int i = 0; i < Res; i++)
            {
                float angle = from + i * (to-from) / (Res - 1);
                Vector2 v = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float crossValue = MathTool.crossProduct(forward, v);
                visionField[i] = new PixelPoint(angle, crossValue);
            }

        }

        /// <summary>
        /// Deactivate the agent (not control by the simulation)
        /// </summary>
        public void deactivate()
        {
            isActive = false;
        }

        public void setVelocity(Vector2 v)
        {
            velocity_ = v;// Vector3.Lerp(getVelocity(), new Vector3(v.x, 0, v.z), 0.3f);
            speed_ = velocity_.magnitude;

            if (speed_ > eps)
                dir_ = velocity_.normalized;
        }
        public void setVelocity(Vector2 dir, float norm/*, float timeStep*/)
        {
            speed_ = norm;
            dir_ = dir;
            velocity_ = norm * dir; // Vector3.Lerp( velocity_, norm * dir, timeStep);
#if VBM_DEBUG
            Debug.DrawLine( ToolsGeneral.convert(position_) + Vector3.up, ToolsGeneral.convert(position_ + dir_* norm)+ Vector3.up, Color.green, 0.3f, false);     
#endif
        }
        public void setPosition(Vector2 v)
        {
            position_ = v;
        }
        public void setPrefVelocity(Vector2 v)
        {
            prefVelocity_ = v;
        }

        public Vector2 getPosition() { return position_; }
        public Vector2 getVelocity() { return velocity_; }
        public Vector2 getPrefVelocity() { return prefVelocity_; }

        /// <summary>
        /// Empty the fake vision field
        /// </summary>
        public void resetVision()
        {
            foreach (PixelPoint p in visionField)
            {
                p.seenObject = null;
            }
        }

        /// <summary>
        /// Check if part of the wall has to been insert in the fake vision field
        /// </summary>
        /// <param name="w">Wall</param>
        public void insertWallNeighbor(Wall w)
        {
            if (!isActive)
                return;

            // Find closest point
            int smallestPoint = 0;
            float smallestDistSq = (w.getPoint(smallestPoint) - position_).SqrMagnitude();
            for (int i=1; i<4; ++i)
            {
                float tmpDistSq= (w.getPoint(i) - position_).SqrMagnitude();
                if (tmpDistSq< smallestDistSq)
                {
                    smallestDistSq = tmpDistSq;
                    smallestPoint = i;
                }
            }

            // Check first side
            Vector2 cp = w.getPoint(smallestPoint)-position_;
            Vector2 op = w.getPoint(smallestPoint+1)-position_;
            if (Mathf.Abs(Vector2.Dot(w.getNormal(smallestPoint), cp)) < neighborsWallDist* neighborsWallDist) // Is wall side close enough
                checkWallSide(w, cp, op);
            // Check second side
            op = w.getPoint(smallestPoint - 1) - position_;
            if (Mathf.Abs(Vector2.Dot(w.getNormal(smallestPoint-1), cp)) < neighborsWallDist* neighborsWallDist) // Is wall side close enough
                checkWallSide(w, cp, op);


        }

        /// <summary>
        /// Insert one side of a wall in the fake vision fields when necessary
        /// </summary>
        /// <param name="w">the whole wall</param>
        /// <param name="cp">Relative position of the Closest point of the wall side</param>
        /// <param name="op">Relative position of the Other point of the wall side</param>
        private void checkWallSide(Wall w, Vector2 cp, Vector2 op)
        {
            float dotProd1 = Vector2.Dot(dir_, cp);
            float dotProd2 = Vector2.Dot(dir_, op);


            // at least one point is visible
            if (dotProd1 > 0 || dotProd2 > 0)
            {
                float crossProdC;
                float crossProdO;

                if (dotProd1 < 0) // Closest point behind
                {
                    Vector2 tmpV = cp - op;
                    crossProdC = Mathf.Sign(MathTool.crossProduct(op, tmpV));
                    Vector2 v = op.normalized;
                    crossProdO = MathTool.crossProduct(dir_, v);
                }
                else if (dotProd2 < 0) // Other point behind
                {
                    Vector2 tmpV = op - cp;
                    crossProdO = Mathf.Sign(MathTool.crossProduct(cp, tmpV));
                    Vector2 v = cp.normalized;
                    crossProdC = MathTool.crossProduct(dir_, v);
                }
                else // Both point in front
                {
                    Vector2 v = cp.normalized;
                    crossProdC = MathTool.crossProduct(dir_, v);
                    v = op.normalized;
                    crossProdO = MathTool.crossProduct(dir_, v);
                }

                float minProd = Mathf.Min(crossProdC, crossProdO);
                float maxProd = Mathf.Max(crossProdC, crossProdO);

                foreach (PixelPoint p in visionField)
                {
                    if (p.crossValue >= maxProd) // no longer in vision field
                        break;
                    if (p.crossValue < minProd) // not yet in vision field
                        continue;

                    // The Wall is visible, get the intersection between the wall and the vision ray
                    float currentAngle = Mathf.Deg2Rad * Vector2.Angle(new Vector2(1, 0), dir_);
                    if (dir_.y < 0)
                    {
                        currentAngle = -currentAngle;
                    }
                    currentAngle = currentAngle + p.angle;
                    Vector2 visionRay = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
                    bool found;
                    Vector2 pointOnWall = MathTool.Intersection(Vector2.zero, visionRay, cp, op, out found);

                    PixelObject newPO = new PixelObject(w, pointOnWall+position_, pointOnWall.sqrMagnitude);
                    p.addObject(newPO);
                }
            }
        }

        /// <summary>
        /// Check if the agent has to been insert in the fake vision field
        /// </summary>
        /// <param name="a">An agent</param>
        public void insertAgentNeighbor(Agent a)
        {
            if (!isActive)
                return;

            Vector2 otherPos = a.position_ - position_;

            if (Vector2.Dot(otherPos, dir_) < 0 || otherPos.sqrMagnitude > neighborsAgentDist*neighborsAgentDist) // Agent is behind or too far
                return;

            Vector2 ortho = Vector2.Perpendicular(otherPos).normalized;
            Vector2 oP1 = otherPos + ortho * (a.radius_);
            Vector2 oP2 = otherPos - ortho * (a.radius_);
            float sideDist = oP1.magnitude;
            oP1 = oP1 / sideDist;
            oP2 = oP2 / sideDist;

            float minProd = MathTool.crossProduct(dir_, oP1);
            float maxProd = minProd;
            float tmp= MathTool.crossProduct(dir_, oP2);
            if (tmp > maxProd)
                maxProd = tmp;
            else
                minProd = tmp;

            foreach (PixelPoint p in visionField)
            {
                if (p.crossValue >= maxProd) // no longer in vision field
                    break;
                if (p.crossValue < minProd) // not yet in vision field
                    continue;

                // The Wall is visible, get the intersection between the wall and the vision ray
                PixelObject newPO = new PixelObject(a, otherPos.sqrMagnitude);
                p.addObject(newPO);
            }
        }

#if VBM_SAMPLING
        private void adaptiveSampling(float timeStep)
        {
            Vector2[] samplePos = new Vector2[48];
            int samplePosCount = 0;

            // Sample stop
            samplePos[samplePosCount] = Vector2.zero;
            samplePosCount++;

            // Sample at the prefVel and around
            samplePos[samplePosCount] = prefVelocity_;
            samplePosCount++;
            //{
            //    Vector2 fw = prefVelocity_ * 0.5f;
            //    Vector2 rw = new Vector2(fw.y, -fw.x);

            //    const int Steps = 8;
            //    for (int i = 0; i < Steps; i++)
            //    {
            //        samplePos[samplePosCount] = fw + rw * Mathf.Sin(i * Mathf.PI * 2 / Steps) + fw * (1 + Mathf.Cos(i * Mathf.PI * 2 / Steps));
            //        samplePosCount++;
            //    }

            //    for (int i = 0; i < Steps; i++)
            //    {
            //        samplePos[samplePosCount] = prefVelocity_ + Random.insideUnitCircle*0.5f;
            //        samplePosCount++;
            //    }
            //}

            // Sample at the previous optimal velocity and around
            samplePos[samplePosCount] = OptimalVel_;
            samplePosCount++;
            //{
            //    Vector2 fw = OptimalVel_ * 0.5f;
            //    Vector2 rw = new Vector2(fw.y, -fw.x);

            //    const int Steps = 8;
            //    for (int i = 0; i < Steps; i++)
            //    {
            //        samplePos[samplePosCount] = fw + rw * Mathf.Sin(i * Mathf.PI * 2 / Steps) + fw * (1 + Mathf.Cos(i * Mathf.PI * 2 / Steps));
            //        samplePosCount++;
            //    }

            //    for (int i = 0; i < Steps; i++)
            //    {
            //        samplePos[samplePosCount] = OptimalVel_ + Random.insideUnitCircle * 0.5f;
            //        samplePosCount++;
            //    }
            //}

            {
                const int Steps = 45; // 48 - 3 - 4 * 8
                for (int i = 0; i < Steps; i++)
                {
                    samplePos[samplePosCount] = Random.insideUnitCircle * 2.0f;
                    samplePosCount++;
                }
            }

            int numBest = 3;
            Vector2[] bestV = new Vector2[numBest];
            float[] bestC = new float[numBest];
            int[] bestL = new int[numBest];

            for (int i = 0; i < numBest; i++)
            {
                bestC[i] = float.PositiveInfinity;
                bestV[i] = OptimalVel_;
                bestL[i] = 0;
            }

            for (int adapt=0; adapt<3; adapt++)
            {

                foreach (Vector2 speedTest in samplePos)
                {
                    float totCost = 0;
                    int numObstacles = 0;
                    // Compute Obstacles Cost
                    foreach (PixelPoint p in visionField)
                    {
                        if (p.seenObject == null)
                            continue;

                        // Relative motion
                        Vector2 relPos = p.seenObject.position_ - getPosition();
                        Vector2 relVelocity = p.seenObject.velocity_ - speedTest;

                        if (relVelocity.sqrMagnitude < eps) // no adaptation if relative speed is zero
                            continue;

                        // Compute ttca
                        float ttca = -Vector2.Dot(relPos, relVelocity) / relVelocity.sqrMagnitude;
                        if (ttca < 0 || ttca > 10)
                            continue;

                        // Compute dca
                        Vector2 caRelPos = (relPos + ttca * relVelocity);
                        float dca = caRelPos.magnitude;
                        if (dca < eps)
                            dca = eps;

                        // Compute grad
                        float C = Cost(ttca, dca);

                        if (C>eps)
                        {
                            totCost += C;
                            numObstacles++;
                        }
                    }
                    if (numObstacles>0)
                        totCost /= numObstacles;

                    float angle = Mathf.Deg2Rad * Vector2.SignedAngle(speedTest, getPrefVelocity());
                    float costAngle = Mathf.Exp(-0.5f * Mathf.Sqrt(angle / sigAngle_));
                    float deltaS = speedTest.magnitude - prefVelocity_.magnitude;
                    float costSpeed = Mathf.Exp(-0.5f * Mathf.Sqrt(deltaS / sigSpeed_));

                    totCost += 1 - (costAngle + costSpeed) / 2;

                    float currCost = totCost;
                    Vector2 currSpeed = speedTest;
                    int currLoop = adapt;
                    for (int i = 0; i < numBest; i++)
                    {
                        if (currCost<bestC[i])
                        {
                            float tmpC = bestC[i];
                            Vector2 tmpV = bestV[i];
                            int tmpL = bestL[i];

                            bestC[i] = currCost;
                            bestV[i] = currSpeed;
                            bestL[i] = currLoop;

                            currCost = tmpC;
                            currSpeed = tmpV;
                            currLoop = tmpL;
                        }
                    }

                }

                // Adapt the sample around the best results
                samplePosCount = 0;
                for (int j = 0; j < numBest; j++)
                {
                    {
                        Vector2 fw = bestV[j] * 0.5f;
                        Vector2 rw = new Vector2(fw.y, -fw.x);

                        if (bestL[j] == adapt)
                        {
                            const int Steps = 8;
                            for (int i = 0; i < Steps; i++)
                            {
                                samplePos[samplePosCount] = fw + rw * Mathf.Sin(i * Mathf.PI * 2 / Steps) + fw * (1 + Mathf.Cos(i * Mathf.PI * 2 / Steps));
                                samplePosCount++;
                            }

                            for (int i = 0; i < Steps; i++)
                            {
                                samplePos[samplePosCount] = bestV[j] + Random.insideUnitCircle * 0.5f;
                                samplePosCount++;
                            }
                        }
                        else
                        {
                            const int Steps = 8 * 2;
                            for (int i = 0; i < Steps; i++)
                            {
                                samplePos[samplePosCount] = bestV[j] + Random.insideUnitCircle;
                                samplePosCount++;
                            }
                        }
                    }
                }
            }

            OptimalVel_ = bestV[0];
            float speedDiff = speed_ - OptimalVel_.magnitude;
            float angleDiff = Mathf.Deg2Rad * Vector2.SignedAngle(dir_, OptimalVel_);

            if (speedDiff < -maxAcceleration * timeStep)
                speedDiff = -maxAcceleration * timeStep;


            if (angleDiff > maxAngularSpeed * timeStep / Mathf.Max(eps, speed_))
                angleDiff = maxAngularSpeed * timeStep / Mathf.Max(eps, speed_);
            else if (angleDiff < -maxAngularSpeed * timeStep / Mathf.Max(eps, speed_))
                angleDiff = -maxAngularSpeed * timeStep / Mathf.Max(eps, speed_);


            //Vector2 debugVel = velocity_;
            // Change velocity
            setVelocity(MathTool.RotateV2(dir_, angleDiff), Mathf.Max(0, speed_ - speedDiff));

        }
#else
        private void gradientDecent(float timeStep)
        {
            float gradCtotSpeed = 0;
            float gradCtotAngle = 0;
            int obstCount = 0;

            int rightPixelsCount = 0; // Dutra code, no idea why
            float rightdCdT = 0; // Dutra code, no idea why

#if VBM_DEBUG
            debugObstacles debugObsts = new debugObstacles();
            int pixelID = -1;
#endif
            foreach (PixelPoint p in visionField)
            {
#if VBM_DEBUG
                pixelID++;
#endif
                if (p.seenObject == null)
                    continue;

                // Relative motion
                Vector2 relPos = p.seenObject.position_ - getPosition();
                Vector2 relVelocity = p.seenObject.velocity_ - velocity_;

                if (relVelocity.sqrMagnitude < eps) // no adaptation if relative speed is zero
                    continue;

                // Compute ttca
                float ttca = -Vector2.Dot(relPos, relVelocity) / relVelocity.sqrMagnitude;
                if (ttca < 0 || ttca > 10)
                    continue;

                // Compute dca
                Vector2 caRelPos = (relPos + ttca * relVelocity);
                float dca = caRelPos.magnitude;
                
                // Compute grad
                float C = Cost(ttca, dca);


                Vector2 velOrtho = new Vector2(velocity_.y, -velocity_.x);
                float gradTtcaAngle = -Vector2.Dot(relPos + 2 * ttca * relVelocity, velOrtho) / Vector2.Dot(relVelocity, relVelocity);// (Mathf.Max(0.01f,Vector2.Dot(relVelocity, relVelocity)));
                float gradTtcaSpeed = Vector2.Dot(relPos + 2 * ttca * relVelocity, dir_) / Vector2.Dot(relVelocity, relVelocity);// (Mathf.Max(0.01f, Vector2.Dot(relVelocity, relVelocity)));

                float gradDcaAngle;
                float gradDcaSpeed;
                if (dca < eps)
                {
                    gradDcaAngle = Vector2.Dot(caRelPos, gradTtcaAngle * relVelocity + ttca * velOrtho) / eps;
                    gradDcaSpeed = Vector2.Dot(caRelPos, gradTtcaSpeed * relVelocity - ttca * dir_) / eps;

                }
                else
                {
                    gradDcaAngle = Vector2.Dot(caRelPos, gradTtcaAngle * relVelocity + ttca * velOrtho) / dca;
                    gradDcaSpeed = Vector2.Dot(caRelPos, gradTtcaSpeed * relVelocity - ttca * dir_) / dca;

                }


                float gradCSpeed = -C * (gradTtcaSpeed * (ttca / (sigTtca_ * sigTtca_)) + gradDcaSpeed * (dca / (sigDca_ * sigDca_)));
                float gradCAngle = -C * (gradTtcaAngle * (ttca / (sigTtca_ * sigTtca_)) + gradDcaAngle * (dca / (sigDca_ * sigDca_)));



                // Add grad
                if (Mathf.Abs(gradCSpeed) > eps && Mathf.Abs(gradCAngle) > eps) // Dutra code, no idea why
                {
                    gradCtotSpeed += gradCSpeed;
                    gradCtotAngle += gradCAngle;
                    obstCount++;

                    if (gradCAngle >= 0) // Dutra code, Cultural Preference to go right?
                    {
                        rightPixelsCount++;
                        rightdCdT += gradCAngle;
                    }
#if VBM_DEBUG
                    debugObst o = new debugObst();
                    o.id_ = p.seenObject.id_;
                    o.isAgent_ = p.seenObject.isAgent_;

                    o.numPixel = 1;
                    o.pixels = new List<int>();
                    o.pixels.Add(pixelID);

                    o.gradTtcaAngle = new List<float>();
                    o.gradTtcaAngle.Add(gradTtcaAngle);
                    o.gradTtcaSpeed = new List<float>();
                    o.gradTtcaSpeed.Add(gradTtcaSpeed);
                    o.gradDcaAngle = new List<float>();
                    o.gradDcaAngle.Add(gradDcaAngle);
                    o.gradDcaSpeed = new List<float>();
                    o.gradDcaSpeed.Add(gradDcaSpeed);

                    o.gradCSpeed = gradCSpeed;
                    o.gradCAngle = gradCAngle;

                    debugObsts.insertObst(o);
#endif

                }
            }


            {
                if (Mathf.Abs(gradCtotAngle) < 1e-1f && Mathf.Abs(rightdCdT) > 1e-2f) // Dutra code, Cultural Preference to go right?
                {
                    float noise = 75e-3f + UnityEngine.Random.Range(0.0f, 1.0f) * 5e-2f;
                    gradCtotAngle += Mathf.Sign(gradCtotAngle) * noise;
                }

                if (obstCount > 0)
                {
                    gradCtotSpeed /= obstCount;
                    gradCtotAngle /= obstCount;
                }
            }

            // Movement grad
            float factor = Mathf.Max(1, 1 / Mathf.Max(eps, speed_)); // Balance the huge value of ttca grad when speed is low
            float deltaS = speed_ - prefVelocity_.magnitude;
            float gradMSpeed = factor * deltaS / (2 * sigSpeed_ * sigSpeed_) * Mathf.Exp(-deltaS * deltaS / (2 * sigSpeed_ * sigSpeed_));

            float angle = Mathf.Deg2Rad * Vector2.SignedAngle(dir_, getPrefVelocity());
            float gradMAngle = -factor * angle / (2 * sigAngle_ * sigAngle_) * Mathf.Exp(-angle * angle / (2 * sigAngle_ * sigAngle_));

            // Sum up grads
            float gradSpeed = timeStep * (gradMSpeed + gradCtotSpeed);
            float gradAngle = timeStep * (gradMAngle + gradCtotAngle);
            //if (id_==5)
            //{
            //    gradSpeed = gradMSpeed;
            //    gradAngle = gradMAngle;
            //}

            //if (gradSpeed > maxAcceleration * timeStep)
            //    gradSpeed = maxAcceleration * timeStep;
            //else 
            if (gradSpeed < -maxAcceleration * timeStep)
                gradSpeed = -maxAcceleration * timeStep;


            if (gradAngle > maxAngularSpeed * timeStep / Mathf.Max(eps, speed_))
                gradAngle = maxAngularSpeed * timeStep / Mathf.Max(eps, speed_);
            else if (gradAngle < -maxAngularSpeed * timeStep / Mathf.Max(eps, speed_))
                gradAngle = -maxAngularSpeed * timeStep / Mathf.Max(eps, speed_);


            //Vector2 debugVel = velocity_;
            // Change velocity
            setVelocity(MathTool.RotateV2(dir_, -gradAngle), Mathf.Max(0, speed_ - gradSpeed)/*,timeStep*/);
        }
#endif

        /// <summary>
        /// Compute the speed according to the object in the fake vision field
        /// </summary>
        public void updateSpeed(float timeStep)
        {
            if (!isActive)
                return;

#if VBM_SAMPLING
            adaptiveSampling(timeStep);
#else
            gradientDecent(timeStep);
#endif


        }

        /// <summary>
        /// Update the position of the agent
        /// </summary>
        /// <param name="timeStep">The step size in seconds</param>
        public void updatePosition(float timeStep)
        {
            position_ = position_ + velocity_ * timeStep;
        }

        /// <summary>
        /// Compute the cost of a pixel from ttca and dca
        /// </summary>
        /// <param name="ttca">Time To Closest Approach</param>
        /// <param name="dca">Distance To Closest Approach</param>
        /// <returns></returns>
        float Cost(float ttca, float dca)
        {
            return Mathf.Exp(-0.5f * ((ttca / sigTtca_) * (ttca / sigTtca_) + (dca / sigDca_) * (dca / sigDca_)));
        }

    }

    /// <summary>
    /// Object contained by the fake vision fields
    /// </summary>
    public class PixelObject
    {
        public int id_;
        public bool isAgent_;
        public float distSq_;
        public float radius;
        public Vector2 position_;
        public Vector2 velocity_;

        public PixelObject(Wall w, Vector2 intersect, float distSq)
        {
            id_ = w.id;
            velocity_ = Vector2.zero;

            position_ = intersect;
            radius = 0;
            
            distSq_ = distSq;

            isAgent_ = false;
        }

        public PixelObject(Agent a, float distSq)
        {
            id_ = a.id_;
            position_ = a.getPosition();
            velocity_ = a.getVelocity();
            radius = a.radius_;

            distSq_ = distSq;

            isAgent_ = true;
        }
    }

    /// <summary>
    /// Pixel for the fake vision field
    /// </summary>
    public class PixelPoint
    {
        public float angle;
        public float crossValue;
        public PixelObject seenObject;

        public PixelPoint(float radAngle, float cross)
        {
            angle = radAngle;
            crossValue = cross;
            seenObject = null;
        }

        public void addObject(PixelObject obj)
        {
            if (seenObject == null)
                seenObject = obj;
            else if (obj.distSq_<seenObject.distSq_)
            {
                seenObject = obj;
            }
        }
    }

#if VBM_DEBUG
    public class debugObst
    {
        public int id_;
        public bool isAgent_;

        public Agent a;
        public Wall w;

        public int numPixel;
        public List<int> pixels;

        public List<float> gradTtcaAngle;
        public List<float> gradTtcaSpeed;
        public List<float> gradDcaAngle;
        public List<float> gradDcaSpeed;

        public float gradCSpeed;
        public float gradCAngle;

    }

    public class debugObstacles
    {
        public List<debugObst> obsts;

        public debugObstacles()
        {
            obsts = new List<debugObst>();
        }

        public void insertObst(debugObst obst)
        {
            foreach (debugObst o in obsts)
            {
                if (o.id_ == obst.id_ && o.isAgent_==obst.isAgent_)
                {
                    o.numPixel++;
                    o.pixels.Add(obst.pixels[0]);
                    o.gradTtcaAngle.Add(obst.gradTtcaAngle[0]);
                    o.gradTtcaSpeed.Add(obst.gradTtcaSpeed[0]);
                    o.gradDcaAngle.Add(obst.gradDcaAngle[0]);
                    o.gradDcaSpeed.Add(obst.gradDcaSpeed[0]);

                    o.gradCSpeed += obst.gradCSpeed;
                    o.gradCAngle += obst.gradCAngle;

                    return;
                }
            }

            obsts.Add(obst);
        }
    }
#endif
}

