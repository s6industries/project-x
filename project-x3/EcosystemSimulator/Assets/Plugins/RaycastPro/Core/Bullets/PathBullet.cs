
namespace RaycastPro.Bullets
{
    using System.Collections.Generic;
    using UnityEngine;
    using RaySensors;


#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Bullets/" + nameof(PathBullet))] 
    public sealed class PathBullet : Bullet
    {
        public List<Vector3> Path = new List<Vector3>();
        
        public float turnSharpness = 15;
        public float duration = 1;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [SerializeField]
        private Rigidbody rigidBody;
        
        [SerializeField]
        private AxisRun axisRun = new AxisRun();

        private float pathLength;

        [Tooltip("Track the source Path in real-time.")]
        [SerializeField] private bool live = true;
        [Tooltip("Considering the path according to the hit point.")]
        [SerializeField] private bool onHit = true;
        // Cached Variables
        private Vector3 _pos, _dir, newPos;
        private float _dt;
        
        protected override void OnCast() => PathSetup(raySource);

        private List<Vector3> _tPath;

        public void PathSetup(RaySensor raySensor)
        {
            Path = new List<Vector3>();
            do
            {
                raySensor.GetPath(ref _tPath, collisionRay && onHit);
                Path.AddRange(_tPath);
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

            if (live) PathSetup(raySource);

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
            if (axisRun.syncAxis) axisRun.DampedSyncAxis(transform, _dir, turnSharpness);
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
                CastTypeField(
                    _so.FindProperty(nameof(moveType)),
                    _so.FindProperty(nameof(speed)), 
                    _so.FindProperty(nameof(duration)),
                    _so.FindProperty(nameof(curve)));
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(turnSharpness)));;
                
                BeginVerticalBox();
                axisRun.EditorPanel(_so.FindProperty(nameof(axisRun)));
                EndVertical();
                
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(live)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(onHit)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(rigidBody)));
            }

            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);
        }
#endif

    }
}