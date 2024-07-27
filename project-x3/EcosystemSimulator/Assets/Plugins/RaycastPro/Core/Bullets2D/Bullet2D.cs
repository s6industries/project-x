namespace RaycastPro.Bullets2D
{
    using RaySensors2D;

#if UNITY_EDITOR
    using UnityEditor;

#endif
    
    public abstract class Bullet2D : BaseBullet
    {
        public BaseCaster caster;
        public RaySensor2D raySource;
        public RaySensor2D collisionRay;
        public float Z => transform.position.z;
        
        internal override void Cast<R>(BaseCaster _caster, R raySensor)
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            caster = _caster;
            
            raySource = raySensor as RaySensor2D;
            
            if (!raySource)
            {
                transform.position = caster.transform.position;
                transform.forward = caster.transform.right;
            }
            
            OnCast(); // Auto Setup 3D Bullet
            onCast?.Invoke(caster);
            if (collisionRay)
            {
                collisionRay.enabled = false;
            }

            onCast?.Invoke(caster);
        }

        public override void SetCollision(bool turn)
        {
            collisionRay.enabled = turn;
        }

        private float ignoreTime;
        protected override void CollisionRun(float deltaTime)
        {
            if (ignoreTime > 0)
            {
                ignoreTime -= deltaTime;
                return;
            }

            if (!collisionRay.Cast()) return;

            if (collisionRay.cloneRaySensor)
            {
                ignoreTime = baseIgnoreTime;
                transform.position = collisionRay.cloneRaySensor.Base;
                transform.right = collisionRay.cloneRaySensor.Direction;
                onPlanar?.Invoke();
            }
            else
            {
                InvokeDamageEvent(collisionRay.hit.transform);
                if (endOnCollide) OnEndCast(caster);
            }
        }

#if UNITY_EDITOR
        
        protected override void CollisionRayField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(collisionRay)));
        }
        internal override void OnGizmos()
        {
            DrawCap(transform.position, transform.right, 4);
        }
#endif
    }
}