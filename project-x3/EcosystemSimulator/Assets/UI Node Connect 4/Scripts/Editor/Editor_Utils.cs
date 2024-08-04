using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.EditorScript
{
    public static class Editor_Utils
    {
        public static void ReferenceDropAreaGUI<T>(this Editor editor, string label, string fieldName, ref T field) // parameters: <base type> property name
        {
            Event evt = Event.current;
            Rect drop_area;

            EditorGUILayout.BeginHorizontal();
            {
                SerializedProperty m_IntProp = editor.serializedObject.FindProperty(fieldName);
                EditorGUILayout.PropertyField(m_IntProp, new GUIContent(label), true);

                if (GUILayout.Button(field?.ToString(), EditorStyles.objectField))
                {
                    EditorUtility.FocusProjectWindow();
                    Object obj = null;
                    if (field != null)
                    {
                        var g = AssetDatabase.FindAssets($"t:Script {field.GetType()}");
                        string path = AssetDatabase.GUIDToAssetPath(g[0]);
                        obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    }
                    else
                    {
                        var g = AssetDatabase.FindAssets($"t:Script {typeof(T).Name}");
                        string path = AssetDatabase.GUIDToAssetPath(g[0]);
                        obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    }

                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button("✕", GUILayout.Width(18)))
                {
                    field = default(T);
                }
            }
            EditorGUILayout.EndHorizontal();
            drop_area = GUILayoutUtility.GetLastRect();

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (drop_area.Contains(evt.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            Object dragged_object = DragAndDrop.objectReferences[0];

                            System.Type type = System.Type.GetType(dragged_object.name + ",Assembly-CSharp");

                            if (type.BaseType != null && type.BaseType == typeof(T))
                            {
                                Undo.RecordObject(editor.target, "set class and create instance");
                                field = (T)type.Assembly.CreateInstance(dragged_object.name);
                            }
                        }
                    }
                    break;
            }
        }
    }
}