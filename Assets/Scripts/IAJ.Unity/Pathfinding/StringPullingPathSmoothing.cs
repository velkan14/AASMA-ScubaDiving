using System;
using Assets.Scripts.IAJ.Unity.Movement;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public static class StringPullingPathSmoothing
    {
        /// <summary>
        /// Method used to smooth a received path, using a string pulling technique
        /// it returns a new path, where the path positions are selected in order to provide a smoother path
        /// </summary>
        /// <param name="data"></param>
        /// <param name="globalPath"></param>
        /// <returns></returns>
        public static GlobalPath SmoothPath(KinematicData data, GlobalPath globalPath)
        {

            //se houver uma posiçao igualzinha a posiçao anterior que ja calculamos 
			//ignoramos a nova posiçao
			Vector3 previousPos = new Vector3 ();

			var smoothedPath = new GlobalPath
            {
                IsPartial = globalPath.IsPartial
            };

			//line point 1
			Vector3 lineP1 = data.position; //start position

			int size = globalPath.PathPositions.Count - 1;
			Vector3 p_next = globalPath.PathPositions [size];
			smoothedPath.PathPositions.Add (p_next);

			int index = globalPath.PathNodes.Count - 1;
			while (index > 0) 
			{
				var node = globalPath.PathNodes[index];
				//last edge before p_next
				smoothedPath.PathNodes.Add(node);
				var edge = node as NavMeshEdge;

				Vector3 p_e = MathHelper.ClosestPointInLineSegment2ToLineSegment1(lineP1, p_next, edge.PointOne, edge.PointTwo, (edge.PointOne + edge.PointTwo) / 2);

				if (previousPos != p_e)
					smoothedPath.PathPositions.Add(p_e);
				previousPos = p_e;
				p_next = p_e;
				index--;
			}

			//index == 0 ---> first edge in path
			smoothedPath.PathPositions.Reverse ();
			smoothedPath.PathNodes.Reverse ();

            return smoothedPath;
        }


    }
}
