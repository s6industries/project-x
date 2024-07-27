using System.Linq;

namespace RaycastPro.Detectors
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(LineDetector))]
    public sealed class LineDetector : ColliderDetector, IRadius, IPulse
    {
        public RayType rayType = RayType.Ray;

        public bool local = true;

        public Vector3 direction = Vector3.forward;
        
        public override bool Performed
        {
            get => DetectedHits.Count > 0;
            protected set { }
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
                    nonAllocatedHits = new RaycastHit[limitCount];
                }
            }
        }
        public int LimitCount
        {
            get => limitCount;
            set
            {
                limitCount = Mathf.Max(0,value);
                nonAllocatedHits = new RaycastHit[limitCount];
            }
        }

        [SerializeField] public float radius = .2f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        [SerializeField] private float height;
        public float Height
        {
            get => height;
            set => height = Mathf.Max(0, value);
        }
        
        public Vector3 extents = new Vector3(.4f, .4f, 0f);

#if UNITY_EDITOR
        private new void OnValidate()
        {
            nonAllocatedHits = new RaycastHit[limitCount];
        }  
#else
        private void OnEnable()
        {
            nonAllocatedHits = new RaycastHit[limitCount];
        }  
#endif

        public Vector3 LocalDirection => transform.TransformDirection(direction);
        public Vector3 Direction => local ? transform.TransformDirection(direction) : direction;

        private Vector3 up;
        protected override void OnCast()
        {
            CachePrevious();
            PreviousHits = DetectedHits.ToArray();

            Clear();

            if (limited) // Non-Allocated
            {
                Array.Clear(nonAllocatedHits, 0, nonAllocatedHits.Length);
                switch (rayType)
                {
                    case RayType.Ray:
                        Physics.RaycastNonAlloc(transform.position, Direction, nonAllocatedHits, direction.magnitude, detectLayer.value, triggerInteraction);
                        break;
                    case RayType.Pipe:

                        if (height > 0)
                        {
                            up = transform.up * height/2;
                            Physics.CapsuleCastNonAlloc(transform.position+up, transform.position-up, radius, Direction.normalized, nonAllocatedHits, direction.magnitude, detectLayer.value, triggerInteraction);
                        }
                        else
                        {
                            Physics.SphereCastNonAlloc(transform.position, radius, Direction.normalized,
                                nonAllocatedHits, direction.magnitude, detectLayer.value, triggerInteraction);
                        }
                        break;
                    case RayType.Box:
                        Physics.BoxCastNonAlloc(transform.position, extents / 2, Direction.normalized, 
                            nonAllocatedHits, transform.rotation, direction.magnitude, detectLayer.value,
                            triggerInteraction);
                        break;
                }
                    
                DetectedHits.Clear();
                DetectedHits.AddRange(nonAllocatedHits);
            }
            else
            {
                DetectedHits.Clear();
                switch (rayType)
                {
                    case RayType.Ray:
                        nonAllocatedHits = Physics.RaycastAll(transform.position, Direction, direction.magnitude,
                            detectLayer.value, triggerInteraction);
                        break;
                    case RayType.Pipe:

                        if (height > 0)
                        {
                            up = transform.up * height/2;
                            nonAllocatedHits = Physics.CapsuleCastAll(transform.position+up, transform.position-up, radius, Direction, direction.magnitude, detectLayer.value, triggerInteraction);
                        }
                        else
                        {
                            nonAllocatedHits = Physics.SphereCastAll(transform.position, radius, Direction,
                                direction.magnitude, detectLayer.value, triggerInteraction);
                        }
                        break;
                    case RayType.Box:
                        nonAllocatedHits = Physics.BoxCastAll(transform.position, extents / 2, Direction,
                            transform.rotation, direction.magnitude, detectLayer.value, triggerInteraction);
                        break;
                }

                if (usingTagFilter)
                {
                    for (index = 0; index < DetectedHits.Count; index++)
                    {
                        if (!DetectedHits[index].collider.CompareTag(tagFilter)) DetectedHits.RemoveAt(index);
                    }
                }
                else
                {
                    DetectedHits.Clear();
                    DetectedHits.AddRange(nonAllocatedHits);
                }
            }

            #region Events

            if (onHit != null) foreach (var _member in DetectedHits) onHit.Invoke(_member);
            if (onNewHit != null)
            {
                foreach (var _member in DetectedHits.Except(PreviousHits)) onNewHit.Invoke(_member);
            }
            if (onLostHit != null)
            {
                foreach (var _member in PreviousHits.Except(DetectedHits)) onLostHit.Invoke(_member);
            }
            
            foreach (var detectedHit in DetectedHits)
            {
                DetectedColliders.Add(detectedHit.collider);
            }
            
            EventPass();
            #endregion
        }

        public void InstantiateOnHits(GameObject gameObject, Transform parent = null)
        {
            foreach (var detectedHit in DetectedHits)
            {
                Instantiate(gameObject, detectedHit.point, Quaternion.LookRotation(detectedHit.normal), parent);
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
#pragma warning restore CS0414
            "Sending a fixed line in the form of Ray, Pipe or Box and collecting all Hits data." + HDirectional+ HIRadius + HIPulse + HRDetector + HINonAllocator;
        internal override void OnGizmos()
        {
            EditorUpdate();

            // === Gizmo Gate Are Written Here because of avoiding #IF UNITY EDITOR check in main class
            
            GizmoColor = Performed ? DetectColor : DefaultColor;
            
            switch (rayType)
            {
                case RayType.Ray:
                    Gizmos.DrawRay( transform.position, Direction);
                    break;
                case RayType.Pipe:
                    var sphereTip = transform.position + Direction;
                    DrawCapsuleLine(transform.position, sphereTip, radius, height, _t: transform);
                    break;
                case RayType.Box:
                    var boxTip = transform.position + Direction;
                    DrawBoxLine(transform.position, boxTip, extents, true);
                    break;
            }

            GizmoColor = HelperColor;
            foreach (var detectedRaycast in DetectedHits) DrawCross(detectedRaycast.point, detectedRaycast.normal);
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so, nameof(direction));
                BeginVerticalBox();
                EditorGUI.BeginChangeCheck();
                var prop = _so.FindProperty(nameof(rayType));
                PropertyEnumField(prop, 3,  "Ray Type".ToContent("Ray Type"), new GUIContent[]
                {
                    "Ray".ToContent("Ray"),
                    "Pipe".ToContent("Pipe"),
                    "Box".ToContent("Box"),
                });
                
                switch (prop.enumValueIndex)
                {
                    case 1:
                        RadiusField(_so);
                        HeightField(_so);
                        break;
                    case 2:
                        ExtentsField(_so);
                        break;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    Texture2D texture2D = null;
                    switch (prop.enumValueIndex)
                    {
                        case 0:
                            texture2D = IconManager.GetIconFromName("Icon_LineDetector");
                            break;
                        case 1:
                            texture2D = IconManager.GetIconFromName("Icon_LineDetectorPipe");
                            break;
                        case 2:
                            texture2D = IconManager.GetIconFromName("Icon_LineDetectorBox");
                            break;
                    }
                    
                    if (texture2D) MonoScript.FromMonoBehaviour(this).SetIcon(texture2D);
                }
                
                EndVertical();
                
                NonAllocatorField(_so, ref nonAllocatedHits, i=> nonAllocatedHits = new RaycastHit[i]);
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                BaseField(_so);
            }

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so,events);
            }

            if (hasInfo)
            {
                InformationField(() =>
                {
                    BeginVertical();
                    foreach (var D in DetectedColliders)
                    {
                        if (!D) continue;

                        GUILayout.BeginHorizontal();
                        GUILayout.Box(D.name, RCProEditor.LabelStyle);
                        GUILayout.Box("<color=#3DED33>Detect</color>",  RCProEditor.BoxStyle, GUILayout.Width(50));
                        
                        GUILayout.EndHorizontal();
                    }
                    EndVertical();
                });
            }
        }

        private static readonly string[] events = new[]
        {
            nameof(onDetectCollider), nameof(onLostCollider), nameof(onLostCollider), nameof(onHit), nameof(onNewHit),
            nameof(onLostHit)
        };
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
        public List<RaycastHit> DetectedHits { get; private set; } = new List<RaycastHit>();
        private RaycastHit[] PreviousHits = Array.Empty<RaycastHit>();
        private RaycastHit[] nonAllocatedHits = Array.Empty<RaycastHit>();
        private int index;

        public RaycastEvent onHit;
        public RaycastEvent onNewHit;
        public RaycastEvent onLostHit;
    }
}