using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.SteeringPipe;
using Assets.Scripts.IAJ.Unity.SteeringPipe.Constraints;
using Assets.Scripts.IAJ.Unity.SteeringPipe.Actuators;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine;
using RAIN.Navigation;
using System.Linq;
using System.Collections.Generic;
using RAIN.Navigation.NavMesh;
using RAIN.Navigation.Graph;

public class PathfindingManager : MonoBehaviour
{
	public const float DRAG = 0.1f;
	public const float MAX_SPEED = 20.0f;
	public const float X_WORLD_SIZE = 200.0f;
	public const float Z_WORLD_SIZE = 200.0f;
	public const float AVOID_MARGIN = 10.0f;
	public const float MAX_ACCELERATION = 10.0f;
	public const float MAX_LOOK_AHEAD = 5.0f;
	public const float PEDESTRIAN_RADIUS = 2.5f;

    //public fields to be set in Unity Editor
    public Camera camera;
    public GameObject characterAvatar;

    //private fields for internal use only
    private Vector3 startPosition;
    private Vector3 endPosition;
	private NavMeshPathGraph navMesh;
	
    private AStarPathfinding aStarPathFinding;

	private BlendedMovement Blended;
    private BlendedMovement BlendedDeadLock;

    private DynamicCharacter character;

	//Steering Pipeline
	private SteeringPipeline steeringPipe; 
	private Targeter targeter;
	private Decomposer decomposer;
	private Actuator actuator;

    private bool draw;

    // Use this for initialization
    void Awake()
    {
        this.draw = false;
        this.navMesh = NavigationManager.Instance.NavMeshGraphs[0];
      

        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        this.character = new DynamicCharacter(this.characterAvatar);

        this.targeter = new Targeter
        {
            goalHasChanged = false
        };

        this.decomposer = new Decomposer
        {
            aStarPathFinding = new NodeArrayAStarPathFinding(this.navMesh, new EuclideanDistanceHeuristic()),
            //aStarPathFinding = new AStarPathfinding(this.navMesh, new NodePriorityHeap(), new MyDictionary(), new EuclideanDistanceHeuristic()),
            debugPosition = Vector3.zero
        };
				
        //Movement when character finds a deadLock (in the SteeringPipeline)
        this.InitializeDeadLock(character, obstacles);

        //(UN)COMMENT to use other actuator
        this.actuator = new HumanActuator (new AStarPathfinding(this.navMesh, new NodePriorityHeap(), new MyDictionary(), new EuclideanDistanceHeuristic()))
        {
            TargetPosition = new KinematicData(),
        };
//        this.actuator = new CarActuator(new AStarPathfinding(this.navMesh, new NodePriorityHeap(), new MyDictionary(), new EuclideanDistanceHeuristic()))
//        {
//            TargetPosition = new KinematicData(),
//            Blended = this.BlendedDeadLock
//        };

        this.steeringPipe = new SteeringPipeline 
		{
			Targeter = this.targeter,
			Decomposers = new List<Decomposer>(),
			Constraints = new List<AvoidObstacleConstraint>(),
            Actuator = this.actuator,
            Character = this.character.KinematicData,
			MaxConstraintSteps = 10,
			Target = new KinematicData(),
			DeadLockMovement = this.BlendedDeadLock
        };

		this.steeringPipe.Decomposers.Add (this.decomposer);
		this.character.Movement = this.steeringPipe;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position;

        this.startPosition = this.character.KinematicData.position;
        if (Input.GetMouseButtonDown(0))
        {
            //if there is a valid position
            if (this.MouseClickPosition(out position))
            {
				//set the target position
				this.steeringPipe.Targeter.clickPosition = position;
				this.steeringPipe.Targeter.goalHasChanged = true;

                //we're setting the end point
                //this is just a small adjustment to better see the debug sphere
                this.endPosition = position;
                this.draw = true;

                this.actuator.TargetPosition.position = position; 
            }
        }
       
        this.character.Update();
        this.character.Movement.Character.position.y = 0.0f;
    }

//    public void OnGUI()
//    {
//        if (this.decomposer.getCurrentSolution() != null)
//        {
//            var time = this.decomposer.aStarPathFinding.TotalProcessingTime * 1000;
//            float timePerNode;
//            if (this.decomposer.aStarPathFinding.TotalProcessedNodes > 0)
//            {
//                timePerNode = time / this.decomposer.aStarPathFinding.TotalProcessedNodes;
//            }
//            else
//            {
//                timePerNode = 0;
//            }
//            var text = "Nodes Visited: " + this.decomposer.aStarPathFinding.TotalProcessedNodes
//                       + "\nMaximum Open Size: " + this.decomposer.aStarPathFinding.MaxOpenNodes
//                       + "\nProcessing time (ms): " + time
//                       + "\nTime per Node (ms):" + timePerNode;
//            GUI.contentColor = Color.black;
//            GUI.Label(new Rect(10, 10, 200, 100), text);
//        }
//    }

//    public void OnDrawGizmos()
//    {
//        if (this.draw)
//        {
//            //draw the current Solution Path if any (for debug purposes)
//            if (this.decomposer.getCurrentSolution() != null)
//            {
//                var previousPosition = this.startPosition;
//                foreach (var pathPosition in this.actuator.currentSolution.PathPositions)
//                {
//                    Debug.DrawLine(previousPosition, pathPosition, Color.red);
//                    previousPosition = pathPosition;
//                }
//
//               previousPosition = this.startPosition;
//                foreach (var pathPosition in this.actuator.currentSmoothedSolution.PathPositions)
//                {
//                    Debug.DrawLine(previousPosition, pathPosition, Color.green);
//                    previousPosition = pathPosition;
//                }
//            }
//
//            //draw the nodes in Open and Closed Sets
//            if (this.decomposer.aStarPathFinding != null)
//            {
//                Gizmos.color = Color.cyan;
//
//                if (this.decomposer.aStarPathFinding.Open != null)
//                {
//                    foreach (var nodeRecord in this.decomposer.aStarPathFinding.Open.All())
//                    {
//                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
//                    }
//                }
//
//                Gizmos.color = Color.blue;
//
//                if (this.decomposer.aStarPathFinding.Closed != null)
//                {
//                    foreach (var nodeRecord in this.decomposer.aStarPathFinding.Closed.All())
//                    {
//                        Gizmos.DrawSphere(nodeRecord.node.LocalPosition, 1.0f);
//                    }
//                }
//            }
//
//            /*Gizmos.color = Color.yellow;
//            //draw the target for the follow path movement
//            if (this.character.Movement != null)
//            {
//                Gizmos.DrawSphere( this.actuator.Movement.Target.position, 2.0f);
//            }*/
//        }
//    }

    private bool MouseClickPosition(out Vector3 position)
    {
        RaycastHit hit;

        var ray = this.camera.ScreenPointToRay(Input.mousePosition);
        //test intersection with objects in the scene
        if (Physics.Raycast(ray, out hit))
        {
            //if there is a collision, we will get the collision point
            position = hit.point;
            return true;
        }

        position = Vector3.zero;
		//if not the point is not valid
        return false;
    }
		
    //Movement used in deadLock situations
    private void InitializeDeadLock(DynamicCharacter character, GameObject[] obstacles)
    {
        this.BlendedDeadLock = new BlendedMovement
        {
            Character = character.KinematicData
        };
				

        var wander = new DynamicWander
        {
            TurnAngle = MathConstants.MATH_1_PI,
            WanderOffset = 5.0f,
            WanderRadius = 5.0f,
            MaxAcceleration = 40.0f,
            Character = character.KinematicData
        };

        this.BlendedDeadLock.Movements.Add(new MovementWithWeight(wander, obstacles.Length));
    }

    private Vector3 auxVector;
    private void UpdateMovingGameObject(DynamicCharacter movingCharacter, int index)
	{
        if (movingCharacter.Movement != null)
        {
            movingCharacter.Update();
            movingCharacter.KinematicData.ApplyWorldLimit(X_WORLD_SIZE, Z_WORLD_SIZE);
            auxVector = movingCharacter.Movement.Character.position;
            auxVector.y = 0.0f;
            movingCharacter.GameObject.transform.position = auxVector;
            this.steeringPipe.Constraints[index].Center = auxVector;
		}
	}
}
