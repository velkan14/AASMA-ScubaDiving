using UnityEngine;
using System.Collections;

namespace Assets.Scripts.IAJ.Unity.SteeringPipe
{
	public class Goal 
	{
		public Vector3 position { get; set; } 
		public Vector3 velocity { get; set; }
		public float orientation { get; set; }
		public float rotation { get; set; }

		public bool hasPosition { get; set; }
		public bool hasvelocity { get; set; }
		public bool hasOrientation { get; set; }
		public bool hasRotation { get; set; }

		public void updateChannel(Goal o) 
		{
			if (o.hasPosition)
				this.position = o.position;
			if (o.hasOrientation)
				this.orientation = o.orientation;
			if (o.hasRotation)
				this.rotation = o.rotation;
			if (o.hasvelocity)
				this.velocity = o.velocity;
		}
	}
}