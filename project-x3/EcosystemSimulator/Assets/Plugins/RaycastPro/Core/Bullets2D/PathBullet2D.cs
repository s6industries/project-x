namespace RaycastPro.Bullets2D
{
    using System.Collections.Generic;
    using UnityEngine;
    using RaySensors2D;

#if UNITY_EDITOR
    using UnityEditor;
#endif


    [AddComponentMenu("RaycastPro/Bullets/" + nameof(PathBullet2D))]
    public sealed class PathBullet2D : Bullet2D, IPath<Vector2>
    {
        public List<Vector2> Path { get; internal set; } = new List<Vector2>();
        
        public float duration = 1;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [SerializeField]
        private AxisRun axisRun = new AxisRun();
        
        [SerializeField]
        private Rigidbody2D rigidBody;
        
        private float pathLength;

        [SerializeField] private bool local;
        
        // Cached Variables
        private Vector3 _pos, _dir;
        private float _dt;
        protected override void OnCast() => PathSetup(raySource);

        private void PathSetup(RaySensor2D raySensor)
        {
            Path = new List<Vector2>();
            do
            {
                if (raySensor is PathRay2D _pathRay)
                {
                    if (_pathRay.DetectIndex > -1)
                    {
                        for (var i = 0; i <= _pathRay.DetectIndex; i++)
                        {
                            Path.Add(_pathRay.PathPoints[i]);
                        }
                        Path.Add(raySensor.hit.point);
                    }
                    else
                    {
                        Path.AddRange(local ? new List<Vector2>(_pathRay.PathPoints) : _pathRay.PathPoints);
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

        private float posM;

        internal override void RuntimeUpdate()
        {
            position = Mathf.Clamp01(position);

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
            if (position >= 1) OnEndCast(caster);
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
            else transform.position = _pos.ToDepth(Z);
            if (axisRun.syncAxis) axisRun.SyncAxis(transform, _dir);
            if (collisionRay) CollisionRun(_dt);
        }
        private float lineDistance;
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