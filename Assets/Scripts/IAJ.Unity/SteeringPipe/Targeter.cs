using UnityEngine;
using System;
using System.Collections;
using Assets.Scripts.IAJ.Unity.Movement;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe
{
	public class Targeter 
	{
		public Vector3 clickPosition { get; set; }
		public bool goalHasChanged { get; set; }

		public Goal getGoal(KinematicData data) 
		{	
			Goal goal = new Goal ();
			goal.position = clickPosition;
			goal.hasPosition = true;
			goal.hasOrientation = false;
			goal.hasRotation = false;
			goal.hasvelocity = false;
			return goal;
		}
	}
}