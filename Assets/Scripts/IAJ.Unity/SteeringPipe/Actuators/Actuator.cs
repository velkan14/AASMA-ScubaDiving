using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Pathfinding;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe.Actuators
{
	public abstract class Actuator 
	{

		public GlobalPath currentSmoothedSolution { get; set; }
		public GlobalPath currentSolution = new GlobalPath();

        public KinematicData TargetPosition;

        protected AStarPathfinding aStarPathFinding { get; set; }

        public GlobalPath getPath(KinematicData data, Goal goal)
        {
            this.aStarPathFinding.NodesPerSearch = 10;
            this.aStarPathFinding.InitializePathfindingSearch(data.position, goal.position);

            if (aStarPathFinding.InProgress)
            {
                var finished = this.aStarPathFinding.Search(out currentSolution);
                if (finished && currentSolution != null)
                {
                    currentSmoothedSolution = StringPullingPathSmoothing.SmoothPath(data, currentSolution);
                    currentSmoothedSolution.CalculateLocalPathsFromPathPositions(data.position);
                }
            }

            return currentSmoothedSolution;
        }

        public abstract MovementOutput getMovement(Path path, KinematicData data, Goal goal);
	}
}