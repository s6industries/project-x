using System.Collections.Generic;

namespace RaycastPro.Detectors
{
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(SightDetector))]
    public sealed class SightDetector : ColliderDetector, IRadius, IPulse
    {
#if UNITY_EDITOR
        private const float BezierWidth = .5f;
        private Vector3 ArcXStartPoint => Quaternion.AngleAxis(-angleX / 2, transform.up) * transform.forward * radius;
        private Vector3 ArcXEndPoint => Quaternion.AngleAxis(angleX / 2, transform.up) * transform.forward * radius;
        private Vector3 ArcYStartPoint =>
            Quaternion.AngleAxis(-angleY / 2, transform.right) * transform.forward * radius;
        private Vector3 ArcYEndPoint => Quaternion.AngleAxis(angleY / 2, transform.right) * transform.forward * radius;
#endif
        
        public float angleX = 120f;
        public float angleY = 90f;

        [SerializeField] public float radius = 2f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }
        [SerializeField] private bool limited;
        [SerializeField] private int limitCount = 3;

        public bool Limited
        {
            get => limited;
            set
            {
                limited = value;
                if (value)
                {
                    colliders = new Collider[limitCount];
                }
            }
        }

        public int LimitCount
        {
            get => limitCount;
            set
            {
                limitCount = Mathf.Max(0, value);
                colliders = new Collider[limitCount];
            }
        }
        
        public float minRadius = 1f;
        public float fullAwareness = 2f;

        private float tempDis;
        private Vector3 _p, _tVec;
        private Transform _t;
        protected override void OnCast()
        {
            CachePrevious();

#if UNITY_EDITOR
            CleanGate();
#endif
            _t = transform;
            _p = _t.position;
            
            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                Physics.OverlapSphereNonAlloc(_p, radius, colliders, detectLayer.value, triggerInteraction);    
            }
            else
            {
                colliders = Physics.OverlapSphere(_p, radius, detectLayer.value, triggerInteraction);
            }

            Clear();
            
            foreach (var c in colliders)
            {
                if (!TagPass(c)) continue;
                
                TDP = DetectFunction(c);
                _tVec = TDP - _p;
                tempDis = _tVec.sqrMagnitude;
                if (tempDis > fullAwareness*fullAwareness) // Full Awareness Shortcut
                {
                    if (tempDis > radius*radius || tempDis < minRadius*minRadius) continue;
                    if (Vector3.Angle(Vector3.ProjectOnPlane(_tVec, _t.up), _t.forward) > angleX / 2) continue;
                    if (Vector3.Angle(_tVec, Vector3.ProjectOnPlane(_tVec, _t.up)) > angleY / 2) continue;
                }
                
                if (IsIgnoreSolver)
                {
#if UNITY_EDITOR
                    PassColliderGate(c);
#endif
                    DetectedColliders.Add(c);
                }
                else if (LOSPass(TDP, c)) DetectedColliders.Add(c);
            }

            EventPass();
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receiving colliders within the specified FOV angles with a detect point solver." + HCDetector + HLOS_Solver + HIPulse + HIRadius + HINonAllocator;
#pragma warning restore CS0414


        private float lerp, sinX, sinY;
        private Vector3 up, right, arcXEndPoint, arcXStartPoint, arcYStartPoint, arcYEndPoint, upSinYMin, rightSinXMin, upSinY, rightSinX;
        private Vector3 s1, s2, s3, s4;
        internal override void OnGizmos()
        {
            EditorUpdate();

            DrawFocusVector();

            GizmoGate?.Invoke();
            _t = transform;
            _p = _t.position;
            up = _t.up;
            right = _t.right;
            
            GizmoColor = DetectedColliders.Count > 0 ? DetectColor : DefaultColor;

            void Draw()
            {
                lerp = minRadius / radius;
                arcXEndPoint = ArcXEndPoint;
                arcXStartPoint = ArcXStartPoint;
                arcYStartPoint = ArcYStartPoint;
                arcYEndPoint = ArcYEndPoint;
                
                if (fullAwareness > 0)
                {
                    if (fullAwareness > minRadius)
                    {
                        Handles.DrawWireArc(_p, -up, arcXStartPoint, 360 - angleX, fullAwareness);
                    }
                    else
                    {
                        Handles.DrawWireDisc(_p, Vector3.up, fullAwareness);
                    }
                }
                
                s1 = Vector3.Lerp(_p, _p + arcXEndPoint, lerp);
                s2 = Vector3.Lerp(_p, _p + arcXStartPoint, lerp);
                s3 = Vector3.Lerp(_p, _p + arcYStartPoint, lerp);
                s4 = Vector3.Lerp(_p, _p + arcYEndPoint, lerp);
                Gizmos.DrawLine(s1, _p + arcXEndPoint);
                Gizmos.DrawLine(s2, _p + arcXStartPoint);
                Gizmos.DrawLine(s3, _p + arcYStartPoint);
                Gizmos.DrawLine(s4, _p + arcYEndPoint);
                sinX = Mathf.Sin(angleX / 2 * Mathf.Deg2Rad);
                sinY = Mathf.Sin(angleY / 2 * Mathf.Deg2Rad);
                upSinYMin = up * (sinY * minRadius);
                rightSinXMin = right * (sinX * minRadius);
                
                upSinY = up * (sinY * radius);
                rightSinX = right * (sinX * radius);
                Handles.DrawWireArc(_p, up, arcXStartPoint, angleX, radius);
                Handles.DrawWireArc(_p, right, arcYStartPoint, angleY, radius);
                Handles.DrawBezier(_p + arcXStartPoint, _p + arcYStartPoint,
                    _p + arcXStartPoint + upSinY, _p + arcYStartPoint - rightSinX,
                    Handles.color, Texture2D.whiteTexture, BezierWidth);
                Handles.DrawBezier(_p + arcXEndPoint, _p + arcYStartPoint, _p + arcXEndPoint + upSinY,
                    _p + arcYStartPoint + rightSinX, Handles.color, Texture2D.whiteTexture, BezierWidth);
                Handles.DrawBezier(_p + arcXStartPoint, _p + arcYEndPoint,
                    _p + arcXStartPoint - upSinY,
                    _p + arcYEndPoint - rightSinX, Handles.color, Texture2D.whiteTexture, BezierWidth);
                Handles.DrawBezier(_p + arcXEndPoint, _p + arcYEndPoint, _p + arcXEndPoint - upSinY,
                    _p + arcYEndPoint + rightSinX, Handles.color, Texture2D.whiteTexture, BezierWidth);
                
                // === MIN RADIUS ===
                if (minRadius > 0)
                {
                    Handles.DrawBezier(s2, s3, s2 + upSinYMin, s3 - rightSinXMin, Handles.color, Texture2D.whiteTexture, BezierWidth);
                    Handles.DrawBezier(s1, s3, s1 + upSinYMin, s3 + rightSinXMin, Handles.color, Texture2D.whiteTexture, BezierWidth);
                    Handles.DrawBezier(s2, s4, s2 - upSinYMin, s4 - rightSinXMin, Handles.color, Texture2D.whiteTexture, BezierWidth);
                    Handles.DrawBezier(s1, s4, s1 - upSinYMin, s4 + rightSinXMin, Handles.color, Texture2D.whiteTexture, BezierWidth);
                    
                    Handles.DrawWireArc(_p, up, arcXStartPoint, angleX, minRadius);
                    Handles.DrawWireArc(_p, right, arcYStartPoint, angleY, minRadius);
                }
            }
            DrawZTest(Draw, Gizmos.color.Alpha(AlphaAmount), Gizmos.color);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                RadiusField(_so);
                RadiusField(_so, nameof(minRadius), "Min Radius".ToContent());
                
                var fullAwarenessProp = _so.FindProperty(nameof(fullAwareness));
                EditorGUILayout.PropertyField(fullAwarenessProp, fullAwarenessProp.displayName.ToContent("The range of full awareness will always be detected regardless of the viewing angle."));
                fullAwarenessProp.floatValue = Mathf.Clamp(fullAwarenessProp.floatValue, 0, radius);
                
                PropertySliderField(_so.FindProperty(nameof(angleX)), 0f, 360f, CArcHorizontal.ToContent("The horizontal range of vision, which is counted in Degress units."));
                PropertySliderField(_so.FindProperty(nameof(angleY)), 0f, 180, CArcVertical.ToContent("The vertical range of vision, which is counted in Degress units."));
                GUI.enabled = true;
            }

            if (hasGeneral) ColliderDetectorGeneralField(_so);

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout)
                    RCProEditor.EventField(_so,
                        new[] {nameof(onDetectCollider), nameof(onNewCollider), nameof(onLostCollider)});
            }

            if (hasInfo) InformationField(PanelGate);
            
        }
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif

    }
}