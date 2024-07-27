namespace RaycastPro.RaySensors
{
    using System.Collections.Generic;
    using Planers;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    [AddComponentMenu("")]
    public sealed class CloneRay : PathRay
    {
        public RaySensor sensor;

        public Planar getter;
        public Transform outer;

        private float radius;
        
        // Non Allocation 
        private readonly List<Vector3> _tPath = new List<Vector3>();
        private Vector3 forward;
        internal void CopyFrom(RaySensor raySensor, Planar _getter, Transform _outer)
        {
            getter = _getter;
            outer = _outer;
            sensor = raySensor;
            detectLayer = raySensor.detectLayer;
            direction = raySensor.direction;
            planarSensitive = raySensor.planarSensitive;
            anyPlanar = raySensor.anyPlanar;
            if (raySensor is PathRay pathRay)
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

        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                if (sensor is IRadius iRadius) radius = iRadius.Radius;
                DetectIndex = PathCast(radius);
            }
        }

        private Vector3 _p, p0;
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            DetectIndex = -1;
            if (baseRaySensor is PathRay pathRay) // THIS FORMULA SUPPORT'S ALL PATH AS WELL
            {
                _tPath.Clear();
                _tPath.Add(Vector3.zero);
                for (var i = pathRay.DetectIndex + 1; i < pathRay.PathPoints.Count; i++)
                {
                    var _dir = pathRay.PathPoints[i] - pathRay.hit.point;
                    _tPath.Add( _dir);
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
                // This algorithm for single phase rays
                getter.GetForward(baseRaySensor, out forward);
                PathPoints.Add( transform.TransformDirection(getter.transform.InverseTransformDirection(Vector3.zero))+transform.position);
                if (transform.parent)
                {
                    PathPoints.Add( transform.forward*sensor.ContinuesDistance+transform.parent.position);
                }
                else
                {
                    PathPoints.Add( transform.TransformDirection((forward*sensor.ContinuesDistance))+transform.position);
                }
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "This ray executes a copy of the input ray to the planar and is simply not adjustable." + HAccurate + HVirtual;
#pragma warning restore CS0414

        [SerializeField] private bool showGizmos;
        internal override void OnGizmos()
        {
            if (showGizmos)
            {
                EditorUpdate();
                if (PathPoints.Count == 0) return;
                if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);
                if (IsManuelMode) UpdatePath();
                DrawPath(PathPoints, hit, radius, detectIndex: DetectIndex, drawSphere: true);
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            BeginVerticalBox();

            EditorGUILayout.PropertyField(_so.FindProperty(nameof(showGizmos)));
            if (IsPlaying)
            {
                if (baseRaySensor) EditorGUILayout.LabelField("Base From: " + baseRaySensor.gameObject.name);
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