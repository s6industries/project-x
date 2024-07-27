namespace RaycastPro.Detectors2D
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Detectors/"+nameof(RadarDetector2D))]
    public sealed class RadarDetector2D : ColliderDetector2D, IRadius
    {
        private float currentAngle;
        
        public float loopTime = 6f;

        [SerializeField] public float radius = 2f;
        
        public bool local = true;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }
        
        public TimeMode timeMode;

        public float cacheTime = 4f;

        public Transform graphicShape;
        
        public float shapeScale;
        public float shapeAngleOffset;
        public float speed => 360 / loopTime;
        
        public readonly Dictionary<Collider2D, float> DetectProfile = new Dictionary<Collider2D, float>();

        private float delta, _distance;
        private Transform _t;
        private Collider2D tCollider;
        private HashSet<Collider2D> _colliders = new HashSet<Collider2D>();
        private Vector3 direction;
        
        protected override void OnCast()
        {
#if UNITY_EDITOR
            CleanGate();
#endif
            PreviousColliders = DetectedColliders.ToArray();
            delta = GetDelta(timeMode);
            
#if UNITY_EDITOR
            if (IsSceneView && !IsPlaying) currentAngle = Time.realtimeSinceStartup * speed;
            else currentAngle += delta * speed;
#else
            currentAngle += delta * speed;
#endif
            _t = transform;
            if (graphicShape)
            {
                graphicShape.transform.position = _t.position;
                graphicShape.eulerAngles = new Vector3(0, 0, currentAngle + shapeAngleOffset);
                graphicShape.localScale = Vector3.Lerp(Vector3.zero, new Vector3(radius, radius), shapeScale);
            }

            direction = Quaternion.AngleAxis(currentAngle, Vector3.forward) * (local ? _t.right : Vector3.right);
            _colliders.Clear();
            
            var _hits2D = Physics2D.RaycastAll(_t.position, direction, radius, detectLayer.value, MinDepth, MaxDepth);
            foreach (var _h in _hits2D)
            {
                tCollider = _h.collider;
                if (!TagPass(tCollider)) continue;
                
                if (IsIgnoreSolver)
                {
                    _colliders.Add(tCollider);
                    continue;
                }
                
                
                TDP = DetectFunction(tCollider);
                
                _distance = Vector2.Distance(transform.position, TDP);
                if (_distance > radius) continue;
                _blockHit = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
                _blockHit = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
#if UNITY_EDITOR
                PassGate(tCollider, TDP, _blockHit);
#endif
                if (_blockHit && _blockHit.transform != tCollider.transform) continue;
                
                _colliders.Add(tCollider);
            }

#if UNITY_EDITOR
            GizmoGate += () =>
            {
                Handles.color = (DetectProfile.Keys.Count > 0 ? DetectColor : DefaultColor).Alpha(.2f);
                var projectedDir = Vector3.ProjectOnPlane(direction, Vector3.forward);
                Handles.DrawSolidArc(_t.position, Vector3.forward, projectedDir, -Mathf.Clamp01(cacheTime / loopTime) * 360f, radius);
                Gizmos.DrawLine(_t.position, _t.position+projectedDir.normalized*radius);
            };
#endif

            #region Add or Refresh Colliders
            foreach (var col in _colliders)
            {
                if (DetectProfile.ContainsKey(col)) DetectProfile[col] = cacheTime;
                else
                {
                    DetectProfile.Add(col, cacheTime);
                    onNewCollider?.Invoke(col);
                }
            }
            #endregion
            
            #region Refresh Time and out Colliders
            DetectedColliders = DetectProfile.Keys.ToList();
            foreach (var col in DetectedColliders)
            {
                DetectProfile[col] -= delta;
                if (DetectProfile[col] <= 0)
                {
                    onLostCollider?.Invoke(col);
                    DetectProfile.Remove(col);
                }
                else
                {
                    onDetectCollider?.Invoke(col);
                    //onRadarDetect?.Invoke(col, DetectProfile[col]);
                }
            }
            #endregion
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Detection of colliders for a limited time during the rotation of the radar hand." + HCDetector + HIRadius + HRotatable;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            DrawDepthCircle(radius);
            
            foreach (var key in DetectProfile.Keys.ToArray())
            {
                Gizmos.color = DetectColor.Alpha(DetectProfile[key] / cacheTime);
                Gizmos.DrawWireCube(key.bounds.center, key.bounds.size);
            }
        }
        
        private readonly string[] events = new []{nameof(onDetectCollider), nameof(onNewCollider), nameof(onLostCollider)};
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
                PropertyMaxField(_so.FindProperty(nameof(loopTime)),  "Loop Time".ToContent(), .1f);
                PropertyMaxField(_so.FindProperty(nameof(cacheTime)),  CCacheTime.ToContent(CCacheTime));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
                var propShape = _so.FindProperty(nameof(graphicShape));
                BeginVerticalBox();
                EditorGUILayout.PropertyField(propShape);
                GUI.enabled = propShape.objectReferenceValue != null;
                PropertySliderField(_so.FindProperty(nameof(shapeScale)), 0f, 1f, "Scale".ToContent());
                PropertySliderField(_so.FindProperty(nameof(shapeAngleOffset)), 0f, 360f, "Angle Offset".ToContent());
                GUI.enabled = true;
                EndVertical();
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                BaseField(_so);
                IgnoreListField(_so);
                SolverField(_so);
            }

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, events);
            }

            if (hasInfo)
            {
                if (hasInfo) InformationField(() =>
                {
                    BeginVertical();
                    foreach (var key in DetectProfile.Keys.ToArray())
                    {
                        BeginHorizontal();
                        EditorGUILayout.LabelField($"{key.gameObject.name}: ",
                            GUILayout.Width(160));
                        PercentProgressField(DetectProfile[key] / cacheTime, "Life");
                        EndHorizontal();
                    }
                    EndVertical();
                });
            }
        }
#endif
    }
}
