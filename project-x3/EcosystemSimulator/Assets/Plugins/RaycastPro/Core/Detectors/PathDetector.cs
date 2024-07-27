namespace RaycastPro.Detectors
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using RaySensors;

#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif

    using UnityEngine;
    
    [AddComponentMenu("RaycastPro/Detectors/" + nameof(PathDetector))]
    public class PathDetector : ColliderDetector, IRadius, IPulse
    {
        /// <summary>
        /// Main Source of Getting Path, Its better to disable it if don't need single casting.
        /// </summary>
        public RaySensor sourceRay;
        
        [SerializeField] private float radius = 2f;
        
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }
        
        public RaycastEvent onHit;
        public RaycastEvent onNewHit;
        public RaycastEvent onLostHit;

        public List<Vector3> PathPoints;
        public override bool Performed
        {
            get => DetectedHits.Count > 0;
            protected set { }
        }
        
        protected override void OnCast()
        {
            if (!sourceRay) return;

            if (!sourceRay.enabled) sourceRay.Cast();
            
            CachePrevious();
            
            sourceRay.GetPath(ref PathPoints);

#if UNITY_EDITOR
            CleanGate();
#endif
            if (sourceRay)
            {
                if (usingTagFilter)
                {
                    if (sourceRay is IRadius radius)
                    {
                        PathCastAll(PathPoints, ref DetectedHits, radius.Radius);
                        foreach (var r in DetectedHits) if (!r.collider.CompareTag(tagFilter)) DetectedHits.Remove(r);
                    }
                    else
                    {
                        PathCastAll(PathPoints, ref DetectedHits);
                        foreach (var r in DetectedHits)
                        {
                            if (!r.collider.CompareTag(tagFilter)) DetectedHits.Remove(r);
#if UNITY_EDITOR
                            else
                            {
                                GizmoGate += () => { DrawCross(r.point, r.normal); };   
                            }
#endif
                        }
                    }
                }
                else
                {
                    if (sourceRay is IRadius _r) PathCastAll(PathPoints, ref DetectedHits, _r.Radius);
                    else PathCastAll(PathPoints, ref DetectedHits, radius);
                }
            }
            
            Clear();
            
            foreach (var _dHit in DetectedHits) DetectedColliders.Add(_dHit.collider);
            
#if UNITY_EDITOR
            foreach (var c in DetectedColliders) PassColliderGate(c);
#endif
            if (onHit != null) foreach (var _member in DetectedHits) onHit.Invoke(_member);
            if (onNewHit != null) foreach (var _member in DetectedHits.Except(PreviousHits)) onNewHit.Invoke(_member);
            if (onLostHit != null) foreach (var _member in PreviousHits.Except(DetectedHits)) onLostHit.Invoke(_member);
            EventPass();
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receive all passing hits from the entered path ray." + HAccurate + HIPulse + HPathRay + HRDetector + HIRadius + HDependent;
#pragma warning restore CS0414

        protected new readonly string[] CEventNames = new []{"onHit", "onNewHit", "onLostHit", "onDetectCollider", "onNewCollider", "onLostCollider"};
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            DrawPath(PathPoints, drawSphere:true, radius: (sourceRay is IRadius _iRad ? _iRad.Radius+DotSize : radius), dotted: true);
            
            Handles.color = DetectColor;
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                if (sourceRay)
                {
                    BeginVerticalBox();
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(sourceRay)));
                    var _tSo = new SerializedObject(sourceRay);
                    _tSo.Update();
                    sourceRay?.EditorPanel(_tSo, hasMain: true, hasGeneral: false, hasEvents: false, hasInfo: false);
                    _tSo.ApplyModifiedProperties();
                    EndVertical();
                }
                else RCProEditor.TypeField("Path Ray", ref sourceRay);

                if (!(sourceRay is IRadius))
                {
                    RadiusField(_so);
                }

            }

            if (hasGeneral)
            {
                GeneralField(_so);
                BaseField(_so);
            }
            
            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }
            if (hasInfo) InformationField(PanelGate);
        }

        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
        public List<RaycastHit> DetectedHits = new List<RaycastHit>();
        public readonly RaycastHit[] PreviousHits = Array.Empty<RaycastHit>();
    }
}