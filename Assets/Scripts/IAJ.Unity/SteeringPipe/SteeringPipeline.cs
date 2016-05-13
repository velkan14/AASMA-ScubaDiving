using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.SteeringPipe.Constraints;
using Assets.Scripts.IAJ.Unity.SteeringPipe.Actuators;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe
{
	public class SteeringPipeline : DynamicMovement
	{

		public Targeter Targeter { get; set; }
		public List<Decomposer> Decomposers { get; set; }
		public List<AvoidObstacleConstraint> Constraints {get; set; }
		public Actuator Actuator { get; set; }

		public int MaxConstraintSteps { get; set; }
		public BlendedMovement DeadLockMovement { get; set; } 

		public override string Name
		{
			get { return "Steering Pipeline"; }
		}
		
		public override KinematicData Target { get; set; }

		public KinematicData Character { get; set; }

		public override MovementOutput GetMovement()
		{
			Goal goal = new Goal ();

			goal.updateChannel(this.Targeter.getGoal(this.Character));


			foreach (var decomposer in this.Decomposers) 
			{
				if (this.Targeter.goalHasChanged)
				{
					//only calculate path if goal has changed
					goal = decomposer.decompose(this.Character, goal);
					this.Targeter.goalHasChanged = false;
				}
			}

			for (int i = 0; i < this.MaxConstraintSteps; i++) 
			{
				bool validPath = true;
				Path path = this.Actuator.getPath(this.Character, goal);

				foreach(var constraint in this.Constraints) 
				{
					if (constraint.willViolate(path))
					{
						validPath = false;
						goal = constraint.suggest(path, this.Character, goal);
						break;
					}
				}

				if (validPath)
				{
					return this.Actuator.getMovement(path, this.Character, goal);
				}

			}
            return this.DeadLockMovement.GetMovement ();
		}
	}
}