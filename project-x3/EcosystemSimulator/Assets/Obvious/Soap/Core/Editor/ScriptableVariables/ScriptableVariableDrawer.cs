using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

# if !ODIN_INSPECTOR
namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableDrawer : UnityEditor.Editor
    {
        private ScriptableBase _scriptableBase = null;
        private ScriptableVariableBase _scriptableVariable = null;
        private static bool _repaintFlag;
        private SerializedProperty _valueProperty;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            //Check for Serializable
            if (_scriptableVariable == null)
                _scriptableVariable = target as ScriptableVariableBase;
            var genericType = _scriptableVariable.GetGenericType;
            
            var canBeSerialized = SoapTypeUtils.IsUnityType(genericType) || SoapTypeUtils.IsSerializable(genericType);
            if (!canBeSerialized)
                SoapEditorUtils.DrawSerializationError(genericType);

            var settings = SoapEditorUtils.GetOrCreateSoapSettings();
            if (settings == null)
                return;
            if (settings.VariableDisplayMode == EVariableDisplayMode.Minimal)
            {
                DrawMinimal();
            }
            else
            {
                var isSceneObject = typeof(Component).IsAssignableFrom(genericType) ||
                                    typeof(GameObject).IsAssignableFrom(genericType);
                if (_valueProperty == null)
                    _valueProperty = serializedObject.FindProperty("_value");

                if (isSceneObject)
                    DrawDefaultSceneObject(genericType);
                else
                    DrawDefault();
            }

            if (serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(target);

            if (!EditorApplication.isPlaying)
                return;

            var container = (IDrawObjectsInInspector)target;
            var objects = container.GetAllObjects();

            SoapInspectorUtils.DrawLine();

            if (objects.Count > 0)
                DisplayAll(objects);
        }

        private void DrawMinimal()
        {
            serializedObject.DrawOnlyField("_value", false);
        }

        private void DrawDefault()
        {
            serializedObject.DrawOnlyField("m_Script", true);
            var propertiesToHide = new[] { "m_Script", "_guid", "_saveGuid" };
            serializedObject.DrawCustomInspector(propertiesToHide);

            if (GUILayout.Button("Reset Value"))
            {
                var so = (IReset)target;
                so.ResetValue();
            }
        }

        private void DrawDefaultSceneObject(Type genericType)
        {
            serializedObject.DrawOnlyField("m_Script", true);
            if (Application.isPlaying)
            {
                var value = EditorGUILayout.ObjectField("Value", _valueProperty.objectReferenceValue, genericType,
                    true);
                target.GetType().GetProperty("Value").SetValue(target, value);
            }
            else
            {
                var tooltip = "The value should only be set at runtime.";
                GUI.enabled = false;
                EditorGUILayout.ObjectField(new GUIContent("Value", tooltip), null, genericType, false);
                GUI.enabled = true;
            }

            var propertiesToHide = new[] { "m_Script", "_value", "_guid", "_saveGuid" };
            serializedObject.DrawCustomInspector(propertiesToHide);
            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (IReset)target;
                so.ResetValue();
            }
        }

        private void DisplayAll(List<UnityEngine.Object> objects)
        {
            var title = $"Objects reacting to OnValueChanged Event : {objects.Count}";
            EditorGUILayout.LabelField(title);
            foreach (var obj in objects)
                EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);
        }

        #region Repaint

        private void OnEnable()
        {
            if (_repaintFlag)
                return;

            _scriptableBase = target as ScriptableBase;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _repaintFlag = true;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            //TODO: investigate this
            if (target == null)
                return;

            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                if (_scriptableBase == null)
                    _scriptableBase = target as ScriptableBase;
                _scriptableBase.RepaintRequest += OnRepaintRequested;
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode)
                _scriptableBase.RepaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested() => Repaint();

        #endregion
    }
}
#endif
