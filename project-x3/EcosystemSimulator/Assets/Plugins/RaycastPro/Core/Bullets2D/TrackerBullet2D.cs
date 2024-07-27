namespace RaycastPro.Bullets2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif


    [AddComponentMenu("RaycastPro/Bullets/" + nameof(TrackerBullet2D))]
    public sealed class TrackerBullet2D : Bullet2D
    {
        public Transform target;
        
        public Vector3 targetPoint;

        public float distanceThreshold = .2f;
        public float drag = 2;
        public Vector3 trackOffset;
        public float turnSharpness = 15;
        
        public float force = 10;
        
        public enum TrackType
        {
            PositionLerp,
            RotationLerp,
        }

        public TrackType trackType = TrackType.PositionLerp;
        
        [SerializeField]
        private AxisRun axisRun = new AxisRun();

        protected override void OnCast()
        {
            transform.position = raySource.Base;
            transform.right = (target.position - transform.position).normalized;
            
            targetPoint = target.position;
            currentForce = force;
        }

        private Transform _t;
        private float _dis, _dt, currentForce;
        private Vector2 _dir;

        internal override void RuntimeUpdate()
        {
            _dt = GetDelta(timeMode);
            UpdateLifeProcess(_dt);
            
            targetPoint = target ? target.position + trackOffset : _t.position;
            _dis = Vector3.Distance(_t.position, targetPoint);
            if (currentForce <= .1f)
            {
                OnEndCast(caster);
                return;
            }
            if (target && _dis <= distanceThreshold)
            {
                OnEndCast(caster);
                return;
            }
            _dt = GetDelta(timeMode);
            _dir = targetPoint - _t.position;
            
            switch (trackType)
            {
                case TrackType.PositionLerp:
                    var lerp = Vector3.Lerp(_t.position.ToDepth(Z), targetPoint.ToDepth(Z), _dt * speed);
                    _t.position = lerp;
                    break;
                case TrackType.RotationLerp:
                    currentForce = Mathf.Lerp(currentForce, 0, 1 - Mathf.Exp(-drag * _dt));
                    _t.position += _dir.normalized.ToDepth() * (currentForce * _dt);
                    break;
            }
            if (axisRun.syncAxis) axisRun.SyncAxis(transform, _dir);
            if (collisionRay) CollisionRun(_dt);
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "After being fired by the caster, it follows the target." + HAccurate + HDependent;
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginVerticalBox();
                PropertyEnumField(_so.FindProperty(nameof(trackType)), 2, "Track Type".ToContent(), new GUIContent[]
                {
                    CPositionLerp.ToContent(TPositionLerp),
                    CRotationLerp.ToContent(TRotationLerp),
                });
                
                if (trackType == TrackType.RotationLerp)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(force)));
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(turnSharpness)));
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(drag)));
                }
                else
                {
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(speed)));
                }
                EndVertical();


                axisRun.EditorPanel(_so.FindProperty(nameof(axisRun)));
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(distanceThreshold)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(trackOffset)));
            }
            
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);
            
            if (hasInfo) InformationField();
        }
#endif
    }
}