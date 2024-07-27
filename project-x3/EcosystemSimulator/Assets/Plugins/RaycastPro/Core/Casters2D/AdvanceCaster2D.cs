
namespace RaycastPro.Casters2D
{
    using Bullets2D;
    using RaySensors2D;
    using UnityEngine;
    using System;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    [AddComponentMenu("RaycastPro/Casters/" + nameof(AdvanceCaster2D))]
    public sealed class AdvanceCaster2D : GunCaster<Bullet2D, Collider2D, RaySensor2D>
    {
        public RaySensor2D[] raySensors = Array.Empty<RaySensor2D>();
        
        public int currentIndex;

        [SerializeField]
        private bool pingPongPhase;

        public CastType castType = CastType.Together;

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
                    
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[currentIndex]))
                    {
                        currentIndex = ++currentIndex % raySensors.Length;
                    }
                    break;
                case CastType.Random:
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[new System.Random().Next(0, raySensors.Length)]))
                    {
                        currentIndex = ++currentIndex % raySensors.Length;
                    }
                    break;
                case CastType.PingPong:
                    if (AmmoCheck() && BulletCast(_bulletIndex, raySensors[currentIndex]))
                    {
                        currentIndex = pingPongPhase ? --currentIndex : ++currentIndex;
                        if (currentIndex == raySensors.Length - 1 || currentIndex == 0) pingPongPhase = !pingPongPhase;
                    }
                    break;
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Bullet caster, with the ability to adjust Ammo, detect all types of bullets automatically and RaySensors different shooting modes." + HAccurate + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            foreach (var sensor in raySensors)
            {
                var tip = sensor ? sensor.Tip : transform.position + transform.forward;
                var position = sensor ? sensor.Base : transform.position;

                DrawCapLine2D(position, tip);
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
                    CRaySensor.ToContent(TRaySensor), i => $"RaySensors {i+1}".ToContent($"Index {i}"));
                EndVertical();
                PropertyEnumField(_so.FindProperty(nameof(castType)), 4, CCastType.ToContent(TCastType), new GUIContent[]
                {
                    CTogether.ToContent(TTogether),
                    CSequence.ToContent(TSequence),
                    CRandom.ToContent(TRandom),
                    CPingPong.ToContent(TPingPong),
                });
                
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