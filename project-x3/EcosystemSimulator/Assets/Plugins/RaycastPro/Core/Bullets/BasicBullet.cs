namespace RaycastPro.Bullets
{
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [AddComponentMenu("RaycastPro/Bullets/" + nameof(BasicBullet))]

    public sealed class BasicBullet : Bullet
    {
        protected override void OnCast() { }
        
        private float delta;
        private Vector3 _forward;

        internal override void RuntimeUpdate()
        {
            delta = GetDelta(timeMode);
            _forward = transform.forward;
            transform.position += _forward * (speed * delta);
            UpdateLifeProcess(delta);
            if (collisionRay) CollisionRun(delta);
        }
        
        protected override void CollisionBehaviour()
        {
            transform.position = collisionRay.cloneRaySensor.Base;
            transform.forward = collisionRay.cloneRaySensor.Direction.normalized;
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "A simple bullet that travels directly from the origin to the tip of the raySensor with avoiding the path." + HAccurate + HDependent + HIRadius;
#pragma warning restore CS0414
        
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(speed)), CSpeed.ToContent());
            }

            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif

    }
}