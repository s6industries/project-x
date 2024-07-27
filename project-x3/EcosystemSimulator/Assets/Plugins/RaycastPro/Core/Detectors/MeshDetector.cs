namespace RaycastPro.Detectors
{
    using UnityEngine;
    using System;
    using System.Linq;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    [RequireComponent(typeof(MeshCollider))]
    [AddComponentMenu("RaycastPro/Detectors/" + nameof(MeshDetector))]
    public sealed class MeshDetector: ColliderDetector, IPulse
    {
        [SerializeField] private MeshCollider meshCollider;

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

        #region Temps
        private Vector3 _h;
        private float m;
        private float cylinderH;
        private Vector3 h;
        private Vector3 _dir;
        #endregion

        private SphereCollider miniSphere;
        private void Reset()
        {
            meshCollider = GetComponent<MeshCollider>();
        }

        private float _vDis;
        private Vector3 _vDir;
        
        // Example method to draw the convex mesh wire gizmo

        private RaycastHit _hit;
        private int RaycastToCenter(Vector3 point, Vector3 to, int hitCount = 0)
        {
            if (Physics.Linecast(point, to, out _hit, detectLayer.value, triggerInteraction))
            {
                if (_hit.transform == transform)
                {
                    hitCount += 1;
                }
                RaycastToCenter(_hit.point+(to-point).normalized*.01f, to, hitCount);
            }
            
            return hitCount;
        }

        private bool CheckMeshPass(Vector3 point) => RaycastToCenter(point, meshCollider.bounds.center) == 0;
        protected override void OnCast()
        {
            CachePrevious();
#if UNITY_EDITOR
            CleanGate();
#endif
            if (limited)
            {
                Array.Clear(colliders, 0, colliders.Length);
                Physics.OverlapBoxNonAlloc(meshCollider.bounds.center, meshCollider.bounds.extents, colliders, transform.rotation, detectLayer.value, triggerInteraction);
            }
            else
            {
                colliders = Physics.OverlapBox(meshCollider.bounds.center, meshCollider.bounds.extents, transform.rotation, detectLayer.value, triggerInteraction);
            }
            
            Clear();
            
            if (IsIgnoreSolver)
            {
                foreach (var c in colliders)
                {
                    if (c.transform != transform && TagPass(c) && CheckMeshPass(c.bounds.center))
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
                var _t = Physics.queriesHitBackfaces;
                Physics.queriesHitBackfaces = true;
                foreach (var c in colliders)
                {
                    if (c.transform == transform || !TagPass(c)) continue;
                    TDP = DetectFunction(c); // 1: Get Detect Point
                    if (CheckMeshPass(BoundsCenter(c)) && LOSPass(TDP, c))
                    {
                        DetectedColliders.Add(c);
                    }
                    
                }
                Physics.queriesHitBackfaces = _t;
            }
            
            
            EventPass();
        }
#if UNITY_EDITOR
        
#pragma warning disable CS0414
        private static string Info =
            "The ability to detect points in the dominant convex mesh." + HDependent + HCDetector + HLOS_Solver + HRotatable + HScalable + HINonAllocator;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            GizmoColor = Performed ? DetectColor : DefaultColor;
            
            if (meshCollider)
            {
                Gizmos.DrawWireMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
            

        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(meshCollider)));
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
        }
#endif

    }
}