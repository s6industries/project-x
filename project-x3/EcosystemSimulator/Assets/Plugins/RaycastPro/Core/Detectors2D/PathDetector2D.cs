namespace RaycastPro
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Detectors2D;
    using RaySensors2D;
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(PathDetector2D))]
    public class PathDetector2D : Detector2D, IPulse
    {
        public RaySensor2D sourceRay;

        public RaycastEvent2D onHit;
        public RaycastEvent2D onNewHit;
        public RaycastEvent2D onLostHit;
        public Collider2DEvent onDetectCollider;
        public Collider2DEvent onNewCollider;
        public Collider2DEvent onLostCollider;
        public override bool Performed
        {
            get => DetectedHits.Count > 0;
            protected set { }
        }
        
        public List<RaycastHit2D> DetectedHits = new List<RaycastHit2D>();
        public RaycastHit2D[] PreviousHits = Array.Empty<RaycastHit2D>();
        
        public HashSet<Collider2D> DetectedColliders = new HashSet<Collider2D>();
        public HashSet<Collider2D> PreviousColliders  = new HashSet<Collider2D>();

        private List<Vector2> PathPoints;
        
        protected override void OnCast()
        {
            if (!sourceRay.enabled) sourceRay.Cast();
            
            PreviousHits = DetectedHits.ToArray();
            
            sourceRay.GetPath2D(ref PathPoints);

#if UNITY_EDITOR
            CleanGate();
#endif

            if (sourceRay)
            {
                DetectedColliders.Clear();
                if (sourceRay is IRadius radius)
                    PathCastAll(PathPoints, ref DetectedHits, radius.Radius, MinDepth, MaxDepth);
                else
                    PathCastAll(PathPoints, ref DetectedHits, 0f, MinDepth, MaxDepth);
#if UNITY_EDITOR
                foreach (var _r in DetectedHits)
                    GizmoGate += () =>
                    {
                        Handles.color = DetectColor;
                        DrawCross(_r.point, Vector3.forward);
                    };
#endif
                foreach (var _dHit in DetectedHits) DetectedColliders.Add(_dHit.collider);
            
#if UNITY_EDITOR
                foreach (var c in DetectedColliders) PassColliderGate(c);
#endif
                if (onHit != null) foreach (var _member in DetectedHits) onHit.Invoke(_member);
                if (onNewHit != null) foreach (var _member in DetectedHits.Except(PreviousHits)) onNewHit.Invoke(_member);
                if (onLostHit != null) foreach (var _member in PreviousHits.Except(DetectedHits)) onLostHit.Invoke(_member);
                
                if (onDetectCollider != null) foreach (var _member in DetectedColliders) onDetectCollider.Invoke(_member);
                if (onNewCollider != null) foreach (var _member in DetectedColliders.Except(PreviousColliders)) onNewCollider.Invoke(_member);
                if (onLostCollider != null) foreach (var _member in PreviousColliders.Except(DetectedColliders)) onLostCollider.Invoke(_member);
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receive all passing hits from the entered path ray." + HAccurate + HPathRay + HIPulse + HRDetector + HDependent;
#pragma warning restore CS0414
        
        protected readonly string[] CEventNames = {"onHit", "onNewHit", "onLostHit", "onDetectCollider", "onNewCollider", "onLostCollider"};
        internal override void OnGizmos()
        {
            EditorUpdate();
            Handles.color = Performed ? DetectColor : DefaultColor;
            DrawPath2D(PathPoints.ToDepth(z), sourceRay.hit.point, drawDisc: false, radius: (sourceRay is IRadius _iRad ? _iRad.Radius+DotSize : 0f), dotted: true);
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
                    var prop = _so.FindProperty(nameof(sourceRay));
                    EditorGUILayout.PropertyField(prop, "Path Ray".ToContent());
                    if (sourceRay)
                    {
                        var _tSO = new SerializedObject(sourceRay);
                        _tSO.Update();
                        sourceRay.EditorPanel(_tSO, hasMain: true, hasGeneral: false, hasEvents: false, hasInfo: false);
                        _tSO.ApplyModifiedProperties();
                    }
                    EndVertical();
                }
                else
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(sourceRay)), "Path Ray".ToContent());
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
#endif
    }
}