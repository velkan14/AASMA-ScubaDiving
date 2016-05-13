using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe.Constraints
{
	public class OutOfMapConstraint : Constraint
	{
		
		public float xWorldSize { get; set; }
	
		public float zWorldSize { get; set; }
	
		public float Margin { get; set; }

		private bool atLeft = false; 
		private bool atRight = false;
		private bool atTop = false;
		private bool atBottom = false;

		public override bool willViolate(Path path)
		{
			if (this.Character.position.x - this.Margin < -xWorldSize)
			{
				atLeft = true;
				return true;
			}
			else if (this.Character.position.x + this.Margin > xWorldSize)
			{
				atRight = true;
				return true;
			}
			if (this.Character.position.z - this.Margin < -zWorldSize)
			{
				atTop = true;
				return true;
			}
			else if (this.Character.position.z + this.Margin > zWorldSize)
			{
				atBottom = true;
				return true;
			}
			return false;
		}
		
		public override Goal suggest(Path path, KinematicData data, Goal goal)
		{
			if (atLeft) {
				Vector3 segmentP1 = new Vector3 (data.position.x + this.Margin, 0.0f, data.position.z - this.Margin);
				Vector3 segmentP2 = new Vector3 (data.position.x + this.Margin, 0.0f, data.position.z - this.Margin);
				Vector3 closest = closestPointOnSegment (data.position, goal.position, goal.position + Vector3.right * 2.0f);
				Vector3 newPoint = data.position + (closest - data.position) / closest.magnitude;
				Debug.DrawLine(goal.position, goal.position + Vector3.right * 10.0f, Color.blue);
				goal.position = newPoint;
				atLeft = false;
			} else if (atRight) {
				Vector3 segmentP1 = new Vector3 (data.position.x - this.Margin, 0.0f, data.position.z + this.Margin);
				Vector3 segmentP2 = new Vector3 (data.position.x - this.Margin, 0.0f, data.position.z - this.Margin);
				Vector3 closest = closestPointOnSegment (data.position, segmentP1, segmentP2);
				Vector3 newPoint = data.position + (closest - data.position) / closest.magnitude;
				goal.position = newPoint;
				atRight = false;
				Debug.DrawLine(segmentP1, segmentP2, Color.blue);
			}
			else if (atTop) 
			{
				Vector3 segmentP1 = new Vector3 (data.position.x - this.Margin, 0.0f, data.position.z + this.Margin);
				Vector3 segmentP2 = new Vector3 (data.position.x + this.Margin, 0.0f, data.position.z + this.Margin);
				Vector3 closest = closestPointOnSegment (data.position, segmentP1, segmentP2);
				Vector3 newPoint = data.position + (closest - data.position) / closest.magnitude;
				goal.position = newPoint;
				atTop = false;
				Debug.DrawLine(segmentP1, segmentP2, Color.blue);
			}
			else if (atBottom) 
			{
				Vector3 segmentP1 = new Vector3 (data.position.x - this.Margin, 0.0f, data.position.z - this.Margin);
				Vector3 segmentP2 = new Vector3 (data.position.x + this.Margin, 0.0f, data.position.z - this.Margin);
				Vector3 closest = closestPointOnSegment (data.position, segmentP1, segmentP2);
				Vector3 newPoint = data.position + (closest - data.position) / closest.magnitude;
				goal.position = newPoint;
				atBottom = false;
				Debug.DrawLine(segmentP1, segmentP2, Color.blue);
			}
			return goal;
		}
	}
}