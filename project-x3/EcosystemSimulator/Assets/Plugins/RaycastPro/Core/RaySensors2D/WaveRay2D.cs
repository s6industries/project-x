/// FIX Allocating
namespace RaycastPro.RaySensors2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(WaveRay2D))]
    public sealed class WaveRay2D : PathRay2D, IRadius
    {
        public TimeMode timeMode = TimeMode.DeltaTime;
        public int segments = 8;
        public float waveSpeed = 1f;
        public float power = 1;
        public float noise;
        public float scale = 1f;
        [SerializeField] public float radius = 0f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public AnimationCurve clump = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public int digitStep;
        private float cycle;
        private float Function(float x)
        {
            main = Mathf.Sin(-cycle + x * Mathf.PI / 8);
            absMain = Mathf.Abs(main);
            main = Mathf.Sign(main) * Mathf.Pow(absMain, power);
            if (digitStep != 0) main = Mathf.Round(main * digitStep) / digitStep;
            return main;
        }

        private float main, absMain, pos, directionY, ScaleY;
        private Vector3 vector3, _vec;
        private Vector3 Function3D(float i, float step)
        {
            pos = i * step;
            directionY = direction.y * pos / direction.x;
            ScaleY = scale * clump.Evaluate(i / segments) * (Function(i) + Random.value * noise);
            vector3 = new Vector3(pos, ScaleY + directionY);
            _vec = local ? transform.TransformDirection(vector3) : vector3;
            return transform.position + _vec;
        }
        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                DetectIndex = PathCast(out hit, radius);
                isDetect = FilterCheck(hit);
            }
        }
        private float dt, step;
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            dt = GetDelta(timeMode);
            cycle += dt * waveSpeed % Mathf.PI * 2;
            step = direction.x / segments;
            for (var i = 0; i <= segments; i++) PathPoints.Add(Function3D(i, step));
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send 2D Ray based on mathematical functions that use the Sinus kernel to retrieve hit information."+HAccurate+HDirectional+HPathRay+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            if (IsSceneView && !IsPlaying) cycle = Time.realtimeSinceStartup*waveSpeed % Mathf.PI*2;
            EditorUpdate();
            FullPathDraw(radius, true);
            DrawNormal2D(hit, z);
            DrawNormalFilter();
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                PropertyMaxIntField(_so.FindProperty(nameof(segments)), CSegments.ToContent(TSegments), 1);
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(waveSpeed)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(scale)));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clump)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClump.ToContent(CClump));
                PropertySliderField(_so.FindProperty(nameof(power)), 0f, 6f, CPower.ToContent(CPower));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(noise)),
                    CNoise.ToContent(CNoise));
                PropertySliderField(_so.FindProperty(nameof(digitStep)), 0, 3, CDigitStep.ToContent(CDigitStep), null);
                RadiusField(_so);
                PropertyTimeModeField(_so.FindProperty(nameof(timeMode)));
            }

            if (hasGeneral) PathRayGeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
#endif
    }
}