
namespace RaycastPro.Detectors
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(TargetDetector))]
    public sealed class TargetDetector : Detector, IRadius, IPulse
    {
        public Transform[] targets = Array.Empty<Transform>();
        
        private float value;

        /// <summary>
        /// Use the Calculated value in process time
        /// </summary>
        public float Value => value;
        /// <summary>
        /// calculate Value when call it
        /// </summary>
        public float DirectValue => (float) BlockedTargets.Count / targets.Length;
        public override bool Performed
        {
            get => BlockedTargets.Count < targets.Length;
            protected set { }
        }

        public Vector3 Center
        {
            get
            {
                var count = 0;
                var average = Vector3.zero;
                foreach (var target in targets)
                {
                    if (target)
                    {
                        count++;
                        average += target.position;
                    }
                }
                return average / count;
            }
        }
        [SerializeField] public float radius = 0f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        public TransformEvent onDetectTransform;
        public TransformEvent onBeginBlocked;
        public TransformEvent onBeginDetected;
        
        private Vector3 _dir;

        public float CastFrom(ColliderDetector cDetector)
        {
            TDP = cDetector.transform.position;
            CoreUpdate();
            return value;
        }
        public float CastFrom(Vector3 position)
        {
            TDP = position;
            CoreUpdate();
            return value;
        }
        private void CoreUpdate()
        {
#if UNITY_EDITOR
            CleanGate();
#endif
            PreviousBlockedTargets = BlockedTargets.ToArray();
            BlockedTargets.Clear();
            foreach (var _t in targets)
            {
                if (!_t) continue;

                _dir = _t.position - TDP;
                RaycastHit _rHit;
                if (radius > 0)
                {
                    Physics.SphereCast(TDP, radius, _dir , out _rHit, _dir.magnitude, detectLayer.value, triggerInteraction);
                }
                else
                {
                    Physics.Linecast(TDP, _t.position, out _rHit, detectLayer.value, triggerInteraction);
                }

                var pass = !_rHit.transform || _t == _rHit.transform;
#if UNITY_EDITOR
                PanelGate += () => DetectorInfoField(_t, _rHit.point, !pass);
                GizmoGate += () =>
                {
                    if (alphaCharge > 0)
                    {
                        DrawBlockLine(TDP, _t.position, _dir, radius, drawCross: true,
                            drawSphereBase: false, drawSphereTarget: true, _rHit, alphaCharge);
                    }
                };
#endif
                if (pass) BlockedTargets.Add(_t);
            }

            // Setup Value
            value = DirectValue;

            if (onDetectTransform != null) foreach (var _member in BlockedTargets) onDetectTransform.Invoke(_member);
            if (onBeginDetected != null)
            {
                foreach (var _member in BlockedTargets.Except(PreviousBlockedTargets)) onBeginDetected.Invoke(_member);
            }
            if (onBeginBlocked != null)
            {
                foreach (var _member in PreviousBlockedTargets.Except(BlockedTargets)) onBeginBlocked.Invoke(_member);
            }
        }
        protected override void OnCast()
        {
            TDP = transform.position;
            CoreUpdate();
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Examining Target points and detecting the blocking of the connection line."+HAccurate+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos() 
        {
            EditorUpdate();
            if (IsLabel)
            {
                Handles.Label(Center,
                    $"<color=#60FFF5>{name}</color> TD: <color=#3EFF3A>{value:P}</color>", RCProEditor.LabelStyle);
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginVerticalBox();
                RCProEditor.PropertyArrayField(_so.FindProperty(nameof(targets)),
                    CTarget.ToContent(TTarget), i => $"Target {i+1}".ToContent($"Index {i}"));
                EndVertical();
                RadiusField(_so);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(detectLayer)),
                    CBlockLayer.ToContent(TBlockLayer));
            }
            if (hasGeneral)
            {
                GeneralField(_so, layerField: false);
                BaseField(_so);
            }
            
            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so,events);
            }

            if (hasInfo)
            {
                InformationField(PanelGate);
                PercentProgressField(DirectValue, "Detected");
            }
        }

        private static readonly string[] events = new[] {nameof(onDetectTransform), nameof(onBeginDetected), nameof(onBeginBlocked)};
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
        public List<Transform> BlockedTargets { get; set; } = new List<Transform>();
        private Transform[] PreviousBlockedTargets { get; set; } = Array.Empty<Transform>();
    }
}