using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using Obvious.Soap.Attributes;
#endif

namespace Obvious.Soap
{
    public abstract class ScriptableVariable<T> : ScriptableVariableBase, ISave, IReset, IDrawObjectsInInspector
    {
        [Tooltip("The value of the variable. This will be reset on play mode exit to the value it had before entering play mode.")]
        [SerializeField]
        protected T _value;

        [Tooltip("Log in the console whenever this variable is changed, loaded or saved.")] [SerializeField]
        private bool _debugLogEnabled;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        [Tooltip("If true, saves the value to Player Prefs and loads it onEnable.")] [SerializeField]
        private bool _saved;

        [Tooltip(
            "The default value of this variable. When loading from PlayerPrefs the first time, it will be set to this value.")]
        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf("_saved")]
        [PropertyOrder(2)]
        [Indent]
        [BoxGroup]
#else
        [ShowIf("_saved", true)]
#endif
        private T _defaultValue;
        
#if ODIN_INSPECTOR
        [PropertyOrder(5)]
#endif
        [Tooltip("Reset to initial value." +
                 " Scene Loaded : when the scene is loaded." +
                 " Application Start : Once, when the application starts.")]
        [SerializeField]
        private ResetType _resetOn = ResetType.SceneLoaded;

        /// <summary> This caches the value when play mode starts. </summary>
        private T _initialValue;

        private readonly List<Object> _listenersObjects = new List<Object>();
#if ODIN_INSPECTOR
        [HideInEditorMode]
        [ShowInInspector,EnableGUI]
        [PropertyOrder(100)]
        public IEnumerable<Object> ObjectsReactingToOnValueChangedEvent  => _listenersObjects;
#endif

        private Action<T> _onValueChanged;
        
        /// <summary> Event raised when the variable value changes. </summary>
        public event Action<T> OnValueChanged
        {
            add
            {
                _onValueChanged += value;

                var listener = value.Target as Object;
                if (listener != null && !_listenersObjects.Contains(listener))
                    _listenersObjects.Add(listener);
            }
            remove
            {
                _onValueChanged -= value;

                var listener = value.Target as Object;
                if (_listenersObjects.Contains(listener))
                    _listenersObjects.Remove(listener);
            }
        }

        /// <summary>
        /// The previous value just after the value changed.
        /// </summary>
        public T PreviousValue { get; private set; }

        /// <summary>
        /// The default value this variable is reset to. 
        /// </summary>
        public T DefaultValue
        {
            get => _defaultValue;
            private set => _defaultValue = value;
        }

        /// <summary>
        /// Modify this to change the value of the variable.
        /// Triggers OnValueChanged event.
        /// </summary>
        public virtual T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;
                _value = value;
                ValueChanged();
            }
        }

        protected void ValueChanged()
        {
            _onValueChanged?.Invoke(_value);

            if (_debugLogEnabled)
            {
                var suffix = _saved ? " <color=#f75369>[Saved]</color>" : "";
                Debug.Log($"{GetColorizedString()}{suffix}", this);
            }

            if (_saved)
                Save();

            PreviousValue = _value;
#if UNITY_EDITOR
            if (this != null) //for runtime variables, the instance will be destroyed so do not repaint.
                SetDirtyAndRepaint();
#endif
        }

        public override Type GetGenericType  => typeof(T);

        private void Awake()
        {
            //Prevents from resetting if no reference in a scene
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            Init();
#endif
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_resetOn != ResetType.SceneLoaded)
                return;

            if (mode == LoadSceneMode.Single)
            {
                if (_saved)
                    Load();
                else
                    ResetValue();
            }
        }

#if UNITY_EDITOR
        private void SetDirtyAndRepaint()
        {
            EditorUtility.SetDirty(this);
            RepaintRequest?.Invoke();
        }

        public void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
                Init();
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
                if (!_saved)
                    ResetValue();
        }

        protected virtual void OnValidate()
        {
            //In non fast play mode, this get called before OnEnable(). Therefore a saved variable can get saved before loading. 
            //This check prevents the latter.
            if (Equals(_value, PreviousValue))
                return;
            ValueChanged();
        }

        /// <summary> Reset the SO to default.</summary>
        internal override void Reset()
        {
            base.Reset();
            _listenersObjects.Clear();
            Value = default;
            _initialValue = default;
            PreviousValue = default;
            _saved = false;
            _resetOn = ResetType.SceneLoaded;
            _debugLogEnabled = false;
        }
#endif
        
        private void Init()
        {
            _initialValue = _value;
            PreviousValue = _value;
            if (_saved)
                Load();
            _listenersObjects.Clear();
        }

        /// <summary> Reset to initial value</summary>
        public void ResetValue()
        {
            Value = _initialValue;
            PreviousValue = _initialValue;
        }

        public virtual void Save()
        {
        }

        public virtual void Load()
        {
            PreviousValue = _value;

            if (_debugLogEnabled)
                Debug.Log($"{GetColorizedString()} <color=#f75369>[Loaded].</color>", this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(name);
            sb.Append(" : ");
            sb.Append(_value);
            return sb.ToString();
        }

        private string GetColorizedString() => $"<color=#f75369>[Variable]</color> {ToString()}";

        public List<Object> GetAllObjects() => _listenersObjects;

        public static implicit operator T(ScriptableVariable<T> variable) => variable.Value;
    }

    /// <summary>
    /// Defines when the variable is reset.
    /// </summary>
    internal enum ResetType
    {
        SceneLoaded,
        ApplicationStarts,
    }

    public enum CustomVariableType
    {
        None,
        Bool,
        Int,
        Float,
        String
    }
}