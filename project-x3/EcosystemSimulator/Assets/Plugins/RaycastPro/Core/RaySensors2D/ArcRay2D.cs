namespace RaycastPro.RaySensors2D
{
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif

    using UnityEngine;

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(ArcRay2D))]
    public sealed class ArcRay2D : PathRay2D, IRadius
    {
        public int segments = 5;
        
        public float elapsedTime = 5f;

        [SerializeField] private float radius = .1f;

        public bool velocityLocal;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }

        public Vector2 velocity;
        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                DetectIndex = PathCast(out hit, radius);
                isDetect = FilterCheck(hit);
            }
        }

        private Vector2 tPoint, g, pos, _tDir, _pos;
        private float t;
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            tPoint = Position2D;
            PathPoints.Add(tPoint);
            pos = Position2D;
            g = velocityLocal ? transform.TransformDirection(velocity).To2D() : velocity;
            _tDir = Direction;
            for (var i = 1; i <= segments; i++)
            {
                t = (float) i / segments * elapsedTime;
                _pos = pos + (_tDir * t + g * (t * t) / 2);
                PathPoints.Add(_pos);
            }
        }

#if UNITY_EDITOR

#pragma warning disable CS0414
        private static string Info = "Send a 2D Ray based on the incoming velocity and return hit Info." + HAccurate +
                                     HDirectional + HPathRay + HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            FullPathDraw(radius, true);
            DrawNormal2D(hit, z);
            DrawNormalFilter();
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(segments)),
                    CSegments.ToContent(TSegments));
                segments = Mathf.Max(1, segments);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(elapsedTime)),
                    "Elapsed Time".ToContent());
                BeginHorizontal();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(velocity)),
                    CVelocity.ToContent(CVelocity));
                LocalField(_so.FindProperty(nameof(velocityLocal)));
                EndHorizontal();
                
                RadiusField(_so);
            }

            if (hasGeneral) PathRayGeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }

#endif
    }
}