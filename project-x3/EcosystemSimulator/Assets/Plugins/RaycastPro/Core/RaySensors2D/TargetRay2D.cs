

namespace RaycastPro.RaySensors2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif


    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(TargetRay2D))]
    public sealed class TargetRay2D : RaySensor2D, IRadius
    {
        public Transform target;

        public float radius = .4f;
        
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }


        public WeightType weightType;

        public float weight;
        public float distance;
        public float offset;
        protected override void OnCast()
        {
            if  (radius > 0)
            {
                var dir = TipDirection;
                hit = Physics2D.CircleCast(Position2D, radius, dir, dir.magnitude, detectLayer.value, MinDepth, MaxDepth);
            }
            else
            {
                hit = Physics2D.Linecast(Position2D, Tip, detectLayer.value, MinDepth, MaxDepth);
            }

            isDetect = FilterCheck(Hit);
        }

#if UNITY_EDITOR
        
#pragma warning disable CS0414
        private static string Info = "Set a line from the origin to the target point and determine the block info" +HAccurate + HIRadius + HDependent;
#pragma warning restore CS0414

        private Vector3 p1, p2, _targetPosition;
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            if (!target) return;
            p1 = transform.position;
            _targetPosition = target.position.ToDepth(z);
            p2 = Tip.ToDepth(z);
            
            if (radius > 0)
            {
                Handles.DrawWireDisc(_targetPosition, Vector3.forward, radius);
                DrawCircleLine(p1, _targetPosition, radius, true);
                GizmoColor = Performed ? DetectColor : DefaultColor;
                DrawCircleLine(p1, p2, radius);
                Handles.DrawWireDisc(p1, Vector3.forward, radius);
                Handles.DrawWireDisc(p2.ToDepth(z), Vector3.forward, radius);
            }
            else
            {
                Handles.DrawDottedLine(p1, _targetPosition, StepSizeLine);
                GizmoColor = Performed ? DetectColor : DefaultColor;
                DrawCapLine(p1, p2);
            }
            DrawDepthLine(p1, p2);
            
            if (Hit) DrawNormal2D(Hit, z);
            DrawNormalFilter();
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                RCProEditor.TypeField(CTarget, ref target);
                RadiusField(_so);
                WeightField(_so.FindProperty(nameof(weightType)),
                    _so.FindProperty(nameof(weight)),
                    _so.FindProperty(nameof(distance)),
                    _so.FindProperty(nameof(offset)));
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) HitInformationField();
        }
#endif
        public override Vector3 Tip
        {
            get
            {
                if (!target) return transform.position;

                var weightedPos = Vector2.zero;
                var position = transform.position;

                switch (weightType)
                {
                    case WeightType.Clamp:
                        weightedPos = Vector3.Lerp(position, target.position, weight);
                        break;
                    case WeightType.Distance:
                        weightedPos = position + (target.position - position).normalized * distance;
                        break;
                    case WeightType.Offset:
                        weightedPos = target.position + (target.position - position).normalized * offset;
                        break;
                }

                return weightedPos.ToDepth(z);
            }
        }

        public override Vector3 RawTip => Tip;

        public override float RayLength => TipLength;
        public override Vector3 Base => transform.position;
    }
}