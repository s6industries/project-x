using System.Collections.Generic;
using UnityEngine.Rendering;

namespace RaycastPro.RaySensors
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif


    [Serializable]
    public class Data
    {
        public float length = 1f;
        [SerializeField]
        private float influence = 1f;

        public float Influence
        {
            get => influence;
            set => influence = Mathf.Clamp01(value);
        }

#if UNITY_EDITOR
        internal void EditorPanel(SerializedProperty _sp)
        {
            RaycastCore.BeginVerticalBox();
            RaycastCore.PropertySliderField(_sp.FindPropertyRelative(nameof(length)), 0f, 1f, "length".ToContent());
            RaycastCore.PropertySliderField(_sp.FindPropertyRelative(nameof(influence)), 0f, 1f, "influence".ToContent());
            RaycastCore.EndVertical();
        }
#endif
    }
    

    
    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(RadialRay))]
    public sealed class RadialRay : RaySensor, IRadius
    {
        public float arcAngle = 60f;
        public Vector3 ArcStartPoint => Quaternion.AngleAxis(-arcAngle / 2, transform.up) * Direction;
        public Vector3 ArcEndPoint => Quaternion.AngleAxis(arcAngle / 2, transform.up) * Direction;
        
        [SerializeField] private float radius;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        
        [SerializeField]
        private float value;
        public float Value => value;
        
        [SerializeField] private int subdivide = 3;
        public int Subdivide
        {
            get => subdivide;
            set => subdivide = (byte) Mathf.Max(1,value);
        }
        public List<RaycastHit> raycastHits = new List<RaycastHit>();

        public override Vector3 RawTip { get; }
        public override Vector3 Tip => transform.position + Direction;
        public override float RayLength => TipLength;
        public override Vector3 Base => transform.position;

        public int Count => Pow + 1;
        private int Pow => (int) Mathf.Pow(2, subdivide);
        
        private RaycastHit _raycastHit;
        private Vector3 _pos, _angledDir;
        private float total, step;
        private bool condition;
        protected override void OnCast()
        {
#if UNITY_EDITOR
            GizmoGate = null;
#endif
            _pos = transform.position;
            total = Pow;
            step = arcAngle / total;
            hit = default;

            value = 0f;
            raycastHits.Clear();
            
            
            for (var i = 0; i <= total; i++)
            {
                _angledDir =  Quaternion.AngleAxis(step * i, transform.up) * ArcStartPoint;

#if UNITY_EDITOR
                RaycastHit _h;
                if (radius > 0)
                {
                    condition = Physics.SphereCast(_pos, radius,_angledDir, out _h, _angledDir.magnitude, detectLayer.value, triggerInteraction);
                }
                else
                {
                    condition = Physics.Raycast(_pos, _angledDir, out _h, _angledDir.magnitude, detectLayer.value, triggerInteraction);
                }

                if (!hit.transform) hit = _raycastHit;

                
                raycastHits.Add(_raycastHit);

                bool _b = _raycastHit.transform;
                var _tDir = _angledDir;
                ASP = ArcStartPoint;
                GizmoGate += () =>
                {
                    GizmoColor = _b ? DetectColor : DefaultColor;
                    
                    DrawBlockLine(transform.position, transform.position + _tDir, _tDir, radius,true, false, false, _h, ClampedAlphaCharge);
                };
                
                if (!hit.transform)
                {
                    hit = _h;
                }

                if (_h.transform) value += 1f;

                raycastHits.Add(_h);
                if (condition && _h.distance <= hit.distance) hit = _h;
#else
                if (radius > 0)
                {
                   condition = Physics.SphereCast(_pos, radius,_angledDir, out _raycastHit, _angledDir.magnitude, detectLayer.value, triggerInteraction);
                }
                else
                {
                    condition = Physics.Raycast(_pos, _angledDir, out _raycastHit, _angledDir.magnitude, detectLayer.value, triggerInteraction);
                }

                if (!hit.transform) hit = _raycastHit;
                if (_raycastHit.transform) value += 1f;
                raycastHits.Add(_raycastHit);
                if (condition && _raycastHit.distance <= hit.distance) hit = _raycastHit;
#endif

            }

            value /= total+1;
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Radial shape emitter, which detects the nearest point, can collect hit information." + HAccurate + HDirectional + HIRadius;
#pragma warning restore CS0414

        private RaycastHit[] _hits;

        private Vector3 ASP; 

        internal override void OnGizmos()
        {
            EditorUpdate();

            DrawNormal(hit);
            
            Handles.color = (Performed ? DetectColor : HelperColor).Alpha(Mathf.Min(AlphaAmount, .4f));

            DrawZTest(() => Handles.DrawSolidArc(transform.position, transform.up, ASP, arcAngle,
                DirectionLength));
;
            
            if (RCProPanel.ShowLabels)
            {
                _hits = raycastHits.ToArray();
                for (var index = 0; index < _hits.Length; index++)
                {
                    if (_hits[index].transform)
                    {
                        Handles.Label(_hits[index].point, index.ToString());
                    }
                }
            }
        }
        
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                RadiusField(_so);
                PropertySliderField(_so.FindProperty(nameof(arcAngle)), 0f, 360f, CArcAngle.ToContent(CArcAngle));
                PropertySliderField(_so.FindProperty(nameof(subdivide)), 0, RCProPanel.maxSubdivideTime, CSubdivide.ToContent(TSubdivide), _ => {});
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo)
            {
                InformationField(() =>
                {
                    if (!hit.transform) return;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(hit.transform.name);
                    GUILayout.Label(hit.distance.ToString());
                    GUILayout.EndHorizontal();
                    PercentProgressField(value, "Value");
                });
            }
        }
#endif
    }
}