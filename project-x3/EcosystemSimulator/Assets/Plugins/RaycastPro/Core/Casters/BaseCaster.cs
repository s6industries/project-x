using static UnityEngine.Random;

namespace RaycastPro
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public abstract class BaseCaster : RaycastCore
    {
        /// <summary>
        /// Allow to manuel Casting.
        /// </summary>
        /// <param name="_bulletIndex">Bullet Array Index</param>
        public abstract void Cast(int _bulletIndex);
        
        [Tooltip("Automatically instantiate into this object.")]
        public Transform poolManager;

        [SerializeField]
        public UnityEvent onRate;
        [SerializeField]
        public UnityEvent onReload;
        
                
        [Tooltip("When turn on, gun auto reload on enable or end of shooting.")]
        public bool autoReload = true;
        
        public abstract void Reload();
        
        #region Update
        protected void Update()
        {
            if (autoUpdate != UpdateMode.Normal) return;
            OnCast();
        }
        protected void LateUpdate()
        {
            if (autoUpdate != UpdateMode.Late) return;
            OnCast();
        }
        protected void FixedUpdate()
        {
            if (autoUpdate != UpdateMode.Fixed) return;
            OnCast();
        }

        #endregion

        protected static void CopyBullet<T>(T to, T from)
        {
            var fields = to.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(to, field.GetValue(from));
            }

            var props = to.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(to, prop.GetValue(from, null), null);
            }
        }

        [Serializable]
        public class Ammo
        {
            [Tooltip("When activate, the cost of shooting will be zero.")]
            [SerializeField] public bool infiniteAmmo = true;
            
            [Tooltip("The total number of bullets out of the magazine")]
            [SerializeField] public int amount = 12;
            
            [Tooltip("The Capacity of each magazine that will enter the reload time when it runs out.")]
            [SerializeField] public int magazineCapacity = 6;
            
            [Tooltip("The number of bullets in the current magazine.")]
            [SerializeField] public int magazineAmount = 6;

            [Tooltip("Interruption time until the magazine is filled")]
            [SerializeField] public float reloadTime = 2f;
            
            [Tooltip("The firing pause time between each shot")]
            [SerializeField] public float rateTime = .1f;
            
            [Tooltip("Randomness")]
            [SerializeField] public float randomness = 0f;
            
            [Tooltip("Chance to miss shooting when cast, 0-1 : 0-100%")]
            [SerializeField] public float missRate = 0f;
            /// <summary>
            /// The number of all available bullets, which is calculated from the "amount + magazineAmount".
            /// </summary>
            public int Total => amount + magazineAmount;

            [SerializeField] internal bool inRate;
            [SerializeField] internal bool inReload;
            public bool IsReloading => inReload;
            public bool IsInRate => inRate;

            public int MagazineAmount
            {
                get => magazineAmount;
                set => magazineAmount = Mathf.Clamp(value, 0, magazineCapacity);
            }
            public int MagazineCapacity
            {
                get => magazineCapacity;
                set => magazineCapacity = Mathf.Max(0, value);
            }
            public int Amount
            {
                get => amount;
                set => amount = Mathf.Max(0, value);
            }
            
            public bool Use(BaseCaster _caster, int _amount = 1)
            {
                if (inRate) return false;
                if (inReload) return false;
                
                if (magazineAmount < _amount)
                {
                    if (_caster.autoReload)
                    {
                        _caster.StartCoroutine(IReload());
                        _caster.onReload?.Invoke();
                    }

                    return false;
                }
                if (value > missRate)
                {       
                    magazineAmount -= _amount;
                    _inRateC = _caster.StartCoroutine(IRate());
                    _caster.onRate?.Invoke();
                }
                return true;
            }

            private Coroutine _inRateC;
            
            /// <summary>
            ///  Change to Ienumerator
            /// </summary>
            internal IEnumerator IRate()
            {
                inRate = true;
                yield return new WaitForSeconds(Range(rateTime - randomness, randomness + randomness));
                inRate = false;
            }

            public float currentReloadTime { get; private set; } = 0f;
            internal IEnumerator IReload()
            {
                inReload = true;
                currentReloadTime = 0f;
                while (currentReloadTime <= reloadTime)
                {
                    currentReloadTime += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                currentReloadTime = 0f;
                
                if (infiniteAmmo) // Bill
                {
                    magazineAmount = magazineCapacity;
                }
                else
                {
                    var needed = magazineCapacity - magazineAmount;
                    var possible = Mathf.Min(amount, needed);
                    magazineAmount += possible;
                    amount -= possible;
                }
                inReload = false;
            }

#if UNITY_EDITOR
            
            internal void EditorPanel(SerializedProperty serializedProperty)
            {
                BeginHorizontal();
                EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(nameof(amount)));
                MiniField(serializedProperty.FindPropertyRelative(nameof(infiniteAmmo)), "I".ToContent("Infinite Ammo"));
                EndHorizontal();
                var mCapProp = serializedProperty.FindPropertyRelative(nameof(magazineCapacity));
                PropertyMaxIntField(mCapProp, "Magazine Capacity".ToContent());
                PropertySliderField(serializedProperty.FindPropertyRelative(nameof(magazineAmount)), 0, mCapProp.intValue, "Magazine Amount".ToContent(), i => {});
                PropertyMaxField(serializedProperty.FindPropertyRelative(nameof(reloadTime)));
                PropertyMaxField(serializedProperty.FindPropertyRelative(nameof(rateTime)));
                PropertyMaxField(serializedProperty.FindPropertyRelative(nameof(randomness)));
                PropertySliderField(serializedProperty.FindPropertyRelative(nameof(missRate)), 0, 1, "Miss Rate".ToContent());
            }
#endif
        }
    }
}