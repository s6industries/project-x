namespace RaycastPro.RaySensors
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(TargetRay))]
    public sealed class TargetRay : RaySensor, IRadius
    {
        [Tooltip("The main target to which the line cast is emitted.")]
        public Transform target;

        [SerializeField] private float radius = .4f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }

        public WeightType weightType;

        public float weight = 1;
        public float distance = 1;
        public float offset = 0;
        protected override void OnCast()
        {
            if  (radius > 0)
            {
                var dir = TipDirection;
                Physics.SphereCast(transform.position, radius, dir, out hit, dir.magnitude,
                    detectLayer.value, triggerInteraction);
            }
            else
            {
                Physics.Linecast(transform.position, Tip, out hit, detectLayer.value,
                    triggerInteraction);
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Set a line from the origin to the target point and hit Info" + HAccurate + HIRadius + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();

            if (!target) return;

            var position = transform.position;

            if (radius > 0)
            {
                Gizmos.DrawWireSphere(target.position, radius);

                DrawCapsuleLine(position, target.position, radius, dotted: true);

                Handles.color = Gizmos.color = Performed ? DetectColor : DefaultColor;

                var p2 = position + (target.position - position).normalized * TipLength;

                DrawCapsuleLine(position, p2, radius);

                Gizmos.DrawWireSphere(position, radius);

                Gizmos.DrawWireSphere(p2, radius);
            }
            else
            {
                Handles.DrawDottedLine(position, target.position, StepSizeLine);

                Handles.color = Performed ? DetectColor : DefaultColor;

                DrawCapLine(position, Tip);
            }

            if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(target)));

                RadiusField(_so);

                WeightField(_so.FindProperty(nameof(weightType)),
                    _so.FindProperty(nameof(weight)),
                    _so.FindProperty(nameof(distance)),
                    _so.FindProperty(nameof(offset)));
            }

            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) InformationField();
        }

#endif
        public override Vector3 Tip
        {
            get
            {
                if (!target) return transform.position;

                Vector3 weightedPos;
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
                    default:
                        weightedPos = target.position;
                        break;
                }

                return weightedPos;
            }
        }

        public override Vector3 RawTip => Tip;

        public override float RayLength => TipLength;
        public override Vector3 Base => transform.position;
    }
}