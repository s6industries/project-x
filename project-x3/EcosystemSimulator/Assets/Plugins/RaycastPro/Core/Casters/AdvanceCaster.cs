namespace RaycastPro.Casters
{
    using System;
    using RaySensors;
    using UnityEngine;
    using Bullets;

#if UNITY_EDITOR
    using UnityEditor;
    using Editor;
#endif
    
    [AddComponentMenu("RaycastPro/Casters/" + nameof(AdvanceCaster))]
    public sealed class AdvanceCaster : GunCaster<Bullet, Collider, RaySensor>
    {
        [SerializeField]
        public RaySensor[] raySensors = Array.Empty<RaySensor>();
        
        [Tooltip("current ray (Gun Barrel) in shooting.")]
        public int rayIndex;
        
        [Tooltip("Ping Phong Phase")]
        public bool PPhase;

        public CastType castType = CastType.Together;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void Cast(int _bulletIndex)
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            // Last Note: Adding Index debugging. Bullet cast returning true when successfully shot
            switch (castType)
            {
                case CastType.Together:

                    if (AmmoCheck(raySensors.Length))
                    {
                        foreach (var ray in raySensors)
                        {
                            BulletCast(_bulletIndex, ray);
                        }
                    }

                    break;
                case CastType.Sequence:
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[rayIndex]))
                    {
                        rayIndex = ++rayIndex % raySensors.Length;
                    }
                    break;
                case CastType.Random:
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[UnityEngine.Random.Range(0, raySensors.Length)]))
                    {
                        rayIndex = ++rayIndex % raySensors.Length;
                    }
                    break;
                case CastType.PingPong:
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[rayIndex]))
                    {
                        rayIndex = PPhase ? --rayIndex : ++rayIndex;
                        if (rayIndex == raySensors.Length - 1 || rayIndex == 0) PPhase = !PPhase;
                    }
                    break;
            }
        }


#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Bullet caster, with the ability to adjust Ammo, detect all types of bullets automatically and RaySensors different shooting modes." + HAccurate + HDependent;
#pragma warning restore CS0414

        private Vector3 tip, position;
        internal override void OnGizmos()
        {
            foreach (var sensor in raySensors)
            {
                if (!sensor) continue;
                
                position = sensor ? sensor.Base : transform.position;
                DrawCapLine(position, position + sensor.TipDirection);
            }
        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                BeginVerticalBox();
                RCProEditor.PropertyArrayField(_so.FindProperty(nameof(raySensors)),
                    CRaySensor.ToContent(TRaySensor), i => $"GunBarrel {i+1}".ToContent($"Index {i}"));
                EndVertical();
                BeginVerticalBox();
                PropertyEnumField(_so.FindProperty(nameof(castType)), 4, CCastType.ToContent(TCastType), new GUIContent[]
                {
                    CTogether.ToContent(TTogether),
                    CSequence.ToContent(TSequence),
                    CRandom.ToContent(TRandom),
                    CPingPong.ToContent(TPingPong),
                });
                EndVertical();
                
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