namespace RaycastPro.Planers2D
{
    using RaySensors2D;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Planers/" + nameof(RefractPlanar2D))]
    public sealed class RefractPlanar2D : Planar2D
    {
        public float refractAngle;

        private RaySensor2D clone;
        private Vector3 forward, point;
        public override void OnReceiveRay(RaySensor2D sensor)
        {
            clone = sensor.cloneRaySensor;
            if (!clone) return;
            forward = GetForward(sensor, transform.right);
            point = sensor.HitPointZ + forward * offset;
            clone.transform.position = point.ToDepth(sensor.z);
            clone.transform.right = Quaternion.AngleAxis(refractAngle, Vector3.forward) *forward;
            ApplyLengthControl(sensor);
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Planar Sensitive 2D Ray Refraction Based on refract Direction.";
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain) PropertySliderField(_so.FindProperty(nameof(refractAngle)), 0f, 360f, "Refract Angle".ToContent());
            
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif
    }
}