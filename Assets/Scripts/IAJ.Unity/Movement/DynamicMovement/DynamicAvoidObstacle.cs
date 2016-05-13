using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
	public class DynamicAvoidObstacle : DynamicSeek
	{
		
		public override string Name
		{
			get { return "Avoid Obstacle"; }
		}

		public bool ShowRays { get; set; }	
		public GameObject obstacle { get; set; }
		public Collider collider { get; set; }
		public float AvoidMargin { get; set; }
		public float MaxLookAhead { get; set; }
		
		public DynamicAvoidObstacle(GameObject obstacle) 
		{
			this.obstacle = obstacle;
			this.Target = new KinematicData();
		}

        RaycastHit leftHit;
        RaycastHit rightHit;
        RaycastHit hit;
        Vector3 leftRayDirection;
        Vector3 rightRayDirection;
        Ray centralRay;
        Ray leftRay;
        Ray rightRay;

        public override MovementOutput GetMovement()
		{
            leftRayDirection = Quaternion.Euler (0, 30, 0) * Character.velocity;
			rightRayDirection = Quaternion.Euler (0, -30, 0) * Character.velocity;
			
			centralRay = new Ray (this.Character.position, this.Character.velocity.normalized * this.MaxLookAhead);
			leftRay = new Ray (this.Character.position, leftRayDirection.normalized * this.MaxLookAhead);
			rightRay = new Ray (this.Character.position, rightRayDirection.normalized * this.MaxLookAhead);
			
			if (Physics.Raycast (centralRay, out hit)) {
				this.Target.position = hit.point + hit.normal * this.AvoidMargin;
				return base.GetMovement ();
			} 
			else if (Physics.Raycast (leftRay, out leftHit))
			{
				this.Target.position = leftHit.point + leftHit.normal * this.AvoidMargin;
				return base.GetMovement ();
			}
			else if (Physics.Raycast (rightRay, out rightHit))
			{
				this.Target.position = rightHit.point + rightHit.normal * this.AvoidMargin;
				return base.GetMovement ();
			}
			
			else
				return new MovementOutput ();	
		}
	}
}