/// FIX Allocating
namespace RaycastPro.RaySensors2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(CurveRay2D))]
    public sealed class CurveRay2D : PathRay2D, IRadius
    {
        public int segments = 16;
        [SerializeField] public float radius = 0f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public AnimationCurve clumpX = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve clumpY = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                DetectIndex = PathCast(out hit, radius);
                isDetect = FilterCheck(hit);
            }
        }
        private float step;
        
        private Vector3 curve;

        protected override void UpdatePath()
        {
            PathPoints.Clear();
            step = direction.x / segments;
            for (int i = 0; i <= segments; i++)
            {
                step = (float) i / segments;
                curve.x = clumpX.Evaluate(step) * direction.x;
                curve.y = clumpY.Evaluate(step) * direction.y;
                PathPoints.Add(transform.position + (local ? transform.TransformDirection(curve) : curve));
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a 2D Ray based on curves and return the hit information."+HAccurate+HDirectional+HPathRay+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {

            EditorUpdate();
            FullPathDraw(radius, true);
            if (hit) DrawNormal(hit.point.ToDepth(z), hit.normal, hit.transform.name);
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
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpX)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpX.ToContent(CClumpX));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpY)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpY.ToContent(CClumpY));
                RadiusField(_so);
            }

            if (hasGeneral) PathRayGeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
#endif
    }
}