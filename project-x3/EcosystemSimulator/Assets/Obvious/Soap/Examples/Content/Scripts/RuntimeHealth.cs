using UnityEngine;

namespace Obvious.Soap.Example
{
    public class RuntimeHealth : MonoBehaviour
    {
        [Tooltip("Leave this null, it will be instantiate at runtime")] [SerializeField]
        private FloatVariable _runtimeHpVariable;

        [SerializeField] private FloatReference _maxHealth = null;

        private void Start()
        {
            //Create the runtime variable instance. It's better to do it in Start because of the execution order:
            //Awake-> SceneLoaded (values are reset) -> Start
            if (_runtimeHpVariable == null)
                _runtimeHpVariable = CreateRuntimeVariable<FloatVariable>($"{gameObject.name}_Hp");

            _runtimeHpVariable.Value = _maxHealth.Value;
            _runtimeHpVariable.OnValueChanged += OnHealthChanged;

            //Initialize the health bar only after the variable has been properly set.
            //You can use events to decouple components if your health bar is in another scene (UI scene for example). 
            //In this case, as it's a local Health bar, a direct reference is fine. 
            GetComponentInChildren<HealthBarSprite>().Init(_runtimeHpVariable);
        }

        private void OnDisable()
        {
            _runtimeHpVariable.OnValueChanged -= OnHealthChanged;
        }

        /// <summary>
        /// Creates a new instance of a ScriptableVariableBase subclass with the given name.
        /// Feel free to make this a static method in a utility class.
        /// </summary>
        /// <typeparam name="T">The type of the ScriptableVariableBase subclass to create.</typeparam>
        /// <param name="name">The name of the new ScriptableVariableBase instance.</param>
        /// <returns>The newly created ScriptableVariableBase instance.</returns>
        private T CreateRuntimeVariable<T>(string name) where T : ScriptableVariableBase
        {
            var runtimeVariable = ScriptableObject.CreateInstance<T>();
            runtimeVariable.name = name;
            return runtimeVariable;
        }

        private void OnHealthChanged(float newValue)
        {
            if (newValue <= 0f)
                gameObject.SetActive(false);
        }

        //In this example, this is called when the enemy collides with the Player.
        public void TakeDamage(int amount) => _runtimeHpVariable.Add(-amount);
    }
}