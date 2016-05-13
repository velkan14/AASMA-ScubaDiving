using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicWander : DynamicSeek
    {
        public DynamicWander()
        {
            this.Target = new KinematicData();
        }
        public override string Name
        {
            get { return "Wander"; }
        }
        public float TurnAngle { get; set; }

        public float WanderOffset { get; set; }
        public float WanderRadius { get; set; }

        protected float WanderOrientation { get; set; }

        Vector3 circleCenter;
        public override MovementOutput GetMovement()
        {
			this.WanderOrientation += RandomHelper.RandomBinomial() * this.TurnAngle;
			this.Target.orientation = this.WanderOrientation + this.Character.orientation;
			circleCenter = this.Character.position + this.WanderOffset * MathHelper.ConvertOrientationToVector (this.Character.orientation);
			this.Target.position = circleCenter + WanderRadius * MathHelper.ConvertOrientationToVector (this.Target.orientation);
            return base.GetMovement ();

        }
    }
}
