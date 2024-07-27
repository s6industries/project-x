namespace RaycastPro.Detectors
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [Serializable]
    public class LightEvent : UnityEvent<Light> {  }
    
    [Obsolete]
    public sealed class LightDetectorLegacy : Detector, IPulse
    {
        public Light[] Lights = Array.Empty<Light>();
        public float minIntensity, maxIntensity = 100;
        
        public Color targetColor;
        public Color colorTolerance = Color.white;

        public LightEvent onDetectLight;
        public LightEvent onNewLight;
        public LightEvent onLostLight;
        public override bool Performed
        {
            get => DetectedLights.Count > 0;
            protected set { }
        }

        #region Public Methods
        public void TurnOnLight(Light _l) => _l.enabled = true;
        public void TurnOffLight(Light _l) => _l.enabled = false;
        #endregion

        private float inRange;
        private bool trueIntensity, trueColor;
        
        private void Start() // Refreshing
        {
            DetectedLights = new List<Light>();
            PreviousLights = Array.Empty<Light>();
        }

        protected override void OnCast()
        {
            DetectedLights.Clear();

#if UNITY_EDITOR
            GizmoGate = null;
#endif

            foreach (var l in Lights)
            {
                if (!l || !InLayer(l.gameObject)) continue;
                if (Vector3.Distance(l.transform.position, transform.position) > l.range) continue;
                trueIntensity = l.intensity < minIntensity || l.intensity > maxIntensity;
                trueColor = l.color.InColorTolerance(targetColor, colorTolerance);
#if UNITY_EDITOR
                GizmoGate += () =>
                {
                    Gizmos.DrawLine(transform.position, l.transform.position);
                    Gizmos.color = trueIntensity && trueColor ? DetectColor : DefaultColor;
                    Gizmos.DrawWireSphere(l.transform.position, l.range);
                    Gizmos.color = l.color;
                    Gizmos.DrawWireSphere(l.transform.position, l.range / .95f);
                };
#endif
                DetectedLights.Add(l);
            }

            CallEvents(DetectedLights, PreviousLights, onDetectLight, onNewLight, onLostLight);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Detection and filtering of scene lights based on filtration." + HDirectional;
#pragma warning restore CS0414
        
        private readonly string[] CEventNames = {"onDetectLight", "onNewLight", "onLostLight"};
        internal override void OnGizmos() => EditorUpdate();
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                if (GUILayout.Button(CCollectAll)) Lights = FindObjectsOfType<Light>();

                var prop = _so.FindProperty(nameof(Lights));
                BeginVerticalBox();
                RCProEditor.PropertyArrayField(prop,
                    CLight.ToContent(),
                    i => $"Light {i + 1}".ToContent($"Index {i}"));

                EndVertical();
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(minIntensity)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(minIntensity)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(maxIntensity)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(targetColor)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(colorTolerance)));
            }

            if (hasGeneral)
            {
                GeneralField(_so, hasTagField: false);
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

        public List<Light> DetectedLights { get; set; } = new List<Light>();
        private Light[] PreviousLights = Array.Empty<Light>();
    }
}