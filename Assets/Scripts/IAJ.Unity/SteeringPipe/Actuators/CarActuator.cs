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
    public class CarActuator : Actuator
    {
        public DynamicMovement Movement { get; set; }
        public BlendedMovement Blended { get; set; }
        public Vector3 lastPosition;


        public CarActuator(AStarPathfinding aStarPathFinding)
        {
            this.aStarPathFinding = aStarPathFinding;
        }

        public override MovementOutput getMovement(Path path, KinematicData data, Goal goal)
        {
            float aux = (TargetPosition.position - data.position).magnitude;
            if (aux < 2.0f)
            {
                this.Movement = new DynamicFollowPath(data, path)
                {
                    MaxAcceleration = 0.0f,
                };

                data.SetOrientationFromVelocity();
                lastPosition = TargetPosition.position;
                return this.Movement.GetMovement();
            }
            else
            {
                this.Movement = new DynamicFollowPath(data, path)
                {
                    MaxAcceleration = 50.0f,
                    PathOffset = 0.2f,
                };
                if ((lastPosition - data.position).magnitude < 5.0)
                {
                    this.Movement = Blended;
                    this.Movement.MaxAcceleration = 20.0f;
                    data.SetOrientationFromVelocity();
                }
                else
                {
                    this.Movement.MaxAcceleration = 50.0f;
                    data.SetOrientationFromVelocity();
                }
                return this.Movement.GetMovement();
            }
        }
    }
}