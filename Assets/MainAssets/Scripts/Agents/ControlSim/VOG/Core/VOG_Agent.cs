/*
 * VOG : Velocity Obstacles & Groups
 * Crowd Simulation mixing Velocity Constraints from RVO with Group Constraints
 * 
 * This simulator is from the paper : 
 * Group Modeling: a Unified Velocity-based Approach
 * By Z. Ren, P. Charalambous, J. Bruneau, Q. Peng1and and J. Pettré 
 *
 * The current code is a modified version of the original code from Z. Ren
 * for a better integration with CrowdMP architecture and needs
 */
#define WEIGHTSORT

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VOG
{

    public class Agent : IAgent
    {
        /// <summary>
        /// FORGROUP////////////////////////////////////////////
        /// </summary>
        //should be hiden or removed
        //   public float persuit = 0f;
        //     public float cooperation = 1f;
        //    public float dampingDist = 2f;
        //
        public float zeroPenalty = 5f;
        public int groupNeighbourNum = 2;
        public float neighbourRadius = 1f;
        public Agent leaderAgent;
        public float followDist = float.PositiveInfinity;
        public float groupNeighbourDist = 80f;
        public int agentID = -1;
        public float weightForDesiredVel = 0.45f;
        public float weightForGroup = 0.5f;
        public float groupHorizonTime = 2f;

        public float curGroupRadius;
        public float relaxRadiusStep = 5f;
        public float weightTuner = 0.9f;
        public float weightTunerChangeStep = 0.02f;
        //large group constraints
        public bool largeGroupConstraint = false;
        public Vector3 groupCenter;
        public Vector3 groupVel;
        public float largeGroupR = 0f;

        public float maxExceedingV = 0f;

        //for statistic
        public float averageDist = 0f;

        //for analysis
        public float unitTime = 0.1f;
        public bool forAnalysis = false;


        //Test ,dynamic vo radius
        public bool isDVOR = false;
        public float dvorRangefactor = 2f;
        public float dvotRangefactor = 4f;
        public float dvoLongPerceptionRadius = 100;
        public int dvoNeighborNum = 20;
        bool longPerceptionStep = false;


        //for using dist constraint to reach the Goal
        public bool isDistGoal = false;
        public bool formDistGoal = false;
        public Vector3 goalPos = new Vector3();
        public Vector3 goalVel = new Vector3();
        public float minGoalR = 5f;
        public float maxGoalR = 10f;
        public float reachGoalTime = 10f;

        public float timeSinceChanged;



        Vector3 smoothPos;

        public Vector3 Position
        {
            get;
            private set;
        }

        public Vector3 InterpolatedPosition
        {
            get { return smoothPos; }
        }

        public Vector3 DesiredVelocity { get; set; }

        public void Teleport(Vector3 pos)
        {
            Position = pos;
            smoothPos = pos;
            prevSmoothPos = pos;
        }

        public void SetYPosition(float yCoordinate)
        {
            Position = new Vector3(Position.x, yCoordinate, Position.z);
            smoothPos.y = yCoordinate;
            prevSmoothPos.y = yCoordinate;
        }

        //Current values for double buffer calculation

        public float radius, height, maxSpeed, neighbourDist, agentTimeHorizon, obstacleTimeHorizon, weight;
        public int groupID;
        public bool locked = false;

        VOGLayer layer, collidesWith;

        public int maxNeighbours;
        public Vector3 position, desiredVelocity, prevSmoothPos;

        public VOGLayer Layer { get; set; }
        public VOGLayer CollidesWith { get; set; }

        public bool Locked { get; set; }
        public float Radius { get; set; }
        public float Height { get; set; }
        public float MaxSpeed { get; set; }
        public float NeighbourDist { get; set; }
        public float AgentTimeHorizon { get; set; }
        public float ObstacleTimeHorizon { get; set; }
        public Vector3 Velocity { get; set; }
        public bool DebugDraw { get; set; }

        public int MaxNeighbours { get; set; }

        /** Used internally for a linked list */
        internal Agent next;

        private Vector3 velocity;
        internal Vector3 newVelocity;

        /** Simulator which handles this agent.
         * Used by this script as a reference and to prevent
         * adding this agent to multiple simulations.
         */
        public Simulator simulator;

        public List<Agent> neighbours = new List<Agent>();
        public List<float> neighbourDists = new List<float>();
        List<ObstacleVertex> obstaclesBuffered = new List<ObstacleVertex>();
        List<ObstacleVertex> obstacles = new List<ObstacleVertex>();
        List<float> obstacleDists = new List<float>();

        /////////////////FOR RVO2
        List<Line> orcaLines = new List<Line>();

        List<Line> projLines = new List<Line>();



        /// <summary>
        /// FORGROUP////////////////////
        /// </summary>
        public List<VO> curVOs = new List<VO>();
        public List<VG> curVGs = new List<VG>();
        public List<Agent> groupNeighbours = new List<Agent>();
        public List<float> relationDists = new List<float>();
        public float cosDeviationAngle = 0.7f;
        public bool weakTarget = false;
        public bool isHigestInfluence = true;


        public bool equalPriority = false;//Control the priority when selecting neighbors

        ////////////////////
        ////////FOR FORMATION GROUP!!!!!!!!!!
        /////////////////
        public bool formConstraint = true;
        public bool vertForm = false;
        public float offsetDist = 0.2f;
        //public bool Line2Constraint = true;

		////FOR GROUP IN GROUP
		public bool isGroupInGroup = false;
		public float increaseRmax = 1f;
		public bool isGroupInGroupGuide = false;
        /// <summary>
        /// FORNAUGHTYCHILDREN
        /// </summary>
        public bool isNaughtyChildren = false;
        public Vector3 adultPos = new Vector3();
        public Vector3 adultVel = new Vector3();
        public float maxAdultR = 20f;

        public List<ObstacleVertex> NeighbourObstacles
        {
            get
            {
                return null;
            }
        }

        public Agent(Vector3 pos)
        {
            MaxSpeed = 2;
            NeighbourDist = 15;
            AgentTimeHorizon = 2;
            ObstacleTimeHorizon = 2;
            Height = 5;
            Radius = 5;
            MaxNeighbours = 10;
            Locked = false;

            position = pos;
            Position = position;
            prevSmoothPos = position;
            smoothPos = position;

            Layer = VOGLayer.DefaultAgent;
            CollidesWith = (VOGLayer)(-1);
        }

        public void BufferSwitch()
        {
            // <==
            radius = Radius;
            height = Height;
            maxSpeed = MaxSpeed;
            neighbourDist = NeighbourDist;
            agentTimeHorizon = AgentTimeHorizon;
            obstacleTimeHorizon = ObstacleTimeHorizon;
            maxNeighbours = MaxNeighbours;
            desiredVelocity = DesiredVelocity;
            locked = Locked;
            collidesWith = CollidesWith;
            layer = Layer;

            //position = Position;

            // ==>
            Velocity = velocity;
            List<ObstacleVertex> tmp = obstaclesBuffered;
            obstaclesBuffered = obstacles;
            obstacles = tmp;
        }

        // Update is called once per frame
        public void Update()
        {
            velocity = newVelocity;

            prevSmoothPos = smoothPos;

            //Note the case P/p
            //position = Position;
            position = prevSmoothPos;
            if (!forAnalysis)
            {
                position = position + velocity * simulator.DeltaTime;
            }
            else
            {
                position = position + velocity * unitTime;
                if (simulator.agentPosList.Count <= agentID)
                {
                    List<Vector2> newList = new List<Vector2>();
                    newList.Add(new Vector2(position.x, position.y));
                    simulator.agentPosList.Add(newList);
                    if (simulator.agentPosList.Count <= agentID)
                    {
                        Debug.Log("WRONG IN AGENT UPDAT: AGENTPOSLIST,,,");
                    }
                }
                else
                {
                    if (agentID >= 0)
                        simulator.agentPosList[agentID].Add(new Vector2(position.x, position.z));
                }
            }
            Position = position;
        }

        public void Interpolate(float t)
        {
            smoothPos = smoothPos + newVelocity * t;
            timeSinceChanged += 10*t;
        }

        public static System.Diagnostics.Stopwatch watch1 = new System.Diagnostics.Stopwatch();
        public static System.Diagnostics.Stopwatch watch2 = new System.Diagnostics.Stopwatch();

        public void CalculateNeighbours()
        {

            neighbours.Clear();
            neighbourDists.Clear();
            ///FORGROUP/////////////
            curVGs.Clear();
            curVOs.Clear();
            groupNeighbours.Clear();
            relationDists.Clear();

            _collision = false;
            float rangeSq;

            if (locked) return;

            //watch1.Start ();
            if (MaxNeighbours > 0)
            {
                rangeSq = neighbourDist * neighbourDist;// 

                //simulator.KDTree.GetAgentNeighbours (this, rangeSq);
                simulator.Quadtree.Query(new Vector2(position.x, position.z), neighbourDist, this);

            }
            if (isDVOR)
            {
                if (dvoNeighborNum > 0)
                {
                    longPerceptionStep = true;
                    simulator.Quadtree.Query(new Vector2(position.x, position.z), dvoLongPerceptionRadius, this);
                }

            }
            longPerceptionStep = false;
            //watch1.Stop ();


            /////FORGROUP/////////////////
            //            float groupRangeSq;
            ///groupNeghbours
            if (groupNeighbourNum > 0)
            {
                //              groupRangeSq = groupNeighbourDist * groupNeighbourDist;

                simulator.Quadtree.Query(new Vector2(position.x, position.z), groupNeighbourDist, this, true);
            }
            //////////////////////////////
            obstacles.Clear();
            obstacleDists.Clear();

            rangeSq = (obstacleTimeHorizon * maxSpeed + radius);
            rangeSq *= rangeSq;
            // Obstacles disabled at the moment
            //simulator.KDTree.GetObstacleNeighbours (this, rangeSq);

        }

        float Sqr(float x)
        {
            return x * x;
        }
        /// <summary>
        /// GROUPPPPPPPPPPP		/// </summary>
        /// <returns>The group agent neighbour.</returns>
        /// <param name="agent">Agent.</param>
        /// <param name="rangeSq">Range sq.</param>
        /// 1/(1+2*relationValue)

        //some problems!!
        public float InsertGroupAgentNeighbourSort(Agent agent, float rangeSq)
        {
            if (this == agent) return rangeSq;

            if ((agent.layer & collidesWith) == 0) return rangeSq;

            //2D Dist
            float dist = Sqr(agent.position.x - position.x) + Sqr(agent.position.z - position.z);

            if (dist < rangeSq)
            {

                float relationDist;
                if (simulator.relationMatrix.Length > 0 && agentID >= 0 && agent.agentID >= 0)
                {
                    //	relationDist=dist*1f/(1f+2f*simulator.relationMatrix[agentID][agent.agentID]);
                    relationDist = dist * (Mathf.Pow(2f, 10f * (0.5f - simulator.relationMatrix[agentID][agent.agentID])));
                }
                else
                    relationDist = dist;

                //NEWADD neighbors in front of the agent have a higher priority
                Vector2 relativePos = new Vector2(agent.position.x - position.x, agent.position.z - position.z);
                relativePos.Normalize();
                Vector2 desiredV = new Vector2(desiredVelocity.x, desiredVelocity.z);
                desiredV.Normalize();
                relationDist *= (1f + 0.5f * (relativePos.x * desiredV.x + relativePos.y + desiredV.y));



                if (groupNeighbours.Count < groupNeighbourNum)
                {
                    groupNeighbours.Add(agent);
                    relationDists.Add(relationDist);
                }

                int i = groupNeighbours.Count - 1;
                if (i > 0 && relationDist < relationDists[i - 1] || relationDist < relationDists[i])
                {
                    while (i != 0 && relationDist < relationDists[i - 1])
                    {
                        groupNeighbours[i] = groupNeighbours[i - 1];
                        relationDists[i] = relationDists[i - 1];
                        i--;
                    }
                    groupNeighbours[i] = agent;
                    relationDists[i] = relationDist;
                }

                if (groupNeighbours.Count == groupNeighbourNum && relationDists[relationDists.Count - 1] < dist)
                {
                    //	rangeSq =(groupNeighbourDist+ relationDists[relationDists.Count-1])/2f;
                }
            }
            return rangeSq;
        }
        public float InsertAgentNeighbourSort(Agent agent, float rangeSq)
        {
            if (this == agent) return rangeSq;

            if ((agent.layer & collidesWith) == 0) return rangeSq;

            //2D Dist
            float dist = Sqr(agent.position.x - position.x) + Sqr(agent.position.z - position.z);

            if (dist < rangeSq)
            {
                if (neighbours.Count < maxNeighbours)
                {
                    neighbours.Add(agent);
                    neighbourDists.Add(dist);
                }

                int i = neighbours.Count - 1;
                if (i > 0 && dist < neighbourDists[i - 1] || dist < neighbourDists[i])
                {
                    while (i != 0 && dist < neighbourDists[i - 1])
                    {
                        neighbours[i] = neighbours[i - 1];
                        neighbourDists[i] = neighbourDists[i - 1];
                        i--;
                    }
                    neighbours[i] = agent;
                    neighbourDists[i] = dist;
                }

                if (neighbours.Count == maxNeighbours)
                {
                    rangeSq = neighbourDists[neighbourDists.Count - 1];
                }
            }
            return rangeSq;
        }

        //current used
        public float InsertGroupAgentNeighbour(Agent agent, float rangeSq)
        {
            if (agentID < 0 || agent.agentID < 0) return rangeSq;
            if (this == agent) return rangeSq;

            if ((agent.layer & collidesWith) == 0) return rangeSq;
            if (simulator.relationMatrix[agentID][agent.agentID] == 0f) return rangeSq;

            Vector3 desVelNormalized = desiredVelocity.normalized;
            float desVelMagnitude = desiredVelocity.magnitude;
            float cosTheta = Vector3.Dot(desVelNormalized, agent.desiredVelocity.normalized);
            bool considerGoals = false;

            if (desVelMagnitude > 0.5f && agent.desiredVelocity.magnitude > 0.5f)
            {
                if (cosTheta <= cosDeviationAngle) // -------30+30=60degree
                {
                    return rangeSq;
                }
                considerGoals = true;
            }


            //2D Dist
            float dist = Sqr(agent.position.x - position.x) + Sqr(agent.position.z - position.z);

            if (dist < rangeSq)
            {
                float relationDist;
                if (simulator.relationMatrix.Length > 0 && agentID >= 0 && agent.agentID >= 0)
                {
                    //	relationDist=dist*1f/(1f+2f*simulator.relationMatrix[agentID][agent.agentID]);
                    /*  if (simulator.relationMatrix[agentID][agent.agentID]<0.3)
                      {
                          //relationDist=
                      }else */
                    {
                        relationDist = dist * (Mathf.Pow(2f, 10f * (0.5f - simulator.relationMatrix[agentID][agent.agentID])));
                    }
                    ////new add ½Ç¶È£¬agentÇ°µÄneighborÈ¨ÖØ¼Ó´ó//neighbors in front of the agent have a higher priority
                    float dott = 0f;
                    if(equalPriority){
                        dott = 1f;
                    }else                    
                    if (desVelMagnitude > 0.5f)
                    {
                        dott = Vector3.Dot(desVelNormalized, (agent.Position - Position).normalized);
                    }
                    else
                    {
                        dott = Vector3.Dot(velocity.normalized, (agent.Position - Position).normalized);
                    }
                    
                    if (!considerGoals)//don't consider different goals
                    {
                        relationDist = (1.5f - 0.5f * dott) * relationDist;
                    }
                    else
                    {
                        relationDist = (2f - cosTheta) * (1.5f - 0.5f * dott) * relationDist;
                    }

                }
                else
                    relationDist = dist;
                if (groupNeighbours.Count < groupNeighbourNum)
                {
                    groupNeighbours.Add(agent);
                    relationDists.Add(relationDist);
                    int k = groupNeighbours.Count - 1;
                    while (k - 1 >= 0 && relationDists[k] < relationDists[k - 1])
                    {
                        groupNeighbours[k] = groupNeighbours[k - 1];
                        relationDists[k] = relationDists[k - 1];

                        k--;
                    }
                    groupNeighbours[k] = agent;
                    relationDists[k] = relationDist;
                }
                else
                {
                    int i = groupNeighbours.Count - 1;
                    if (i == 0 && (relationDist < relationDists[i]))
                    {
                        groupNeighbours[i] = agent;
                        relationDists[i] = relationDist;

                    }
                    else if (i > 0 && relationDist < relationDists[i])
                    {
                        while (i != 0 && (relationDist < relationDists[i - 1]))
                        {
                            groupNeighbours[i] = groupNeighbours[i - 1];
                            relationDists[i] = relationDists[i - 1];

                            i--;
                        }
                        groupNeighbours[i] = agent;
                        relationDists[i] = relationDist;
                    }
                }

                if (groupNeighbours.Count == groupNeighbourNum && relationDists[relationDists.Count - 1] < dist)
                {
                    //	rangeSq =(groupNeighbourDist+ relationDists[relationDists.Count-1])/2f;
                }
            }
            return rangeSq;
        }
        public float InsertAgentNeighbour(Agent agent, float rangeSq)
        {
            if (this == agent) return rangeSq;

            if ((agent.layer & collidesWith) == 0) return rangeSq;

            //2D Dist
            float dist = Sqr(agent.position.x - position.x) + Sqr(agent.position.z - position.z);

            if (dist < rangeSq)
            {

                if (longPerceptionStep&&agentID>=0&&agent.agentID>=0)
                {
                    if (dist < neighbourDist * neighbourDist || (simulator.relationMatrix[agentID][agent.agentID] != 0))
                    {
                        return rangeSq;
                    }
                    if (neighbours.Count < maxNeighbours + dvoNeighborNum)
                    {
                        neighbours.Add(agent);
                        neighbourDists.Add(dist);
                    }
                }
                else if (neighbours.Count < maxNeighbours)
                {
                    neighbours.Add(agent);
                    neighbourDists.Add(dist);
                }

                int i = neighbours.Count - 1;
                if (dist < neighbourDists[i])
                {
                    while (i != 0 && dist < neighbourDists[i - 1])
                    {
                        neighbours[i] = neighbours[i - 1];
                        neighbourDists[i] = neighbourDists[i - 1];
                        i--;
                    }
                    neighbours[i] = agent;
                    neighbourDists[i] = dist;
                }

                if (longPerceptionStep)
                {
                    if (neighbours.Count == maxNeighbours + dvoNeighborNum)
                    {
                        rangeSq = neighbourDists[neighbourDists.Count - 1];
                    }
                }
                else if (neighbours.Count == maxNeighbours)
                {
                    rangeSq = neighbourDists[neighbourDists.Count - 1];
                }
            }
            return rangeSq;
        }

        /*public void UpdateNeighbours () {
            neighbours.Clear ();
            float sqrDist = neighbourDistance*neighbourDistance;
            for ( int i = 0; i < simulator.agents.Count; i++ ) {
                float dist = (simulator.agents[i].position - position).sqrMagnitude;
                if ( dist <= sqrDist ) {
                    neighbours.Add ( simulator.agents[i] );
                }
            }
        }*/

        public void InsertObstacleNeighbour(ObstacleVertex ob1, float rangeSq)
        {
            ObstacleVertex ob2 = ob1.next;

            float dist = AstarMath.DistancePointSegmentStrict(ob1.position, ob2.position, Position);

            if (dist < rangeSq)
            {
                obstacles.Add(ob1);
                obstacleDists.Add(dist);

                int i = obstacles.Count - 1;
                while (i != 0 && dist < obstacleDists[i - 1])
                {
                    obstacles[i] = obstacles[i - 1];
                    obstacleDists[i] = obstacleDists[i - 1];
                    i--;
                }
                obstacles[i] = ob1;
                obstacleDists[i] = dist;
            }
        }

        static Vector3 To3D(Vector2 p)
        {
            return new Vector3(p.x, 0, p.y);
        }

        static void DrawCircle(Vector2 _p, float radius, Color col)
        {
            DrawCircle(_p, radius, 0, 2 * Mathf.PI, col);
        }

        static void DrawCircle(Vector2 _p, float radius, float a0, float a1, Color col)
        {
            Vector3 p = To3D(_p);

            while (a0 > a1) a0 -= 2 * Mathf.PI;

            Vector3 prev = new Vector3(Mathf.Cos(a0) * radius, 0, Mathf.Sin(a0) * radius);
            const float steps = 40.0f;
            for (int i = 0; i <= steps; i++)
            {
                Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(a0, a1, i / steps)) * radius, 0, Mathf.Sin(Mathf.Lerp(a0, a1, i / steps)) * radius);
                Debug.DrawLine(p + prev, p + c, col);
                prev = c;
            }
        }

        static void DrawVO(Vector2 circleCenter, float radius, Vector2 origin)
        {
            float alpha = Mathf.Atan2((origin - circleCenter).y, (origin - circleCenter).x);
            float gamma = radius / (origin - circleCenter).magnitude;
            float delta = gamma <= 1.0f ? Mathf.Abs(Mathf.Acos(gamma)) : 0;

            DrawCircle(circleCenter, radius, alpha - delta, alpha + delta, Color.black);
            Vector2 p1 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta)) * radius;
            Vector2 p2 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta)) * radius;

            Vector2 p1t = -new Vector2(-p1.y, p1.x);
            Vector2 p2t = new Vector2(-p2.y, p2.x);
            p1 += circleCenter;
            p2 += circleCenter;

            Debug.DrawRay(To3D(p1), To3D(p1t).normalized * 100, Color.black);
            Debug.DrawRay(To3D(p2), To3D(p2t).normalized * 100, Color.black);
        }

        static void DrawCross(Vector2 p, float size = 1)
        {
            DrawCross(p, Color.white, size);
        }

        static void DrawCross(Vector2 p, Color col, float size = 1)
        {
            size *= 0.5f;
            Debug.DrawLine(new Vector3(p.x, 0, p.y) - Vector3.right * size, new Vector3(p.x, 0, p.y) + Vector3.right * size, col);
            Debug.DrawLine(new Vector3(p.x, 0, p.y) - Vector3.forward * size, new Vector3(p.x, 0, p.y) + Vector3.forward * size, col);
        }

        /// <summary>
        /// //////FORGROUP////////////////////////////////
        /// </summary>
        /////////////////VG//
        /// 
        /// 

        public struct VG
        {
            public int agentID;
            public Vector2 origin;
            public Vector2 globalCenter;
            public Vector2 line1, line2, dir1, dir2;
            public Vector2 cutoffLine1, cutoffLine2, cutoffDir1, cutoffDir2;


            public float Alpha, Gamma, Delta;

            public float group_radius;
            public float invradius;
            float sqrCutoffDistance;
            //       bool leftSide;
            public bool inRange; //in/out of the range

            public bool usingForm;
            //  public Vector2 formLine1, formLine2,formDir1, formDir2;

            public VG(Vector2 center, Vector2 offset, float group_radius, Vector2 sideChooser, float inverseDt, int agentID, float offsetDist = 0f)
            {
                //Modified£º°ÑsiderChooser×÷Îª formDistµÄ·¨Ïà£¬£¬

                this.group_radius = group_radius;
                this.origin = offset;
                this.agentID = agentID;

                //  formLine1=new Vector2();
                //  formLine2=new Vector2();
                //  formDir1 = new Vector2();
                //  formDir2 = new Vector2();
                usingForm = false;

                //in Range/out Range
                if (center.magnitude < group_radius + group_radius/5)
                {
                    globalCenter = center + offset;
                    inRange = true;
                    //line1
                    //line2
                    //line1=(iVO.line1-offset).normalized*group_radius+offset;
                    //dir1=iVO.dir1;
                    //line2=(iVO.line2-offset).normalized*group_radius+offset;
                    //dir2=iVO.dir2;

                    line1 = new Vector2(group_radius + center.x, center.y) + offset;
                    dir1 = new Vector2(0, -1);

                    line2 = new Vector2(center.x - group_radius, center.y) + offset;
                    dir2 = new Vector2(0, 1);

                    cutoffLine1 = new Vector2(center.x, center.y - group_radius) + offset;
                    cutoffDir1 = new Vector2(-1, 0);

                    cutoffLine2 = new Vector2(center.x, center.y + group_radius) + offset;
                    cutoffDir2 = new Vector2(1, 0);
                    //cutOffLine

                    //cutoffLine1=center.normalized * (center.magnitude - group_radius);
                    //cutoffDir1=new Vector2(cutoffLine.y,-cutoffLine.x).normalized;
                    //cutoffLine1 += offset;
                    Alpha = Gamma = Delta = 0f;
                    invradius = group_radius;
                    sqrCutoffDistance = center.magnitude + group_radius;

                    if (offsetDist > 0f)
                    {
                        usingForm = true;



                        line1 = center + (sideChooser * offsetDist) + offset;
                        dir1 = new Vector2(sideChooser.y, -sideChooser.x);
                        line2 = center - (sideChooser * offsetDist) + offset;
                        dir2 = new Vector2(-sideChooser.y, sideChooser.x);
                        cutoffLine1 = center + (dir1 * group_radius) + offset;
                        cutoffDir1 = -sideChooser;
                        cutoffLine2 = center + (dir2 * group_radius) + offset;
                        cutoffDir2 = sideChooser;
                    }else if(offsetDist<0f){  //half polygon constraint
                        usingForm = true;

                        line1 = center + (sideChooser * offsetDist) + offset;
                        dir1 = new Vector2(sideChooser.y, -sideChooser.x);
                        line2 = center - (sideChooser * offsetDist) + offset;
                        dir2 = new Vector2(-sideChooser.y, sideChooser.x);
                        cutoffLine1 = center + (dir1 * group_radius) + offset;
                       
                        cutoffDir1 = -sideChooser;
                       // cutoffLine2 = center + (dir2 * group_radius) + offset;
                        cutoffLine2 = center + (-dir1 * 6f) + offset;
                        cutoffDir2 = sideChooser;

                    }

                    //  leftSide = false;
                    return;
                }

                inRange = false;


                sqrCutoffDistance = center.magnitude + group_radius;
                cutoffLine2 = center.normalized * sqrCutoffDistance;
                cutoffDir2 = new Vector2(cutoffLine2.y, -cutoffLine2.x).normalized;
                cutoffLine2 += offset;

                center *= inverseDt;
                group_radius *= inverseDt;
                globalCenter = center + offset;
                invradius = group_radius;


                sqrCutoffDistance = center.magnitude - group_radius;
                cutoffLine1 = center.normalized * sqrCutoffDistance;
                cutoffDir1 = new Vector2(-cutoffLine1.y, cutoffLine1.x).normalized;
                cutoffLine1 += offset;

                sqrCutoffDistance *= sqrCutoffDistance;
                float alpha = Mathf.Atan2(-center.y, -center.x);

                float delta = Mathf.Abs(Mathf.Acos(group_radius / center.magnitude));

                // Bounding Lines
                //leftside, from the view of VO center
                //    leftSide = Polygon.Left(Vector2.zero, center, sideChooser);

                // Point on circle
                line1 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta)) * group_radius;
                // Vector tangent to circle which is the correct line tangent
                dir1 = new Vector2(line1.y, -line1.x).normalized;

                // Point on circle
                line2 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta)) * group_radius;
                // Vector tangent to circle which is the correct line tangent
                dir2 = new Vector2(line2.y, -line2.x).normalized;

                line1 += globalCenter;
                line2 += globalCenter;

                Alpha = Mathf.Atan2((offset - globalCenter).y, (offset - globalCenter).x);
                Gamma = group_radius / (offset - globalCenter).magnitude;
                Delta = Gamma <= 1.0f ? Mathf.Abs(Mathf.Acos(Gamma)) : 0;
            }


            /** Returns if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
             * Also returns true if the points are colinear */
            static bool Left(Vector2 a, Vector2 dir, Vector2 p)
            {
                return (dir.x) * (p.y - a.y) - (p.x - a.x) * (dir.y) <= 0; //
            }

            /** Returns if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
             * Also returns true if the points are colinear */
            static float Det(Vector2 a, Vector2 dir, Vector2 p)
            {
                return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
            }

            public Vector2 Sample(Vector2 p, out float weight) //inrange problem
            {

                if (inRange)
                {
                    //if(false){
                    // Calculate double signed area of the triangle consisting of the points
                    // {line1, line1+dir1, p}
                    Vector2 insertP = globalCenter - p;
                    if (insertP.magnitude < group_radius)
                    {
                        weight = 0;
                        return Vector2.zero;
                    }
                    insertP = -insertP.normalized * group_radius + globalCenter;
                    //weight = (insertP - p).magnitude * 0.5f;
                    weight = (insertP - p).magnitude;
                    return insertP - p;
                }
                weight = 0;
                float l1 = Det(line1, dir1, p);
                float l2 = Det(line2, dir2, p);
                float l3 = Det(cutoffLine1, cutoffDir1, p);
                float l4 = Det(cutoffLine2, cutoffDir2, p);

                Vector2 d = new Vector2(0f, 0f);
                if (l1 < 0)
                {
                    d += new Vector2(-dir1.y, dir1.x) * l1;// *0.5f;
                    if (weight < -l1) weight = -l1;
                    //weight = -l1 * 0.5f;
                }
                if (l2 < 0)
                {
                    d += (new Vector2(-dir2.y, dir2.x)) * l2;// *0.5f;
                    if (weight < -l2) weight = -l2;
                    //weight = -l2 * 0.5f;
                }
                if (l3 < 0)
                {
                    d += (new Vector2(-cutoffDir1.y, cutoffDir1.x)) * l3;// *0.5f;
                    if (weight < -l3) weight = -l3;
                    //weight = -l3 * 0.5f;
                }
                if (l4 < 0)
                {
                    d += (new Vector2(-cutoffDir2.y, cutoffDir2.x)) * l4;// *0.5f;
                    if (weight < -l4) weight = -l4;
                    //weight = -l4 * 0.5f;
                }
                return d;
                //}
            }

            public float ScalarSample(Vector2 p)
            {
                Vector2 d = new Vector2(0f, 0f);
                float weight = 0f;

                if (inRange)
                {
                    Vector2 insertP = globalCenter - p;
                    float p_cDist = insertP.magnitude;
                    if (usingForm)
                    {
                        float l1 = Det(line1, dir1, p);
                        float l2 = Det(line2, dir2, p);
                        float l3 = Det(cutoffLine1, cutoffDir1, p);
                        float l4 = Det(cutoffLine2, cutoffDir2, p);

                        int outCount = 0;

                        if (l1 < 0)
                        {
                            //   d += new Vector2(-dir1.y, dir1.x) * l1 * 0.5f;
                            if (weight < -l1) weight = -l1;
                            outCount++;
                        }
                        if (l2 < 0)
                        {
                            //  d += (new Vector2(-dir2.y, dir2.x)) * l2 * 0.5f;
                            if (weight < -l2) weight = -l2;
                            outCount++;
                        }
                        if (l3 < 0)
                        {
                            //  d += (new Vector2(-cutoffDir1.y, cutoffDir1.x)) * l3 * 0.5f;

                            if (weight < -l3) weight = -l3;
                            outCount++;
                        }
                        if (l4 < 0)
                        {
                            // d += (new Vector2(-cutoffDir2.y, cutoffDir2.x)) * l4 * 0.5f;
                            if (weight < -l4) weight = -l4;
                            outCount++;
                        }
                        //  if (outCount > 1) weight  =weight*1.4f;
                        return weight;// *0.5f; 

                    }
                    else
                    {
                        if (p_cDist < group_radius)
                        {
                            return 0;
                        }
                        //  insertP = -insertP.normalized * group_radius + globalCenter;
                        //  return (insertP - p).magnitude * 0.5f; 
                        return (p_cDist - group_radius);// *0.5f;
                    }
                }
                //weight = 0;

                {
                    float l1 = Det(line1, dir1, p);
                    float l2 = Det(line2, dir2, p);
                    float l3 = Det(cutoffLine1, cutoffDir1, p);
                    float l4 = Det(cutoffLine2, cutoffDir2, p);

                    int outCount = 0;

                    if (l1 < 0)
                    {
                        //   d += new Vector2(-dir1.y, dir1.x) * l1 * 0.5f;
                        if (weight < -l1) weight = -l1;
                        outCount++;
                    }
                    if (l2 < 0)
                    {
                        //  d += (new Vector2(-dir2.y, dir2.x)) * l2 * 0.5f;
                        if (weight < -l2) weight = -l2;
                        outCount++;
                    }
                    if (l3 < 0)
                    {
                        //  d += (new Vector2(-cutoffDir1.y, cutoffDir1.x)) * l3 * 0.5f;

                        if (weight < -l3) weight = -l3;
                        outCount++;
                    }
                    if (l4 < 0)
                    {
                        // d += (new Vector2(-cutoffDir2.y, cutoffDir2.x)) * l4 * 0.5f;
                        if (weight < -l4) weight = -l4;
                        outCount++;
                    }
                    //  if (outCount > 1) weight  =weight*1.4f;
                    return weight;// *0.5f; 
                }
            }

            public float ScalarSampleBKP(Vector2 p)
            {
                Vector2 d = new Vector2(0f, 0f);
                float weight = 0f;

                if (inRange)
                {
                    Vector2 insertP = globalCenter - p;
                    float p_cDist = insertP.magnitude;
                    if (p_cDist < group_radius)
                    {
                        return 0;
                    }
                    //  insertP = -insertP.normalized * group_radius + globalCenter;
                    //  return (insertP - p).magnitude * 0.5f; 
                    return (p_cDist - group_radius) * 0.5f;
                }
                //weight = 0;


                float l1 = Det(line1, dir1, p);
                float l2 = Det(line2, dir2, p);
                float l3 = Det(cutoffLine1, cutoffDir1, p);
                float l4 = Det(cutoffLine2, cutoffDir2, p);

                int outCount = 0;

                if (l1 < 0)
                {
                    //   d += new Vector2(-dir1.y, dir1.x) * l1 * 0.5f;
                    if (weight < -l1) weight = -l1;
                    outCount++;
                }
                if (l2 < 0)
                {
                    //  d += (new Vector2(-dir2.y, dir2.x)) * l2 * 0.5f;
                    if (weight < -l2) weight = -l2;
                    outCount++;
                }
                if (l3 < 0)
                {
                    //  d += (new Vector2(-cutoffDir1.y, cutoffDir1.x)) * l3 * 0.5f;

                    if (weight < -l3) weight = -l3;
                    outCount++;
                }
                if (l4 < 0)
                {
                    // d += (new Vector2(-cutoffDir2.y, cutoffDir2.x)) * l4 * 0.5f;
                    if (weight < -l4) weight = -l4;
                    outCount++;
                }
                //  if (outCount > 1) weight  =weight*1.4f;
                return weight * 0.5f;
            }
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////
        /// </summary>


        public struct VO
        {

            //FORGROUP
            public Vector2 globalCenter;
            public float Alpha, Gamma, Delta;
            //public 
            public Vector2 origin, center;

            //public float radius;

            public Vector2 line1, line2, dir1, dir2;

            public Vector2 cutoffLine, cutoffDir;

            float sqrCutoffDistance;
            bool leftSide;

            bool colliding;

            float radius;
            public float invradius;

            float weightFactor;

            /** Creates a VO to avoid the half plane created by the point \a p0 and has a tangent in the direction of \a dir.
             * \param p0 a point on the half plane border
             * \param dir the normalized tangent to the half plane
             */
            public VO(Vector2 offset, Vector2 p0, Vector2 dir, float weightFactor)
            {
                globalCenter = new Vector2();
                colliding = true;
                line1 = p0;
                dir1 = -dir;

                // Fully initialize the struct, compiler complains otherwise
                origin = Vector2.zero;
                center = Vector2.zero;
                line2 = Vector2.zero;
                dir2 = Vector2.zero;
                cutoffLine = Vector2.zero;
                cutoffDir = Vector2.zero;
                sqrCutoffDistance = 0;
                leftSide = false;
                radius = 0;
                invradius = radius;
                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                this.weightFactor = weightFactor * 0.5f;
                Alpha = Gamma = Delta = 0f;
                //Debug.DrawRay ( To3D(offset + line1), To3D(dir1)*10, Color.red);
            }

            /** Creates a VO to avoid the three half planes with {point, tangent}s of {p1, p2-p1}, {p1, tang1}, {p2, tang2}.
             * tang1 and tang2 should be normalized.
             */
            public VO(Vector2 offset, Vector2 p1, Vector2 p2, Vector2 tang1, Vector2 tang2, float weightFactor)
            {
                globalCenter = new Vector2();
                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                this.weightFactor = weightFactor * 0.5f;

                colliding = false;
                cutoffLine = p1;
                /** \todo Square root can theoretically be removed by passing another parameter */
                cutoffDir = (p2 - p1).normalized;
                line1 = p1;
                dir1 = tang1;
                line2 = p2;
                dir2 = tang2;

                //dir1 = -dir1;
                dir2 = -dir2;
                cutoffDir = -cutoffDir;

                // Fully initialize the struct, compiler complains otherwise
                origin = Vector2.zero;
                center = Vector2.zero;
                sqrCutoffDistance = 0;
                leftSide = false;
                radius = 0;
                invradius = radius;

                weightFactor = 5;
                Alpha = Gamma = Delta = 0f;
                //Debug.DrawRay (To3D(cutoffLine+offset), To3D(cutoffDir)*10, Color.blue);
                //Debug.DrawRay (To3D(line1+offset), To3D(dir1)*10, Color.blue);
                //Debug.DrawRay (To3D(line2+offset), To3D(dir2)*10, Color.cyan);
            }

            /** Creates a VO for avoiding another agent */
            public VO(Vector2 center, Vector2 offset, float radius, Vector2 sideChooser, float inverseDt, float weightFactor)
            {
                Alpha = 0f;
                Gamma = 0f;
                Delta = 0f;
                invradius = 0f;
                // globalCenter = new Vector2();
                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                this.weightFactor = weightFactor * 0.5f;

                //this.radius = radius;
                //Vector2 globalCenter;
                this.origin = offset;
                weightFactor = 0.5f;

                // Collision?
                if (center.magnitude < radius)
                {
                    colliding = true;
                    leftSide = false;

                    line1 = center.normalized * (center.magnitude - radius);
                    dir1 = new Vector2(line1.y, -line1.x).normalized;
                    line1 += offset;

                    cutoffDir = Vector2.zero;
                    cutoffLine = Vector2.zero;
                    sqrCutoffDistance = 0;
                    dir2 = Vector2.zero;
                    line2 = Vector2.zero;
                    this.center = Vector2.zero;
                    this.radius = 0;
                    globalCenter = new Vector2();
                }
                else
                {

                    colliding = false;

                    center *= inverseDt;
                    radius *= inverseDt;
                    globalCenter = center + offset;

                    sqrCutoffDistance = center.magnitude - radius;

                    this.center = center;
                    cutoffLine = center.normalized * sqrCutoffDistance;
                    cutoffDir = new Vector2(-cutoffLine.y, cutoffLine.x).normalized;
                    cutoffLine += offset;

                    sqrCutoffDistance *= sqrCutoffDistance;
                    float alpha = Mathf.Atan2(-center.y, -center.x);

                    float delta = Mathf.Abs(Mathf.Acos(radius / center.magnitude));

                    this.radius = radius;

                    // Bounding Lines

                    leftSide = Polygon.Left(Vector2.zero, center, sideChooser);

                    // Point on circle
                    line1 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta)) * radius;
                    // Vector tangent to circle which is the correct line tangent
                    dir1 = new Vector2(line1.y, -line1.x).normalized;

                    // Point on circle
                    line2 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta)) * radius;
                    // Vector tangent to circle which is the correct line tangent
                    dir2 = new Vector2(line2.y, -line2.x).normalized;

                    line1 += globalCenter;
                    line2 += globalCenter;

                    //Debug.DrawRay ( To3D(line1), To3D(dir1), Color.cyan );
                    //Debug.DrawRay ( To3D(line2), To3D(dir2), Color.cyan );
                }
            }

            /** Creates a VO for avoiding another agent */
            public VO(Vector2 center, Vector2 offset, float radius, Vector2 sideChooser, float inverseDt, float weightFactor, Vector3 pos, bool dvo, ref bool iscollsion)
            {
                globalCenter = new Vector2();
                Alpha = Gamma = Delta = 0f;
                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                this.weightFactor = weightFactor * 0.5f;

                //this.radius = radius;
                //Vector2 globalCenter;
                this.origin = offset;
                weightFactor = 0.5f;

                // Collision?
                if (center.magnitude < radius)
                {
                    colliding = true;
                    leftSide = false;
                    iscollsion = true;

                    line1 = center.normalized * (center.magnitude - radius);
                    dir1 = new Vector2(line1.y, -line1.x).normalized;
                    line1 += offset;

                    cutoffDir = Vector2.zero;
                    cutoffLine = Vector2.zero;
                    sqrCutoffDistance = 0;
                    dir2 = Vector2.zero;
                    line2 = Vector2.zero;
                    this.center = Vector2.zero;
                    this.radius = 0;
                    invradius = this.radius;
                }
                else
                {

                    colliding = false;
                    iscollsion = false;
                    center *= inverseDt;
                    radius *= inverseDt;
                    invradius = radius;
                    globalCenter = center + offset;

                    sqrCutoffDistance = center.magnitude - radius;

                    this.center = center;
                    cutoffLine = center.normalized * sqrCutoffDistance;
                    cutoffDir = new Vector2(-cutoffLine.y, cutoffLine.x).normalized;
                    cutoffLine += offset;

                    sqrCutoffDistance *= sqrCutoffDistance;
                    float alpha = Mathf.Atan2(-center.y, -center.x);

                    float delta = Mathf.Abs(Mathf.Acos(radius / center.magnitude));

                    this.radius = radius;

                    // Bounding Lines

                    leftSide = Polygon.Left(Vector2.zero, center, sideChooser);

                    // Point on circle
                    line1 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta)) * radius;
                    // Vector tangent to circle which is the correct line tangent
                    dir1 = new Vector2(line1.y, -line1.x).normalized;

                    // Point on circle
                    line2 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta)) * radius;
                    // Vector tangent to circle which is the correct line tangent
                    dir2 = new Vector2(line2.y, -line2.x).normalized;

                    line1 += globalCenter;
                    line2 += globalCenter;

                    //Debug.DrawRay ( To3D(line1), To3D(dir1), Color.cyan );
                    //Debug.DrawRay ( To3D(line2), To3D(dir2), Color.cyan );
                    Alpha = Mathf.Atan2((offset - globalCenter).y, (offset - globalCenter).x);
                    Gamma = radius / (offset - globalCenter).magnitude;
                    Delta = Gamma <= 1.0f ? Mathf.Abs(Mathf.Acos(Gamma)) : 0;
                    //if(dvo)DrawVO(globalCenter+new Vector2 (pos.x,pos.z), radius, origin+new Vector2 (pos.x,pos.z));

                }
            }

            /** Returns if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
             * Also returns true if the points are colinear.
             */
            public static bool Left(Vector2 a, Vector2 dir, Vector2 p)
            {
                return (dir.x) * (p.y - a.y) - (p.x - a.x) * (dir.y) <= 0;
            }

            /** Returns a negative number of if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
             * The number can be seen as the double signed area of the triangle {a, a+dir, p} multiplied by the length of \a dir.
             * If length(dir)=1 this is also the distance from p to the line {a, a+dir}.
             */
            public static float Det(Vector2 a, Vector2 dir, Vector2 p)
            {
                return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
            }

            public Vector2 Sample(Vector2 p, out float weight)
            {

                if (colliding)
                {
                    // Calculate double signed area of the triangle consisting of the points
                    // {line1, line1+dir1, p}
                    float l1 = Det(line1, dir1, p);

                    // Serves as a check for which side of the line the point p is
                    if (l1 >= 0)
                    {
                        /*float dot1 = Vector2.Dot ( p - line1, dir1 );
						
                        Vector2 c1 = dot1 * dir1 + line1;
                        return (c1-p);*/
                        weight = l1 * weightFactor;
                        return new Vector2(-dir1.y, dir1.x) * weight * GlobalIncompressibility; // 10 is an arbitrary constant signifying incompressability
                        // (the higher the value, the more the agents will avoid penetration)
                    }
                    else
                    {
                        weight = 0;
                        return new Vector2(0, 0);
                    }
                }

                float det3 = Det(cutoffLine, cutoffDir, p);
                if (det3 <= 0)
                {
                    weight = 0;
                    return Vector2.zero;
                }
                else
                {
                    float det1 = Det(line1, dir1, p);
                    float det2 = Det(line2, dir2, p);
                    if (det1 >= 0 && det2 >= 0)
                    {
                        // We are inside both of the half planes
                        // (all three if we count the cutoff line)
                        // and thus inside the forbidden region in velocity space

                        /*float magn = ( p - origin ).sqrMagnitude;
                        if ( magn < sqrCutoffDistance ) {
                            weight = 0;
                            return Vector2.zero;
                        }*/


                        if (leftSide)
                        {

                            if (det3 < radius)
                            {
                                weight = det3 * weightFactor;
                                return new Vector2(-cutoffDir.y, cutoffDir.x) * weight;

                                /*Vector2 dir = (p - center);
                                float magn = dir.magnitude;
                                weight = radius-magn;
                                dir *= (1.0f/magn)*weight;
                                return dir;*/
                            }

                            weight = det1;
                            return new Vector2(-dir1.y, dir1.x) * weight;
                        }
                        else
                        {
                            if (det3 < radius)
                            {
                                weight = det3 * weightFactor;
                                return new Vector2(-cutoffDir.y, cutoffDir.x) * weight;

                                /*Vector2 dir = (p - center);
                                float magn = dir.magnitude;
                                weight = radius-magn;
                                dir *= (1.0f/magn)*weight;
                                return dir;*/
                            }

                            /*if ( det3 < det2 ) {
                                weight = det3*0.5f;
                                return new Vector2(-cutoffDir.y, cutoffDir.x)*weight;
                            }*/

                            weight = det2 * weightFactor;
                            return new Vector2(-dir2.y, dir2.x) * weight;
                        }
                    }
                }

                weight = 0;
                return new Vector2(0, 0);
            }

            public float ScalarSample(Vector2 p)
            {

                if (colliding)
                {
                    // Calculate double signed area of the triangle consisting of the points
                    // {line1, line1+dir1, p}
                    float l1 = Det(line1, dir1, p);

                    // Serves as a check for which side of the line the point p is
                    if (l1 >= 0)
                    {
                        /*float dot1 = Vector2.Dot ( p - line1, dir1 );
						
                        Vector2 c1 = dot1 * dir1 + line1;
                        return (c1-p);*/
                        return l1 * GlobalIncompressibility * weightFactor;
                    }
                    else
                    {
                        return 0;
                    }
                }

                float det3 = Det(cutoffLine, cutoffDir, p);
                if (det3 <= 0)
                {
                    return 0;
                }

                {
                    float det1 = Det(line1, dir1, p);
                    float det2 = Det(line2, dir2, p);
                    if (det1 >= 0 && det2 >= 0)
                    {
                        /*float magn = ( p - origin ).sqrMagnitude;
                        if ( magn < sqrCutoffDistance ) {
                            weight = 0;
                            return Vector2.zero;
                        }*/


                        if (leftSide)
                        {

                            if (det3 < radius)
                            {
                                return det3 * weightFactor;

                                //return radius - (p-center).magnitude;
                                /*Vector2 dir = (p - center);
                                float magn = dir.magnitude;
                                weight = radius-magn;
                                dir *= (1.0f/magn)*weight;
                                return dir;*/
                            }

                            return det1 * weightFactor;
                        }
                        else
                        {
                            if (det3 < radius)
                            {
                                return det3 * weightFactor;

                                //return radius - (p-center).magnitude;

                                /*Vector2 dir = (p - center);
                                float magn = dir.magnitude;
                                weight = radius-magn;
                                dir *= (1.0f/magn)*weight;
                                return dir;*/
                            }

                            return det2 * weightFactor;
                        }
                    }
                }

                return 0;
            }
        }


        internal float det(Vector2 p, Vector2 q) { return p.x * q.y - p.y * q.x; }
        internal float sqr(float a) { return a * a; }
        const float rvo_infty = 99999999f;
        /* Time to collision of a ray to a disc.
            \param p The start position of the ray
            \param v The velocity vector of the ray
            \param p2 The center position of the disc
            \param radius The radius of the disc
            \param collision Specifies whether the time to collision is computed (false), or the time from collision (true).
            \returns Returns the time to collision of ray p + tv to disc D(p2, radius), and #RVO_INFTY when the disc is not hit. If collision is true, the value is negative.
           */
        internal float timeToCollision(Vector2 p, Vector2 v, Vector2 p2, float radius, bool collision)
        {
            Vector2 ba = p2 - p;
            float sq_diam = radius * radius;
            float time;

            //float discr = -sqr(det(v, ba)) + sq_diam * absSq(v);
            float discr = -sqr(det(v, ba)) + sq_diam * v.sqrMagnitude;

            if (discr > 0)
            {
                if (collision)
                {

                    //time = (v * ba + std::sqrt(discr)) / absSq(v);
                    time = (Vector2.Dot(v, ba) + Mathf.Sqrt(discr)) / v.sqrMagnitude;
                    if (time < 0)
                    {
                        time = -rvo_infty;
                    }
                }
                else
                {
                    //time = (v * ba - std::sqrt(discr)) / absSq(v);
                    time = (Vector2.Dot(v, ba) - Mathf.Sqrt(discr)) / v.sqrMagnitude;
                    if (time < 0)
                    {
                        time = rvo_infty;
                    }
                }
            }
            else
            {
                if (collision)
                {
                    time = -rvo_infty;
                }
                else
                {
                    time = rvo_infty;
                }
            }
            return time;
        }

        bool _collision = false;

        internal void CalculateVelocityRVO1(Simulator.WorkerContext context)
        {
            if (locked)
            {
                newVelocity = Vector2.zero;
                return;
            }

            if (context.vos.Length < neighbours.Count + simulator.obstacles.Count)
            {
                context.vos = new VO[Mathf.Max(context.vos.Length * 2, neighbours.Count + simulator.obstacles.Count)];
            }

            Vector2 position2D = new Vector2(position.x, position.z);

            var vos = context.vos;
            var voCount = 0;

            Vector2 optimalVelocity = new Vector2(velocity.x, velocity.z); //_v

            /////////////////////////////////////////////////////////////////////////////////////////RVO1         

            float min_penalty = rvo_infty;// RVO_INFTY;
            Vector2 vCand;
            int _velSampleCount = 250;
            Vector2 _vPref = new Vector2(desiredVelocity.x, desiredVelocity.z);
            float _maxSpeed = maxSpeed;

            float _safetyFactor = 7.5f;
            Vector2 _vNew;

            _collision = false;

            // Select num_samples candidate velocities within the circle of radius _maxSpeed
            for (int n = 0; n < _velSampleCount; ++n)
            {
                if (n == 0)
                {
                    vCand = _vPref;
                }
                else
                {
                    vCand = new Vector2(Random.Range(-maxSpeed, maxSpeed), Random.Range(-maxSpeed, maxSpeed));
                    /* do {
                       vCand = new Vector2( 2.0f* rand() - RAND_MAX, 2.0f*rand() - RAND_MAX);
                     } while (absSq(vCand) > sqr((float) RAND_MAX));
                     vCand *= (_maxSpeed / RAND_MAX);
                     * */
                }

                float dV; // distance between candidate velocity and preferred velocity
                dV = (vCand - _vPref).magnitude;


                // searching for smallest time to collision
                float ct = rvo_infty; // time to collision
                // iterate over neighbors
                for (int o = 0; o < neighbours.Count; o++)
                {
                    float ct_j; // time to collision with agent o
                    Vector2 Vab;
                    //int type = j->second.first;
                    //int id = j->second.second;




                    //if (type == AGENT) 


                    Agent other = neighbours[o];

                    if (other == this) continue;


                    Vector2 otherOptimalVelocity = new Vector2(other.Velocity.x, other.Velocity.z); //other->_V

                    Vector2 otherPos2D = new Vector2(other.position.x, other.position.z);

                    //if (absSq(_p - other->_p) < sqr(_r + other->_r))
                    if ((position2D - otherPos2D).sqrMagnitude < sqr(radius + other.radius))
                    {
                        _collision = true;
                        dV = 0;
                    }
                    else
                    {
                        _collision = false;
                    }


                    //Vab = 2*vCand - _v - other->_v;
                    Vab = 2 * vCand - optimalVelocity - otherOptimalVelocity;

                    //  float time = timeToCollision(_p, Vab, other->_p, _r + other->_r, _collision);
                    float time = timeToCollision(position2D, Vab, otherPos2D, radius + other.radius, _collision);
                    if (_collision)
                    {
                        //ct_j = -std::ceil(time / _sim->_timeStep );
                        ct_j = -Mathf.Ceil(time / simulator.stepTime);

                        //ct_j -= absSq(vCand) / sqr(_maxSpeed);
                        ct_j -= vCand.sqrMagnitude / sqr(_maxSpeed);
                    }
                    else
                    {
                        ct_j = time;
                    }


                    if (ct_j < ct)
                    {
                        ct = ct_j;
                        // pruning search if no better penalty can be obtained anymore for this velocity
                        if (_safetyFactor / ct + dV >= min_penalty)
                        {
                            break;
                        }
                    }
                }

                float penalty = _safetyFactor / ct + dV;
                if (penalty < min_penalty)
                {
                    min_penalty = penalty;
                    _vNew = vCand;
                    Vector2 result = _vNew;
                    if (DebugDraw) DrawCross(result + position2D);
                    newVelocity = To3D(Vector2.ClampMagnitude(result, maxSpeed));
                }
            }//for sample
        }


        internal void CalculateVelocityORIGIN(Simulator.WorkerContext context)
        {
            if (locked)
            {
                newVelocity = Vector2.zero;
                return;
            }

            if (context.vos.Length < neighbours.Count + simulator.obstacles.Count)
            {
                context.vos = new VO[Mathf.Max(context.vos.Length * 2, neighbours.Count + simulator.obstacles.Count)];
            }

            Vector2 position2D = new Vector2(position.x, position.z);

            var vos = context.vos;
            var voCount = 0;

            Vector2 optimalVelocity = new Vector2(velocity.x, velocity.z);

            float inverseAgentTimeHorizon = 1.0f / agentTimeHorizon;

            float wallThickness = simulator.WallThickness;

            float wallWeight = simulator.algorithm == Simulator.SamplingAlgorithm.GradientDecent ? 1 : WallWeight;

            for (int i = 0; i < simulator.obstacles.Count; i++)
            {
                var obstacle = simulator.obstacles[i];
                var vertex = obstacle;
                do
                {

                    if (vertex.ignore || position.y > vertex.position.y + vertex.height || position.y + height < vertex.position.y || (vertex.layer & collidesWith) == 0)
                    {
                        vertex = vertex.next;
                        continue;
                    }

                    float cross = VO.Det(new Vector2(vertex.position.x, vertex.position.z), vertex.dir, position2D);// vertex.dir.x * ( vertex.position.z - position.z ) - vertex.dir.y * ( vertex.position.x - position.x );

                    // Signed distance from the line (not segment), lines are infinite
                    // Usually divided by vertex.dir.magnitude, but that is known to be 1
                    float signedDist = cross;

                    float dotFactor = Vector2.Dot(vertex.dir, position2D - new Vector2(vertex.position.x, vertex.position.z));

                    // It is closest to the segment
                    // if the dotFactor is <= 0 or >= length of the segment
                    // WallThickness*0.1 is added as a margin to avoid false positives when moving along the edges of square obstacles
                    bool closestIsEndpoints = dotFactor <= wallThickness * 0.05f || dotFactor >= (new Vector2(vertex.position.x, vertex.position.z) - new Vector2(vertex.next.position.x, vertex.next.position.z)).magnitude - wallThickness * 0.05f;

                    if (Mathf.Abs(signedDist) < neighbourDist)
                    {
                        if (signedDist <= 0 && !closestIsEndpoints && signedDist > -wallThickness)
                        {
                            // Inside the wall on the "wrong" side
                            vos[voCount] = new VO(position2D, new Vector2(vertex.position.x, vertex.position.z) - position2D, vertex.dir, wallWeight * 2);
                            voCount++;
                        }
                        else if (signedDist > 0)
                        {
                            //Debug.DrawLine (position, (vertex.position+vertex.next.position)*0.5f, Color.yellow);
                            Vector2 p1 = new Vector2(vertex.position.x, vertex.position.z) - position2D;
                            Vector2 p2 = new Vector2(vertex.next.position.x, vertex.next.position.z) - position2D;
                            Vector2 tang1 = (p1).normalized;
                            Vector2 tang2 = (p2).normalized;
                            vos[voCount] = new VO(position2D, p1, p2, tang1, tang2, wallWeight);
                            voCount++;

                        }
                    }
                    vertex = vertex.next;
                } while (vertex != obstacle);
            }

            for (int o = 0; o < neighbours.Count; o++)
            {

                Agent other = neighbours[o];

                if (other == this) continue;

                float maxY = System.Math.Min(position.y + height, other.position.y + other.height);
                float minY = System.Math.Max(position.y, other.position.y);

                //The agents cannot collide since they
                //are on different y-levels
                if (maxY - minY < 0)
                {
                    continue;
                }

                Vector2 otherOptimalVelocity = new Vector2(other.Velocity.x, other.velocity.z);


                float totalRadius = radius + other.radius;

                // Describes a circle on the border of the VO
                //float boundingRadius = totalRadius * inverseAgentTimeHorizon;
                Vector2 voBoundingOrigin = new Vector2(other.position.x, other.position.z) - position2D;

                //float boundingDist = voBoundingOrigin.magnitude;

                Vector2 relativeVelocity = optimalVelocity - otherOptimalVelocity;

                {
                    //voBoundingOrigin *= inverseAgentTimeHorizon;
                    //boundingDist *= inverseAgentTimeHorizon;

                    // Common case, no collision

                    Vector2 voCenter;
                    if (other.locked)
                    {
                        voCenter = otherOptimalVelocity;
                    }
                    else
                    {
                        voCenter = (optimalVelocity + otherOptimalVelocity) * 0.5f;
                    }

                    vos[voCount] = new VO(voBoundingOrigin, voCenter, totalRadius, relativeVelocity, inverseAgentTimeHorizon, 1);
                    voCount++;
                    if (DebugDraw) DrawVO(position2D + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, position2D + voCenter);
                }


            }


            Vector2 result = Vector2.zero;

            if (simulator.algorithm == Simulator.SamplingAlgorithm.GradientDecent)
            {
                if (this.DebugDraw)
                {
                    const int PlotWidth = 40;
                    const float WorldPlotWidth = 15;

                    for (int x = 0; x < PlotWidth; x++)
                    {
                        for (int y = 0; y < PlotWidth; y++)
                        {
                            Vector2 p = new Vector2(x * WorldPlotWidth / PlotWidth, y * WorldPlotWidth / PlotWidth);

                            Vector2 dir = Vector2.zero;
                            float weight = 0;
                            for (int i = 0; i < voCount; i++)
                            {
                                float w = 0;
                                dir += vos[i].Sample(p - position2D, out w);
                                if (w > weight) weight = w;
                            }
                            Vector2 d2 = (new Vector2(desiredVelocity.x, desiredVelocity.z) - (p - position2D));
                            dir += d2 * DesiredVelocityScale;

                            if (d2.magnitude * DesiredVelocityWeight > weight) weight = d2.magnitude * DesiredVelocityWeight;

                            if (weight > 0) dir /= weight;

                            //Vector2 d3 = simulator.SampleDensity (p+position2D);
                            Debug.DrawRay(To3D(p), To3D(d2 * 0.00f), Color.blue);
                            //simulator.Plot (p, Rainbow(weight*simulator.colorScale));

                            float sc = 0;
                            Vector2 p0 = p - Vector2.one * WorldPlotWidth * 0.5f;
                            Vector2 p1 = Trace(vos, voCount, p0, 0.01f, out sc);
                            if ((p0 - p1).sqrMagnitude < Sqr(WorldPlotWidth / PlotWidth) * 2.6f)
                            {
                                Debug.DrawRay(To3D(p1 + position2D), Vector3.up * 1, Color.red);
                            }
                        }
                    }
                }

                //if ( debug ) {
                float best = float.PositiveInfinity;

                float cutoff = new Vector2(velocity.x, velocity.z).magnitude * simulator.qualityCutoff;

                //for ( int i = 0; i < 10; i++ ) {
                {
                    result = Trace(vos, voCount, new Vector2(desiredVelocity.x, desiredVelocity.z), cutoff, out best);
                    if (DebugDraw) DrawCross(result + position2D, Color.yellow, 0.5f);
                }

                // Can be uncommented for higher quality local avoidance
                /*for ( int i = 0; i < 3; i++ ) {
                    Vector2 p = desiredVelocity + new Vector2(Mathf.Cos(Mathf.PI*2*(i/3.0f)), Mathf.Sin(Mathf.PI*2*(i/3.0f)));
                    float score;
                    Vector2 res = Trace ( vos, voCount, p, velocity.magnitude*simulator.qualityCutoff, out score );
					
                    if ( score < best ) {
                        //if ( score < best*0.9f ) Debug.Log ("Better " + score + " < " + best);
                        result = res;
                        best = score;
                    }
                }*/

                {
                    Vector2 p = this.Velocity;
                    float score;
                    Vector2 res = Trace(vos, voCount, p, cutoff, out score);

                    if (score < best)
                    {
                        //if ( score < best*0.9f ) Debug.Log ("Better " + score + " < " + best);
                        result = res;
                        best = score;
                    }
                    if (DebugDraw) DrawCross(res + position2D, Color.magenta, 0.5f);
                }
            }
            else
            {
                // Adaptive sampling

                Vector2[] samplePos = context.samplePos;
                float[] sampleSize = context.sampleSize;
                int samplePosCount = 0;


                Vector2 desired2D = new Vector2(desiredVelocity.x, desiredVelocity.z);
                float sampleScale = Mathf.Max(radius, Mathf.Max(desired2D.magnitude, Velocity.magnitude));
                samplePos[samplePosCount] = desired2D;
                sampleSize[samplePosCount] = sampleScale * 0.3f;
                samplePosCount++;

                const float GridScale = 0.3f;

                // Initial 9 samples
                samplePos[samplePosCount] = optimalVelocity;
                sampleSize[samplePosCount] = sampleScale * GridScale;
                samplePosCount++;

                {
                    Vector2 fw = optimalVelocity * 0.5f;
                    Vector2 rw = new Vector2(fw.y, -fw.x);

                    const int Steps = 8;
                    for (int i = 0; i < Steps; i++)
                    {
                        samplePos[samplePosCount] = rw * Mathf.Sin(i * Mathf.PI * 2 / Steps) + fw * (1 + Mathf.Cos(i * Mathf.PI * 2 / Steps));
                        sampleSize[samplePosCount] = (1.0f - (Mathf.Abs(i - Steps * 0.5f) / Steps)) * sampleScale * 0.5f;
                        samplePosCount++;
                    }

                    const float InnerScale = 0.6f;
                    fw *= InnerScale;
                    rw *= InnerScale;

                    const int Steps2 = 6;
                    for (int i = 0; i < Steps2; i++)
                    {
                        samplePos[samplePosCount] = rw * Mathf.Cos((i + 0.5f) * Mathf.PI * 2 / Steps2) + fw * ((1.0f / InnerScale) + Mathf.Sin((i + 0.5f) * Mathf.PI * 2 / Steps2));
                        sampleSize[samplePosCount] = sampleScale * 0.3f;
                        samplePosCount++;
                    }

                    const float TargetScale = 0.2f;

                    const int Steps3 = 6;
                    for (int i = 0; i < Steps3; i++)
                    {
                        samplePos[samplePosCount] = optimalVelocity + new Vector2(sampleScale * TargetScale * Mathf.Cos((i + 0.5f) * Mathf.PI * 2 / Steps3), sampleScale * TargetScale * Mathf.Sin((i + 0.5f) * Mathf.PI * 2 / Steps3));
                        sampleSize[samplePosCount] = sampleScale * TargetScale * 2;
                        samplePosCount++;
                    }
                }

                samplePos[samplePosCount] = optimalVelocity * 0.5f;
                sampleSize[samplePosCount] = sampleScale * 0.4f;
                samplePosCount++;

                const int KeepCount = Simulator.WorkerContext.KeepCount;
                Vector2[] bestPos = context.bestPos;
                float[] bestSizes = context.bestSizes;
                float[] bestScores = context.bestScores;

                for (int i = 0; i < KeepCount; i++)
                {
                    bestScores[i] = float.PositiveInfinity;
                }
                bestScores[KeepCount] = float.NegativeInfinity;

                Vector2 bestEver = optimalVelocity;
                float bestEverScore = float.PositiveInfinity;

                for (int sub = 0; sub < 3; sub++)
                {

                    for (int i = 0; i < samplePosCount; i++)
                    {

                        float score = 0;
                        for (int vo = 0; vo < voCount; vo++)
                        {
                            score = System.Math.Max(score, vos[vo].ScalarSample(samplePos[i]));
                        }
                        // Note that velocity is a vector and speed is a scalar, not the same thing
                        float bonusForDesiredVelocity = (samplePos[i] - desired2D).magnitude;

                        // This didn't work out as well as I though
                        // Code left here because I might reenable it later
                        //float bonusForDesiredSpeed = Mathf.Abs (samplePos[i].magnitude - desired2D.magnitude);

                        float biasedScore = score + bonusForDesiredVelocity * DesiredVelocityWeight;// + bonusForDesiredSpeed*0;
                        //score += bonusForDesiredVelocity * 0.001f;//origin
                        score += bonusForDesiredVelocity * weightForDesiredVel * 0.3f;

                        if (DebugDraw)
                        {
                            DrawCross(position2D + samplePos[i], Rainbow(Mathf.Log(score + 1) * 5), sampleSize[i] * 0.5f);
                        }

                        if (biasedScore < bestScores[0])
                        {
                            for (int j = 0; j < KeepCount; j++)
                            {
                                if (biasedScore >= bestScores[j + 1])
                                {
                                    bestScores[j] = biasedScore;
                                    bestSizes[j] = sampleSize[i];
                                    bestPos[j] = samplePos[i];
                                    break;
                                }
                            }
                        }

                        if (score < bestEverScore)
                        {
                            bestEver = samplePos[i];
                            bestEverScore = score;

                            if (score == 0)
                            {
                                sub = 100;
                                break;
                            }
                        }
                    }

                    samplePosCount = 0;

                    for (int i = 0; i < KeepCount; i++)
                    {
                        Vector2 p = bestPos[i];
                        float s = bestSizes[i];
                        bestScores[i] = float.PositiveInfinity;

                        const float Half = 0.6f;

                        float offset = s * Half * 0.5f;

                        samplePos[samplePosCount + 0] = (p + new Vector2(+offset, +offset));
                        samplePos[samplePosCount + 1] = (p + new Vector2(-offset, +offset));
                        samplePos[samplePosCount + 2] = (p + new Vector2(-offset, -offset));
                        samplePos[samplePosCount + 3] = (p + new Vector2(+offset, -offset));

                        s *= s * Half;
                        sampleSize[samplePosCount + 0] = (s);
                        sampleSize[samplePosCount + 1] = (s);
                        sampleSize[samplePosCount + 2] = (s);
                        sampleSize[samplePosCount + 3] = (s);
                        samplePosCount += 4;
                    }
                }

                result = bestEver;
            }


            if (DebugDraw) DrawCross(result + position2D);


            newVelocity = To3D(Vector2.ClampMagnitude(result, maxSpeed));

        }

        public void CalculateVelocityRVO2()
        {
            orcaLines.Clear();

            if (locked)
            {
                newVelocity = new Vector3(0, 0, 0);
                return;
            }

            Vector2 velocity2D = new Vector2(Velocity.x, Velocity.z);

#if ASTARDEBUG
			if (DebugDraw) {
				Debug.DrawRay (position,desiredVelocity,Color.green);
				Debug.DrawRay (position,Velocity,Color.blue);
			}
#endif
            //no obstacles now
            for (int i = 0; i < obstacles.Count; i++)
            {
                
            }


            //Phase II

            float invTimeHorizon = 1.0f / agentTimeHorizon;

            int numObstacleLines = orcaLines.Count;

            /* Create agent ORCA lines. */
            for (int i = 0; i < neighbours.Count; ++i)
            {
                Agent other = neighbours[i];

                //Debug.DrawLine (position,other.position,Color.red);

                float maxY = System.Math.Min(position.y + height, other.position.y + other.height);
                float minY = System.Math.Max(position.y, other.position.y);

                //The agents cannot collide since they
                //are on different y-levels
                if (maxY - minY < 0)
                {
                    continue;
                }

                Vector3 relativePosition3D = (other.position - position);
                Vector3 relativeVelocity3D = Velocity - other.Velocity;

                Vector2 relativePosition = new Vector2(relativePosition3D.x, relativePosition3D.z);
                Vector2 relativeVelocity = new Vector2(relativeVelocity3D.x, relativeVelocity3D.z);

                float distSq = relativePosition.sqrMagnitude;
                float combinedRadius = radius + other.radius;
                float combinedRadiusSq = combinedRadius * combinedRadius;

                Line line;
                Vector2 u;

                if (distSq > combinedRadiusSq)
                {
                    /* No collision. */
                    Vector2 w = relativeVelocity - invTimeHorizon * relativePosition;
                    /* Vector from cutoff center to relative velocity. */
                    float wLengthSq = w.sqrMagnitude;

                    float dotProduct1 = Vector2.Dot(w, relativePosition);

                    if (dotProduct1 < 0.0f && dotProduct1 * dotProduct1 > combinedRadiusSq * wLengthSq)
                    {
                        /* Project on cut-off circle. */
                        float wLength = (float)System.Math.Sqrt(wLengthSq);

                        //Normalized w
                        Vector2 unitW = w / wLength;

                        line.dir = new Vector2(unitW.y, -unitW.x);
                        u = (combinedRadius * invTimeHorizon - wLength) * unitW;

#if ASTARDEBUG
						if (DebugDraw)
							Debug.DrawRay (position, u,Color.red);	
#endif
                    }
                    else
                    {
                        /* Project on legs. */
                        float leg = (float)System.Math.Sqrt(distSq - combinedRadiusSq);

                        if (det(relativePosition, w) > 0.0f)
                        {
                            /* Project on left leg. */
                            line.dir = new Vector2(relativePosition.x * leg - relativePosition.y * combinedRadius, relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                        }
                        else
                        {
                            /* Project on right leg. */
                            line.dir = -new Vector2(relativePosition.x * leg + relativePosition.y * combinedRadius, -relativePosition.x * combinedRadius + relativePosition.y * leg) / distSq;
                        }

                        float dotProduct2 = Vector2.Dot(relativeVelocity, line.dir);

                        u = dotProduct2 * line.dir - relativeVelocity;
                    }
                }
                else
                {
                    /* Collision. Project on cut-off circle of time timeStep. */
                    float invTimeStep = 1.0f / simulator.DeltaTime;

                    /* Vector from cutoff center to relative velocity. */
                    Vector2 w = relativeVelocity - invTimeStep * relativePosition;
                    //vel - pos/delta

                    float wLength = w.magnitude;

                    //Normalized w
                    Vector2 unitW = w / wLength;

                    line.dir = new Vector2(unitW.y, -unitW.x);
                    u = (combinedRadius * invTimeStep - wLength) * unitW;


                }

                if (other.locked)
                    line.point = velocity2D + 1.0f * u;

                line.point = velocity2D + 0.5f * u;

                orcaLines.Add(line);
            }

#if ASTARDEBUG
			if (DebugDraw) {
				for (int i=0;i<orcaLines.Count;i++) {
					Debug.DrawRay (position+new Vector3 (orcaLines[i].point.x,0,orcaLines[i].point.y)-new Vector3 (orcaLines[i].dir.x,0,orcaLines[i].dir.y)*100,new Vector3 (orcaLines[i].dir.x,0,orcaLines[i].dir.y)*200,Color.blue);
				}
			}
#endif

            Vector2 resultVelocity = Vector2.zero;

            int lineFail = LinearProgram2(orcaLines, maxSpeed, new Vector2(desiredVelocity.x, desiredVelocity.z), false, ref resultVelocity);

            if (lineFail < orcaLines.Count)
            {
                LinearProgram3(orcaLines, numObstacleLines, lineFail, maxSpeed, ref resultVelocity);
            }

            newVelocity = new Vector3(resultVelocity.x, 0, resultVelocity.y);
            newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);

#if ASTARDEBUG
			if (DebugDraw)
				Debug.DrawRay (position,newVelocity,Color.magenta);
#endif
        }

        bool LinearProgram1(List<Line> lines, int lineNo, float radius, Vector2 optVelocity, bool directionOpt, ref Vector2 result)
        {
            float dotProduct = Vector2.Dot(lines[lineNo].point, lines[lineNo].dir);
            float discriminant = dotProduct * dotProduct + radius * radius - lines[lineNo].point.sqrMagnitude;

            if (discriminant < 0.0f)
            {
                /* Max speed circle fully invalidates line lineNo. */
                return false;
            }

            float sqrtDiscriminant = (float)System.Math.Sqrt(discriminant);
            float tLeft = -dotProduct - sqrtDiscriminant;
            float tRight = -dotProduct + sqrtDiscriminant;

            for (int i = 0; i < lineNo; i++)
            {
                //Calculate Intersection
                float denominator = det(lines[lineNo].dir, lines[i].dir);
                float numerator = det(lines[i].dir, lines[lineNo].point - lines[i].point);

                if (System.Math.Abs(denominator) <= float.Epsilon)
                {
                    /* Lines lineNo and i are (almost) parallel. */
                    if (numerator < 0.0f)
                    {
                        return false;
                    }
                    else
                    {
                        continue;
                    }
                }

                float t = numerator / denominator;

                if (denominator >= 0.0f)
                {
                    /* Line i bounds line lineNo on the right. */
                    tRight = System.Math.Min(tRight, t);
                }
                else
                {
                    /* Line i bounds line lineNo on the left. */
                    tLeft = System.Math.Max(tLeft, t);
                }

                if (tLeft > tRight)
                {
                    return false;
                }
            }

            if (directionOpt)
            {
                /* Optimize direction. */
                if (Vector2.Dot(optVelocity, lines[lineNo].dir) > 0.0f)
                {
                    /* Take right extreme. */
                    result = lines[lineNo].point + tRight * lines[lineNo].dir;
                }
                else
                {
                    /* Take left extreme. */
                    result = lines[lineNo].point + tLeft * lines[lineNo].dir;
                }
            }
            else
            {
                /* Optimize closest point. */
                float t = Vector2.Dot(lines[lineNo].dir, optVelocity - lines[lineNo].point);

                if (t < tLeft)
                {
                    result = lines[lineNo].point + tLeft * lines[lineNo].dir;
                }
                else if (t > tRight)
                {
                    result = lines[lineNo].point + tRight * lines[lineNo].dir;
                }
                else
                {
                    result = lines[lineNo].point + t * lines[lineNo].dir;
                }
            }

            return true;
        }

        int LinearProgram2(List<Line> lines, float radius, Vector2 optVelocity, bool directionOpt, ref Vector2 result)
        {
            if (directionOpt)
            {
                /* Optimize direction. Note that the optimization velocity is of unit
                 * length in this case.
                 */
                result = optVelocity * radius;
            }
            else if (optVelocity.sqrMagnitude > radius * radius)
            {
                /* Optimize closest point and outside circle. */
                result = optVelocity.normalized * radius;
            }
            else
            {
                /* Optimize closest point and inside circle. */
                result = optVelocity;
            }

            for (int i = 0; i < lines.Count; ++i)
            {
                if (det(lines[i].dir, lines[i].point - result) > 0.0f)
                {
                    /* Result does not satisfy constraint i. Compute new optimal result. */
                    Vector2 tempResult = result;
                    if (!LinearProgram1(lines, i, radius, optVelocity, directionOpt, ref result))
                    {
                        result = tempResult;
                        return i;
                    }
                }
            }

            return lines.Count;
        }

        void LinearProgram3(List<Line> lines, int numObstLines, int beginLine, float radius, ref Vector2 result)
        {
            float distance = 0.0f;

            for (int i = beginLine; i < lines.Count; i++)
            {
                if (det(lines[i].dir, lines[i].point - result) > distance)
                {
                    /* Result does not satisfy constraint of line i. */
                    //std.vector<Line> projLines(lines.begin(), lines.begin() + numObstLines);
                    projLines.Clear();

                    for (int j = 0; j < numObstLines; ++j)
                    {
                        projLines.Add(lines[j]);
                    }

                    for (int j = numObstLines; j < i; ++j)
                    {
                        Line line;

                        float determinant = det(lines[i].dir, lines[j].dir);

                        if (System.Math.Abs(determinant) <= float.Epsilon)
                        {
                            /* Line i and line j are parallel. */
                            if (Vector2.Dot(lines[i].dir, lines[j].dir) > 0.0f)
                            {
                                /* Line i and line j point in the same direction. */
                                continue;
                            }
                            else
                            {
                                /* Line i and line j point in opposite direction. */
                                line.point = 0.5f * (lines[i].point + lines[j].point);
                            }
                        }
                        else
                        {
                            line.point = lines[i].point + (det(lines[j].dir, lines[i].point - lines[j].point) / determinant) * lines[i].dir;
                        }

                        line.dir = (lines[j].dir - lines[i].dir).normalized;
                        projLines.Add(line);
                    }

                    Vector2 tempResult = result;
                    if (LinearProgram2(projLines, radius, new Vector2(-lines[i].dir.y, lines[i].dir.x), true, ref result) < projLines.Count)
                    {
                        /* This should in principle not happen.  The result is by definition
                         * already in the feasible region of this linear program. If it fails,
                         * it is due to small floating point error, and the current result is
                         * kept.
                         */
                        result = tempResult;
                    }

                    distance = det(lines[i].dir, lines[i].point - result);
                }
            }
        }

        //CalculateVelocity for group
        internal void CalculateVelocity(Simulator.WorkerContext context)
        {
            if (simulator.usingRVO2){
                CalculateVelocityRVO2();
            }
            else if(simulator.usingRVO1){
                CalculateVelocityRVO1(context);
            }else
            if (!simulator.simulateGroup)
            {
                CalculateVelocityORIGIN(context);

            }
            else //simulateGroup
            {


                if (locked)
                {
                    newVelocity = Vector2.zero;
                    return;
                }

                if (context.vos.Length < neighbours.Count + simulator.obstacles.Count + 1)
                {
                    context.vos = new VO[Mathf.Max(context.vos.Length * 2, neighbours.Count + simulator.obstacles.Count + 1)]; //+1 new add for reaching goal by dist
                }
                if (context.vgs.Length < groupNeighbours.Count + 2)
                {
                    context.vgs = new VG[Mathf.Max(context.vgs.Length * 2, groupNeighbours.Count + 2)]; //+1 new add for large group +1 new add for reaching goal by dist
                }

                Vector2 position2D = new Vector2(position.x, position.z);

                var vos = context.vos;
                var voCount = 0;
                var vgs = context.vgs;
                var vgCount = 0;
                // var voaCount = 0;
                Vector2 optimalVelocity = new Vector2(velocity.x, velocity.z);

                float inverseAgentTimeHorizon = 1.0f / agentTimeHorizon;
                bool isCollision = false;


                float wallThickness = simulator.WallThickness;

                float wallWeight = simulator.algorithm == Simulator.SamplingAlgorithm.GradientDecent ? 1 : WallWeight;

                for (int i = 0; i < simulator.obstacles.Count; i++)
                {
                    var obstacle = simulator.obstacles[i];
                    var vertex = obstacle;
                    do
                    {

                        if (vertex.ignore || position.y > vertex.position.y + vertex.height || position.y + height < vertex.position.y || (vertex.layer & collidesWith) == 0)
                        {
                            vertex = vertex.next;
                            continue;
                        }

                        float cross = VO.Det(new Vector2(vertex.position.x, vertex.position.z), vertex.dir, position2D);// vertex.dir.x * ( vertex.position.z - position.z ) - vertex.dir.y * ( vertex.position.x - position.x );

                        // Signed distance from the line (not segment), lines are infinite
                        // Usually divided by vertex.dir.magnitude, but that is known to be 1
                        float signedDist = cross;

                        float dotFactor = Vector2.Dot(vertex.dir, position2D - new Vector2(vertex.position.x, vertex.position.z));

                        // It is closest to the segment
                        // if the dotFactor is <= 0 or >= length of the segment
                        // WallThickness*0.1 is added as a margin to avoid false positives when moving along the edges of square obstacles
                        bool closestIsEndpoints = dotFactor <= wallThickness * 0.05f || dotFactor >= (new Vector2(vertex.position.x, vertex.position.z) - new Vector2(vertex.next.position.x, vertex.next.position.z)).magnitude - wallThickness * 0.05f;

                        if (Mathf.Abs(signedDist) < neighbourDist)
                        {
                            if (signedDist <= 0 && !closestIsEndpoints && signedDist > -wallThickness)
                            {
                                // Inside the wall on the "wrong" side
                                vos[voCount] = new VO(position2D, new Vector2(vertex.position.x, vertex.position.z) - position2D, vertex.dir, wallWeight * 2);
                                voCount++;
                            }
                            else if (signedDist > 0)
                            {
                                //Debug.DrawLine (position, (vertex.position+vertex.next.position)*0.5f, Color.yellow);
                                Vector2 p1 = new Vector2(vertex.position.x, vertex.position.z) - position2D;
                                Vector2 p2 = new Vector2(vertex.next.position.x, vertex.next.position.z) - position2D;
                                Vector2 tang1 = (p1).normalized;
                                Vector2 tang2 = (p2).normalized;
                                vos[voCount] = new VO(position2D, p1, p2, tang1, tang2, wallWeight);
                                voCount++;

                            }
                        }
                        vertex = vertex.next;
                    } while (vertex != obstacle);
                }

                for (int o = 0; o < neighbours.Count; o++)
                {

                    Agent other = neighbours[o];

                    if (other == this) continue;

                    float maxY = System.Math.Min(position.y + height, other.position.y + other.height);
                    float minY = System.Math.Max(position.y, other.position.y);

                    //The agents cannot collide since they
                    //are on different y-levels
                    if (maxY - minY < 0)
                    {
                        continue;
                    }

                    Vector2 otherOptimalVelocity = new Vector2(other.Velocity.x, other.velocity.z);


                    float totalRadius = radius + other.radius;

                    //new ADD for dynamic VO radius
                    if (isDVOR)
                    {
                        if (agentID < 0||other.agentID<0|| agentID >= 0 && other.agentID>=0 && simulator.relationMatrix[agentID][other.agentID] == 0)
                        {
                            totalRadius = radius * dvorRangefactor + other.radius;
                            inverseAgentTimeHorizon /= dvotRangefactor;
                        }
                        else
                        {
                            totalRadius = radius + other.radius;
                            inverseAgentTimeHorizon = 1.0f / agentTimeHorizon;
                        }
                    }


                    // Describes a circle on the border of the VO
                    //float boundingRadius = totalRadius * inverseAgentTimeHorizon;
                    Vector2 voBoundingOrigin = new Vector2(other.position.x, other.position.z) - position2D;

                    //float boundingDist = voBoundingOrigin.magnitude;

                    Vector2 relativeVelocity = optimalVelocity - otherOptimalVelocity;

                    {
                        //voBoundingOrigin *= inverseAgentTimeHorizon;
                        //boundingDist *= inverseAgentTimeHorizon;

                        // Common case, no collision

                        Vector2 voCenter;
                        if (other.locked)
                        {
                            voCenter = otherOptimalVelocity;
                        }
                        else
                        {
                            voCenter = (optimalVelocity + otherOptimalVelocity) * 0.5f;
                        }

                        vos[voCount] = new VO(voBoundingOrigin, voCenter, totalRadius, relativeVelocity, inverseAgentTimeHorizon, 1f, position, DebugDraw, ref isCollision);
                        curVOs.Add(vos[voCount]);
                        voCount++;
                        //voaCount++;
                        if (DebugDraw) DrawVO(position2D + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, position2D + voCenter);
                    }


                }
                if (isDistGoal)
                {
                    Vector2 voBoundingOrigin = new Vector2(goalPos.x, goalPos.z) - position2D;
                    vos[voCount] = new VO(voBoundingOrigin, new Vector2(goalVel.x, goalVel.z), radius + minGoalR, optimalVelocity, 1f, 1f, position, DebugDraw, ref isCollision);
                    curVOs.Add(vos[voCount]);
                    voCount++;
                }

                isHigestInfluence = true;
                for (int o = 0; o < groupNeighbours.Count; o++)
                {//VG



                    Agent other = groupNeighbours[o];

                    if (other == this) continue;

                    float maxY = System.Math.Min(position.y + height, other.position.y + other.height);
                    float minY = System.Math.Max(position.y, other.position.y);

                    //The agents cannot collide since they
                    //are on different y-levels
                    if (maxY - minY < 0)
                    {
                        continue;
                    }

                    ///////////////////////////
                    if (weakTarget)
                    {
                        if (simulator.relationMatrix[agentID][other.agentID] > simulator.relationMatrix[other.agentID][agentID])  //more influenced
                        {
                            isHigestInfluence = false;
                        }
                    }
                    if (isNaughtyChildren)
                    {
                        adultPos = groupNeighbours[0].position;
                        adultVel = groupNeighbours[0].velocity;
                        neighbourRadius = maxAdultR;
                    }
                    //////////////////////////////////
                    Vector2 otherOptimalVelocity = new Vector2(other.Velocity.x, other.Velocity.z);


                    //float totalRadius = radius + other.radius;

                    // Describes a circle on the border of the VO
                    //float boundingRadius = totalRadius * inverseAgentTimeHorizon;
                    Vector2 voBoundingOrigin = new Vector2(other.position.x, other.position.z) - position2D;

                    //float boundingDist = voBoundingOrigin.magnitude;

                    Vector2 relativeVelocity = optimalVelocity - otherOptimalVelocity;


                    //	if(!isCollision&&other.groupID>0&&other.groupID==this.groupID&&vgCount<groupNeighbourNum){
                    //	if(other.groupID>0&&other.groupID==this.groupID&&vgCount<groupNeighbourNum){

                    //if(vgCount<groupNeighbours.Count)
                    {

                        float groupNeighbourR;
                        if (agentID >= 0 && other.agentID >= 0)
                            //groupNeighbourR=neighbourRadius*(15f-14f*simulator.relationMatrix[agentID][other.agentID]);
                            groupNeighbourR = neighbourRadius;
                        //groupNeighbourR=neighbourRadius*Mathf.Pow (10,2-simulator.relationMatrix[agentID][other.agentID]);
                        else
                            continue;


                        float VGoffsetDist = offsetDist;
                        //if(Line2Constraint){
                        //    if (groupNeighbours.Count > 1)
                        //    {
                        //        if (simulator.relationMatrix[agentID][other.agentID] > simulator.relationMatrix[agentID][groupNeighbours[(o == 0 ? 1 : 0)].agentID])
                        //        {
                        //            vertForm = true;
                        //            VGoffsetDist = offsetDist;
                        //        }
                        //        else
                        //        {
                        //            vertForm = false;
                        //        }
                        //    }
                        //}

						if(isGroupInGroup&&simulator.relationMatrix[agentID][other.agentID]==1f){

							if(isGroupInGroupGuide){
								Vector2 dirDirector;
								dirDirector = (new Vector2(other.desiredVelocity.x, other.desiredVelocity.z)).normalized;
								dirDirector = new Vector2(dirDirector.y, -dirDirector.x);
								VGoffsetDist=-VGoffsetDist;
								vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR*increaseRmax, dirDirector, 1f / groupHorizonTime, other.agentID, VGoffsetDist);

							}else
								vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR*increaseRmax, relativeVelocity, 1f / groupHorizonTime, other.agentID);


						}else

                        if (formConstraint)
                        {
                            Vector2 dirDirector;
                            if (desiredVelocity.magnitude > 0.1f)
                                dirDirector = (new Vector2(desiredVelocity.x, desiredVelocity.z)).normalized;
                            else if (other.desiredVelocity.magnitude > 0.1f)
                            {
                                dirDirector = (new Vector2(other.desiredVelocity.x, other.desiredVelocity.z)).normalized;
                            }
                            else
                            {
                                dirDirector = (new Vector2(velocity.x, velocity.z)).normalized;
                            }

                            vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR, dirDirector, 1f / groupHorizonTime, other.agentID, VGoffsetDist);
                            curVGs.Add(vgs[vgCount]);
                            vgCount++;
                            //if (vertForm)
                            //{
                            //    dirDirector = new Vector2(dirDirector.y, -dirDirector.x);
                            //    curVGs.Add(vgs[vgCount]);
                            //    vgCount++;
                            //    vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR, dirDirector, 1f / groupHorizonTime, other.agentID, VGoffsetDist);
                            //}
                        }
                        /*   else if (vertForm)
                           {
                               Vector2 dirDirector;
                               if (other.desiredVelocity.magnitude > 0.1f)
                               {
                                   dirDirector = (new Vector2(other.desiredVelocity.z, -other.desiredVelocity.x)).normalized;
                                   vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR, dirDirector, 1f / groupHorizonTime, other.agentID, offsetDist);
                               }
                           } */
                        //else
                        //{
                            
                        //}
                        // vgs[vgCount] = new VG(voBoundingOrigin, (optimalVelocity + otherOptimalVelocity) * 0.5f, groupNeighbourR, relativeVelocity, 1f/groupHorizonTime, other.agentID);

                        //vgs[vgCount]= new VG( voBoundingOrigin, otherOptimalVelocity, groupNeighbourR, relativeVelocity, inverseAgentTimeHorizon/10);
                        vgs[vgCount] = new VG(voBoundingOrigin, otherOptimalVelocity, groupNeighbourR, relativeVelocity, 1f / groupHorizonTime, other.agentID);
                        curVGs.Add(vgs[vgCount]);
                        vgCount++;
                    }
                }
                if (largeGroupConstraint) //new add for largeGroup
                {
                    //Vector2 optimalVelocity = new Vector2(velocity.x, velocity.z);
                    Vector2 voBoundingOrigin = new Vector2(groupCenter.x, groupCenter.z) - position2D;
                    // Vector2 groupVel2=new Vector2(groupVel.x,groupVel.z);
                    // Vector2 relativeVelocity = optimalVelocity - groupVel2;
                    vgs[vgCount] = new VG(voBoundingOrigin, Vector2.zero, largeGroupR, optimalVelocity, 1f / groupHorizonTime, -1);
                    curVGs.Add(vgs[vgCount]);
                    vgCount++;
                }
                if (isDistGoal)//new add for reaching goal by dist
                {
                    Vector2 voBoundingOrigin = new Vector2(goalPos.x, goalPos.z) - position2D;

                    if (formDistGoal)
                    {
                        Vector2 dirDirector = new Vector2(goalVel.x,goalVel.z);
                        dirDirector.Normalize();
                        dirDirector = new Vector2(dirDirector.y,-dirDirector.x);

                         vgs[vgCount] = new VG(voBoundingOrigin, new Vector2(goalVel.x,goalVel.z), maxGoalR, dirDirector, 1f / reachGoalTime, -1, -offsetDist);
                    }else{                        
                         vgs[vgCount] = new VG(voBoundingOrigin, new Vector2(goalVel.x,goalVel.z), maxGoalR, optimalVelocity, 1f / reachGoalTime, -1);
                    }
                        curVGs.Add(vgs[vgCount]);
                    vgCount++;
                }

                Vector2 result = Vector2.zero;

                if (simulator.algorithm == Simulator.SamplingAlgorithm.GradientDecent)
                //if(false)
                {
                    if (this.DebugDraw)
                    {
                        const int PlotWidth = 40;
                        const float WorldPlotWidth = 15;

                        for (int x = 0; x < PlotWidth; x++)
                        {
                            for (int y = 0; y < PlotWidth; y++)
                            {
                                Vector2 p = new Vector2(x * WorldPlotWidth / PlotWidth, y * WorldPlotWidth / PlotWidth);

                                Vector2 dir = Vector2.zero;
                                float weight = 0;
                                for (int i = 0; i < voCount; i++)
                                {
                                    float w = 0;
                                    dir += vos[i].Sample(p - position2D, out w);
                                    if (w > weight) weight = w;
                                }
                                Vector2 d2 = (new Vector2(desiredVelocity.x, desiredVelocity.z) - (p - position2D));
                                dir += d2 * DesiredVelocityScale;

                                if (d2.magnitude * DesiredVelocityWeight > weight) weight = d2.magnitude * DesiredVelocityWeight;

                                if (weight > 0) dir /= weight;

                                //Vector2 d3 = simulator.SampleDensity (p+position2D);
                                Debug.DrawRay(To3D(p), To3D(d2 * 0.00f), Color.blue);
                                //simulator.Plot (p, Rainbow(weight*simulator.colorScale));

                                float sc = 0;
                                Vector2 p0 = p - Vector2.one * WorldPlotWidth * 0.5f;
                                Vector2 p1 = Trace(vos, voCount, p0, 0.01f, out sc);
                                if ((p0 - p1).sqrMagnitude < Sqr(WorldPlotWidth / PlotWidth) * 2.6f)
                                {
                                    Debug.DrawRay(To3D(p1 + position2D), Vector3.up * 1, Color.red);
                                }
                            }
                        }
                    }

                    //if ( debug ) {
                    float best = float.PositiveInfinity;

                    float cutoff = new Vector2(velocity.x, velocity.z).magnitude * simulator.qualityCutoff;

                    //for ( int i = 0; i < 10; i++ ) {
                    {
                        //result = Trace ( vos, voCount, new Vector2(desiredVelocity.x, desiredVelocity.z), cutoff, out best );
                        result = TraceVOnVG(vos, voCount, vgs, vgCount, new Vector2(desiredVelocity.x, desiredVelocity.z), cutoff, out best);
                        if (DebugDraw)
                        {
                            DrawCross(result + position2D, Color.yellow, 0.5f);
                            Debug.DrawRay(position, To3D(result), Color.blue);
                        }
                    }

                    // Can be uncommented for higher quality local avoidance
                    /*for ( int i = 0; i < 3; i++ ) {
                        Vector2 p = desiredVelocity + new Vector2(Mathf.Cos(Mathf.PI*2*(i/3.0f)), Mathf.Sin(Mathf.PI*2*(i/3.0f)));
                        float score;
                        Vector2 res = Trace ( vos, voCount, p, velocity.magnitude*simulator.qualityCutoff, out score );
					
                        if ( score < best ) {
                            //if ( score < best*0.9f ) Debug.Log ("Better " + score + " < " + best);
                            result = res;
                            best = score;
                        }
                    }*/

                    {
                        Vector2 p = this.Velocity;
                        float score;
                        //Vector2 res = Trace ( vos, voCount, p, cutoff, out score );
                        Vector2 res = TraceVOnVG(vos, voCount, vgs, vgCount, p, cutoff, out score);
                        if (score < best)
                        {
                            //if ( score < best*0.9f ) Debug.Log ("Better " + score + " < " + best);
                            result = res;
                            best = score;
                        }
                        if (DebugDraw) DrawCross(res + position2D, Color.magenta, 0.5f);
                    }
                }
                else
                {
                    // Adaptive sampling

                    Vector2[] samplePos = context.samplePos;
                    float[] sampleSize = context.sampleSize;
                    int samplePosCount = 0;


                    Vector2 desired2D = new Vector2(desiredVelocity.x, desiredVelocity.z);
                    float sampleScale = Mathf.Max(radius, Mathf.Max(desired2D.magnitude, Velocity.magnitude));
                    if (sampleScale <= radius)
                    {
                        sampleScale *= 4;
                    }
                    samplePos[samplePosCount] = desired2D;
                    sampleSize[samplePosCount] = sampleScale * 0.3f;
                    samplePosCount++;

                    const float GridScale = 0.3f;

                    // Initial 9 samples
                    samplePos[samplePosCount] = optimalVelocity;
                    sampleSize[samplePosCount] = sampleScale * GridScale;
                    samplePosCount++;

                    {
                        Vector2 fw = optimalVelocity * 0.5f;
                        Vector2 rw = new Vector2(fw.y, -fw.x);

                        const int Steps = 8;
                        for (int i = 0; i < Steps; i++)
                        {
                            samplePos[samplePosCount] = rw * Mathf.Sin(i * Mathf.PI * 2 / Steps) + fw * (1 + Mathf.Cos(i * Mathf.PI * 2 / Steps));
                            sampleSize[samplePosCount] = (1.0f - (Mathf.Abs(i - Steps * 0.5f) / Steps)) * sampleScale * 0.5f;
                            samplePosCount++;
                        }

                        const float InnerScale = 0.6f;
                        fw *= InnerScale;
                        rw *= InnerScale;

                        const int Steps2 = 6;
                        for (int i = 0; i < Steps2; i++)
                        {
                            samplePos[samplePosCount] = rw * Mathf.Cos((i + 0.5f) * Mathf.PI * 2 / Steps2) + fw * ((1.0f / InnerScale) + Mathf.Sin((i + 0.5f) * Mathf.PI * 2 / Steps2));
                            sampleSize[samplePosCount] = sampleScale * 0.3f;
                            samplePosCount++;
                        }

                        const float TargetScale = 0.2f;

                        const int Steps3 = 6;
                        for (int i = 0; i < Steps3; i++)
                        {
                            samplePos[samplePosCount] = optimalVelocity + new Vector2(sampleScale * TargetScale * Mathf.Cos((i + 0.5f) * Mathf.PI * 2 / Steps3), sampleScale * TargetScale * Mathf.Sin((i + 0.5f) * Mathf.PI * 2 / Steps3));
                            sampleSize[samplePosCount] = sampleScale * TargetScale * 2;
                            samplePosCount++;
                        }
                    }

                    samplePos[samplePosCount] = optimalVelocity * 0.5f;
                    sampleSize[samplePosCount] = sampleScale * 0.4f;
                    samplePosCount++;


                    if (optimalVelocity.magnitude < 0.1f)
                    {
                        const int extSteps = 16;
                        for (int i = 0; i < extSteps; i++)
                        {
                            samplePos[samplePosCount] = new Vector2(Mathf.Sin(i * Mathf.PI * 2 / extSteps), Mathf.Cos(i * Mathf.PI * 2 / extSteps)) * sampleScale;
                            sampleSize[samplePosCount] = sampleScale * 0.5f;
                            samplePosCount++;
                        }
                        const int extSteps2 = 8;
                        const float inner2 = 0.5f;
                        for (int i = 0; i < extSteps2; i++)
                        {
                            samplePos[samplePosCount] = new Vector2(Mathf.Sin(i * Mathf.PI * 2 / extSteps), Mathf.Cos(i * Mathf.PI * 2 / extSteps)) * sampleScale * inner2;
                            sampleSize[samplePosCount] = sampleScale * inner2 * 0.5f;
                            samplePosCount++;
                        }

                    }


                    const int KeepCount = Simulator.WorkerContext.KeepCount;
                    Vector2[] bestPos = context.bestPos;
                    float[] bestSizes = context.bestSizes;
                    float[] bestScores = context.bestScores;
                    /////FORGROUP
                    float[] bestScores_G = context.bestScores_G;

                    for (int i = 0; i < KeepCount; i++)
                    {
                        bestScores[i] = float.PositiveInfinity;
                        bestScores_G[i] = float.PositiveInfinity;
                    }
                    bestScores[KeepCount] = float.NegativeInfinity;
                    bestScores[KeepCount] = float.NegativeInfinity;

                    Vector2 bestEver = optimalVelocity;
                    float bestEverScore = float.PositiveInfinity;
                    //                    float bestEverScore_G = float.PositiveInfinity;

                    for (int sub = 0; sub < 3; sub++)
                    {

                        for (int i = 0; i < samplePosCount; i++)
                        {

                            float score = 0;
                            for (int vo = 0; vo < voCount; vo++)
                            {
                                score = System.Math.Max(score, vos[vo].ScalarSample(samplePos[i]));
                            }
                            //FORGROUPPPPPPPPP
                            //²ßÂÔÐÞ¸Ä£¬£¬£¬Èç¹ûÎÞ·¨±ÜÅö³É¹¦£¬ÔòÊÇ·ñ¿¼ÂÇ×éÈº
                            //Èô¿¼ÂÇ×éÈº£¬ÔòÈ¨ÖØ¹ØÏµÈçºÎµ÷½Ú
                            //»òÕßÊ¹ÓÃ×ÖµäÐò
                            //weighted up
                            float score_G = 0;
                            float talking = 1f;
                            for (int vg = 0; vg < vgCount; vg++)
                            {
                                // score_G = System.Math.Max(score_G, vgs[vg].ScalarSample(samplePos[i]));
                                //newADD begin
                                float lamda = 1;
                                if (simulator.relationMatrix.Length > 0 && agentID >= 0 && vgs[vg].agentID >= 0)
                                {
                                    lamda = simulator.relationMatrix[agentID][vgs[vg].agentID];

                                }
                                float currScore = vgs[vg].ScalarSample(samplePos[i]);
                                score_G += lamda * currScore;

                                if (vgs[vg].usingForm)
                                    talking = Mathf.Min(talking, 0.5f+0.5f*Mathf.Min(currScore,1));

                                //newADD end

                            }
                            //newADD begin
                            if (vgCount > 0)
                                score_G = score_G / vgCount;
                            //newADD end
                            score += score_G * weightForGroup/* * weightTuner*/ * talking;

                            // Note that velocity is a vector and speed is a scalar, not the same thing
                            //float bonusForDesiredVelocity = (samplePos[i] - desired2D).magnitude + Mathf.Abs(samplePos[i].magnitude - desired2D.magnitude);
                            //float bonusForCurrentVelocity = Mathf.Min((samplePos[i] - optimalVelocity).magnitude,1);

                            float bonusForDesiredVelocity = (samplePos[i].normalized - desired2D.normalized).sqrMagnitude + Mathf.Abs(samplePos[i].magnitude - desired2D.magnitude);
                            float bonusForCurrentVelocity = (samplePos[i].normalized - optimalVelocity.normalized).sqrMagnitude;

                            //float bonusForCurrentVelocity = (Vector3.Dot(samplePos[i], optimalVelocity) < 0 ? 2 - Mathf.Abs(Vector3.Cross(samplePos[i].normalized, optimalVelocity.normalized).z) : Mathf.Abs(Vector3.Cross(samplePos[i].normalized, optimalVelocity.normalized).z)) / 2;
                            float NoBackwardMovement = Vector3.Dot(samplePos[i], desired2D) < 0 ? 2* zeroPenalty : 0;

                            // This didn't work out as well as I though
                            // Code left here because I might reenable it later
                            //float bonusForDesiredSpeed = Mathf.Abs (samplePos[i].magnitude - desired2D.magnitude);

                            if (isDistGoal)
                            {
                                weightForDesiredVel = DesiredVelocityWeight = 0f;
                            }

                            float biasedScore = score + bonusForDesiredVelocity * DesiredVelocityWeight;// + bonusForDesiredSpeed*0;
                            ///////////////////////////////////////////////////////TO BE SOVLED
                            //score += bonusForDesiredVelocity*0.001f;
                            score += bonusForDesiredVelocity * weightForDesiredVel /** (1f - weightTuner)*/ + NoBackwardMovement + bonusForCurrentVelocity / (timeSinceChanged* timeSinceChanged);

                            //NEWADD for zero velocity
                            if (samplePos[i].sqrMagnitude < 0.1f && desiredVelocity.sqrMagnitude > 0.1f)
                            {
                                score += zeroPenalty;
                            }

                            if (DebugDraw)
                            {
                                DrawCross(position2D + samplePos[i], Rainbow(Mathf.Log(score + 1) * 5), sampleSize[i] * 0.5f);
                            }

#if WEIGHTSORT
                            if (biasedScore <= bestScores[0])
                            {
                                for (int j = 0; j < KeepCount; j++)
                                {
                                    if (biasedScore > bestScores[j + 1])
                                    {
                                        bestScores[j] = biasedScore;
                                        bestSizes[j] = sampleSize[i];
                                        bestPos[j] = samplePos[i];
                                        break;
                                    }
                                }
                            }

                            if (score < bestEverScore)
                            {
                                bestEver = samplePos[i];
                                bestEverScore = score;
                                //    bestEverScore_G = score_G;

                                if (score == 0)
                                {
                                    sub = 100;
                                    break;
                                }
                            }




                            //ccccFORGROUP 
#elif DICTSORT
                        if (biasedScore < bestScores[0] || (biasedScore == bestScores[0] && score_G<=bestScores_G[0]))
                        {
							for ( int j = 0; j < KeepCount; j++ ) {
                                if (biasedScore > bestScores[j + 1] || (biasedScore == bestScores[j+1] && score_G >= bestScores_G[j+1]))
                                {
                                    bestScores[j] = biasedScore;
                                    bestScores_G[j] = score_G;
                                    bestSizes[j] = sampleSize[i];
                                    bestPos[j] = samplePos[i];
                                    break;
                                }            
							}
						}

						if ( score < bestEverScore||(score==bestEverScore && score_G<bestEverScore_G ) ) {
							bestEver = samplePos[i];
							bestEverScore = score;
                            bestEverScore_G = score_G;

							if ( score == 0 && score_G==0 ) {
								sub = 100;
								break;
							}
						}
#endif
                        }

                        samplePosCount = 0;

                        for (int i = 0; i < KeepCount; i++)
                        {
                            Vector2 p = bestPos[i];
                            float s = bestSizes[i];
                            bestScores[i] = float.PositiveInfinity;

                            const float Half = 0.6f;

                            float offset = s * Half * 0.5f;

                            samplePos[samplePosCount + 0] = (p + new Vector2(+offset, +offset));
                            samplePos[samplePosCount + 1] = (p + new Vector2(-offset, +offset));
                            samplePos[samplePosCount + 2] = (p + new Vector2(-offset, -offset));
                            samplePos[samplePosCount + 3] = (p + new Vector2(+offset, -offset));

                            s *= s * Half;
                            sampleSize[samplePosCount + 0] = (s);
                            sampleSize[samplePosCount + 1] = (s);
                            sampleSize[samplePosCount + 2] = (s);
                            sampleSize[samplePosCount + 3] = (s);
                            samplePosCount += 4;
                        }
                    }

                    result = bestEver;
                }
                //NEWADD dynamic change weight
                //if (desiredVelocity.magnitude > 0.5f && result.magnitude < 0.2f * maxSpeed)
                //{
                //    weightTuner = Mathf.Max(0.1f, weightTuner - weightTunerChangeStep);
                //}
                //else if (result.magnitude > 0.3f * maxSpeed)
                //{
                //    weightTuner = Mathf.Min(0.9f, weightTuner + 2f * weightTunerChangeStep);
                //}

                if (result.sqrMagnitude < 0.01)
                    result = new Vector2(0, 0);

                if (DebugDraw)
                    DrawCross(result + position2D, Color.blue, 10f);


                //newVelocity = To3D(Vector2.ClampMagnitude(result, maxSpeed));+
                float diff = (result - optimalVelocity).sqrMagnitude;
                if (diff>0.01)
                    timeSinceChanged =  1/(25*diff);

                newVelocity = To3D(Vector2.ClampMagnitude(result, maxSpeed + maxExceedingV));
            }
            if (simulator.calculateStatistics)
            {
                averageDist = 0f;
                for (int i = 0; i < groupNeighbours.Count; i++)
                {
                    averageDist += (groupNeighbours[i].position - position).magnitude;
                }
                if (groupNeighbours.Count > 0)
                    averageDist /= groupNeighbours.Count;
            }
        }

        public static float DesiredVelocityWeight = 0.02f;
        public static float DesiredVelocityScale = 0.1f;
        //public static float DesiredSpeedScale = 0.0f;
        public static float GlobalIncompressibility = 100;

        /** Extra weight that walls will have */
        const float WallWeight = 5;

        static Color Rainbow(float v)
        {
            Color c = new Color(v, 0, 0);
            if (c.r > 1) { c.g = c.r - 1; c.r = 1; }
            if (c.g > 1) { c.b = c.g - 1; c.g = 1; }
            return c;

        }
        //////////////
        //////FORGROUP
        /////////////////////////////
#if BKB 
        Vector2 TraceOnlyConstraint(VO[] vos, int voCount, Vector2 p, float cutoff, out float score)
        {
            score = 0;
            float stepScale = simulator.stepScale;

            //while ( true ) {
            for (int s = 0; s < 50; s++)
            {


                float step = 1.0f - (s / 50.0f);
                step *= stepScale;

                Vector2 dir = Vector2.zero;
                float mx = 0;
                for (int i = 0; i < voCount; i++)
                {
                    float w;
                    Vector2 d = vos[i].Sample(p, out w);
                    dir += d;

                    if (w > mx) mx = w;
                    //mx = System.Math.Max (mx, d.sqrMagnitude);
                }
                /*
                                Vector2 d2 = (new Vector2(desiredVelocity.x,desiredVelocity.z) - p);
                                float weight = d2.magnitude*DesiredVelocityWeight;
                                dir += d2*DesiredVelocityScale;
                                mx = System.Math.Max (mx, weight);
                */
                /*if ( simulator.densityScale > 0 ) {
                    d2 = simulator.SampleDensity (p+position);
                    dir += d2;
                    mx = System.Math.Max (mx, d2.magnitude);
                }*/

                score = mx;//dir.sqrMagnitude;

                float sq = dir.sqrMagnitude;
                if (sq > 0) dir *= mx / Mathf.Sqrt(sq); // +1 to avoid division by zero
                //Vector2.ClampMagnitude (dir, Mathf.Sqrt(mx));
                //dir /= vos.Count+1;

                dir *= step;

                //Vector2 prev = p;
                p += dir;

                if (score < cutoff) break;
                //if (debug) Debug.DrawLine ( To3D(prev+position), To3D(p+position), Color.green);
            }

            return p;
        }

        Vector2 TraceOnlyConstraint2(VO[] vos, int voCount, VG[] vgs, int vgCount, Vector2 p, float cutoff, out float score)
        {
            score = 0;
            float stepScale = simulator.stepScale;

            //while ( true ) {
            for (int s = 0; s < 50; s++)
            {


                float step = 1.0f - (s / 50.0f);
                step *= stepScale;

                Vector2 dir = Vector2.zero;
                float mx = 0;
                for (int i = 0; i < voCount; i++)
                {
                    float w;
                    Vector2 d = vos[i].Sample(p, out w);
                    dir += d;

                    if (w > mx) mx = w;
                    //mx = System.Math.Max (mx, d.sqrMagnitude);
                }
                for (int i = 0; i < vgCount; i++)
                {
                    float w;
                    Vector2 d = vgs[i].Sample(p, out w);
                    dir += d;
                    if (w > mx) mx = w;
                }
                /*
                Vector2 d2 = (new Vector2(desiredVelocity.x,desiredVelocity.z) - p);
                float weight = d2.magnitude*DesiredVelocityWeight;
                dir += d2*DesiredVelocityScale;
                mx = System.Math.Max (mx, weight);
*/
                /*if ( simulator.densityScale > 0 ) {
                    d2 = simulator.SampleDensity (p+position);
                    dir += d2;
                    mx = System.Math.Max (mx, d2.magnitude);
                }*/

                score = mx;//dir.sqrMagnitude;

                float sq = dir.sqrMagnitude;
                if (sq > 0) dir *= mx / Mathf.Sqrt(sq); // +1 to avoid division by zero
                //Vector2.ClampMagnitude (dir, Mathf.Sqrt(mx));
                //dir /= vos.Count+1;

                dir *= step;

                //Vector2 prev = p;
                p += dir;

                if (score < cutoff) break;
                //if (debug) Debug.DrawLine ( To3D(prev+position), To3D(p+position), Color.green);
            }

            return p;
        }

        Vector2 TraceVOnVGV1(VO[] vos, int voCount, VG[] vgs, int vgCount, Vector2 p, float cutoff, out float score)
        {

            score = 0;
            float stepScale = simulator.stepScale;

            //while ( true ) {
            for (int s = 0; s < 50; s++)
            {


                float step = 1.0f - (s / 50.0f);
                step *= stepScale;

                Vector2 dir = Vector2.zero;
                float mx = 0;
                for (int i = 0; i < voCount; i++)
                {
                    float w;
                    Vector2 d = vos[i].Sample(p, out w);
                    dir += d;

                    if (w > mx) mx = w;
                    //mx = System.Math.Max (mx, d.sqrMagnitude);
                }
                for (int i = 0; i < vgCount; i++)
                {
                    float w;
                    Vector2 d = vgs[i].Sample(p, out w);
                    //dir+=d;

                    dir += d * cooperation;
                    w *= cooperation;
                    if (w > mx) mx = w;
                }

                Vector2 d2 = (new Vector2(desiredVelocity.x, desiredVelocity.z) - p);


                float weight = d2.magnitude * DesiredVelocityWeight * persuit;
                dir += d2 * DesiredVelocityScale * persuit;
                mx = System.Math.Max(mx, weight);

                /*if ( simulator.densityScale > 0 ) {
                    d2 = simulator.SampleDensity (p+position);
                    dir += d2;
                    mx = System.Math.Max (mx, d2.magnitude);
                }*/

                score = mx;//dir.sqrMagnitude;

                float sq = dir.sqrMagnitude;
                if (sq > 0) dir *= mx / Mathf.Sqrt(sq); // +1 to avoid division by zero
                //Vector2.ClampMagnitude (dir, Mathf.Sqrt(mx));
                //dir /= vos.Count+1;

                dir *= step;

                //Vector2 prev = p;
                p += dir;

                if (score < cutoff) break;
                //if (debug) Debug.DrawLine ( To3D(prev+position), To3D(p+position), Color.green);
            }

            return p;
        }
#endif
        Vector2 TraceVOnVG(VO[] vos, int voCount, VG[] vgs, int vgCount, Vector2 p, float cutoff, out float score)
        {

            score = 0;
            float stepScale = simulator.stepScale;

            float bestScore = float.PositiveInfinity;
            Vector2 bestP = p;

            for (int s = 0; s < 50; s++)
            {


                float step = 1.0f - (s / 50.0f);
                step *= stepScale;

                Vector2 dir = Vector2.zero;
                float mx = 0;
                for (int i = 0; i < voCount; i++)
                {
                    float w;
                    Vector2 d = vos[i].Sample(p, out w);
                    dir += d;

                    if (w > mx) mx = w;
                    //mx = System.Math.Max (mx, d.sqrMagnitude);
                }
                for (int i = 0; i < vgCount; i++)
                {
                    float w;
                    Vector2 d = vgs[i].Sample(p, out w);
                    dir += d;

                    // dir += d * cooperation;
                    // w *= cooperation;
                    if (w > mx) mx = w;
                }


                // This didn't work out as well as I though
                // Code left here because I might reenable it later
                //Vector2 bonusForDesiredSpeed = p.normalized *  new Vector2(desiredVelocity.x,desiredVelocity.z).magnitude - p;

                Vector2 bonusForDesiredVelocity = (new Vector2(desiredVelocity.x, desiredVelocity.z) - p);

                float weight = bonusForDesiredVelocity.magnitude * DesiredVelocityWeight;// + bonusForDesiredSpeed.magnitude*DesiredSpeedScale;
                dir += bonusForDesiredVelocity * DesiredVelocityScale;// + bonusForDesiredSpeed*DesiredSpeedScale;
                mx = System.Math.Max(mx, weight);


                score = mx;



                if (score < bestScore)
                {
                    bestScore = score;
                }

                bestP = p;
                if (score <= cutoff && s > 10) break;

                float sq = dir.sqrMagnitude;
                if (sq > 0) dir *= mx / Mathf.Sqrt(sq);

                dir *= step;
                Vector2 prev = p;
                p += dir;
                if (DebugDraw) Debug.DrawLine(To3D(prev) + position, To3D(p) + position, Rainbow(0.1f / score) * new Color(1, 1, 1, 0.2f));
            }


            score = bestScore;
            return bestP;
        }

        /** Traces the vector field constructed out of the velocity obstacles.
         * Returns the position which gives the minimum score (approximately).
          */
        Vector2 Trace(VO[] vos, int voCount, Vector2 p, float cutoff, out float score)
        {

            score = 0;
            float stepScale = simulator.stepScale;

            float bestScore = float.PositiveInfinity;
            Vector2 bestP = p;

            for (int s = 0; s < 50; s++)
            {


                float step = 1.0f - (s / 50.0f);
                step *= stepScale;

                Vector2 dir = Vector2.zero;
                float mx = 0;
                for (int i = 0; i < voCount; i++)
                {
                    float w;
                    Vector2 d = vos[i].Sample(p, out w);
                    dir += d;

                    if (w > mx) mx = w;
                    //mx = System.Math.Max (mx, d.sqrMagnitude);
                }


                // This didn't work out as well as I though
                // Code left here because I might reenable it later
                //Vector2 bonusForDesiredSpeed = p.normalized *  new Vector2(desiredVelocity.x,desiredVelocity.z).magnitude - p;

                Vector2 bonusForDesiredVelocity = (new Vector2(desiredVelocity.x, desiredVelocity.z) - p);

                float weight = bonusForDesiredVelocity.magnitude * DesiredVelocityWeight;// + bonusForDesiredSpeed.magnitude*DesiredSpeedScale;
                dir += bonusForDesiredVelocity * DesiredVelocityScale;// + bonusForDesiredSpeed*DesiredSpeedScale;
                mx = System.Math.Max(mx, weight);


                score = mx;



                if (score < bestScore)
                {
                    bestScore = score;
                }

                bestP = p;
                if (score <= cutoff && s > 10) break;

                float sq = dir.sqrMagnitude;
                if (sq > 0) dir *= mx / Mathf.Sqrt(sq);

                dir *= step;
                Vector2 prev = p;
                p += dir;
                if (DebugDraw) Debug.DrawLine(To3D(prev) + position, To3D(p) + position, Rainbow(0.1f / score) * new Color(1, 1, 1, 0.2f));
            }


            score = bestScore;
            return bestP;
        }

        /** Returns the intersection factors for line 1 and line 2. The intersection factors is a distance along the line \a start - \a end where the other line intersects it.\n
         * \code intersectionPoint = start1 + factor1 * (end1-start1) \endcode
         * \code intersectionPoint2 = start2 + factor2 * (end2-start2) \endcode
         * Lines are treated as infinite.\n
         * false is returned if the lines are parallel and true if they are not.
         */
        public static bool IntersectionFactor(Vector2 start1, Vector2 dir1, Vector2 start2, Vector2 dir2, out float factor)
        {

            float den = dir2.y * dir1.x - dir2.x * dir1.y;

            // Parallel
            if (den == 0)
            {
                factor = 0;
                return false;
            }

            float nom = dir2.x * (start1.y - start2.y) - dir2.y * (start1.x - start2.x);

            factor = nom / den;

            return true;
        }
    }
}
