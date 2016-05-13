using UnityEngine;
using System;
using System.Collections;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation.NavMesh;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe
{
	public class Decomposer 
	{
		public AStarPathfinding aStarPathFinding { get; set; }

		private GlobalPath currentSolution;

		public Vector3 debugPosition { get; set; }

		public Goal decompose(KinematicData data, Goal goal)
		{
			if (data.position == goal.position)
				return goal;

			this.aStarPathFinding.NodesPerSearch = 100;
			this.aStarPathFinding.InitializePathfindingSearch(data.position, goal.position);

			if (aStarPathFinding.InProgress)
			{
				var finished = this.aStarPathFinding.Search(out currentSolution);
				currentSolution.CalculateLocalPathsFromPathPositions(data.position);
			}

			Vector3 pos = this.currentSolution.PathPositions [0];
			this.debugPosition = pos;
			goal.position = pos;
			return goal;
		}
	
		public GlobalPath getCurrentSolution()
		{
			return this.currentSolution;
		}

	
	}
}