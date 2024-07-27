namespace RaycastPro.RaySensors
{
    using UnityEngine;
    using System;
    using System.Linq;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public class HybridRay : PathRay, IRadius
    {
        [SerializeField] private RaySensor[] raySensors = Array.Empty<RaySensor>();
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
        private int i, l;

        protected override void UpdatePath()
        {
            _t = transform;
            PathPoints.Clear();

            for (i = 0; i < raySensors.Length; i++)
            {
                var _r = raySensors[i];
                if (!_r) continue;
                if (sequenceOnTip && i > 0) _r.transform.position = raySensors[i-1].Tip;
                if (_r is PathRay pathRay)
                {
                    PathPoints.AddRange(pathRay.PathPoints);
                }
                else
                {
                    PathPoints.Add(_r.transform.position);
                    PathPoints.Add(_r.Tip);;
                }
                
                l = i;
            }
        }

        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif
            UpdatePath();

            if (pathCast) DetectIndex = PathCast(radius);
            else if (sequenceCast)
            {
                hit = new RaycastHit();
                foreach (var raySensor in raySensors)
                {
                    if (!raySensor) continue;
                    if (!raySensor.enabled)
                    {
                        if (raySensor.Cast())
                        {
                            hit = raySensor.hit;
                            break;
                        }
                    }
                    else if (raySensor.Performed)
                    {
                        hit = raySensor.hit;
                        break;
                    }
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
                FullPathDraw(radius, true, true);
                DrawPath(PathPoints, radius: radius, coneCap: true, dotted: true, color: HelperColor);
            }
            else
            {
                foreach (var _rs in raySensors)
                {
                    if (_rs.gizmosUpdate != GizmosMode.Fix) _rs.OnGizmos();
                }
            }

            if (hit.transform)
            {
                GizmoColor = DetectColor;
                DrawNormal(hit, doubleDisc: true, color: DetectColor);
                DrawCross(hit.point, hit.normal);
                Handles.DrawWireDisc(hit.point, hit.normal, DotSize * .4f);
            }
        }
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
                    CSequenceOnTip.ToContent(TSequenceOnTip));
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
