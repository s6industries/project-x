namespace RaycastPro.Sensor
{
    using System.Collections.Generic;
    using RaySensors;
    using RaySensors2D;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Utility/" + nameof(RayStamp))]
    public sealed class RayStamp : BaseUtility
    {
        public RaySensor raySensor;
        public RaySensor2D raySensor2D;

        public Transform stamp;
        public override bool Performed { get; protected set; }

        public float position;
        public bool fixOnHit;
        public float offset;
        public bool AutoHide;
        public WeightType weightType = WeightType.Distance;

        public float weight;
        public float distance;

        [SerializeField] internal AxisRun syncStamp = new AxisRun();

        private Vector3 _direction;
        
        protected override void OnCast()
        {
            if (raySensor)
            {
                if (AutoHide) stamp.gameObject.SetActive(raySensor.Performed);
                UpdateStamp();
            }
            else if (raySensor2D)
            {
                if (AutoHide) stamp.gameObject.SetActive(raySensor2D.Performed);
                UpdateStamp(); 
            }
        }

        private List<Vector3> rayPath;
        private float edgeLength;

        private void Reset()
        {
            if (TryGetComponent(out raySensor))
            { }
            else TryGetComponent(out raySensor2D);
        }

        private void ApplyPosition(float lengthPos)
        {
            for (var i = 1; i < rayPath.Count; i++)
            {
                edgeLength = rayPath.GetEdgeLength(i);
                if (lengthPos <= edgeLength)
                {
                    stamp.position = Vector3.Lerp(rayPath[i - 1], rayPath[i], lengthPos / edgeLength);
                    _direction = rayPath[i] - rayPath[i - 1];
                    break;
                }
                lengthPos -= edgeLength;
            }
        }

        private float _tDistance, _tLength;
        private float TrueLength => (fixOnHit && raySensor.Performed) ? raySensor.HitLength : rayPath.GetPathLength();
        private float TrueLength2D => (fixOnHit && raySensor2D.Performed) ? raySensor2D.HitLength : rayPath.GetPathLength();
        public void UpdateStamp()
        {
            if (!stamp) return;
            
            if (raySensor)
            {
                if (raySensor.cloneRaySensor && raySensor.cloneRaySensor.enabled) return;
                raySensor.GetPath(ref rayPath);
                _tLength = TrueLength;
                _direction = stamp.forward;
            }
            else if (raySensor2D)
            {
                if (raySensor2D.cloneRaySensor && raySensor2D.cloneRaySensor.enabled) return;
                raySensor2D.GetPath(ref rayPath);
                _tLength = TrueLength2D;
                _direction = stamp.forward;
            }
            
            switch (weightType)
            {
                case WeightType.Clamp:
                    _tDistance = weight * _tLength;
                    break;
                case WeightType.Distance:
                    _tDistance = Mathf.Min(distance, _tLength);
                    break;
                case WeightType.Offset:
                {
                    _tDistance = _tLength - offset;
                }
                    break;
            }
            
            ApplyPosition(_tDistance);
            
            if (syncStamp.syncAxis)
            {
                syncStamp.SyncAxis(stamp, _direction);
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "A Transform handler on ray body with controlling option." + HUtility + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            if (IsSceneView && !IsPlaying) OnCast();
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                if (!raySensor2D)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(raySensor)));
                }

                if (!raySensor)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(raySensor2D)));
                }
                
                
                WeightField(_so.FindProperty(nameof(weightType)),
                    _so.FindProperty(nameof(weight)),
                    _so.FindProperty(nameof(distance)),
                    _so.FindProperty(nameof(offset)));
                StampField(_so);
            }
            if (hasGeneral)
            {
                BaseField(_so);
            }
        }
        
        private void StampField(SerializedObject _so)
        {
            if (stamp) BeginVerticalBox();
            EditorGUILayout.ObjectField(_so.FindProperty(nameof(stamp)), CStamp.ToContent(TStamp));
            if (!stamp) return;
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(fixOnHit)),
                CStampOnHit.ToContent(TStampOnHit));
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(AutoHide)),
                CStampAutoHide.ToContent(TStampAutoHide));
            syncStamp.EditorPanel(_so.FindProperty(nameof(syncStamp)));
            EndVertical();
        }
#endif
    }
}