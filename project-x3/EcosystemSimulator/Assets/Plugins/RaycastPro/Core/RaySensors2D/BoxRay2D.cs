using UnityEngine.UIElements;

namespace RaycastPro.RaySensors2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(BoxRay2D))]
    public sealed class BoxRay2D : RaySensor2D
    {
        public Vector2 size = new Vector2(.1f, .4f);

        private float angle;
        protected override void OnCast()
        {
            angle = local ? -Vector2.Angle(transform.right.To2D(), Vector2.right) : 0;
            hit = Physics2D.BoxCast(transform.position, size, angle,
                Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
            isDetect = FilterCheck(hit);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Emit a 2D Box Ray in the specified direction and return the Hit information."+HDirectional;
#pragma warning restore CS0414

        private Vector3 p1, p2;
        internal override void OnGizmos()
        {
            EditorUpdate();
            p1 = Base;
            p2 = Tip;
            DrawDepthLine(p1, p2);
            GizmoColor = Performed ? DetectColor : DefaultColor;
            DrawBoxRay(transform, transform.position, direction, size, z, local);
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
                var prop = _so.FindProperty(nameof(size));
                EditorGUILayout.PropertyField(prop, "Size".ToContent("Size"));
                prop.vector2Value = new Vector2(Mathf.Max(0, prop.vector2Value.x), Mathf.Max(0, prop.vector2Value.y));
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
#endif
        public override Vector3 Tip => transform.position + (Direction + Direction.normalized * (size.x)).ToDepth();

        public override Vector3 RawTip => transform.position + Direction.ToDepth();
        public override float RayLength => direction.magnitude + size.x;
        public override Vector3 Base => transform.position;
    }
}