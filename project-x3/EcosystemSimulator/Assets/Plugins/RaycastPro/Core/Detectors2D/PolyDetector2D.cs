

namespace RaycastPro.Detectors2D
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(PolyDetector2D))]
    public class PolyDetector2D : ColliderDetector2D, IPulse
    {
        [SerializeField] private float minRadius = 4f;
        [SerializeField] private float maxRadius = 8f;

        [SerializeField] private byte edgeCount = 5;

        private Vector2[] worldPointsNear = Array.Empty<Vector2>(), worldPointsFar = Array.Empty<Vector2>();

        public byte EdgeCount // Temp Setup for optimizing on OnStateChange Event
        {
            get => edgeCount;
            set
            {
                edgeCount = value;
                
                Resize();
            }
        }

        public float MinRadius
        {
            get => minRadius;
            set => minRadius = Mathf.Min(maxRadius, value);
            // refresh = true;
        }
        public float MaxRadius
        {
            get => maxRadius;
            set => maxRadius = Mathf.Max(minRadius, value);
            //refresh = true;
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
                    colliders = new Collider2D[limitCount];
                }
            }
        }
        public int LimitCount
        {
            get => limitCount;
            set
            {
                limitCount = Mathf.Max(0,value);
                colliders = new Collider2D[limitCount];
            }
        }

        [SerializeField] private Collider2D[] colliders = Array.Empty<Collider2D>();
        
        private void Resize()
        {
            worldPointsNear = new Vector2[edgeCount + 1];
            worldPointsFar = new Vector2[edgeCount + 1];
        }

        private Vector3 _pos, forward;
        private float step;
        private Quaternion Q;
        private void CalculatePoints()
        {
            _pos = transform.position;
            forward = transform.right;
            step = 360f / edgeCount;
            for (var i = 0; i < edgeCount; i++)
            {
                Q = Quaternion.AngleAxis(step * i, Vector3.forward);
                if (minRadius > 0) worldPointsNear[i] = Q * (forward * minRadius) + _pos;
                worldPointsFar[i] = Q * (forward * maxRadius) + _pos;
            }
            
            worldPointsNear[edgeCount] = worldPointsNear[0];
            worldPointsFar[edgeCount] = worldPointsFar[0];
        }

        private Vector2 pos2D;
        private RaycastHit2D blockHit;
        protected override void OnCast()
        {
            PreviousColliders = DetectedColliders.ToArray();
            
#if UNITY_EDITOR
            CleanGate();
#endif

            if (worldPointsFar.Length != edgeCount + 1) Resize();
            pos2D = transform.position.To2D();
            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                
                Physics2D.OverlapCircleNonAlloc(pos2D, maxRadius, colliders, detectLayer.value, MinDepth, MaxDepth);    
            }
            else
            {
                colliders = Physics2D.OverlapCircleAll(pos2D, maxRadius, detectLayer.value, MinDepth, MaxDepth);
            }
            
            Clear();
            
            CalculatePoints();
            
            foreach (var c in colliders)
            {
                if (!TagPass(c)) continue;
                if (IsIgnoreSolver && PassCondition(c.transform.position))
                {
#if UNITY_EDITOR
                    PassColliderGate(c);
#endif
                    DetectedColliders.Add(c);
                    if (collectLOS) DetectedLOSHits.Add(c, _blockHit);
                    continue;
                }
                TDP = DetectFunction(c);
                if (!PassCondition(TDP)) continue;
                blockHit = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
#if UNITY_EDITOR
                PassGate(c, TDP, _blockHit);
#endif
                if (!_blockHit || _blockHit.transform == c.transform) DetectedColliders.Add(c);
            }
        }
        private bool PassCondition(Vector2 point)
        {
            return IsInPolygon2D(worldPointsFar, point) && (minRadius <= 0 || !IsInPolygon2D(worldPointsNear, point));
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Auto Generate a poly and made it to Realtime collider detector." +
                                     HCDetector + HLOS_Solver + HINonAllocator;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            
             if (edgeCount == 0 || worldPointsFar == null) return;
             
                for (var i = 0; i < edgeCount; i++)
                {
                    GizmoColor = DefaultColor;
                    Handles.DrawLine(worldPointsFar[i].ToDepth(z), worldPointsFar[i + 1].ToDepth(z));
                    Handles.DrawLine( worldPointsNear[i].ToDepth(z),  worldPointsNear[i+1].ToDepth(z));

                    if (minRadius > 0)
                    {
                        Handles.DrawLine(worldPointsNear[i].ToDepth(MinDepth), worldPointsNear[i+1].ToDepth(MinDepth));
                        Handles.DrawLine(worldPointsNear[i].ToDepth(MaxDepth), worldPointsNear[i+1].ToDepth(MaxDepth));
                        var vector3s = new Vector3[]
                        {
                            worldPointsNear[i],
                            worldPointsNear[i + 1],
                            worldPointsFar[i + 1],
                            worldPointsFar[i],
                        };

                        Handles.color = GetPolygonColor(minRadius > maxRadius);

                        Handles.DrawAAConvexPolygon(vector3s.ToDepth(z));
                    }
                }

                if (minRadius <= 0)
                {
                    Handles.color = GetPolygonColor();

                    Handles.DrawAAConvexPolygon(worldPointsFar.ToDepth(z));
                }
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                RadiusField(_so, nameof(minRadius), CMinRadius.ToContent(TMinRadius));
                RadiusField(_so, nameof(maxRadius), CMaxRadius.ToContent(TMaxRadius));
                PropertyIntSliderField(_so.FindProperty(nameof(edgeCount)), 3, 32, CEdgeCount.ToContent(TEdgeCount));
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                NonAllocatorField(_so, _so.FindProperty(nameof(colliders)));
                BaseField(_so);
                SolverField(_so);
                IgnoreListField(_so);
            }

            if (hasEvents) EventField(_so);
            if (hasEvents)
            {
                if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }
            if (hasInfo) InformationField(PanelGate);
            }
#endif
    }
}