
namespace RaycastPro.RaySensors
{
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    using UnityEngine;
    using Random = UnityEngine.Random;
    
    [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(WaveRay))]
    public sealed class WaveRay : PathRay, IRadius
    {
        public TimeMode timeMode = TimeMode.DeltaTime;
        public int segments = 8;
        [SerializeField] private float radius;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public float waveSpeed = 1f;
        public float power = 1;
        public Vector2 noise;
        public Vector2 scale = new Vector2(0f, 1f);
        public AnimationCurve clumpY = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve clumpX = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float offsetX;
        public int digitStep;
        private float cycle;
        private float Function(float x)
        {
            main = Mathf.Sin(-cycle + x*Mathf.PI/8);
            absMain = Mathf.Abs(main);
            main = Mathf.Sign(main) * Mathf.Pow(absMain, power);
            if (digitStep != 0) main = Mathf.Round(main * digitStep) / digitStep;
            return main;
        }

        private float main, absMain, pos, directionY, directionX, time, scaleX, scaleY;
        private Vector3 vec;
        private Vector3 Function3D(float i, float step)
        {
            pos = i * step;
            directionY = direction.y * pos/direction.z;
            directionX = direction.x * pos;
            time = i / segments;
            scaleX = scale.x * clumpX.Evaluate(time) * (Function(offsetX + i) + Random.value * noise.x);
            scaleY = scale.y * clumpY.Evaluate(time) * (Function(i) + Random.value * noise.y);
            vec = new Vector3(scaleX + directionX, scaleY + directionY, pos);
            return transform.position + (local ? transform.TransformDirection(vec) : vec);
        }

        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast) DetectIndex = PathCast(radius);
        }

        private float dt, step;
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            dt = GetDelta(timeMode);
            cycle += dt*waveSpeed % Mathf.PI*2;
            step = direction.z / segments;
            for (var i = 0; i <= segments; i++) PathPoints.Add(Function3D(i, step));
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send ray based on mathematical functions that use the Sinus kernel to retrieve hit information."+HAccurate+HDirectional+HPathRay+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            if (IsSceneView && !IsPlaying) cycle = Time.realtimeSinceStartup*waveSpeed % Mathf.PI*2;
            EditorUpdate();
            FullPathDraw(radius,  true);
            if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                PropertyMaxIntField(_so.FindProperty(nameof(segments)), CSegments.ToContent(TSegments), 1);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(waveSpeed)),
                    CWaveSpeed.ToContent(CWaveSpeed));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(scale)),
                    CScale.ToContent(CScale));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpX)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpX.ToContent(CClumpX));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpY)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpY.ToContent(CClumpY));
                PropertySliderField(_so.FindProperty(nameof(offsetX)), 0f, Mathf.PI * 2f, "OffsetX".ToContent("Wave OffsetX in unit of \"Radians\"."));
                PropertySliderField(_so.FindProperty(nameof(power)), 0f, 6f, CPower.ToContent(CPower));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(noise)),
                    CNoise.ToContent(CNoise));
                PropertySliderField(_so.FindProperty(nameof(digitStep)), 0, 3, CDigitStep.ToContent(CDigitStep), null);
                RadiusField(_so);
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            }

            if (hasGeneral) PathRayGeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif
    }
}
