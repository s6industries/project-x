
using UnityEngine.TestTools;

namespace RaycastPro.RaySensors
{
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    using UnityEngine;
    using Random = UnityEngine.Random;
    
   [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(CurveRay))]
  
    public sealed class CurveRay : PathRay, IRadius
    {
        public int segments = 8;
        [SerializeField] private float radius;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        public AnimationCurve clumpY = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve clumpX = AnimationCurve.EaseInOut(0, 0, 1, 0);
        public AnimationCurve clumpZ = AnimationCurve.Linear(0, 0, 1, 1);
        

        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast) DetectIndex = PathCast(radius);
        }

        private float step;
        private Vector3 curve;

        
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            for (int i = 0; i <= segments; i++)
            {
                step = (float) i / segments;
                curve.x = clumpX.Evaluate(step) * direction.x;
                curve.y = clumpY.Evaluate(step) * direction.y;
                curve.z = clumpZ.Evaluate(step) * direction.z;
                PathPoints.Add(transform.position + (local ? transform.TransformDirection(curve) : curve));
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a ray based on curves and return the hit information."+HAccurate+HDirectional+HPathRay+HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
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
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpX)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpX.ToContent(CClumpX));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpY)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpY.ToContent(CClumpY));
                EditorGUILayout.CurveField(_so.FindProperty(nameof(clumpZ)), RCProEditor.Aqua, new Rect(0, 0, 1, 1), CClumpZ.ToContent(CClumpZ));
                RadiusField(_so);
            }

            if (hasGeneral) PathRayGeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif
    }
}
