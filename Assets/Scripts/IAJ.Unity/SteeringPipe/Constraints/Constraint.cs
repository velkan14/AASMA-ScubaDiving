using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe.Constraints
{
	public abstract class Constraint
	{
        public KinematicData Character { get; set; }

        public abstract bool willViolate (Path path);

		public abstract Goal suggest(Path path, KinematicData data, Goal goal);

		// Calculate the distance between
		// point pt and the segment p1 --> p2.
		public double FindDistanceToSegment(Vector3 pt, Vector3 p1, Vector3 p2)
		{
			float dx = p2.x - p1.x;
			float dy = p2.z - p1.z;
			if ((dx == 0) && (dy == 0))
			{
				// It's a point not a line segment.
				dx = pt.x - p1.x;
				dy = pt.z - p1.z;
				return Math.Sqrt(dx * dx + dy * dy);
			}
			
			// Calculate the t that minimizes the distance.
			float t = ((pt.x - p1.x) * dx + (pt.z - p1.z) * dy) /
				(dx * dx + dy * dy);
			
			// See if this represents one of the segment's
			// end points or a point in the middle.
			if (t < 0)
			{
				dx = pt.x - p1.x;
				dy = pt.z - p1.z;
			}
			else if (t > 1)
			{
				dx = pt.x - p2.x;
				dy = pt.z - p2.z;
			}
			else
			{
				var closest = new Vector3(p1.x + t * dx, 0.0f, p1.z + t * dy);
				dx = pt.x - closest.x;
				dy = pt.z - closest.z;
			}
			
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public Vector3 closestPointOnSegment(Vector3 center, Vector3 lineP1, Vector3 lineP2)
		{
			float length_squared = (float) Math.Sqrt (lineP1.x - lineP2.x) + (float) Math.Sqrt (lineP1.z - lineP2.z);
			
			if (length_squared == 0)
				return lineP2;
			
			float t = Vector3.Dot (center - lineP1, lineP2 - lineP1) / length_squared;
			
			if (t < 0.0f)
				return lineP1;
			if (t > 1.0f)
				return lineP2;
			
			Vector3 aux = new Vector3 (lineP1.x + t, lineP1.y, lineP1.z + t);
			Vector3 projection = Vector3.Scale(aux, (lineP2 - lineP1));
			return projection;
		}
	}
}