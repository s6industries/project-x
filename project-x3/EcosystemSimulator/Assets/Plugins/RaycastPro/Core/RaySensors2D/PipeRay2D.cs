

namespace RaycastPro.RaySensors2D
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    using UnityEngine;
    
    [HelpURL("https://www.youtube.com/watch?v=D-C4J_zpkbQ")]
    [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(PipeRay2D))]
    public sealed class PipeRay2D : RaySensor2D, IRadius
    {
        [SerializeField] private float radius = .4f;
        
        [SerializeField] private float height;

        public float Height
        {
            get => height;
            set => height = Mathf.Max(0, value);
        }
        
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }

        private float angle;

        protected override void OnCast()
        {
            if (height > 0)
            {
                angle = local ? -Vector2.SignedAngle(transform.right, Vector2.right) : 0;
                hit = Physics2D.CapsuleCast(transform.position, new Vector2(Mathf.Max(radius*2, 0.001f), height+radius*2), CapsuleDirection2D.Vertical, angle, Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
            }
            else
            {
                hit = Physics2D.CircleCast(transform.position, radius, Direction, direction.magnitude, detectLayer.value, MinDepth, MaxDepth);
            }

            isDetect = FilterCheck(hit);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Emit a 2D pipe shape ray in the specified direction and return the Hit information."+HAccurate+HDirectional+HIRadius;
#pragma warning restore CS0414

        private Vector3 p1, p2;
        internal override void OnGizmos()
        {
            EditorUpdate();
            p1 = Base;
            p2 = Tip;
            DrawDepthLine(p1, p2);
            Handles.color = Performed ? DetectColor : DefaultColor;
            DrawCircleRay(p1, direction,Direction, local, radius, height);
            if (!hit) return;
            DrawNormal(hit.point.ToDepth(z), hit.normal);
            DrawNormalFilter();
        }
        
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                RadiusField(_so);
                HeightField(_so);
            }

            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) HitInformationField();
        }
#endif
        public override Vector3 Tip => transform.position + (Direction + Direction.normalized * radius).ToDepth();

        public override Vector3 RawTip => transform.position + Direction.ToDepth();

        public override float RayLength => direction.magnitude + radius;
        public override Vector3 Base => transform.position;
    }
}
