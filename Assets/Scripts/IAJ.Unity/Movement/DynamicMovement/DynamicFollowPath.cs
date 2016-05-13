using System;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Movement.DynamicMovement
{
    public class DynamicFollowPath : DynamicSeek
    {
        public Path Path { get; set; }
        public float PathOffset { get; set; }

        public float CurrentParam { get; set; }

        private MovementOutput EmptyMovementOutput { get; set; }

        public DynamicFollowPath(KinematicData character, Path path) 
        {
            this.Target = new KinematicData();
            this.Character = character;
            this.Path = path;
            this.EmptyMovementOutput = new MovementOutput();
        }

        float targetParam;
        public override MovementOutput GetMovement()
        {
            if(Path.PathEnd(CurrentParam))
            {
                return EmptyMovementOutput;
            }
            else
            {
                CurrentParam = Path.GetParam(base.Character.position, CurrentParam);
                targetParam = CurrentParam + PathOffset;
                CurrentParam = targetParam;
				Target.position = Path.GetPosition(targetParam); 
                return base.GetMovement();
            }

        }
    }
}
