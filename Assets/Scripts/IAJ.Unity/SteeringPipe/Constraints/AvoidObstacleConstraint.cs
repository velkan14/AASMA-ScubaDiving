using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe.Constraints
{
	public class AvoidObstacleConstraint : Constraint
	{

		public Vector3 Center { get; set; }
		public float Radius { get; set; }
		public float Margin { get; set; }

		public KinematicData Character { get; set; }

		private Vector3 segmentP1 = Vector3.zero;
		private Vector3 segmentP2 = Vector3.zero;

		private int problemIndex;

        public override bool willViolate(Path path)
        {
            //Check each segment of the path in turn
            GlobalPath currentPath = path as GlobalPath;

            this.segmentP1 = this.Character.position;
            this.segmentP2 = currentPath.PathPositions[0];

            double aux = FindDistanceToSegment(this.Center, this.segmentP1, this.segmentP2);
            if (aux < this.Radius + this.Margin)
            {
                return true;
            }

            return false;
        }

        public override Goal suggest(Path path, KinematicData data, Goal goal)
		{
			//Find the closest point on the segment to the pedestrian center
			Vector3 closest = closestPointOnSegment (this.Center, data.position, goal.position);

			Vector3 newPoint = this.Center + (closest - this.Center) * this.Radius * this.Margin / closest.magnitude;

			goal.position = newPoint;
			return goal;
		}
	}
}