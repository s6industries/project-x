using System.Collections.Generic;

namespace RaycastPro.Detectors
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(PolyDetector))]
    public class PolyDetector : ColliderDetector, IPulse
    {
        public float minRadius = 2f;
        public float maxRadius = 4f;

        public float height = 2f;
        [SerializeField] private int edgeCount = 5;
        
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
        public int EdgeCount
        {
            get => edgeCount;
            internal set
            {
                edgeCount = value;
                
                Resize();
            }
        }
        
        [SerializeField] private Vector3[] worldPointsNear = new Vector3[6];
        [SerializeField] private Vector3[] worldPointsFar = new Vector3[6];
        [SerializeField] private Vector3[] upPointsNear = new Vector3[6];
        [SerializeField] private Vector3[] upPointsFar = new Vector3[6];
        [SerializeField] private Vector3[] downPointsNear = new Vector3[6];
        [SerializeField] private Vector3[] downPointsFar = new Vector3[6];

        private void Resize()
        {
            worldPointsNear = new Vector3[edgeCount + 1];
            worldPointsFar = new Vector3[edgeCount + 1];
            upPointsNear = new Vector3[edgeCount + 1];
            upPointsFar = new Vector3[edgeCount + 1];
            downPointsNear = new Vector3[edgeCount + 1];
            downPointsFar = new Vector3[edgeCount + 1];
        }

        private Vector3 position, up, forward, h;
        private int i;
        private float step;
        private Quaternion axis;
        protected override void OnCast()
        {
            CachePrevious();
#if UNITY_EDITOR
            CleanGate();
#endif
            if (worldPointsFar.Length != edgeCount + 1) Resize();
            position = transform.position;
            up = Vector3.up;
            forward = Vector3.ProjectOnPlane(transform.forward, up);
            h = height / 2 * up;
            step = 360f / edgeCount;
            
            for (i = 0; i <= edgeCount; i++)
            {
                if (i < edgeCount)
                {
                    axis = Quaternion.AngleAxis(step * i, up);
                    worldPointsFar[i] = axis * (forward * maxRadius);
                    worldPointsFar[i] += position;

                    if (minRadius > 0)
                    {
                        worldPointsNear[i] = axis * (forward * minRadius);
                        worldPointsNear[i] += position;
                    }
                }
                else
                {
                    worldPointsNear[edgeCount] = worldPointsNear[0];
                    worldPointsFar[edgeCount] = worldPointsFar[0];
                }

                if (minRadius > 0)
                {
                    upPointsNear[i] = worldPointsNear[i] + h;
                    downPointsNear[i] = worldPointsNear[i] - h;
                }

                upPointsFar[i] = worldPointsFar[i] + h;
                downPointsFar[i] = worldPointsFar[i] - h;
            }
            
            Clear();
            
            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                Physics.OverlapBoxNonAlloc(position, new Vector3(maxRadius, height / 2, maxRadius), colliders,
                    Quaternion.identity, detectLayer.value, triggerInteraction);
            }
            else
            {
                colliders = Physics.OverlapBox(position, new Vector3(maxRadius, height / 2, maxRadius),
                    Quaternion.identity, detectLayer.value, triggerInteraction);
            }
            
            if (IsIgnoreSolver)
            {
                foreach (var c in colliders)
                {
                    if (TagPass(c) && PassCondition(c.transform.position))
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
                    TDP = DetectFunction(c);
                    if (PassCondition(TDP) && LOSPass(TDP, c)) DetectedColliders.Add(c);
                }
            }
            EventPass();
        }
        private bool PassCondition(Vector3 point)
        {
            return IsInPolygon(worldPointsFar, point) && (minRadius <= 0 || !IsInPolygon(worldPointsNear, point));
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receiving colliders within the Volumetric polygon with a detect point solver." +
                               HIPulse + HCDetector + HLOS_Solver + HINonAllocator;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();

            GizmoColor = (minRadius > maxRadius ? BlockColor : DefaultColor);
            
            for (var i = 0; i < edgeCount; i++)
            {
                if (minRadius > 0)
                {
                    Handles.DrawLine(worldPointsNear[i], worldPointsNear[i + 1]);
                    Handles.DrawLine(upPointsNear[i], upPointsNear[i + 1]);
                    Handles.DrawLine(downPointsNear[i], downPointsNear[i + 1]);
                    Handles.DrawLine(upPointsNear[i], downPointsNear[i]);
                }

                Handles.DrawLine(worldPointsFar[i], worldPointsFar[i + 1]);
                Handles.DrawLine(upPointsFar[i], upPointsFar[i + 1]);
                Handles.DrawLine(downPointsFar[i], downPointsFar[i + 1]);
                Handles.DrawLine(upPointsFar[i], downPointsFar[i]);
            }

            DrawFocusVector();
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                RadiusField(_so, nameof(minRadius), CMinRadius.ToContent(TMinRadius));
                RadiusField(_so, nameof(maxRadius), CMaxRadius.ToContent(TMaxRadius));
                HeightField(_so);
                PropertyIntSliderField(_so.FindProperty(nameof(edgeCount)), 3, 32, CEdgeCount.ToContent(TEdgeCount));
            }
            if (hasGeneral) ColliderDetectorGeneralField(_so);
            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }

            if (hasInfo) InformationField(PanelGate);
        }

        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
    }
}