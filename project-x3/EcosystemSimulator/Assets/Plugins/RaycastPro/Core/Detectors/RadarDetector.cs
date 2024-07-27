using UnityEngine.Events;

namespace RaycastPro.Detectors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [Serializable]
    public class RadarEvent : UnityEvent<Collider, float> {}

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(RadarDetector))]
    public sealed class RadarDetector : ColliderDetector, IRadius
    {
        private float currentAngle;
        [SerializeField] public float radius = 2f;
        public bool local = true;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }
        public TimeMode timeMode = TimeMode.DeltaTime;
        public float cacheTime = 2f;
        public float height;

        public RadarEvent onRadarDetect;
        
        public Transform graphicShape;
        public Vector3 eulerOffset;
        public Axis shapeAxis;
        public float loopTime = 6f;
        public float speed => 360 / loopTime;
        public readonly Dictionary<Collider, float> DetectProfile = new Dictionary<Collider, float>();

        private float delta;
        private Transform _t;
        private Vector3 tUp, tForward, _dir, point;
        private Collider tCollider;
        private readonly HashSet<Collider> _colliders = new HashSet<Collider>();
        protected override void OnCast()
        {
#if UNITY_EDITOR
            CleanGate();
#endif

            CachePrevious();
            
            delta = GetDelta(timeMode);

#if UNITY_EDITOR
            if (IsSceneView && !IsPlaying) currentAngle = Time.realtimeSinceStartup * speed;
            else currentAngle += delta * speed;
#else
            currentAngle += delta * speed;
#endif

            _t = transform;
            tUp = local ? _t.up : Vector3.up;
            tForward = local ? _t.forward : Vector3.forward;
            if (graphicShape)
            {
                graphicShape.transform.position = _t.position;

                switch (shapeAxis)
                {
                    case Axis.X: graphicShape.eulerAngles = eulerOffset + new Vector3(currentAngle, 0, 0); break;
                    case Axis.Y: graphicShape.eulerAngles = eulerOffset + new Vector3(0, currentAngle, 0); break;
                    case Axis.Z: graphicShape.eulerAngles = eulerOffset + new Vector3(0, 0, currentAngle); break;
                }
            }



            _dir = Quaternion.AngleAxis(currentAngle, tUp) * tForward;
            _colliders.Clear();
            
            foreach (var _h in Physics.BoxCastAll(_t.position, new Vector3(0, height, 0), _dir,
                         _t.rotation, radius, detectLayer.value, triggerInteraction))
            {
                tCollider = _h.collider;
                if (!TagPass(tCollider)) continue;
                if (IsIgnoreSolver)
                {
                    _colliders.Add(tCollider);
                    continue;
                }
                point = DetectFunction(tCollider); // 1: Get Detect Point
                if (LOSPass(point, tCollider)) _colliders.Add(tCollider);
            }

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
            DetectedColliders = DetectProfile.Keys.ToHashSet();
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
                    onRadarDetect?.Invoke(col, DetectProfile[col]);
                }
            }
            #endregion
            
            
#if UNITY_EDITOR
            GizmoGate += () =>
            {
                Handles.color = (DetectProfile.Keys.Count > 0 ? DetectColor : DefaultColor).Alpha(.2f);
                Handles.DrawSolidArc(_t.position, tUp, _dir, -Mathf.Clamp01(cacheTime / loopTime) * 360f,
                    radius);
                Gizmos.DrawLine(_t.position, _t.position + _dir * radius);
                DrawWidthLine(_t.position, _t.position + _dir * radius, tUp * height);
            };
#endif
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "Detection of colliders for a limited time during the rotation of the radar hand." + HCDetector + HIRadius +
            HRotatable;
#pragma warning restore CS0414

        private static readonly string[] events = new string[]{"onDetectCollider", "onRadarDetect", "onNewCollider", "onLostCollider"};
        
        internal override void OnGizmos()
        {
            EditorUpdate();

            if (IsManuelMode) SceneView.RepaintAll();
            DrawFocusVector();

            foreach (var key in DetectProfile.Keys.ToArray())
            {
                Gizmos.color = DetectColor.Alpha(DetectProfile[key] / cacheTime);
                Gizmos.DrawWireCube(key.bounds.center, key.bounds.size);
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true, bool hasEvents = true, bool hasInfo = true)
        {
            
            if (hasMain)
            {
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
                HeightField(_so);
                PropertyMaxField(_so.FindProperty(nameof(loopTime)),  "Loop Time".ToContent(), .1f);
                PropertyMaxField(_so.FindProperty(nameof(cacheTime)),  CCacheTime.ToContent(CCacheTime));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
                #region ===SHAPE===

                BeginVerticalBox();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(graphicShape)), "Shape".ToContent("Shape"));
                GUI.enabled = graphicShape;
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(eulerOffset)));
                PropertyEnumField(_so.FindProperty(nameof(shapeAxis)), 3, "Shape Axis".ToContent(), new GUIContent[]
                {
                    "X".ToContent(), "Y".ToContent(), "Z".ToContent(),
                });
                
                GUI.enabled = true;

                EndVertical();

                #endregion
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
        
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif

    }
}