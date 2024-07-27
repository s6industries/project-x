namespace RaycastPro.Planers2D
{
    using System.Collections.Generic;
    using RaySensors2D;
    using UnityEngine;

#if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Planers/" + nameof(DividePlanar2D))]
    public class DividePlanar2D : Planar2D
    {
        public float arcAngle = 30f;

        public int count = 5;

        public readonly Dictionary<RaySensor2D, List<RaySensor2D>> CloneProfile =
            new Dictionary<RaySensor2D, List<RaySensor2D>>();

        private Vector3 point, forward;
        private int cloneCount;
        private float step;

        private RaySensor2D _tRay;
        public override void OnReceiveRay(RaySensor2D sensor)
        {
            if (!sensor.cloneRaySensor) return;

            clone = sensor.cloneRaySensor;
            forward = GetForward(sensor, transform.right);
            point = sensor.HitPointZ;
            clone.transform.position = point;
            clone.transform.right = forward;
            clone.transform.position += forward * offset;
            ApplyLengthControl(sensor);
            if (!CloneProfile.ContainsKey(clone)) return;

            cloneCount = CloneProfile[clone].Count;
            step = arcAngle / cloneCount;

            for (var index = 0; index < CloneProfile[clone].Count; index++)
            {
                _tRay = CloneProfile[clone][index];
                _tRay.transform.right = Quaternion.AngleAxis(step * index - arcAngle / 2, Vector3.forward) * forward;
            }
        }

        private RaySensor2D clone;
        private readonly List<RaySensor2D> clones = new List<RaySensor2D>();

        public override void OnBeginReceiveRay(RaySensor2D sensor)
        {
            base.OnBeginReceiveRay(sensor);
            clone = sensor.cloneRaySensor;
            ApplyLengthControl(sensor);
            clones.Clear();
            for (var i = 0; i < count; i++) clones.Add(Instantiate(clone));
            CloneProfile.Add(clone, clones);
            foreach (var c in clones)
            {
#if UNITY_EDITOR
                RenameClone(c, "C_Divide");
#endif
                c.baseRaySensor = sensor;
                if (c is PathRay2D _pR)
                {
                    _pR.pathCast &= clonePathCast;
                }
                c.transform.SetParent(clone.transform, true);
                c.transform.localPosition = Vector3.zero;
            }

            OnReceiveRay(sensor);
            Destroy(clone.liner);
            clone.enabled = false;

#if UNITY_EDITOR
            clone.gizmosUpdate = GizmosMode.Off;
#endif
        }
        public override bool OnEndReceiveRay(RaySensor2D sensor)
        {
            if (sensor.cloneRaySensor)
            {
                Destroy(sensor.cloneRaySensor.gameObject);
            }

            return true;
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Division of Planar Sensitive 2D Ray in angles and count entered.";
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                PropertySliderField(_so.FindProperty(nameof(arcAngle)), 0f, 180f, CArcAngle.ToContent());
                PropertyMaxIntField(_so.FindProperty(nameof(count)), CCount.ToContent(), 1);
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo)
            {
                InformationField();
            }
        }
#endif
    }
}