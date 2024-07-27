namespace RaycastPro.Detectors
{
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(SteeringDetector))]
    public sealed class SteeringDetector : Detector, IRadius
    {
        [Tooltip("Destination location, a Transform that Solver detects obstacles in the way as much as possible and goes towards it.")]
        public Transform destination;
        [Tooltip("The dimensions of the auxiliary ray used to detect the cavity. It is better to adjust it according to the size of the character itself.")]
        public float colliderSize = .1f;
        
        [SerializeField] private float radius = 20f;
        
        [Tooltip("Limiting Rays will be sent in the Local direction.")]
        public bool local;
        
        public float angleX = 120f;
        public float angleY = 90f;

        [Tooltip("The number of Random Rays sent per second.")]
        public int iteration = 8;
        
        [Tooltip("Speed of change of steering direction. Number 15 is relatively sharp and lower numbers like 4 and 3 are heavy.")]
        public float sharpness = 6;
        
        [Tooltip("The Mark Solver acts as a memory for recording points to help the Steering avoid returning to its old location. A lower number makes the detector more dynamic, but it increases the probability of searching for the previous location.")]
        public int markSolverCount = 6;
        
        [Tooltip("The effect of Mark Solver on determining the final direction. When the memory becomes zero, the location is lost.")]
        public float markSolverInfluence = 1;
        
        public float obstacleNormalInfluence = 1f;
        public float obstacleDistanceInfluence = 1f;
        
        [Tooltip("This solver can help to improve obstacle detection by checking the LOS of the end of the line of each iteration with the destination, with a more performance price.")]
        public bool spiderSolver = true;
        
        public TimeMode timeMode = TimeMode.DeltaTime;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
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
        private Vector3 _pos, _randomVector, _dir, _rRadiusVector, _qVec;
        private RaycastHit _raycastHit;
        #endregion

        private Transform currentDestination;
        private int hitCounts;
        private float distValue;
        private float zeroHitOverTime;
        private float weightLocateTimer;
        /// <summary>
        /// Average Point of all detected hits.
        /// </summary>
        private Vector3 averageWeight;
        /// <summary>
        /// Average Normal of all detected hits.
        /// </summary>
        private Vector3 averageNormal;

        public Vector3 Weight => averageWeight;

        /// <summary>
        /// Non-Normalized Steering Direction
        /// </summary>
        public Vector3 RawSteeringDirection => averageNormal + (_pos - averageWeight).normalized;
        
        /// <summary>
        /// Normalized Steering Direction
        /// </summary>
        public Vector3 SteeringDirection => (averageNormal+(_pos-averageWeight).normalized).normalized;
        
        /// <summary>
        /// Steering Direction as Quaternion
        /// </summary>
        public Quaternion SteeringLook => Quaternion.LookRotation(RawSteeringDirection,  Vector3.up);
        
        public float Distance => Vector3.Distance(transform.position, destination.position);

        private readonly Queue<Vector3> weightLocate = new Queue<Vector3>();

        private float _F;
        private Vector3 _DirN;

        private bool IsDirect
        {
            get
            {
                Physics.Linecast(_pos,  destination.position, out _raycastHit, detectLayer.value, triggerInteraction);
                return !_raycastHit.transform || _raycastHit.transform == destination;
            }
        }

        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif

            if (!destination) return;

            _pos = transform.position;
            
            hitCounts = 0;
            delta = GetDelta(timeMode);
            
            _dir = (destination.position - _pos);
            _DirN = _dir.normalized;
            _dis = Vector3.Distance(destination.position, _pos);
            _cRadius = Mathf.Min(_dis,radius)*Random.value;
            
            if (IsDirect) // Direct Destination
            {
                Physics.SphereCast(_pos-_DirN*colliderSize, colliderSize , _dir, out _raycastHit, _dir.magnitude, detectLayer.value, triggerInteraction);
                if (!_raycastHit.transform || _raycastHit.transform == destination) // When No Obstacle
                {
                    _F = 1 - Mathf.Exp(-sharpness * delta);
                    averageWeight = Vector3.Lerp(averageWeight, _pos-_DirN, _F);
                    averageNormal = Vector3.Lerp(averageNormal, _DirN, _F);
#if UNITY_EDITOR
                    GizmoGate += () =>
                    {
                        Handles.color = HelperColor;
                        DrawCapsuleLine(_pos, destination.position, colliderSize);
                    };
#endif
                    return;
                }
            }
            else // Mark Solver Activating
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
                    var _allW = Vector3.zero;
#if UNITY_EDITOR
                    _qVec = Vector3.up * (DotSize * 4f);
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
                        _F = 1 - Mathf.Exp(-delta * markSolverInfluence);
                        averageWeight = Vector3.Lerp(averageWeight, (_allW / weightLocate.Count), _F);
                        averageNormal = Vector3.Lerp(averageNormal, (_pos - _allW / weightLocate.Count).normalized, _F);
                    }
                }
            }

            // On Obstacle Solver
            for (i = 0; i < iteration; i++)
            {
                _randomVector = Quaternion.Euler(Random.Range(-angleY, angleY)/2, Random.Range(-angleX, angleX)/2, 0) *
                                (local ? transform.forward : Vector3.forward);
                _rRadiusVector = _randomVector * _cRadius;
                if (Physics.Raycast(_pos, _randomVector, out _raycastHit, _cRadius, detectLayer.value, triggerInteraction))
                {
                    hitCounts++;
                    distValue = Mathf.Pow(_raycastHit.distance / _cRadius , 2);
                    _F = 1 - Mathf.Exp(F);
                    F = -delta * hitCounts/iteration * (1 - distValue) * sharpness;
                    averageWeight = Vector3.Lerp(averageWeight, _raycastHit.point, _F * obstacleDistanceInfluence);
                    averageNormal = Vector3.Lerp(averageNormal,
                        Vector3.Lerp(_raycastHit.normal*(radius-_raycastHit.distance), (destination.position-_raycastHit.point).normalized, distValue), _F * obstacleNormalInfluence);
                    
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
                else if (spiderSolver) // Spider Solver
                {
                    var _avPoint = Vector3.zero;
                    var _avDir = Vector3.zero;
                    var _spCount = 0;
                         
                    if (Vector3.Distance(_pos+_rRadiusVector, destination.position) <= _dis) 
                    {
                        Physics.Linecast(_pos + _rRadiusVector, destination.position, out _raycastHit,
                            detectLayer.value, triggerInteraction);
                        if (!_raycastHit.transform || _raycastHit.transform == destination.transform)
                        {
                            _avPoint += _pos - _randomVector;
                            _avDir += _randomVector.normalized;
                            _spCount++;
                        }
                    }

                    if (_spCount > 0)
                    {
                        _F = 1 - Mathf.Exp(-sharpness * delta);
                        averageWeight = Vector3.Lerp(averageWeight, _avPoint/_spCount , _F);
                        averageNormal = Vector3.Lerp(averageNormal, _avDir/_spCount, _F);
                    }
                }
            }
         
            
            if  (hitCounts == 0) // On Free Move
            {
                zeroHitOverTime = Mathf.Min(zeroHitOverTime + delta, .2f);
                if (zeroHitOverTime >= .2f)
                {
                    _F = 1 - Mathf.Exp(-sharpness * delta);
                    averageWeight = Vector3.Lerp(averageWeight, _pos, _F);
                    averageNormal = Vector3.Lerp(averageNormal, (destination.position-transform.position).normalized, _F);
                }
            }
            else
            {
                zeroHitOverTime = Mathf.Max(zeroHitOverTime - delta, 0f);
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Try finding a way to destination with detecting obstacles." + HDependent + HRDetector + HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            if (IsGuide && IsPlaying)
            {
                DrawNormal(transform.position, SteeringDirection, "Steering Direction", DiscSize);
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
                
                PropertySliderField(_so.FindProperty(nameof(angleX)), 0f, 360f, "Arc X".ToContent("Throw rays into range of Horizontal."));
                PropertySliderField(_so.FindProperty(nameof(angleY)), 0f, 360f, "Arc Y".ToContent("Throw rays into range of Vertical."));
                
                PropertySliderField(_so.FindProperty(nameof(obstacleNormalInfluence)), 0f, 1, "Obstacle Normal Influence".ToContent());
                PropertySliderField(_so.FindProperty(nameof(obstacleDistanceInfluence)), 0f, 1, "Obstacle Distance Influence".ToContent());
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(colliderSize)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(markSolverCount)));
                PropertySliderField(_so.FindProperty(nameof(markSolverInfluence)), 0f, 10f, "Mark Solver Influence".ToContent());
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(spiderSolver)));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            }

            if (hasGeneral)
            {
                GeneralField(_so, layerField: false);
                BaseField(_so);
            }
            
            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField(PanelGate);
        }
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
    }
}