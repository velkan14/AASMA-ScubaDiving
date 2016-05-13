using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }

        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic) : base(graph, null, null, heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
        }

        //don't forget to add the override keyword here if you define a ProcessChildNode method in the base class
        protected NodeRecord ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge)
        {
            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm, just do NOT CHANGE THIS, or you algorithm wont work
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = bestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            }

            childNodeRecord.gValue = bestNode.gValue + (childNode.LocalPosition - bestNode.node.LocalPosition).magnitude;
            childNodeRecord.hValue = this.Heuristic.H(childNode, this.GoalNode);
            childNodeRecord.fValue = F(childNodeRecord);


            return childNodeRecord;

        }

        public override bool Search(out GlobalPath solution, bool returnPartialSolution = true)
        {
            int visitedNodesPerCycle = 0;
			float initialTime = Time.realtimeSinceStartup;

            //the next line is here just to avoid a compiler error, remove it
            while (true)
            {
				var openCount = this.Open.CountOpen();
				if (openCount > this.MaxOpenNodes)
					this.MaxOpenNodes = openCount;

                if (this.Open.CountOpen() == 0)
                {
                    solution = null;
                    return false;
                }

                NodeRecord bestNode = this.Open.GetBestAndRemove();
                visitedNodesPerCycle++;

                if (bestNode.node.Equals(GoalNode))
                {
					this.TotalProcessingTime = Time.realtimeSinceStartup - initialTime;
                    solution = CalculateSolution(bestNode, returnPartialSolution);
                    this.InProgress = false;
                    return true;
                }

                if (returnPartialSolution && visitedNodesPerCycle == this.NodesPerSearch)
                {
                    solution = CalculateSolution(bestNode, true);
                    return true;
                }

                this.Closed.AddToClosed(bestNode);

                var outConnections = bestNode.node.OutEdgeCount;
                for (int i = 0; i < outConnections; i++)
                {
                    var childNode = this.ProcessChildNode(bestNode, bestNode.node.EdgeOut(i));

                    if (childNode == null)
                        continue;

                    if (childNode.parent == null)
                        childNode.parent = bestNode;

                    var NodeInOpen = this.Open.SearchInOpen(childNode);
                    var NodeInClosed = this.Closed.SearchInClosed(childNode);

                    if (NodeInOpen == null && NodeInClosed == null)
                    {
                        this.Open.AddToOpen(childNode);

                    }
                    else if (NodeInOpen != null && NodeInOpen.fValue > childNode.fValue)
                    {
                        this.Open.Replace(NodeInOpen, childNode);
                        MaxOpenNodes++;
                    }
                    else if (NodeInClosed != null && NodeInClosed.fValue > childNode.fValue)
                    {
                        this.Closed.RemoveFromClosed(childNode);
                        this.Open.AddToOpen(childNode);
                    }
                }

            }
        }


        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }
}
