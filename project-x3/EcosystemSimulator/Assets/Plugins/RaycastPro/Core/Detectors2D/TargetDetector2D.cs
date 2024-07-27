
namespace RaycastPro.Detectors2D
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using Detectors;

#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif


    [AddComponentMenu("RaycastPro/Detectors/" + nameof(TargetDetector2D))]
    public sealed class TargetDetector2D : Detector2D, IRadius, IPulse
    {
        public Transform[] targets = Array.Empty<Transform>();
        public float Value => (float) BlockedTargets.Count / targets.Length;
        [SerializeField] public float radius = 0f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        public TransformEvent onDetectTransform;
        public TransformEvent onBeginBlocked;
        public TransformEvent onBeginDetected;
        public override bool Performed
        {
            get => BlockedTargets.Count < targets.Length;
            protected set { }
        }

        private Vector3 pos;
        private Transform _t;

        public float CastFrom(Vector2 position)
        {
            TDP = position.ToDepth(z);
            CoreUpdate();
            return Value;
        }

        private void CoreUpdate()
        {
#if UNITY_EDITOR
            CleanGate();
#endif
            PreviousBlockedTargets = BlockedTargets.ToArray();
            BlockedTargets.Clear();
            pos = transform.position;
            for (var index = 0; index < targets.Length; index++)
            {
                _t = targets[index];
                if (!detectLayer.InLayer(_t.gameObject)) continue;

                var _tPos = _t.position;
                RaycastHit2D hit2D;
                if (radius > 0)
                {
                    var dir = _tPos - pos;
                    hit2D = Physics2D.CircleCast(pos, radius, dir, dir.magnitude, detectLayer.value, MinDepth, MaxDepth);
                }
                else
                {
                    hit2D = Physics2D.Linecast(pos, _tPos, detectLayer.value, MinDepth, MaxDepth);
                }

                var pass = !hit2D || hit2D.transform == _t;

#if UNITY_EDITOR
                PanelGate += () => DetectorInfoField(_t, _tPos, !pass);
                GizmoGate += () =>
                {
                    if (radius > 0)
                    {
                        Gizmos.color = Handles.color = pass ? DefaultColor : BlockColor;
                        DrawCircleLine(pos.ToDepth(z), _tPos.ToDepth(z), radius, _t, hit2D);
                    }
                    else
                    {
                        DrawBlockLine2D(pos, _tPos, z, _t, hit2D);
                    }
                };
#endif
                if (pass) BlockedTargets.Add(_t);
            }
            
            CallEvents(BlockedTargets, PreviousBlockedTargets, onDetectTransform, onBeginDetected, onBeginBlocked);

        }

        protected override void OnCast()
        {
            TDP = transform.position;
            CoreUpdate();
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Examining Target points and detecting the blocking of the connection line."+HAccurate+HIRadius+HIPulse;
#pragma warning restore CS0414
        internal override void OnGizmos() => EditorUpdate();
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginVerticalBox();
                RCProEditor.PropertyArrayField(_so.FindProperty(nameof(targets)),
                    CTarget.ToContent(TTarget), i => $"Target {i+1}".ToContent($"Index {i}"));
                EndVertical();
                RadiusField(_so);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(detectLayer)), CBlockLayer.ToContent(TBlockLayer));
            }
            
            if (hasGeneral)
            {
                PulseField(_so);
                TagField(_so);
                DepthField(_so);
                BaseField(_so);
            }

            if (hasEvents)  EventField(_so);

            if (hasInfo)
            {
                InformationField(PanelGate);
                PercentProgressField(Value, "Detected");
            }
        }
#endif
        public List<Transform> BlockedTargets { get; set; } = new List<Transform>();
        public Transform[] PreviousBlockedTargets { get; set; } = Array.Empty<Transform>();
    }
}