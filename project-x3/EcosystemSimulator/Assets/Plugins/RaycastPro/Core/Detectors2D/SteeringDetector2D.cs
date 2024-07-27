namespace RaycastPro.Detectors2D
{
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(SteeringDetector2D))]
    public sealed class SteeringDetector2D : Detector2D
    {  
        [Tooltip("Destination location, a Transform that Solver detects obstacles in the way as much as possible and goes towards it.")]
        public Transform destination;
        [Tooltip("The volume of the follower, which plays a role in path selection calculations and makes it avoid passing into small openings.")]
        public float colliderSize = .1f;
        [SerializeField] private float radius = 20f;

        public bool local;

        [Tooltip("")] public int iteration = 8;
        [Tooltip("")] public float sharpness = 6;
        [Tooltip("")] public int markSolverCount = 6;
        public float markSolverInfluence = 1;

        [Tooltip(
            "This solver can help to improve obstacle detection by checking the LOS of the end of the line of each iteration with the destination, with a more performance price.")]
        public bool spiderSolver = true;

        public TimeMode timeMode = TimeMode.DeltaTime;

        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }

        public override bool Performed
        {
            get => hitCounts > 0;
            protected set { }
        }

        #region cached;

        private int i;
        private float delta, _dis, _cRadius;
        private float F;
        private Vector2 _pos, _randomVector, _dir, _rRadiusVector, _qVec;
        private RaycastHit2D _raycastHit;

        #endregion

        private Transform currentDestination;
        private int hitCounts;
        private float distValue;
        private float zeroHitOverTime;
        private float weightLocateTimer;

        /// <summary>
        /// Average Point of all detected hits.
        /// </summary>
        private Vector2 averageWeight;

        /// <summary>
        /// Average Normal of all detected hits.
        /// </summary>
        private Vector2 averageNormal;

        public Vector3 Weight => averageWeight.ToDepth(z);
        public Vector2 SteeringDirection => (averageNormal + (_pos - averageWeight).normalized).normalized;

        private readonly Queue<Vector2> weightLocate = new Queue<Vector2>();

        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif

            if (!destination) return;

            _pos = transform.position;

            hitCounts = 0;
            delta = GetDelta(timeMode);

            _dir = (destination.position - _pos.ToDepth(z));
            _dis = Vector2.Distance(destination.position, _pos);
            _cRadius = Mathf.Min(_dis, radius) * Random.value;

            _raycastHit = Physics2D.Linecast(_pos, destination.position, detectLayer.value, MinDepth, MaxDepth);
            if (!_raycastHit.transform || _raycastHit.transform == destination) // Direct Destination
            {
                _raycastHit = Physics2D.CircleCast(_pos - _dir.normalized * colliderSize, colliderSize, _dir,
                    _dir.magnitude, detectLayer.value, MinDepth, MaxDepth);
                if (!_raycastHit.transform || _raycastHit.transform == destination) // When No Obstacle
                {
                    averageWeight = Vector3.Lerp(averageWeight, _pos - _dir.normalized,
                        1 - Mathf.Exp(-sharpness * delta));
                    averageNormal = Vector3.Lerp(averageNormal, _dir.normalized, 1 - Mathf.Exp(-sharpness * delta));
#if UNITY_EDITOR
                    GizmoGate += () =>
                    {
                        Handles.color = HelperColor;
                        DrawCircleLine(_pos, destination.position, colliderSize);
                    };
#endif
                    return;
                }
            }
            else // Solver Activating
            {
                if (weightLocateTimer >= 1f)
                {
                    weightLocateTimer = 0f;

                    if (weightLocate.Count >= markSolverCount) weightLocate.Dequeue();
                    weightLocate.Enqueue(_pos);
                }
                else
                {
                    weightLocateTimer += delta;
                }

                if (markSolverInfluence > 0)
                {
                    var _allW = Vector2.zero;
#if UNITY_EDITOR
                    _qVec = Vector2.up * (DotSize * 4f);
#endif
                    foreach (var _tVec in weightLocate)
                    {
                        _allW += _tVec;
#if UNITY_EDITOR
                        GizmoGate += () =>
                        {
                            Handles.color = HelperColor;
                            DrawLineZTest(_tVec, _tVec + _qVec);
                        };
#endif
                    }

                    if (weightLocate.Count > 0) // Mark Solver
                    {
                        averageWeight = Vector3.Lerp(averageWeight, (_allW / weightLocate.Count),
                            1 - Mathf.Exp(-delta * markSolverInfluence));
                        averageNormal = Vector3.Lerp(averageNormal, (_pos - _allW / weightLocate.Count).normalized,
                            1 - Mathf.Exp(-delta * markSolverInfluence));
                    }
                }
            }

            // On Obstacle Solver
            for (i = 0; i < iteration; i++)
            {
                _randomVector = Random.insideUnitCircle;
                _rRadiusVector = _randomVector * _cRadius;

                _raycastHit = Physics2D.Raycast(_pos, _randomVector, Random.value * _cRadius, detectLayer.value, MinDepth, MaxDepth);
                if (_raycastHit)
                {
                    hitCounts++;
                    distValue = Mathf.Pow(_raycastHit.distance / _cRadius, 2);
                    F = -delta * hitCounts / iteration * (1 - distValue) * sharpness;
                    averageWeight = Vector3.Lerp(averageWeight, _raycastHit.point, 1 - Mathf.Exp(F));
                    averageNormal = Vector3.Lerp(averageNormal,
                        Vector3.Lerp(_raycastHit.normal * (radius - _raycastHit.distance),
                            (destination.position - _raycastHit.point.ToDepth()).normalized, distValue),
                        1 - Mathf.Exp(F));

#if UNITY_EDITOR
                    var _p = _raycastHit.point;
                    var _rP = _pos + _rRadiusVector;
                    GizmoGate += () =>
                    {
                        Handles.color = DetectColor;
                        DrawLineZTest(_pos, _p);

                        Handles.color = BlockColor;
                        DrawLineZTest(_p, _rP, true);
                    };
#endif
                }
                else
                {
                    if (spiderSolver) // Spider Solver
                    {
                        if (Vector2.Distance(_pos + _rRadiusVector, destination.position) <= _dis)
                        {
                            _raycastHit = Physics2D.Linecast(_pos + _rRadiusVector, destination.position,
                                detectLayer.value, MinDepth, MaxDepth);
                            if (!_raycastHit.transform || _raycastHit.transform == destination.transform)
                            {
                                averageWeight = Vector3.Lerp(averageWeight, _pos - _randomVector,
                                    1 - Mathf.Exp(-iteration * delta));
                                averageNormal = Vector3.Lerp(averageNormal, _randomVector.normalized,
                                    1 - Mathf.Exp(-iteration * delta));
                            }
                        }
                    }
                }
            }

            if (hitCounts == 0) // On Free Move
            {
                zeroHitOverTime = Mathf.Min(zeroHitOverTime + delta, .2f);
                if (zeroHitOverTime >= .2f)
                {
                    averageWeight = Vector3.Lerp(averageWeight, _pos,
                        1 - Mathf.Exp(-sharpness * delta));
                    averageNormal = Vector3.Lerp(averageNormal, (destination.position - transform.position).normalized,
                        1 - Mathf.Exp(-sharpness * delta));
                }
            }
            else
            {
                zeroHitOverTime = Mathf.Max(zeroHitOverTime - delta, 0f);
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Environment detector based on spreading Random Rays around and finding the best path to move." + HAccurate + HRDetector + HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();

            if (IsGuide && IsPlaying)
            {
                DrawNormal(transform.position, SteeringDirection, "Steering Direction", radius, DiscSize);
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(destination)));
                DetectLayerField(_so);
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(iteration)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(sharpness)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(colliderSize)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(markSolverCount)));
                PropertySliderField(_so.FindProperty(nameof(markSolverInfluence)), 0f, 10f,
                    "Mark Solver Influence".ToContent());
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(spiderSolver)));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                BaseField(_so);
            }

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField(PanelGate);
        }
#endif
    }
}
