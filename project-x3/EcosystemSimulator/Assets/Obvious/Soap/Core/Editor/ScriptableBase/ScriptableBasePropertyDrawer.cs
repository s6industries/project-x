using UnityEngine;
using UnityEditor;

namespace Obvious.Soap.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableBase), true)]
    public abstract class ScriptableBasePropertyDrawer : PropertyDrawer
    {
        private UnityEditor.Editor _editor;
        private const float WidthRatioWhenNull = 0.82f;
        protected virtual float WidthRatio => 0.82f;
        protected bool? _canBeSubAsset;
        private bool CanBeSubAsset => _canBeSubAsset != null && _canBeSubAsset.Value;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (_canBeSubAsset == null)
                _canBeSubAsset = SoapEditorUtils.CanBeSubAsset(property, fieldInfo);

            var targetObject = property.objectReferenceValue;
            if (targetObject == null)
            {
                DrawIfNull(position, property, label);
                return;
            }

            DrawIfNotNull(position, property, label, targetObject);

            EditorGUI.EndProperty();
        }

        protected void DrawIfNull(Rect position, SerializedProperty property, GUIContent label)
        {
            //Field info is null if the element is a serialized dictionary of Odin
            var widthRatio = fieldInfo == null ? 1f : WidthRatioWhenNull; 
            var rect = DrawCustomPropertyField(position, property, label, widthRatio);

            if (fieldInfo != null)
            {
                var guiContent = new GUIContent("Create",
                    "Creates the SO at current selected folder in the Project Window");
                if (GUI.Button(rect, guiContent))
                {
                    if (CanBeSubAsset)
                        CreateSubAsset(property);
                    else
                        CreateScriptableObjectAtPath(property);
                }
            }

            EditorGUI.EndProperty();
        }

        private void CreateScriptableObjectAtPath(SerializedProperty property)
        {
            var isCustomPath = SoapEditorUtils.GetOrCreateSoapSettings().CreatePathMode == ECreatePathMode.Manual;
            var path = isCustomPath
                ? SoapEditorUtils.CustomCreatePath
                : SoapFileUtils.GetSelectedFolderPathInProjectWindow();
            var newName = GetFieldName();
            property.objectReferenceValue =
                SoapEditorUtils.CreateScriptableObjectAt(fieldInfo.FieldType, newName, path);
        }

        private void CreateSubAsset(SerializedProperty property)
        {
            var mainAsset = property.serializedObject.targetObject;
            var subAsset = ScriptableObject.CreateInstance(fieldInfo.FieldType);
            subAsset.name = fieldInfo.Name;
            AssetDatabase.AddObjectToAsset(subAsset, mainAsset);
            property.objectReferenceValue = subAsset;
            property.serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
        
        protected void DrawIfNotNull(Rect position, SerializedProperty property, GUIContent label,
            Object targetObject)
        {
            var rect = position;
            var labelRect = position;
            
            var offset = CanBeSubAsset ? EditorGUIUtility.singleLineHeight : 0f;
            labelRect.width = position.width *.4f - offset; 
            
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, new GUIContent(""), true);
            if (property.isExpanded)
            {
                //Draw an embedded inspector 
                if (CanBeSubAsset)
                    label.image = SoapInspectorUtils.Icons.SubAsset;
                rect.width = position.width;
                EditorGUI.PropertyField(rect, property, label);
                EditorGUI.indentLevel++;
                var cacheBgColor = GUI.backgroundColor;
                GUI.backgroundColor = SoapEditorUtils.SoapColor;
                GUILayout.BeginVertical(GUI.skin.box);
                if (_editor == null)
                    UnityEditor.Editor.CreateCachedEditor(targetObject, null, ref _editor);
                _editor.OnInspectorGUI();
                GUI.backgroundColor = cacheBgColor;
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            else
            {
                DrawUnExpanded(position, property, label, targetObject);
            }
        }

        protected virtual string GetFieldName()
        {
            return fieldInfo.Name;
        }

        protected virtual void DrawUnExpanded(Rect position, SerializedProperty property, GUIContent label,
            Object targetObject)
        {
            var rect = DrawCustomPropertyField(position, property, label, WidthRatio);
            DrawShortcut(rect, property, targetObject);
        }

        protected virtual void DrawShortcut(Rect position, SerializedProperty property, Object targetObject)
        {
        }

        private Rect DrawCustomPropertyField(Rect position, SerializedProperty property, GUIContent label,
            float widthRatio)
        {
            if (CanBeSubAsset)
                label.image = SoapInspectorUtils.Icons.SubAsset;

            var propertyRect = new Rect(position);
            propertyRect.width = position.width * widthRatio;
            //this sets the property rect position to the right of the label
            propertyRect = EditorGUI.PrefixLabel(propertyRect, label);

            if (CanBeSubAsset && property.objectReferenceValue != null)
            {
                //draw a small X button to delete the asset
                propertyRect.x -= EditorGUIUtility.singleLineHeight + 2f;
                var buttonRect = new Rect(propertyRect);
                buttonRect.width = EditorGUIUtility.singleLineHeight;
                var content = new GUIContent(GUIContent.none);
                content.image = SoapInspectorUtils.Icons.Cancel;
                if (GUI.Button(buttonRect, content))
                {
                    SoapEditorUtils.DeleteSubAsset(property.objectReferenceValue);
                    return position;
                }

                propertyRect.x += buttonRect.width + 2f;
            }

            GUI.enabled = !CanBeSubAsset;
            EditorGUI.PropertyField(propertyRect, property, GUIContent.none);
            GUI.enabled = true;
            var rectPosition = new Rect(propertyRect);
            rectPosition.xMin = propertyRect.xMax + 5f;
            rectPosition.width = position.width * (1 - widthRatio) - 5f;
            return rectPosition;
        }
    }
}