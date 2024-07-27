using System.Linq;

namespace RaycastPro.RaySensors2D
{
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public abstract class PathRay2D : RaySensor2D
    {
        public bool pathCast = true;
        public override Vector3 RawTip => Tip;
        public override Vector3 Tip => PathPoints.LastOrBase(transform.position).ToDepth(z);
        public override float RayLength => PathPoints.GetPathLength();
        public override Vector3 Base => PathPoints.Count > 0 ? PathPoints[0].ToDepth(z) : transform.position;
        public override Vector2 HitDirection => PathPoints.LastDirection(LocalDirection);
        
        /// <summary>
        /// The length traveled from Ray to reach the target point
        /// </summary>
        public override float HitLength
        {
            get
            {
                var len = 0f;
                if (DetectIndex > -1)
                {
                    len = Vector3.Distance(hit.point,PathPoints[DetectIndex]);
                    for (var i = 1; i <= DetectIndex; i++)
                    {
                        len += PathPoints.GetEdgeLength(i);
                    }
                }
                else
                {
                    len = PathPoints.GetPathLength();
                }
                return len;
            }
        }
        public override void GetPath2D(ref List<Vector2> path) => path = PathPoints;
        public override void GetPath(ref List<Vector3> path) => path = PathPoints.ToDepth(z);
        
        public List<Vector2> PathPoints = new List<Vector2>();
        public override float ContinuesDistance
        {
            get
            {
                if (hit)
                {
                    var distance = 0f;
                    for (var i = PathPoints.Count - 1; i > 0; i--)
                    {
                        if (i == DetectIndex + 1)
                        {
                            distance += (PathPoints[i] - hit.point).magnitude;
                            break;
                        }
                        distance += (PathPoints[i] - PathPoints[i - 1]).magnitude;
                    }
                    return distance;
                }
                return 0;
            }
        }

        public int DetectIndex { get; internal set; } = -1;
        private Vector2 _dir;
        protected int PathCast(out RaycastHit2D hit, float radius = 0)
        {
            hit = new RaycastHit2D();
            if (radius == 0)
            {
                for (var i = 0; i < PathPoints.Count - 1; i++)
                {
                    hit = Physics2D.Linecast(PathPoints[i], PathPoints[i + 1], detectLayer.value, MinDepth, MaxDepth);
                    if (hit) return i;
                    
                }
            }
            else
            {
                for (var i = 0; i < PathPoints.Count - 1; i++)
                {
                    _dir = PathPoints[i + 1] - PathPoints[i];
                    hit = Physics2D.CircleCast(PathPoints[i], radius, _dir, _dir.magnitude, detectLayer.value, MinDepth, MaxDepth);
                    if (hit) return i;
                }
            }
            return -1;
        }
        protected abstract void UpdatePath();
        

        private Queue<Vector3> path;
        public override void UpdateLiner()
        {
            if (!liner) return;

            if (path == null) path = new Queue<Vector3>();

            if (linerClamped)
            {
                var (point1, index1) = PathPoints.GetPathInfo(linerBasePosition);
                var (point2, index2) = PathPoints.GetPathInfo(linerEndPosition);

                if (linerCutOnHit && hit) // Cut and detect Hit
                {
                    var detectIndex = DetectIndex;
                    var detectIndexPoint = PathPoints[detectIndex];
                    Vector2 hitPoint = linerFixCut
                        ? GetPointOnLine(PathPoints[detectIndex].ToDepth(z), PathPoints[detectIndex + 1].ToDepth(z), HitPointZ) : HitPointZ;
                    var sqrHitDistance = (hitPoint - detectIndexPoint).sqrMagnitude;
                    if (index1 < detectIndex + 1 || (index1 == detectIndex + 1 && (point1 - detectIndexPoint).sqrMagnitude <= sqrHitDistance))
                    {
                        path.Clear();
                        path.Enqueue(point1.ToDepth(z));
                        var maxPoint = Mathf.Min(index2, detectIndex + 1);
                        // Add remaining Points according to minimum index
                        for (var i = index1; i < maxPoint; i++) path.Enqueue(PathPoints[i].ToDepth(z));
                        if (detectIndex + 1 > index2) path.Enqueue(point2.ToDepth());
                        else
                            path.Enqueue((point2 - detectIndexPoint).sqrMagnitude <= sqrHitDistance ? point2.ToDepth() : hitPoint.ToDepth(z));

                        liner.positionCount = path.Count;
                        liner.SetPositions(path.ToArray());
                    }
                    else // base Point over of Hit Point
                    {
                        liner.positionCount = 0;
                    }
                }
                else // Cut Without Detection
                {
                    path.Clear();
                    path.Enqueue(point1.ToDepth(z));
                    liner.positionCount = index2 - index1 + 2;
                    for (var i = index1; i < index2; i++) path.Enqueue(PathPoints[i].ToDepth(z));
                    path.Enqueue(point2.ToDepth(z));
                    liner.SetPositions(path.ToArray());
                }
            }
            else
            {
                if (linerCutOnHit)
                {
                    if (DetectIndex > -1)
                    {
                        liner.positionCount = DetectIndex+2;
                        for (var i = 0; i <= DetectIndex; i++)
                        {
                            liner.SetPosition(i, PathPoints[i].ToDepth(z));
                        }
                        liner.SetPosition(DetectIndex+1,
                           linerFixCut ? GetPointOnLine(PathPoints[DetectIndex], PathPoints[DetectIndex + 1], hit.point).ToDepth(z) : HitPointZ);
                        return;
                    }
                    liner.positionCount = PathPoints.Count;
                    liner.SetPositions(PathPoints.ToDepth(z).ToArray());
                }
                else
                {
                    liner.positionCount = PathPoints.Count;
                    liner.SetPositions(PathPoints.ToDepth(z).ToArray());
                }
            }
        }
#if UNITY_EDITOR
        protected void FullPathDraw(float radius = 0f, bool cap = false)
        {
            if (InEditMode) UpdatePath();
            
            DrawDepthLine(PathPoints[0],  PathPoints.Last());
            
            DrawPath2D(PathPoints.ToDepth(z), isDetect: isDetect, breakPoint: HitPointZ, radius: radius, detectIndex: DetectIndex, z: z, drawDisc: true, coneCap: cap);
        }
        protected void PathRayGeneralField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(pathCast)));
            GeneralField(_so);
        }
#endif
    }
}