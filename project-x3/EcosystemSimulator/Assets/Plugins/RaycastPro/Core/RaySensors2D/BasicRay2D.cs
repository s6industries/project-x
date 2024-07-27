namespace RaycastPro.RaySensors2D
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;
    [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(BasicRay2D))]
    public sealed class BasicRay2D : RaySensor2D
    {
        protected override void OnCast()
        {
            hit = Physics2D.Raycast(transform.position, Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
            isDetect = FilterCheck(hit);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Emit single line 2DRay in the specified direction and return the Hit information."+HAccurate+HDirectional;
#pragma warning restore CS0414

        private Vector3 p1, p2;
        internal override void OnGizmos()
        {
            EditorUpdate();

            p1 = transform.position;
            p2 = transform.position + Direction.ToDepth();
            if (IsManuelMode)
            {
                DrawLineZTest(p1, p2, false, DefaultColor);
            }
            else
            {
                DrawBlockLine(p1, p2, hit, z, 1);
            }
            DrawNormal2D(hit, z);
            DrawDepthLine(p1, p2);
            DrawNormalFilter();
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true, bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain) DirectionField(_so);
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
#endif
        public override Vector3 Tip => transform.position + Direction.ToDepth();

        public override Vector3 RawTip => Tip;
        
        public override float RayLength => direction.magnitude;
        public override Vector3 Base => transform.position;
    }
}