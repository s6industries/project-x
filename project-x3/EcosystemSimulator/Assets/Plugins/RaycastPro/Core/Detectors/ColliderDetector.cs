namespace RaycastPro.Detectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    using UnityEngine;
    public abstract class ColliderDetector : Detector
    {
        public ColliderEvent onDetectCollider;
        public ColliderEvent onNewCollider;
        public ColliderEvent onLostCollider;
        public override bool Performed
        {
            get => DetectedColliders.Any();
            protected set { }
        }
        
        [SerializeField] public Collider[] ignoreList = Array.Empty<Collider>();
        
        [SerializeField]
        protected Collider[] colliders = Array.Empty<Collider>();

        /// <summary>
        /// Main List of Detected colliders. Use this for Get Colliders.
        /// </summary>
        public HashSet<Collider> DetectedColliders { get; protected set; } = new HashSet<Collider>();

        /// <summary>
        /// Array of Last Frame Detected colliders.
        /// </summary>
        public HashSet<Collider> PreviousColliders { get; protected set; } = new HashSet<Collider>();

        [Tooltip("This option is considered for optimization and limits the detection point in the bounds of a cube.")]
        public bool boundsSolver;
        [Tooltip("If selected, the detection point will be mounted on Collider Bounds Center. otherwise on transform.position.")]
        public bool boundsCenter;
        [Tooltip("Detector collect RaycastHits on \"DetectedLOSHits\" Dictionary. Key: Collider, Value: RaycastHit")]
        public bool collectLOS;
        
        protected readonly Dictionary<Collider, RaycastHit> detectedLOSHits = new Dictionary<Collider, RaycastHit>();
        public Dictionary<Collider, RaycastHit> DetectedLOSHits => detectedLOSHits;

        #region Methods
        public Collider FirstMember => DetectedColliders.FirstOrDefault();

        public Collider LastMember => DetectedColliders.LastOrDefault();
        
        public Vector3 GetAveragePosition
        {
            get
            {
                var average = Vector3.zero;
                foreach (var c in DetectedColliders) average += c.transform.position;
                return average / DetectedColliders.Count;
            }
        }

        /// <summary>
        /// It will calculate nearest collider based on current detected colliders on detector.
        /// </summary>
        /// <param name="nearest">Define a collider2D in your script and ref to it for get the nearest.</param>
        public void GetNearestCollider(out Collider nearest)
        {
            var _cDistance = Mathf.Infinity;
            nearest = null;
            foreach (var _col in DetectedColliders)
            {
                _tDis = (_col.transform.position - transform.position).sqrMagnitude;
                if (_tDis < _cDistance)
                {
                    _cDistance = _tDis;
                    nearest = _col;
                }
            }
        }
        /// <summary>
        /// It will calculate the furthest collider based on current detected colliders on detector.
        /// </summary>
        /// <param name="furthest">Define a collider2D in your script and ref to it for get the furthest.</param>
        public void GetFurthestCollider(out Collider furthest)
        {
            var _cDistance = 0f;
            furthest = null;
            foreach (var _col in DetectedColliders)
            {
                _tDis = (_col.transform.position - transform.position).sqrMagnitude;
                if (_tDis > _cDistance)
                {
                    _cDistance = _tDis;
                    furthest = _col;
                }
            }
        }
        
        

        #endregion

        
        protected float _tDis;

        #region Public Methods

        public void ActiveObject(Collider collider) => collider.gameObject.SetActive(true);
        
        public void DeactiveObject(Collider collider) => collider.gameObject.SetActive(false);

        public void ActiveMeshRenderer(Collider collider) => collider.GetComponent<MeshRenderer>().enabled = true;
        
        public void DeactiveMeshRenderer(Collider collider) => collider.GetComponent<MeshRenderer>().enabled = false;
        
        public void InstantiateOnDetections(GameObject obj)
        {
            foreach (var c in DetectedColliders) Instantiate(obj, c.transform.position, c.transform.rotation);
        }
        
        public void ApplyExplosionForce(float force, float radius)
        {
            foreach (var c in DetectedColliders)
            {
                if (c.TryGetComponent(out Rigidbody _r))
                {
                    _r.AddExplosionForce(force, transform.position, radius);
                }
            }
        }
        public void AddForceToDetections(float force)
        {
            foreach (var c in DetectedColliders)
            {
                if (c.TryGetComponent(out Rigidbody _r))
                {
                    _r.AddForce((c.transform.position-transform.position).normalized * force, ForceMode.Force);
                }
            }
        }
        public void AddDynamicForceToDetections(float force)
        {
            foreach (var c in DetectedColliders)
            {
                if (c.TryGetComponent(out Rigidbody _r))
                {
                    _r.AddForce((c.transform.position-transform.position) * force, ForceMode.Force);
                }
            }
        }
        public void AddGravityForceToDetections(float radius, float force)
        {
            foreach (var c in DetectedColliders)
            {
                if (c.TryGetComponent(out Rigidbody _r))
                {
                    var direction = c.transform.position - transform.position;
                    _r.AddForce((radius*radius - direction.sqrMagnitude) * direction * force, ForceMode.Force);
                }
            }
        }
        public void DestroyDetections(float delay)
        {
            foreach (var c in DetectedColliders) Destroy(c.gameObject, delay);
        }
        #endregion


        protected virtual void Start() // Refreshing
        {
            PreviousColliders.Clear();
            DetectedColliders.Clear();
            detectedLOSHits.Clear();
        }

        protected void CachePrevious()
        {
            if (onLostCollider != null || onNewCollider != null)
            {
                PreviousColliders = new HashSet<Collider>(DetectedColliders);
            }
        }

        protected void Clear()
        {
            DetectedColliders.Clear();
            if (collectLOS) detectedLOSHits.Clear();
        }
        /// <summary>
        /// Call: onDetectCollider, OnDetectNew, OnLostDetect in Optimized foreach loop
        /// </summary>
        protected void EventPass()
        {
            if (onDetectCollider != null) foreach (var c in DetectedColliders) onDetectCollider.Invoke(c);
            if (onNewCollider != null)
            {
                foreach (var c in DetectedColliders.Except(PreviousColliders)) onNewCollider.Invoke(c);
            }
            if (onLostCollider != null)
            {
                foreach (var c in PreviousColliders.Except(DetectedColliders)) onLostCollider.Invoke(c);
            }
        }

        protected Vector3 BoundsCenter(Collider _c) => boundsCenter ? _c.bounds.center : _c.transform.position;
        
        protected Func<Collider, Vector3> SetupDetectFunction()
        {
            switch (solverType)
            {
                case SolverType.Ignore: return c => c.transform.position;
                case SolverType.Pivot: return BoundsCenter;
                case SolverType.Nearest:
                    if (boundsSolver) return c => c.ClosestPointOnBounds(transform.position);
                    return c => c.ClosestPoint(transform.position);
                case SolverType.Furthest:
                    if (boundsSolver) return c => c.ClosestPointOnBounds(transform.position + (BoundsCenter(c) - transform.position) * int.MaxValue);
                    return c => c.ClosestPoint(transform.position + (c.transform.position - transform.position) * int.MaxValue);
                case SolverType.Focused:
                    if (boundsSolver) return c => c.ClosestPointOnBounds(transform.TransformPoint(detectVector));
                    return c => c.ClosestPoint(transform.TransformPoint(detectVector));
                case SolverType.Dodge:
                    return c =>
                    {
                        var closetPoint = boundsSolver || c is MeshCollider
                            ? (Func<Vector3, Vector3>) c.ClosestPointOnBounds : c.ClosestPoint;
                        var _ct = c.transform;
                        var cPos = BoundsCenter(c);
                        var crossUp = Vector3.Cross(cPos - transform.position, transform.right);
                        var cross = Vector3.Cross(cPos - transform.position, transform.up);
                        var value = blockLayer.value;
                        TDP = cPos;
                        if (!Physics.Linecast(transform.position, TDP, out var hit, value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        TDP = c.bounds.center;
                        if (!Physics.Linecast(transform.position, TDP, out hit, value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        TDP = closetPoint(cPos + cross * int.MaxValue);
                        if (!Physics.Linecast(transform.position, TDP, out hit, value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        TDP = closetPoint(cPos - cross * int.MaxValue);
                        if (!Physics.Linecast(transform.position, TDP, out hit,value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        TDP = closetPoint(cPos + crossUp * int.MaxValue);
                        if (!Physics.Linecast(transform.position, TDP, out hit, value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        TDP = closetPoint(cPos - crossUp * int.MaxValue);
                        if (!Physics.Linecast(transform.position, TDP, out hit, value, triggerInteraction) ||
                            hit.transform == _ct) return TDP;
                        return cPos;
                    };
            }
            return BoundsCenter;
        }
        /// <summary>
        /// Sync Component Type List with detected colliders.
        /// </summary>
        /// <param name="detections"></param>
        /// <typeparam name="T"></typeparam>
        public void SyncDetection<T>(List<T> detections, Action<T> onNew = null, Action<T> onLost = null)
        {
            // Starter Fix
            foreach (var detectedCollider in DetectedColliders)
            {
                detections.Add(detectedCollider.GetComponent<T>());
            }
            
            // Save States in list when new collider Detected
            onNewCollider.AddListener(C =>
            {
                if (C.TryGetComponent(out T instance))
                {
                    detections.Add(instance);
                    onNew?.Invoke(instance);
                }
            });
            // Save States in list when new collider Detected
            onLostCollider.AddListener(C =>
            {
                if (C.TryGetComponent(out T instance))
                {
                    detections.Remove(instance);
                    onLost?.Invoke(instance);
                }
            });
        }
        /// <summary>
        /// UnSync Component Type List with detected colliders.
        /// </summary>
        /// <param name="detections"></param>
        /// <typeparam name="T"></typeparam>
        public void UnSyncDetection<T>(List<T> detections, Action<T> onNew = null, Action<T> onLost = null)
        {
            onNewCollider?.RemoveListener(C =>
            {
                if (C.TryGetComponent(out T instance))
                {
                    detections.Add(instance);
                    onNew?.Invoke(instance);
                }

            });
            onLostCollider?.RemoveListener(C =>
            {
                if (C.TryGetComponent(out T instance))
                {
                    detections.Remove(instance);
                    onLost?.Invoke(instance);
                }
            });
        }

        /// <summary>
        /// Check Line of Sight
        /// </summary>
        /// <param name="point"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool LOSPass(Vector3 point, Collider c)
        {
            if (!checkLineOfSight)
            {
#if UNITY_EDITOR
                SetupGates(c, point, false, default);
#endif
                return true;
            }

            if (Physics.Linecast(transform.position, point, out var blockHit, blockLayer.value, triggerInteraction) &&
                blockHit.transform != c.transform)
            {
                if (collectLOS) detectedLOSHits.Add(c, blockHit);
#if UNITY_EDITOR
                SetupGates(c, point, true, blockHit);
#endif
                return false;
            }
#if UNITY_EDITOR
            SetupGates(c, point, false, default);
#endif
            return true;
        }
        
        protected Func<Collider, Vector3> DetectFunction;
        private void OnEnable() => DetectFunction = SetupDetectFunction();
        protected bool TagPass(Collider c) => c && !ignoreList.Contains(c) && (!usingTagFilter || c.CompareTag(tagFilter));
        protected bool TagPass(GameObject g) => g && (!usingTagFilter || g.CompareTag(tagFilter));
#if UNITY_EDITOR
        
        protected readonly string[] CEventNames = {"onDetectCollider", "onNewCollider", "onLostCollider"};
        protected bool GuideCondition => RCProPanel.DrawGuide && gizmosUpdate == GizmosMode.Select;

        protected void OnValidate()
        {
            DetectFunction = SetupDetectFunction();
        }
        
        protected void SetupGates(Collider c, Vector3 point, bool blocked, RaycastHit blockHit)
        {
            void DrawDetectBox(Collider _col)
            {
                if (boundsSolver)
                {
                    Gizmos.DrawWireCube(_col.bounds.center, _col.bounds.size);
                }
                else
                {
                    if (_col is MeshCollider _meshD)
                    {
                        Gizmos.DrawWireMesh(_meshD.sharedMesh, _col.transform.position, _col.transform.rotation);
                    }
                    else
                    {
                        Gizmos.DrawWireCube(_col.bounds.center, _col.bounds.size);
                    }
                }
            }

            PanelGate += () => DetectorInfoField(c.transform, point, blocked);
            GizmoGate += () =>
            {
                if (blocked)
                {
                    DrawBlockLine(transform.position, point, c.transform, blockHit);
                    if (IsGuide)
                    {
                        GizmoColor = BlockColor;
                        DrawDetectBox(c);
                    }
                }
                else
                {
                    GizmoColor = DetectColor;
                    if (IsLabel) Handles.Label(c.transform.position, c.name);
                    if (IsDetectLine) DrawLineZTest(transform.position, point);
                    if (IsGuide)
                    {
                        DrawDetectBox(c);
                        DrawDetectorGuide(point);
                    }
                }
            };
        }
        
        protected void ColliderDetectorGeneralField(SerializedObject _so)
        {
            GeneralField(_so);
            NonAllocatorField(_so, _so.FindProperty(nameof(colliders)));
            BaseField(_so);
            SolverField(_so);
            IgnoreListField(_so);
        }
        
        protected void SolverField(SerializedObject _so)
        {
            BaseSolverField(_so, () =>
            {
                if (IsIgnoreSolver) return;
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(blockLayer)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(boundsSolver)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(collectLOS)));
                if (IsPivotSolver)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(boundsCenter)));
                }
                if (IsFocusedSolver)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(detectVector)), CFocusPoint.ToContent(TFocusPoint));
                }
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(checkLineOfSight)), CCheckLineOfSight.ToContent(TCheckLineOfSight));
            });
        }
        
        protected void IgnoreListField(SerializedObject _so)
        {
            BeginVerticalBox();
            RCProEditor.PropertyArrayField(_so.FindProperty(nameof(ignoreList)), "Ignore List".ToContent(),
                (i) => $"Collider {i+1}".ToContent($"Index {i}"));
            EndVertical();
        }
#endif
    }
}