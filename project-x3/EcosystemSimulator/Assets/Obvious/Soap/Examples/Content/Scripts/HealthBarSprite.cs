using UnityEngine;

namespace Obvious.Soap.Example
{
    public class HealthBarSprite : MonoBehaviour
    {
        [SerializeField] private Transform _fillTransform = null;
        private FloatVariable _hpVariable;
        private float _maxHealth;

        public void Init(FloatVariable runtimeVariable)
        {
            _hpVariable = runtimeVariable;
            _maxHealth = runtimeVariable.Value;
            _hpVariable.OnValueChanged += OnHealthChanged;
            OnHealthChanged(_maxHealth);
        }

        private void OnDisable()
        {
            _hpVariable.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float currentHpValue)
        {
            var hpRatio = Mathf.Clamp01(currentHpValue / _maxHealth);
            _fillTransform.localPosition = new Vector3((-1 + hpRatio) * 0.5f, 0,0);
            _fillTransform.localScale = new Vector3(hpRatio,1,1);
        }
    }
}