namespace RaycastPro.Bullets
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Bullets/" + nameof(TrackerBullet))]
    public sealed class TrackerBullet : Bullet
    {
        public Transform target;
        public Rigidbody body;
        
        public Vector3 targetPoint;

        public float force = 10f;
        public float drag = 10;
        public Vector3 trackOffset;
        
        public float distanceThreshold = .2f;

        public float turnSharpness = 15;
        public enum TrackType
        {
            PositionLerp,
            RotationLerp,
        }
        public TrackType trackType = TrackType.PositionLerp;

        protected override void OnCast()
        {
            if (raySource)
            {
                transform.position = raySource.Base;
                transform.rotation = Quaternion.LookRotation(raySource.LocalDirection, transform.up);
            }

#if UNITY_EDITOR
            if (!target)
            {
                RCProEditor.Log($"<color=#4AFF98>{caster.name}</color> missing <color=#FF1E21>TrackTarget</color> transform!");
            }
#endif

            if (target)
            {
                targetPoint = target.position;
            }
            else
            {
                targetPoint = raySource.TipTarget;
            }
            
            currentForce = force;
            _t = transform;
        }

        private Transform _t;
        private float _dis, _dt, currentForce;
        private Vector3 _dir;
        private Quaternion look;

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
            
            if (collisionRay)  CollisionRun(_dt);
            
            switch (trackType)
            {
                case TrackType.PositionLerp:
                    var lerp = Vector3.Lerp(_t.position, targetPoint, _dt * speed);
                    if (body)
                    {
                        body.MovePosition(lerp);
                    }
                    else
                    {
                        _t.position = lerp;
                    }
                    break;
                case TrackType.RotationLerp:
                    
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_dir, transform.up), 1 - Mathf.Exp(-turnSharpness * _dt));
                    currentForce = Mathf.Lerp(currentForce, 0, 1 - Mathf.Exp(-drag * _dt));
                    if (body)
                    {
                        body.AddForce(transform.forward * (currentForce * _dt));
                    }
                    else
                    {
                        _t.position += transform.forward * (currentForce * _dt);
                    }

                    break;
            }
        }
        
        protected override void CollisionBehaviour()
        {
            transform.position = collisionRay.cloneRaySensor.Base;
            transform.forward = collisionRay.cloneRaySensor.Direction.normalized;
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
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(body)));
                
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