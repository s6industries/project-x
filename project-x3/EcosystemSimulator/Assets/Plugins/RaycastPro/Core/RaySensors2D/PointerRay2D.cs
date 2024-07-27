namespace RaycastPro.RaySensors2D
{
    using UnityEngine;

#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(PointerRay2D))]
    public sealed class PointerRay2D : RaySensor2D, IRadius
    {
        public Camera mainCamera;

        public Camera.MonoOrStereoscopicEye eyeType = Camera.MonoOrStereoscopicEye.Mono;
        [SerializeField] public float radius = .4f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }

        private Vector3 mousePoint;
        private Vector2 _dir;
        protected override void OnCast()
        {
            if (!mainCamera) return;

#if ENABLE_INPUT_SYSTEM
            mousePoint = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue(), eyeType);
#else
            mousePoint = mainCamera.ScreenToWorldPoint(Input.mousePosition, eyeType);
#endif

            if (direction.y != 0)
            {
#if UNITY_2021_2
                mousePoint += local ? transform.up * direction.y : Vector2.up * direction.y;  
#else
                mousePoint += local ? transform.up * direction.y : Vector3.up * direction.y;  
#endif

            }
            if (radius > 0)
            {
                _dir = (mousePoint - transform.position);
                hit = Physics2D.CircleCast(transform.position, radius, _dir,  Mathf.Min(direction.x,_dir.magnitude), detectLayer.value, MinDepth, MaxDepth);
            }
            else
            {
                hit = Physics2D.Linecast(transform.position, Tip, detectLayer.value, MinDepth, MaxDepth);
            }
            isDetect = FilterCheck(hit);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "A mouse location tracker that is able to emit from the desired object and is used to immediately launch this feature."+HAccurate+HIRadius+HDependent;
#pragma warning restore CS0414

        private Vector3 basePoint, tip;
        internal override void OnGizmos()
        {
            EditorUpdate();
            GizmoColor = Performed ? DetectColor : DefaultColor;
            basePoint = Base;
            tip = Tip;
            DrawCircleLine(basePoint, tip, radius, true, hit);
            DrawDepthLine(basePoint, tip);
            DrawNormal2D(hit, z);
            DrawNormalFilter();
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true, bool hasEvents = true,
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
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(eyeType)));
            }

            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
        
        private bool InEditMode => IsSceneView || !Application.isPlaying;
#endif

        public override Vector3 RawTip => Tip;
#if UNITY_EDITOR
        public override Vector3 Tip => InEditMode
            ? transform.position + LocalDirection3D : Vector2.Lerp(transform.position, mousePoint, direction.x / RayLength).ToDepth(z);
#else

        public override Vector3 Tip => Vector3.Lerp(transform.position, mousePoint, direction.x / RayLength).ToDepth(z);
#endif
        public override float RayLength => Vector2.Distance(transform.position, mousePoint);
        public override Vector3 Base => transform.position;
    }
}