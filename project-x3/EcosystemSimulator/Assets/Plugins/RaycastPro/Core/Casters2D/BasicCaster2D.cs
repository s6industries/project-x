namespace RaycastPro.Casters2D
{
    using UnityEngine;
    using RaySensors2D;
    using Bullets2D;
    using UnityEngine.Events;
    
#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif

    [AddComponentMenu("RaycastPro/Casters/" + nameof(BasicCaster2D))]
    public sealed class BasicCaster2D : GunCaster<Bullet2D, Collider2D, RaySensor2D>
    {
        [Tooltip("Automatically, this ray will shoot along the LocalDirection and source BasePoint location.")]
        public RaySensor2D raySource;
        
        public override void Cast(int _bulletIndex)
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            if (AmmoCheck())
            {
                BulletCast(_bulletIndex, raySource);
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "A simple shooter with the ability to cover Basic Bullets that can help you to test and launch the gun immediately." +
            HAccurate + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            var _t = transform;
            DrawCapLine2D(_t.position, _t.position + _t.right);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(raySource)));
                
                GunField(_so);
            }
            if (hasGeneral) GeneralField(_so);

            if (hasEvents) 
            {
                EventFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents.ToContent(TEvents),
                    RCProEditor.HeaderFoldout);
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (EventFoldout) RCProEditor.EventField(_so, events);
            }
            if (hasInfo) InformationField();
        }
        private readonly string[] events = new[] {nameof(onCast), nameof(onReload), nameof(onRate)};
#endif
    }
}