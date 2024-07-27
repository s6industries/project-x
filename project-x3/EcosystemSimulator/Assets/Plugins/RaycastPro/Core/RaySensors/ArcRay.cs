namespace RaycastPro.RaySensors
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [HelpURL("https://www.youtube.com/watch?v=OdonhX2GQII")]
    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(ArcRay))]
    public sealed class ArcRay : PathRay, IRadius
    {
        public int segments = 8;
        
        public float elapsedTime = 5f;

        public bool velocityLocal;

        [SerializeField] private float radius;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public Vector3 velocity;
        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                DetectIndex = PathCast(radius);
            }
        }
        
        /// <summary>
        /// Using in Gizmo and OnCast Separately
        /// </summary>
        /// <returns></returns>
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            _tPos = transform.position;
            PathPoints.Add(_tPos);
            g = velocityLocal ? transform.TransformDirection(velocity) : velocity;
            _dir = Direction;
            for (var i = 1; i <= segments; i++)
            {
                t = (float) i / segments * elapsedTime;
                _pos = _tPos + (_dir * t + g * (t * t) / 2);
                PathPoints.Add(_pos);
            }
        }
        private float t;
        private Vector3 g, _dir, _tPos, _pos;
        
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a ray based on the incoming velocity and return hit Info." + HAccurate + HDirectional + HPathRay + HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            FullPathDraw(radius, true);
            
            if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                BeginHorizontal();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(velocity)));
                LocalField(_so.FindProperty(nameof(velocityLocal)));
                EndHorizontal();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(segments)));
                segments = Mathf.Max(1, segments);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(elapsedTime)));
                RadiusField(_so);
            }

            if (hasGeneral) PathRayGeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif
    }
}