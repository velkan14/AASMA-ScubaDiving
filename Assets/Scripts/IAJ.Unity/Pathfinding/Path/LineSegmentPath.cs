﻿using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Path
{
    public class LineSegmentPath : LocalPath
    {
        protected Vector3 LineVector;
        public LineSegmentPath(Vector3 start, Vector3 end)
        {
            this.StartPosition = start;
            this.EndPosition = end;
            this.LineVector = end - start;
        }

        public override Vector3 GetPosition(float param)
        {
            return Vector3.Lerp(StartPosition, EndPosition, param);
        }

        public override bool PathEnd(float param)
        {
            if (param >= 0.8)
                return true;
            else
                return false;
        }

        public override float GetParam(Vector3 position, float lastParam)
        {
            return Utils.MathHelper.closestParamInLineSegmentToPoint(StartPosition, EndPosition, position);
        }
    }
}
