namespace RaycastPro.Detectors2D
{
    using UnityEngine;
    using System;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(BoxDetector2D))]
    public sealed class BoxDetector2D : ColliderDetector2D, IPulse
    {
        public Vector2 size = Vector2.one;
        public Vector2 difference;
        public Vector2 Size
        {
            get => size;
            set => size = new Vector2(Mathf.Max(value.x, 0), Mathf.Max(value.y, 0));
        }
        public Vector2 Difference
        {
            get => difference;
            set => difference = new Vector2(Mathf.Clamp(value.x, 0, size.x), Mathf.Clamp(value.y, 0, size.y));
        }
        
        [SerializeField] private bool limited;
        [SerializeField] private int limitCount = 3;

        [SerializeField]
        public bool local;
        
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

        private Vector2 pos2D;
        private float angle;
        private float delta, aveZ;

        private Vector3 pos;
        private Vector3 _tPoint;
        private Bounds bounds, boundsMin;
        protected override void OnCast()
        {
#if UNITY_EDITOR
            CleanGate();
#endif
            PreviousColliders = DetectedColliders.ToArray();
            pos2D = transform.position.To2D();
            angle = local ? -Vector2.Angle(transform.right.To2D(), Vector2.right) : 0;

            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                Physics2D.OverlapBoxNonAlloc(pos2D, size,  angle, colliders, detectLayer.value, MinDepth, MaxDepth);    
            }
            else
            {
                colliders = Physics2D.OverlapBoxAll(pos2D, size, angle,
                    detectLayer.value, MinDepth, MaxDepth);
            }

            #region Bounds Calculation
            delta = Mathf.Abs(minDepth - maxDepth);
            aveZ = (MinDepth + MaxDepth) / 2;
            pos = Vector3.zero.ToDepth(aveZ);
            bounds = new Bounds(pos, size.ToDepth(delta));
            boundsMin = new Bounds(pos, difference.ToDepth(delta));
            #endregion
            
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
                    TDP = DetectFunction(c);
                    _tPoint = transform.InverseTransformDirection(TDP.To2D() - Position2D).ToDepth();
                    if (!bounds.Contains(_tPoint) || boundsMin.Contains(_tPoint)) continue;
                    
                    _blockHit = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
#if UNITY_EDITOR
                    PassGate(c, TDP, _blockHit);
#endif
                    if (!_blockHit || _blockHit.transform == c.transform)
                    {
                        DetectedColliders.Add(c);
                        if (collectLOS) DetectedLOSHits.Add(c, _blockHit);
                    }
                }
            }
            
#if UNITY_EDITOR
            GizmoGate += () =>
            {
                Gizmos.color = DetectedColliders.Count > 0 ? DetectColor : DefaultColor;
                DrawBox2D(transform, size, MinDepth, MaxDepth, local);
            };
#endif
            EventPass();
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receiving colliders within the specified bounds with a detect point solver." +
                                     HAccurate + HIPulse + HCDetector + HLOS_Solver + HRotatable + HINonAllocator;
#pragma warning restore CS0414
        
        private Vector3 boundsSizeZ0, boundsMinSizeZ0, sizeY, sizeX;
        private Matrix4x4 matrix;

        private Vector3[] points = new Vector3[4];
        internal override void OnGizmos()
        {
            EditorUpdate();
            if (IsIgnoreSolver) return;
            DrawFocusLine();
            
            Gizmos.color = HelperColor;
            boundsSizeZ0 = bounds.size.ToDepth() / 2;
            boundsMinSizeZ0 = boundsMin.size.ToDepth() / 2;
            sizeY = new Vector3(0f, boundsMin.size.y);
            sizeX = Vector3.right * boundsMin.size.x;
            matrix = Handles.matrix;
            Handles.matrix = transform.worldToLocalMatrix.inverse;
            Handles.color = (Performed ? DetectColor : DefaultColor).Alpha(RCProPanel.alphaAmount);

            points[0] = boundsSizeZ0;
            points[1] = boundsSizeZ0.ToXFlip();
            points[2] = -boundsMinSizeZ0 + sizeY;
            points[3] = -boundsMinSizeZ0.ToXFlip() + sizeY;
            Handles.DrawAAConvexPolygon(points);

            points[0] = -boundsSizeZ0.ToXFlip();
            points[1] = boundsMinSizeZ0 - sizeY;
            points[2] = boundsMinSizeZ0.ToXFlip() - sizeY;
            points[3] = -boundsSizeZ0;
            Handles.DrawAAConvexPolygon(points);

            points[0] = -boundsSizeZ0;
            points[1] = -boundsSizeZ0.ToYFlip();
            points[2] = boundsMinSizeZ0 - sizeX;
            points[3] = boundsMinSizeZ0.ToYFlip() - sizeX;
            Handles.DrawAAConvexPolygon(points);

            points[0] = boundsSizeZ0;
            points[1] = boundsSizeZ0.ToYFlip();
            points[2] = -boundsMinSizeZ0 + sizeX;
            points[3] = -boundsMinSizeZ0.ToYFlip() + sizeX;
            Handles.DrawAAConvexPolygon(points);

            Handles.matrix = matrix;
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginHorizontal();
                var sizeProp = _so.FindProperty(nameof(size));
                EditorGUILayout.PropertyField(sizeProp,
                    "Size".ToContent());
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
                GUI.enabled = !IsIgnoreSolver;
                var diffProp = _so.FindProperty(nameof(difference));
                EditorGUILayout.PropertyField(diffProp, "Difference".ToContent());
                diffProp.vector2Value = new Vector2(Mathf.Clamp(diffProp.vector2Value.x, 0, sizeProp.vector2Value.x),
                    Mathf.Clamp(diffProp.vector2Value.y, 0, sizeProp.vector2Value.y));
                GUI.enabled = true;
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                NonAllocatorField(_so, _so.FindProperty(nameof(colliders)));
                BaseField(_so);
                SolverField(_so);
                IgnoreListField(_so);
            }

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }

            if (hasInfo)
            {
                InformationField(PanelGate);
            }
            
        }
#endif
    }
}