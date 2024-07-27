namespace RaycastPro.RaySensors
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [HelpURL("https://www.youtube.com/watch?v=Oaj0dqbfgFM")]
    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(ReflectRay))]
    public sealed class ReflectRay : PathRay, IRadius
    {
        /// <summary>
        /// Read this list for accessing Raycast Hits
        /// </summary>
        public readonly List<RaycastHit> reflectHits = new List<RaycastHit>();
        public LayerMask reflectLayer;
        
        [SerializeField] private float radius;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        public Axis planeAxis;
        public bool hasFreezeAxis;

        [Tooltip("The number of ray reflection that will be cut off when reaching it. Negative numbers use free direction length.")]
        public int maxReflect = -1;

        private Vector3 point, _direction;
        private float distance;
        private RaycastHit _tHit;
        private int DI;
        protected override void UpdatePath()
        {
            Vector3 ApplyFreeze(Vector3 dir)
            {
                if (!hasFreezeAxis) return dir;
                switch (planeAxis)
                {
                    case Axis.X: dir.x = 0; break;
                    case Axis.Y: dir.y = 0; break;
                    case Axis.Z: dir.z = 0; break;
                }
                return dir;
            }
            PathPoints.Clear();
            reflectHits.Clear();
            point = transform.position;
            _direction = Direction;
            _direction = ApplyFreeze(_direction);
            PathPoints.Add(transform.position);
            distance = direction.magnitude;
            DetectIndex = -1;
            hit = new RaycastHit();
            // Strictly Queries Hit back most be false
            var physicsSetting = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = false;
            DI = -1;
            while (true)
            {
                DI++;
                if (maxReflect > 0 && DI+1 > maxReflect) break;
                _tHit = new RaycastHit();
                if (radius > 0)
                {
                    Physics.SphereCast(point, radius, _direction, out _tHit, distance, reflectLayer.value | detectLayer.value,
                        triggerInteraction);
                }
                else
                    Physics.Raycast(point, _direction, out _tHit, distance, reflectLayer.value | detectLayer.value,
                        triggerInteraction);
                    
                if(_tHit.transform)
                {
                    reflectHits.Add(_tHit);
                    PathPoints.Add(_tHit.point);
                    if (detectLayer.InLayer(_tHit.transform.gameObject))
                    {
                        DetectIndex = DI;
                        hit = _tHit;
                        break;
                    }
                    distance -= (_tHit.point - point).magnitude;
                    point = _tHit.point;
                    _direction = Vector3.Reflect(_direction, _tHit.normal);
                    _direction = ApplyFreeze(_direction);
                    continue;
                }
                PathPoints.Add(point + _direction.normalized * distance);
                break;
            }
            Physics.queriesHitBackfaces = physicsSetting;
        }

        protected override void OnCast() => UpdatePath();

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a reflective ray to the <i>Reflect layer</i> and detect the point of impact in the <i>Detect layer</i>." +
                                                 HAccurate + HPathRay + HRecursive+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();

            FullPathDraw(radius, true);

            for (var index = 0; index < reflectHits.Count-1; index++)
            {
                var _hit = reflectHits[index];
                DrawCross(_hit.point, _hit.normal);
            }

            if (reflectHits.Count > 0)
            {
                DrawNormal(reflectHits[reflectHits.Count-1]);
            }
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Plane Axis");
                hasFreezeAxis = EditorGUILayout.Toggle(hasFreezeAxis, GUILayout.Width(20));
                GUI.enabled = hasFreezeAxis;
                planeAxis = (Axis) GUILayout.SelectionGrid((int) planeAxis, Enum.GetNames(typeof(Axis)), 3);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                RadiusField(_so);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(maxReflect)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(reflectLayer)),
                    CReflectLayer.ToContent(TReflectLayer));
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) InformationField(() => reflectHits.ForEach(r =>
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{reflectHits.IndexOf(r)}: {r.transform.name}");
                GUILayout.Label(r.point.ToString());
                GUILayout.EndHorizontal();
            }));
        }
#endif
    }
}