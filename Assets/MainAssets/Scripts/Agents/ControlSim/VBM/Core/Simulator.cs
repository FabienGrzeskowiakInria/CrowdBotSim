using System.Collections.Generic;
using UnityEngine;

namespace VBM
{
    public class Simulator
    {
        List<Agent> agents_;
        List<Wall> walls_;

        public Simulator()
        {
            agents_ = new List<Agent>();
            walls_ = new List<Wall>();
        }

        public int addAgent(Vector2 position, float radius,  Vector2 velocity)
        {
            Agent agent = new Agent();
            agent.id_ = agents_.Count;
            agent.setPosition(position);
            agent.radius_ = radius;
            agent.setVelocity(velocity);
            agents_.Add(agent);

            return agent.id_;
        }

        public int addAgent(Vector2 position, float radius, Vector2 velocity, float neighborsAgentDist, float neighborsWallDist, float sigTtca, float sigDca, float sigSpeed, float sigAngle)
        {
            Agent agent = new Agent(sigTtca, sigDca, sigSpeed, sigAngle);
            agent.id_ = agents_.Count;
            agent.setPosition(position);
            agent.radius_ = radius;
            agent.setVelocity(velocity);

            agent.neighborsAgentDist = neighborsAgentDist;
            agent.neighborsWallDist = neighborsWallDist;

            agents_.Add(agent);

            return agent.id_;

        }

        public int addNonResponsiveAgent(Vector2 position, float radius)
        {
            Agent agent = new Agent();
            agent.id_ = agents_.Count;
            agent.setPosition(position);
            agent.radius_ = radius;
            agent.setVelocity(Vector2.zero);
            agents_.Add(agent);
            agent.deactivate();

            return agent.id_;
        }

        public void addWall(Vector2[] wallPoints)
        {
            Wall wall = new Wall(wallPoints, walls_.Count);
            walls_.Add(wall);
        }

        public void Clear()
        {
            agents_.Clear();
            walls_.Clear();
        }

        public Vector2 getAgentPosition(int id)
        {
            return agents_[id].getPosition();
        }
        public Vector2 getAgentVelocity(int id)
        {
            return agents_[id].getVelocity();
        }
        public void setAgentPosition(int id, Vector2 position)
        {
            agents_[id].setPosition(position);

        }
        public void setAgentPrefVelocity(int id, Vector2 goal)
        {
            agents_[id].setPrefVelocity(goal);
        }

        private void FindNeighbors(Agent a)
        {

            a.resetVision();

            //TODO: KDTREE
            foreach (Wall w in walls_)
            {
                a.insertWallNeighbor(w);
            }


            foreach (Agent other in agents_)
            {
                if (other!=a)
                    a.insertAgentNeighbor(other);
            }

        }

        private struct Matrix2x2
        {
            float a11_;
            float a12_;
            float a21_;
            float a22_;
            public Matrix2x2(Vector2 v1, Vector2 v2)
            {
                a11_ = v1.x;
                a21_ = v1.y;
                a12_ = v2.x;
                a22_ = v2.y;
            }
            public Vector2 product(Vector2 v)
            {
                return new Vector2(a11_ * v.x + a12_ * v.y, a21_ * v.x + a22_ * v.y);
            }
        }

        public void doStep(float timeStep)
        {
            foreach (Agent a in agents_)
            {
                FindNeighbors(a);
            }

            foreach (Agent a in agents_)
            {
                a.updateSpeed(timeStep);
            }

            foreach (Agent a in agents_)
            {
                a.updatePosition(timeStep);
            }
        }

    }
}
