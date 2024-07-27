
namespace RaycastPro.Bullets
{
    using System.Collections.Generic;
    using UnityEngine;
    using RaySensors;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Bullets/" + nameof(LinerBullet))] 
    public sealed class LinerBullet : Bullet, IPath<Vector3>
    {
        public List<Vector3> Path { get; internal set; } = new List<Vector3>();
        
        public float duration = 1;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [SerializeField]
        private Rigidbody rigidBody;
        
        [SerializeField]
        private AxisRun axisRun = new AxisRun();

        private float pathLength;

        [SerializeField] private bool local = true;

        // Cached Variables
        private Vector3 _pos, _dir;
        private float _dt;
        protected override void OnCast() => PathSetup(raySource);

        private void PathSetup(RaySensor raySensor)
        {
            Path = new List<Vector3>();
            do
            {
                if (raySensor is PathRay _pathRay)
                {
                    if (_pathRay.DetectIndex > -1)
                    {
                        for (var i = 0; i <= _pathRay.DetectIndex; i++)
                        {
                            Path.Add(_pathRay.PathPoints[i]);
                        }
                        Path.Add(raySensor.HitPoint);
                    }
                    else
                    {
                        Path.AddRange(local ? new List<Vector3>(_pathRay.PathPoints) : _pathRay.PathPoints);
                    }
                }
                else
                {
                    Path.Add(raySensor.Base);
                    Path.Add(raySensor.TipTarget);
                }
                
                raySensor = raySensor.cloneRaySensor;
                
            } while (raySensor);

            pathLength = Path.GetPathLength();
        }

        internal override void RuntimeUpdate()
        {
            position = Mathf.Clamp01(position);
            float posM;

            if (moveType == MoveType.Curve)
            {
                posM = curve.Evaluate(position) * pathLength;
            }
            else
            {
                posM = position * pathLength;
            }
            _dt = GetDelta(timeMode);
            UpdateLifeProcess(_dt);
                
            switch (moveType)
            {
                case MoveType.Speed:
                    position += _dt * speed / pathLength;
                    break;
                case MoveType.Duration:
                    position += _dt / duration;
                    break;
                case MoveType.Curve:
                    position += _dt / duration;
                    break;
            }
            
            if (position >= 1)
            {
                OnEndCast(caster);
            }
            
            for (var i = 1; i < Path.Count; i++)
            {
                lineDistance = Path.GetEdgeLength(i);
                if (posM <= lineDistance)
                {
                    _pos = Vector3.Lerp(Path[i - 1], Path[i], posM / lineDistance);
                    _dir = Path[i] - Path[i - 1];

                    break;
                }
                posM -= lineDistance;
            }
            if (rigidBody) rigidBody.MovePosition(_pos);
            else transform.position = _pos;
            if (collisionRay) CollisionRun(_dt);
            if (axisRun.syncAxis) axisRun.DampedSyncAxis(transform, _dir, 4);
        }

        private float lineDistance;
        protected override void CollisionBehaviour() { }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "A smart bullet that can recognize the path of the PathRay and move on it." + HAccurate + HDependent;
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(local)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(rigidBody)));
                CastTypeField(
                    _so.FindProperty(nameof(moveType)),
                    _so.FindProperty(nameof(speed)), 
                    _so.FindProperty(nameof(duration)),
                    _so.FindProperty(nameof(curve)));
                axisRun.EditorPanel(_so.FindProperty(nameof(axisRun)));
            }

            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif

    }
}