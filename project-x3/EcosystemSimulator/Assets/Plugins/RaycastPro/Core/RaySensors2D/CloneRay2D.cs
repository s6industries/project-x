namespace RaycastPro.RaySensors2D
{
    using Planers2D;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
        using UnityEditor;
#endif


    [AddComponentMenu("")]
    public sealed class CloneRay2D : PathRay2D
    {
        private Planar2D getter;
        private Transform outer;

        private RaySensor2D sensor;
        
        // Non Allocation 
        private readonly List<Vector2> _tPath = new List<Vector2>();
        
        private float radius;

        internal void CopyFrom(RaySensor2D raySensor, Planar2D _getter, Transform _outer)
        {
            getter = _getter;
            outer = _outer;
            sensor = raySensor;
            detectLayer = raySensor.detectLayer;
            direction = raySensor.direction;
            planarSensitive = raySensor.planarSensitive;
            anyPlanar = raySensor.anyPlanar;
            if (raySensor is PathRay2D pathRay)
            {
                pathCast = getter.clonePathCast && pathRay.pathCast;
            }
            if (!anyPlanar) planers = raySensor.planers;
            if (raySensor is IRadius iRadius) radius = iRadius.Radius;
            planers = raySensor.planers;
            stamp = raySensor.stamp;
            stampAutoHide = raySensor.stampAutoHide;
            stampOffset = raySensor.stampOffset;
            stampOnHit = raySensor.stampOnHit;
            syncStamp = raySensor.syncStamp;
            linerCutOnHit = raySensor.linerCutOnHit;
#if UNITY_EDITOR
            gizmosUpdate = raySensor.gizmosUpdate;
#endif

            if (raySensor.liner)
            {
                liner = CopyComponent(raySensor.liner, gameObject);
                linerCutOnHit = raySensor.linerCutOnHit;
                linerClamped = raySensor.linerClamped;
                linerBasePosition = raySensor.linerBasePosition;
                linerEndPosition = raySensor.linerEndPosition;
                UpdateLiner();
            }
        }

        private Vector3 _p;
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            DetectIndex = -1;

            if (baseRaySensor is PathRay2D pathRay)
            {
                _tPath.Clear();
                _tPath.Add(Vector2.zero);
                for (var i = pathRay.DetectIndex + 1; i < pathRay.PathPoints.Count; i++)
                {
                    var _dir = pathRay.PathPoints[i] - pathRay.hit.point;
                    _tPath.Add(_dir);
                }
                for (var index = 0; index < _tPath.Count; index++)
                {
                    _p = _tPath[index];
                    _p = pathRay.transform.InverseTransformDirection(_p);
                    PathPoints.Add(transform.TransformDirection(_p) + transform.position);
                }
            }
            else
            {
               PathPoints.Add(outer.TransformPoint(getter.transform.InverseTransformPoint(sensor.hit.point)).ToDepth(z));
               PathPoints.Add(outer.TransformPoint(getter.transform.InverseTransformPoint(sensor.Tip)).ToDepth(z));
            }
        }
        protected override void OnCast()
        {
            UpdatePath();
            
            if (pathCast)
            {
                if (sensor is IRadius iRadius) radius = iRadius.Radius;
                DetectIndex = PathCast(out hit, radius);
                isDetect = FilterCheck(hit);
            }
        }

#if UNITY_EDITOR

#pragma warning disable CS0414
        private static string Info = "This ray executes a copy of the input ray to the planar and is simply not adjustable." + HVirtual;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            if (PathPoints.Count == 0) return;
            if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);
            FullPathDraw(radius);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            BeginVerticalBox();
            
            if (IsPlaying)
            {
                if (sensor) EditorGUILayout.LabelField("Clone From: " + sensor.gameObject.name);
                if (getter) EditorGUILayout.LabelField("Getter: " + getter.gameObject.name);
                if (outer) EditorGUILayout.LabelField("Outer: " + outer.gameObject.name);
            }

            EditorGUILayout.LabelField("Clone Rays can't be modified.");
            
            EndVertical();
            BaseField(_so, hasInfluence: false, hasInteraction: false, hasUpdateMode: false);
            InformationField();
        }
#endif
    }
}