using System.Collections.Generic;

namespace RaycastPro.Detectors
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(RangeDetector))]
    public sealed class RangeDetector : ColliderDetector, IRadius, IPulse
    {
        [SerializeField] private float radius = 2f;
        
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }

        [SerializeField] private float height;

        [SerializeField] public bool local = true;

        public float Height
        {
            get => height;
            set => height = Mathf.Max(0, value);
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

        private Collider nearestMember;

        public Collider NearestMember => nearestMember;
        private bool HeightCheck(Vector3 point)
        {
            _h = _t.InverseTransformDirection(point - _t.position);
            m = new Vector2(_h.x, _h.z).sqrMagnitude;
            return (Mathf.Abs(_h.y) <= cylinderH && m <= radius * radius) ||
                   Vector3.Distance(point, _t.position + h) <= radius ||
                   Vector3.Distance(point, _t.position - h) <= radius;
        }

        #region Temps
        private Vector3 _h;
        private float m, _distance;
        private Transform _t;
        private float cylinderH;
        private Vector3 h;
        #endregion
        
        
        protected override void OnCast()
        {
            CachePrevious();
#if UNITY_EDITOR
            CleanGate();
#endif
            _t = transform;
            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                
                if (height > 0)
                {
                    cylinderH = height - radius;
                    h =  (local ? _t.up : Vector3.up);
                    Physics.OverlapCapsuleNonAlloc(_t.position - h, _t.position + h, radius, colliders,
                        detectLayer.value, triggerInteraction);
                }
                else
                {
                    Physics.OverlapSphereNonAlloc(_t.position, radius, colliders, detectLayer.value,
                        triggerInteraction);
                }
            }
            else
            {
                if (height > 0)
                {
                    cylinderH = height - radius;
                    h = (local ?_t.up : Vector3.up) * height/2;
                    colliders = Physics.OverlapCapsule(_t.position - h, _t.position + h, radius, detectLayer.value,
                        triggerInteraction);
                }
                else
                {
                    colliders = Physics.OverlapSphere(_t.position, radius, detectLayer.value, triggerInteraction);
                }
            }
            
            Clear();
            
            if (IsIgnoreSolver)
            {
                foreach (var c in colliders)
                {
                    if (TagPass(c))
                    {
#if UNITY_EDITOR
                        PassColliderGate(c);
#endif
                        DetectedColliders.Add(c);
                    }
                }
            }
            else
            {
                foreach (var c in colliders)
                {
                    if (!TagPass(c)) continue;
                    TDP = DetectFunction(c); // 1: Get Detect Point
                    if (height > 0)
                    {
                        if (HeightCheck(TDP) && LOSPass(TDP, c))
                        {
                            if (_distance <= _tDis)
                            {
                                _tDis = _distance;
                                nearestMember = c;
                            }
                            DetectedColliders.Add(c);
                        }
                    }
                    else
                    {
                        if ((_t.position-TDP).sqrMagnitude <= radius*radius && LOSPass(TDP, c))
                        {
                            if (_distance <= _tDis)
                            {
                                _tDis = _distance;
                                nearestMember = c;
                            }
                            DetectedColliders.Add(c);
                        }
                    }
                }
            }
            EventPass();
        }
#if UNITY_EDITOR
        
#pragma warning disable CS0414
        private static string Info =
            "Receiving colliders within the specified range along with a detect point solver." + HAccurate +
            HCDetector + HLOS_Solver + HIRadius + HRotatable + HINonAllocator;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            DrawFocusVector();
            GizmoColor = DefaultColor;
            _t = transform;
            if (height > 0)
            {
                h = (local ? _t.up : Vector3.up) * (height / 2);
                DrawCapsuleLine(_t.position - h, _t.position + h, radius, _t: transform);
            }
            else
            {
                DrawSphere(_t.position, _t.up, radius);
            }
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
                
                HeightField(_so);
            }
            if (hasGeneral) ColliderDetectorGeneralField(_so);

            if (hasEvents)
            {
                EventField(_so); if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }

            if (hasInfo) InformationField(PanelGate);
        }

        private Vector3 direct, cross;
        private float distance;
        private ISceneGUI _sceneGUIImplementation;
        protected override void DrawDetectorGuide(Vector3 point)
        {
            if (!GuideCondition) return;
            _t = transform;
            direct = point - _t.position;
            distance = Vector3.Distance(_t.position, point);
            if (height == 0)
            {
                Handles.color = DetectColor;
                cross = -Vector3.Cross(direct, _t.right);
                Handles.DrawWireArc(_t.position, cross, direct, 10f, distance);
                Handles.DrawWireArc(_t.position, cross, direct, -10f, distance);

                Handles.color = HelperColor;
                Handles.DrawWireArc(_t.position, cross, direct, 20f, radius);
                Handles.DrawWireArc(_t.position, cross, direct, -20f, radius);
                
                Handles.color = HelperColor;
                Handles.DrawDottedLine(point, _t.position + direct.normalized * radius, StepSizeLine);
            }
        }
#endif

    }
}