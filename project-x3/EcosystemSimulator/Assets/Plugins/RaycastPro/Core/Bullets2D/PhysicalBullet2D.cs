namespace RaycastPro.Bullets2D
{
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    
    [RequireComponent(typeof(Rigidbody2D))]

    [AddComponentMenu("RaycastPro/Bullets/" + nameof(PhysicalBullet2D))]
    public sealed class PhysicalBullet2D : Bullet2D
    {
        [SerializeField] private Rigidbody2D body2D;
        protected override void OnCast()
        {
            if (!body2D) body2D = GetComponent<Rigidbody2D>();

            
            transform.position = raySource.Base;
            transform.right = raySource.TipDirection;

            body2D.angularVelocity = 0;
            body2D.velocity = Vector2.zero;
            
            body2D.AddForce(transform.right * power, forceMode);
        }

        internal override void RuntimeUpdate() => UpdateLifeProcess(GetDelta(timeMode));
        public float power = 1f;

        public ForceMode2D forceMode = ForceMode2D.Force;
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void AddForce(Vector3 direction) => GetComponent<Rigidbody2D>().AddForce(direction * power, forceMode);
#if UNITY_EDITOR
        
#pragma warning disable CS0414
        private static string Info = "A bullet with a rigidbody that is thrown in the direction of the raySensor." + HAccurate + HDependent;
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(body2D)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(power)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(forceMode)));
            }

            if (hasGeneral)
            {
                GeneralField(_so);
            }
        }
#endif

    }
}