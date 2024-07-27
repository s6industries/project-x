namespace RaycastPro.RaySensors
{
    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(PointerRay))]
    public sealed class PointerRay : RaySensor, IRadius
    {
        public Camera mainCamera;
        
        [SerializeField] private float radius = .4f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        [Tooltip("It will make ray origin directly on camera.")]
        public bool cameraBase = false;
        
        [Tooltip("When you enable this option, the first ray casting from camera to detect Hit point then second ray from target Object will cast according to the mouseHit..")]
        public bool secondRay = true;
        
        public Camera.MonoOrStereoscopicEye eyeType = Camera.MonoOrStereoscopicEye.Mono;
        private Ray mouseRay;

        public override Vector3 RawTip { get; }
        public override Vector3 Tip => mouseRay.origin + mouseRay.direction * direction.z;
        public override float RayLength => direction.z;
        public override Vector3 Base => cameraBase ? mainCamera.transform.position : transform.position;
        
        private Vector3 input;
        private RaycastHit mouseHit;
        private Vector3 secondDir;
        private Vector3 p1, p2;
        private Vector3 pos;
        
        private void Reset()
        {
            mainCamera = Camera.main;
            
            if (!mainCamera)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
            
            void DrawGizmos()
            {
                GizmoGate += () =>
                {
                    GUI.color = HelperColor;
                    var _mt = transform;

                    if (cameraBase || transform == mainCamera.transform)
                    {
                        _mt = IsSceneView ? SceneCamera.transform : mainCamera.transform;
                    }

                    if (IsPlaying)
                    {
                        p1 = _mt.position;
                        p2 = p1 + secondDir.normalized * direction.z;
                    }
                    else
                    {
                        p1 = _mt.position;
                        p2 =  p1 + _mt.forward * direction.z;
                    }

                    
                    GizmoColor = Performed ? DetectColor : DefaultColor;
                    DrawLineZTest(p1 + _mt.right * radius, p2 + _mt.right * radius);
                    DrawLineZTest(p1 + _mt.up * radius, p2 + _mt.up * radius);
                    DrawLineZTest(p1 - _mt.right * radius, p2 - _mt.right * radius);
                    DrawLineZTest(p1 - _mt.up * radius, p2 - _mt.up * radius);

                    Handles.DrawWireDisc((p1 + p2) / 2, p2 - p1, radius);
                    Handles.DrawWireDisc(p2, p2 - p1, radius);

                    if (IsPlaying) DrawDetectLine(p1, p2, hit, Performed);

                    if (hit.transform) DrawNormal(hit);
                };
            }
#endif
            
            if (!mainCamera) return;

#if ENABLE_INPUT_SYSTEM
            input = Mouse.current.position.ReadValue();
#else
            input = Input.mousePosition;
#endif
            mouseRay = mainCamera.ScreenPointToRay(input, eyeType);

            pos = Base;
            
            if (secondRay && Physics.Raycast(mouseRay.origin, mouseRay.direction, out mouseHit, direction.z, detectLayer.value, triggerInteraction))
            {
                secondDir = mouseHit.point - pos;
            }
            else
            {
                secondDir = (mouseRay.origin + mouseRay.direction*direction.z) - pos;
            }
            
            if (radius > 0)
            {
                Physics.SphereCast(pos, radius, secondDir, out hit, direction.z, detectLayer.value, triggerInteraction);
            }
            else
            {
                Physics.Raycast(pos,secondDir, out hit, direction.z, detectLayer.value, triggerInteraction);
            }

#if UNITY_EDITOR
            DrawGizmos();
#endif
        }
        
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "A mouse location tracker that is able to emit from the desired object and is used to immediately launch this feature."+HAccurate+HIRadius+HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos() => EditorUpdate();

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginHorizontal();
                var propCamera = _so.FindProperty(nameof(mainCamera));
                EditorGUILayout.PropertyField(propCamera);
                if (!mainCamera && GUILayout.Button("Main", GUILayout.Width(50f)))
                {
                    propCamera.objectReferenceValue = Camera.main;
                }
                EndHorizontal();
                DirectionField(_so);
                RadiusField(_so);
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(cameraBase)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(secondRay)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(eyeType)));
            }
            
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) InformationField();
        }
#endif
    }
}
