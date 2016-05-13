using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe.Actuators
{
	public class HumanActuator : Actuator
	{
		public DynamicMovement Movement { get; set; }

        public HumanActuator(AStarPathfinding aStarPathFinding)
		{
			this.aStarPathFinding = aStarPathFinding;
		}
		
		public override MovementOutput getMovement(Path path, KinematicData data, Goal goal)
		{	
			Vector3 direction = TargetPosition.position - data.position;
			float distanceToGoal = direction.magnitude;
			
			if (distanceToGoal < 2.0f) {
				this.Movement = new DynamicFollowPath (data, path)
				{
					MaxAcceleration = 0.0f,
				};
				
				data.SetOrientationFromVelocity ();
				return this.Movement.GetMovement ();
			}
			
			this.Movement = new DynamicFollowPath (data, path)
			{
				MaxAcceleration = 100.0f,
				PathOffset = 0.2f
			};
			
			return this.Movement.GetMovement ();
		}
	}
}