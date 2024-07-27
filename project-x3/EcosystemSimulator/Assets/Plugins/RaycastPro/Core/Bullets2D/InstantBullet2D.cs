namespace RaycastPro.Bullets
{
    using Bullets2D;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Bullets/" + nameof(InstantBullet2D))]
    public class InstantBullet2D : Bullet2D
    {
        internal override void RuntimeUpdate() => UpdateLifeProcess(GetDelta(timeMode));

        protected override void OnCast()
        {
            if (collisionRay.planarSensitive)
            {
                var clone = raySource.LastClone;
                transform.position = clone.TipTarget;
                // Hit Direction cuz don't want to place on hit Normal
                transform.right = clone.HitDirection.normalized;
                if (raySource.hit) InvokeDamageEvent(raySource.hit.transform);
            }
            else
            {
                transform.position = raySource.TipTarget;
                transform.right = raySource.HitDirection.normalized;
                if (raySource.hit) InvokeDamageEvent(raySource.hit.transform);
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Instant shots to Target Tip of sensor ray." + HDependent;
#pragma warning restore CS0414
        
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain) { }
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }
#endif
    }
}
