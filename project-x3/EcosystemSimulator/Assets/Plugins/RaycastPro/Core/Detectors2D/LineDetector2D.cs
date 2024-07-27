using System.Collections.Generic;
using System.Linq;

namespace RaycastPro.Detectors2D
{
    using UnityEngine;
    using System;
    using RaySensors2D;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(LineDetector2D))]
    public sealed class LineDetector2D : ColliderDetector2D, IRadius, IPulse
    {
        public List<RaycastHit2D> DetectedHits { get; private set; } = new List<RaycastHit2D>();
        private RaycastHit2D[] PreviousHits = Array.Empty<RaycastHit2D>();
        private RaycastHit2D[] nonAllocatedHits = Array.Empty<RaycastHit2D>();
        private int index;
        
        public RaycastEvent2D onHit;
        public RaycastEvent2D onNewHit;
        public RaycastEvent2D onLostHit;
        
        [SerializeField] private bool limited;
        [SerializeField] private int limitCount = 3;
        
        public override bool Performed
        {
            get => DetectedHits.Count > 0;
            protected set { }
        }
        public bool local = true;
        public bool Limited
        {
            get => limited;
            set
            {
                limited = value;
                if (value)
                {
                    nonAllocatedHits = new RaycastHit2D[limitCount];
                }
            }
        }
        public int LimitCount
        {
            get => limitCount;
            set
            {
                limitCount = Mathf.Max(0,value);
                nonAllocatedHits = new RaycastHit2D[limitCount];
            }
        }
        

        
        public Vector2 direction = new Vector2(1f, 0);
        public RayType rayType = RayType.Ray;
        [SerializeField] public float radius = .4f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }
        public float height;
        public Vector2 extents;
        public Vector3 Direction => local ? LocalDirection : direction.ToDepth();
        public Vector3 LocalDirection => transform.TransformDirection(direction);
        private float angle;
        protected override void OnCast()
        {
            PreviousColliders = DetectedColliders.ToArray();
            PreviousHits = DetectedHits.ToArray();
            
            Clear();

            if (limited)
            {
                Array.Clear(nonAllocatedHits, 0, nonAllocatedHits.Length);
                switch (rayType)
                {
                    case RayType.Ray:
                        Physics2D.RaycastNonAlloc(transform.position, LocalDirection.normalized, nonAllocatedHits,
                            direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
                        break;
                    case RayType.Pipe:
                        if (height > 0)
                        {
                            angle = local ? -Vector2.Angle(transform.right.To2D(), Vector2.right) : 0;
                            Physics2D.CapsuleCastNonAlloc(transform.position, new Vector2(Mathf.Max(radius*2, 0.001f), height+radius*2), CapsuleDirection2D.Vertical, angle, Direction, nonAllocatedHits, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
                        }
                        else
                        {
                            Physics2D.CircleCastNonAlloc(transform.position, radius, Direction, nonAllocatedHits, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
                        }
                        break;
                    case RayType.Box:
                        Physics2D.BoxCastNonAlloc(transform.position, extents / 2, Vector2.Angle(Direction, Vector2.right), LocalDirection, nonAllocatedHits, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
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
                        DetectedHits.AddRange(Physics2D.RaycastAll(transform.position, Direction, direction.magnitude,
                            detectLayer.value, MinDepth, MaxDepth));
                        break;
                    case RayType.Pipe:
                        if (height > 0)
                        {
                            angle = local ? -Vector2.Angle(transform.right.To2D(), Vector2.right) : 0;
                            DetectedHits.AddRange(Physics2D.CapsuleCastAll(transform.position, new Vector2(Mathf.Max(radius*2, 0.001f), height+radius*2), CapsuleDirection2D.Vertical, angle, Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth));
                        }
                        else
                        {
                            DetectedHits.AddRange(Physics2D.CircleCastAll(transform.position, radius, Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth));
                        }
                        break;
                    case RayType.Box:
                        DetectedHits.AddRange(Physics2D.BoxCastAll(transform.position, extents / 2,
                            Vector2.Angle(Direction, Vector2.right),
                            Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth));
                        break;
                }
            }
            if (usingTagFilter)
            {
                for (index = 0; index < DetectedHits.Count; index++)
                {
                    if (!DetectedHits[index].collider.CompareTag(tagFilter)) DetectedHits.RemoveAt(index);
                }
            }

            #region Events
            CallEvents(DetectedHits, PreviousHits, onHit, onNewHit, onLostHit);
            
            EventPass();
            #endregion
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            nonAllocatedHits = new RaycastHit2D[limitCount];
        }  
#else
        private void OnEnable()
        {
            nonAllocatedHits = new RaycastHit2D[limitCount];
        }  
#endif

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "Sending a fixed 2D line in the form of Ray, Pipe or Box and collecting all Hits data." + HDirectional + HRDetector + HIRadius + HIPulse + HINonAllocator;
#pragma warning restore CS0414 
        private static readonly string[] events = new[]
        {
            nameof(onDetectCollider), nameof(onLostCollider), nameof(onLostCollider), nameof(onHit), nameof(onNewHit),
            nameof(onLostHit)
        };
        private readonly GUIContent[] tips =
        {
            "Ray".ToContent("Ray"),
            "Pipe".ToContent("Pipe"),
            "Box".ToContent("Box"),
        };
        internal override void OnGizmos()
        {
            EditorUpdate();

            // === Gizmo Gate Are Written Here because of avoiding #IF UNITY EDITOR check in main class
            var _pos = transform.position;
            GizmoColor = DefaultColor;
            var Tip = transform.position + Direction;
            
            switch (rayType)
            {
                case RayType.Ray:
                    Gizmos.DrawRay(_pos, Direction);
                    break;
                case RayType.Pipe:
                    DrawCircleRay(_pos, direction, LocalDirection, local, radius, height);
                    break;
                case RayType.Box:
                    RaySensor2D.DrawBoxRay(transform, _pos, direction, extents, z, local);
                    break;
            }

            RaySensor2D.DrawDepthLine(_pos, Tip, MinDepth, MaxDepth);

            Handles.color = DetectColor;
            
            foreach (var raycastHit2D in DetectedHits)
            {
                if (raycastHit2D)
                {
                    
                    DrawCross(raycastHit2D.point.ToDepth(z), Vector3.forward);
                }
                
            }
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
                PropertyEnumField(prop, 3,  "Ray Type".ToContent(), tips);
                
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
                            texture2D = IconManager.GetIconFromName("Icon_LineDetector2D");
                            break;
                        case 1:
                            texture2D = IconManager.GetIconFromName("Icon_LineDetector2DPipe");
                            break;
                        case 2:
                            texture2D = IconManager.GetIconFromName("Icon_LineDetector2DBox");
                            break;
                    }
                    
                    if (texture2D != null) MonoScript.FromMonoBehaviour(this).SetIcon(texture2D);
                }
                EndVertical();
                NonAllocatorField(_so, ref nonAllocatedHits, i => nonAllocatedHits = new RaycastHit2D[i]);
            }

            
            if (hasGeneral)
            {
                GeneralField(_so);
                BaseField(_so);
            }

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, events);
            }

            if (hasInfo) InformationField(() =>
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
#endif
    }
}