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
	/** ORCA Line.
	 * Simply holds a point and a direction, nothing fancy.
	 * 
	 * \astarpro 
	 */
	public struct Line {
		public Vector2 point;
		public Vector2 dir;
	}
}