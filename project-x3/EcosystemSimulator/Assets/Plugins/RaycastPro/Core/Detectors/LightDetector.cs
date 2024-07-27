using UnityEngine.Experimental.GlobalIllumination;

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
    
    public sealed class LightDetector : Detector, IPulse
    {
        [Tooltip("Collider your sounds on the specified layer and assign a special Collider Detector to automatically feed the sounds to the filtering source.")]
        public ColliderDetector lightFinder;
        
        public List<Light> sources = new List<Light>();
        
        public UnityEvent onFullShadow;
        public UnityEvent onFullLight;
        
                
        public float raycastRadius = 2f;
        
        public TimeMode timeMode = TimeMode.DeltaTime;

        public float changeSharpness = 15;
        
        private Color currentColor, sumColor, lerpColor;
        public override bool Performed {
            get => value > 0;
            protected set { }
        }

        private float value, total;

        private int hits;

        private RaycastHit _hit;
        
        private void Start() // Refreshing
        {
            Sync();
        }

        /// <summary>
        /// Light Finder will be sync on source
        /// </summary>
        public void Sync()
        {
            lightFinder?.SyncDetection(sources);
        }

        public void UnSync()
        {
            lightFinder?.UnSyncDetection(sources);
        }

        private void Reset()
        {
            OnValidate();
        }

        private int count;
        private void OnValidate()
        {
            count = 0;
            total = 0;
            foreach (var t in sources)
            {
                if (t)
                {
                    count++;
                    total += t.intensity;
                }
            }
        }

        private float delta;
        private float lastValue;
        protected override void OnCast()
        {
#if UNITY_EDITOR
            CleanGate();
#endif
            lastValue = value;
            
            value = 1;
            foreach (var _l in sources)
            {
                if (!_l) continue;
                if (_l.type == LightType.Directional)
                {
                    RaycastHit _h;
                    if (raycastRadius > 0)
                    {
                        Physics.Raycast(transform.position, -_l.transform.forward, out _h, Mathf.Infinity,
                            blockLayer.value, triggerInteraction);
                    }
                    else
                    {
                        Physics.SphereCast(transform.position, raycastRadius, -_l.transform.forward, out _h,
                            Mathf.Infinity,
                            blockLayer.value, triggerInteraction);
                    }

                    if (_h.transform)
                    {
                        value -= _l.intensity / total;
                    }
                }
                else
                {
                    var _dir = _l.transform.position - transform.position;
                    RaycastHit _h;
                    if (raycastRadius > 0)
                    {
                        Physics.Raycast(transform.position, _dir, out _h, _dir.magnitude
                         ,blockLayer.value, triggerInteraction);
                    }
                    else
                    {
                        Physics.SphereCast(transform.position, raycastRadius, _dir, out _h, _dir.magnitude, blockLayer.value, triggerInteraction);
                    }
                    
                    if (_h.transform)
                    {
                        value -= _l.intensity / total;
                    }
                }
            }
#if UNITY_EDITOR
            GizmoGate += () =>
            {
                GizmoColor = Color.Lerp(Color.black, Color.white, value);
                if (lightFinder && lightFinder is IRadius iRadius)
                {
                    
                    DrawSphere(transform.position, transform.up, iRadius.Radius+RCProPanel.elementDotSize);
                    DrawSphere(transform.position, transform.up, iRadius.Radius);
                }
            };
#endif

            if (lastValue != value)
            {
                if (value == 1f)
                {
                    onFullLight?.Invoke();
                }
                else if (value == 0f)
                {
                    onFullShadow?.Invoke();
                }
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Raycast processing scattered in the environment to detect the color and brightness of light with the input of specific sources." + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos() => EditorUpdate();
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(lightFinder)));

                if (!lightFinder)
                {
                    RCProEditor.PropertyArrayField(_so.FindProperty(nameof(sources)), "Lights".ToContent("Points"),
                        (i) => $"Light {i+1}".ToContent($"Index {i}"));
                }


                EditorGUILayout.PropertyField(_so.FindProperty(nameof(blockLayer)));

                EditorGUILayout.PropertyField(_so.FindProperty(nameof(raycastRadius)));

                BeginVerticalBox();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(changeSharpness)));
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
                EndVertical();

            }

            if (hasGeneral)
            {
                GeneralField(_so, hasTagField: false);
                BaseField(_so);
            }

            if (hasEvents)
            {
                EventField(_so);
                if (EventFoldout) RCProEditor.EventField(_so, new string[] {nameof(onFullLight), nameof(onFullShadow)});
            }

            if (hasInfo) InformationField(PanelGate);
        }
        protected override void DrawDetectorGuide(Vector3 point) { }
#endif
    }
}