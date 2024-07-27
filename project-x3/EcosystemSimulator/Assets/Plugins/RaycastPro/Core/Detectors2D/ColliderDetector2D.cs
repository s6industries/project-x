namespace RaycastPro.Detectors2D
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    using UnityEngine;

    public abstract class ColliderDetector2D : Detector2D
    {

        protected RaycastHit2D _blockHit;
        
        public Collider2DEvent onDetectCollider;
        public Collider2DEvent onNewCollider;
        public Collider2DEvent onLostCollider;
        public List<Collider2D> DetectedColliders { get; protected set; } = new List<Collider2D>();
        public Collider2D[] PreviousColliders { get; protected set; } = Array.Empty<Collider2D>();
        protected readonly Dictionary<Collider2D, RaycastHit2D> detectedLOSHits = new Dictionary<Collider2D, RaycastHit2D>();
        public Dictionary<Collider2D, RaycastHit2D> DetectedLOSHits => detectedLOSHits;
        
        
        public Collider2D[] ignoreList = Array.Empty<Collider2D>();
        
        /// <summary>
        /// This will find Smart Solver Point, Setup in: (OnEnable)
        /// </summary>
        protected Func<Collider2D, Vector2> DetectFunction;
        
        public override bool Performed
        {
            get => DetectedColliders.Any(); protected set {}
            
        }
        
        #region Methods
        public Collider2D FirstMember => DetectedColliders.FirstOrDefault();

        public Collider2D LastMember => DetectedColliders.LastOrDefault();
        
        /// <summary>
        /// It will calculate nearest collider based on current detected colliders on detector.
        /// </summary>
        /// <param name="nearest">Define a collider2D in your script and ref to it for get the nearest.</param>
        public void GetNearestCollider(ref Collider2D nearest)
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
        public void GetFurthestCollider(ref Collider2D furthest)
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
        
        protected void Start() // Refreshing
        {
            PreviousColliders = Array.Empty<Collider2D>();
            DetectedColliders = new List<Collider2D>();
        }
        
        protected float _tDis;
        
        [Tooltip("This option is considered for optimization and limits the detection point in the bounds of a cube.")]
        public bool boundsSolver;
        [Tooltip("If selected, the detection point will be mounted on Collider Bounds Center. otherwise on transform.position.")]
        public bool boundsCenter;
        [Tooltip("Detector collect RaycastHits on \"DetectedLOSHits\" Dictionary. Key: Collider, Value: RaycastHit")]
        public bool collectLOS;

        protected void EventPass()
        {
            if (onDetectCollider != null)
            {
                foreach (var c in DetectedColliders) onDetectCollider.Invoke(c);
            }
            if (onNewCollider != null)
            {
                foreach (var c in DetectedColliders.Except(PreviousColliders)) onNewCollider.Invoke(c);
            }
            if (onLostCollider != null)
            {
                foreach (var c in PreviousColliders.Except(DetectedColliders)) onLostCollider.Invoke(c);
            }
        }

        protected void ColliderPass(Collider2D col)
        {
            DetectedColliders.Add(col);
            if (collectLOS) DetectedLOSHits.Add(col, _blockHit);
        }

        /// <summary>
        /// Check Tag Filter and Ignore List
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected bool TagPass(Collider2D c) => c && !ignoreList.Contains(c) && (!usingTagFilter || c.CompareTag(tagFilter));

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

        protected void Clear()
        {
            DetectedColliders.Clear();
            if (collectLOS) DetectedLOSHits.Clear();
        }
        private void OnEnable() => DetectFunction = SetupDetectFunction();
        
                protected Func<Collider2D, Vector2> SetupDetectFunction()
        {
            switch (solverType)
            {
                case SolverType.Ignore: return c => c.transform.position;
                case SolverType.Pivot: return c => c.transform.position;
                case SolverType.Nearest: return c => c.ClosestPoint(transform.position);
                case SolverType.Furthest:
                {
                    return c => c.ClosestPoint(c.transform.position + (c.transform.position - transform.position) * 1000);
                }
                case SolverType.Focused: return c => c.ClosestPoint(FocusPoint);
                case SolverType.Dodge:
                    return c =>
                    {
                        TDP = BoundsCenter(c);
                        var maxUp = c.transform.up * 1000000;
                        var maxRight = c.transform.right * 1000000;
                        var _ct = c.transform;
                        var hit2D = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                        if (!hit2D || hit2D.transform == _ct) return TDP;

                        TDP = c.ClosestPoint(TDP + maxUp);
                        hit2D = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                        if (!hit2D || hit2D.transform == _ct) return TDP;

                        TDP = c.ClosestPoint(TDP - maxUp);
                        hit2D = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                        if (!hit2D || hit2D.transform == _ct) return TDP;

                        TDP = c.ClosestPoint(TDP + maxRight);
                        hit2D = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                        if (!hit2D || hit2D.transform == _ct) return TDP;

                        TDP = c.ClosestPoint(TDP - maxRight);
                        hit2D = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                        if (!hit2D || hit2D.transform == _ct) return TDP;
                        return c.bounds.center;
                    };
            }
            return c => c.transform.position;
        }
        
        protected Vector3 BoundsCenter(Collider2D _c) => boundsCenter ? _c.bounds.center : _c.transform.position;
        
#if UNITY_EDITOR
        protected readonly string[] CEventNames = {"onDetectCollider", "onNewCollider", "onLostCollider"};

        private void OnValidate()
        {
            DetectFunction = SetupDetectFunction();
        }

        protected Color GetPolygonColor(bool blockCondition = false) =>
            (blockCondition ? BlockColor : DetectedColliders.Any() ? DetectColor : DefaultColor).Alpha(RCProPanel.alphaAmount);

        protected void IgnoreListField(SerializedObject _so)
        {
            BeginVerticalBox();
            RCProEditor.PropertyArrayField(_so.FindProperty(nameof(ignoreList)), "Ignore List".ToContent(),
                (i) => $"Collider {i+1}".ToContent($"Index {i}"));
            EndVertical();
        }
        
        protected void SolverField(SerializedObject _so)
        {
            BaseSolverField(_so, () =>
            {
                if (IsIgnoreSolver) return;

                EditorGUILayout.PropertyField(_so.FindProperty(nameof(blockLayer)),
                    CBlockLayer.ToContent(TBlockLayer));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(boundsSolver)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(collectLOS)));
                
                if (IsPivotSolver)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(boundsCenter)));
                }

                if (IsFocusedSolver)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(focusPointOffset)),
                        CFocusPoint.ToContent(TFocusPoint));
                }
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(checkLineOfSight)),
                    CCheckLineOfSight.ToContent(TCheckLineOfSight));

                GUI.enabled = checkLineOfSight;

                GUI.enabled = true; 
            });
        }
#endif
    }
}