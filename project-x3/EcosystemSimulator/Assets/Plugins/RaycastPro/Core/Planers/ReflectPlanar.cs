using UnityEditor;

namespace RaycastPro.Planers
{
    using RaySensors;
    using UnityEngine;

    [AddComponentMenu("RaycastPro/Planers/" + nameof(ReflectPlanar))]
    public sealed class ReflectPlanar : Planar
    {
        public override void GetForward(RaySensor raySensor, out Vector3 forward)
        {
            switch (baseDirection)
            {
                case DirectionOutput.NegativeHitNormal: forward = -raySensor.hit.normal; return;
                case DirectionOutput.HitDirection: forward = raySensor.HitDirection; return;
                case DirectionOutput.SensorLocal: forward = raySensor.LocalDirection.normalized; return;
            }
            forward = transform.forward;
        }
        
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "The reflection of the Planar Sensitive Ray from the Hit Point."+HDependent+HAccurate;
#pragma warning restore CS0414


        internal override void OnGizmos() => DrawPlanar();
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);
        }
#endif
        
        private Vector3 forward, look;
        private RaySensor clone;

        internal override void OnReceiveRay(RaySensor sensor)
        {
            clone = sensor.cloneRaySensor;
            if (!clone) return;
            if (clone.liner) clone.liner.enabled = sensor.liner.enabled;
            
            GetForward(sensor, out forward);
            clone.transform.forward = Vector3.Reflect(sensor.TipDirection, forward).normalized;
            clone.transform.position = sensor.hit.point - forward * offset;
            ApplyLengthControl(sensor);
        }
    }
}