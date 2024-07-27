using UnityEngine.Events;

namespace RaycastPro.Bullets
{
    using Planers;
    using RaySensors;
    using UnityEngine;
    
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    public abstract class Bullet : BaseBullet
    {
        public BaseCaster caster;
        public RaySensor raySource;
        public RaySensor collisionRay;

        protected float ignoreTime;
        
        private void OnDestroy() => onEnd?.Invoke(caster);
        internal override void Cast<R>(BaseCaster _caster, R raySensor)
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            raySource = raySensor as RaySensor;

            caster = _caster;
            
            if (!raySource)
            {
                transform.position = caster.transform.position;
                transform.forward = caster.transform.forward;
            }

            OnCast(); // Auto Setup 3D Bullet
            onCast?.Invoke(caster);
            
            if (collisionRay) collisionRay.enabled = false;
        }

        /// <summary>
        /// Don't Forget to setup basics like caster and raySource beforeUsing.
        /// </summary>
        internal virtual void DirectCast()
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            
            OnCast(); // Auto Setup 3D Bullet
            onCast?.Invoke(caster);
            
            if (collisionRay) collisionRay.enabled = false;
        }
        
        public override void SetCollision(bool turn) => collisionRay.enabled = turn;

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

                CollisionBehaviour();
                onPlanar?.Invoke();
            }
            else
            {
                InvokeDamageEvent(collisionRay.hit.transform);
                if (endOnCollide) OnEndCast(caster);
            }
        }

        protected abstract void CollisionBehaviour();
#if UNITY_EDITOR

        protected override void CollisionRayField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(collisionRay)));
        }

        internal override void OnGizmos() => DrawCap(transform.position, transform.forward, DiscSize);
#endif
    }
}