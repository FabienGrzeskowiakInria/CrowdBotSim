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
using UnityEngine;
using System.Collections;

namespace VOG
{
	
	/** One vertex in an obstacle.
	  * This is a linked list and one vertex can therefore be used to reference the whole obstacle
	  * \astarpro 
	  */
	public class ObstacleVertex {
		public bool ignore;

		/** Position of the vertex */
		public Vector3 position;
		public Vector2 dir;
		
		/** Height of the obstacle in this vertex */
		public float height;

		public VOGLayer layer;

		public bool convex;
		/** True if this vertex was created by the KDTree for internal reasons */
		public bool split = false;

		/** Next vertex in the obstacle */
		public ObstacleVertex next;
		/** Previous vertex in the obstacle */
		public ObstacleVertex prev;
	}
}