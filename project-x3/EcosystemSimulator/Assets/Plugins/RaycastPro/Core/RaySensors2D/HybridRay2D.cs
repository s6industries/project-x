namespace RaycastPro.RaySensors2D
{
    using System;
    using System.Linq;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Rey Sensors/"+nameof(HybridRay2D))]
    public sealed class HybridRay2D : PathRay2D, IRadius
    {
        [SerializeField] private RaySensor2D[] raySensors = Array.Empty<RaySensor2D>();
        public bool sequenceOnTip = false;
        public bool sequenceCast;
        [SerializeField] private float radius = .4f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public override Vector3 Tip => raySensors.Last().Tip;
        public override float RayLength
        {
            get
            {
                var length = 0f;
                foreach (var raySensor in raySensors) if (raySensor) length += raySensor.RayLength;
                return length;
            }
        }
        public override Vector3 Base => raySensors.First().Base;

        private Transform _t;
        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif
            UpdatePath();
            if (pathCast) DetectIndex = PathCast(out hit, radius);
            else  if (sequenceCast)
            {
                hit = default;
                foreach (var raySensor in raySensors)
                {
                    if (!raySensor || !raySensor.Performed) continue;
                    hit = raySensor.Hit;
                    break;
                }
            }
            
            isDetect = FilterCheck(hit);

        }

        protected override void UpdatePath()
        {
            _t = transform;
            PathPoints.Clear();
            PathPoints.Add(transform.position);
            for (var i = 0; i < raySensors.Length; i++)
            {
                var _r = raySensors[i];
                if (!_r) continue;
                if (sequenceOnTip && i > 0) _r.transform.position = raySensors[i-1].Tip;
                if (_r is PathRay2D pathRay)
                {
                    PathPoints.AddRange(pathRay.PathPoints);
                }
                else
                {
                    PathPoints.Add(_r.transform.position);
                    PathPoints.Add(_r.Tip);
                }
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "It has the ability to stack and convert rays into a path." + HDependent + HPathRay;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            if (pathCast)
            {
                FullPathDraw();
                DrawPath2D(PathPoints.ToDepth(z), hit.point, radius: radius, coneCap: true, dotted: true, _color: HelperColor);
            }
            else
            {
                foreach (var _rs in raySensors)
                {
                    if (_rs.gizmosUpdate != GizmosMode.Fix) _rs.OnGizmos();
                }
            }

            if (hit)
            {
                DrawNormalFilter();
                GizmoColor = DetectColor;
                hitDepth = hit.point.ToDepth(z);
                DrawCross(hitDepth, Vector3.forward);
                
                DrawNormal(hitDepth+hit.normal.ToDepth()*DotSize, hit.normal);
            }
            DrawNormalFilter();
        }

        private Vector3 hitDepth;
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginVerticalBox();
                RCProEditor.PropertyArrayField(_so.FindProperty(nameof(raySensors)),
                    CRaySensor.ToContent(TRaySensor), i => $"RaySensors {i+1}".ToContent($"Index {i}"));
                EndVertical();
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(sequenceOnTip)),
                    "Sequence On Tip".ToContent());
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(sequenceCast)));
                GUI.enabled = pathCast;
                RadiusField(_so);
                GUI.enabled = true;
            }
            
            if (hasGeneral) PathRayGeneralField(_so);

            if (hasEvents) EventField(_so);
            
            if (hasInfo) InformationField();
        }
#endif
    }
}