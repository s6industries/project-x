#if UNITY_EDITOR
namespace RaycastPro.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using RaycastPro;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(RaycastCore), true), CanEditMultipleObjects]
    public sealed class RCProEditor : Editor
    {
        internal static readonly Color Aqua = new Color(0.1686275f, 0.7764706f, 0.8235294f, 1f);
        internal static readonly Color Violet = new Color(0.8784314f, 0.254902f, 0.5058824f, 1f);

        internal static string RPro => $"<color=#2BC6D2>RaycastPro: </color>";

        // ReSharper disable Unity.PerformanceAnalysis
        public static void Log(string log) => Debug.Log(RPro+log);
        
        internal static string AQUA_Text(string text) => $"<color=#2BC6D2>{text}</color>";
        internal static string VIOLET_Text(string text) => $"<color=#E04181>{text}</color>";
        
        private const string INFO = "Info";

        private readonly string[] MultiEditingPropsName = new string[]
        {
            "destination", "detectLayer","direction", "radius", "height", "iteration", "colliderSize"
        };
        private SerializedProperty _cProp;
        private SerializedObject _cSO;
        private RaycastCore[] _cores;
        
        // [RCProPanel.SavePreference]
        // public bool sceneGUI;
        
        public override void OnInspectorGUI()
        {
            if (!target || !(target is RaycastCore pro)) return;

            if (!RCProPanel.rcProInspector || target.GetType().GetCustomAttribute<RCProPanel.RawEditor>(true) != null)
            {
                base.OnInspectorGUI();
                
                return;
            }
            
            GUI.color = Color.white;
            _cores = Selection.gameObjects.Select(o => o.GetComponent<RaycastCore>()).ToArray();
            var _isMulti = _cores.Count() > 1;
            HeaderField(target.GetType().Name.ToRegex() + (_isMulti ? " (Multi Editing)" : ""));
            GUI.backgroundColor = Violet;
            InfoField(pro);
            GUI.contentColor = Aqua;
            EditorGUILayout.BeginVertical();
            
            _cSO = _isMulti ? new SerializedObject(_cores) : new SerializedObject(pro);
            _cSO.Update();
            
            EditorGUI.BeginChangeCheck();
            pro.EditorPanel(_cSO);
            if (EditorGUI.EndChangeCheck()) _cSO.ApplyModifiedProperties();
            
            EditorGUILayout.EndVertical();
        }
        
        public void OnSceneGUI()
        {
            if (target is ISceneGUI iScene && target is RaycastCore core)
            {
                _cSO = new SerializedObject(core);
                _cSO.Update();
                EditorGUI.BeginChangeCheck();
                iScene.OnSceneGUI();
                if (EditorGUI.EndChangeCheck()) _cSO.ApplyModifiedProperties();
            }
        }
        internal static GUIStyle HeaderStyle =>
            new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

        internal static GUIStyle BoxStyle =>
            new GUIStyle(EditorStyles.helpBox)
            {
                richText = true,
                contentOffset = Vector2.zero,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(4,4,2,4),
                padding = new RectOffset(4,4,2,4),
            };
        internal static GUIStyle LabelStyle =>
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                richText = true
            };

        internal static GUIStyle HeaderFoldout
        {
            get
            {
                var style = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    margin = new RectOffset(12, 4, 2, 2),
                    padding = new RectOffset(0, 0, 0, 0),
                };
                return style;
            }
        }
        internal static void EventField(SerializedObject serializedObject, IEnumerable<string> propertyNames)
        {
            var style = new GUIStyle { alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.BeginVertical(style);
            foreach (var propertyName in propertyNames) EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName));
            EditorGUILayout.EndVertical();
        }
        internal static void HeaderField(string header, string tooltip = "")
        {
           
            GUILayout.Space(4);
            
            GUILine(Color.white);

            if (tooltip != "") GUILayout.Label(header.ToContent(tooltip), HeaderStyle);

            else GUILayout.Label(header,HeaderStyle);

            GUILayout.Space(1);
            
            GUILine(Color.white);
            
            GUILayout.Space(4);
        }
        internal static void BetterSliderField(ref float minValue, ref float maxValue, float minLimit, float maxLimit, float labelWidth = 40f)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.FloatField(Mathf.Round(minValue*100)/100, GUILayout.Width(labelWidth));
            EditorGUILayout.MinMaxSlider( ref minValue, ref maxValue, minLimit, maxLimit);
            EditorGUILayout.FloatField(Mathf.Round(maxValue*100)/100, GUILayout.Width(labelWidth));
            EditorGUILayout.EndHorizontal();
        }        
        internal static void LayerField(GUIContent label, ref LayerMask layerMask) // layerField
        {
            LayerMask tempMask = EditorGUILayout.MaskField(label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            
            layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
        internal static LayerMask LayerField(GUIContent label, LayerMask layerMask) // layerField
        {
            LayerMask tempMask = EditorGUILayout.MaskField(label, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            
            return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
        
        private static GUIStyle cleanStyle => new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(5,5,5,5),
            padding = new RectOffset(5,5,5,5),
            fixedWidth = 64,
            fixedHeight = 64,
        };
        private static GUIStyle labelStyle => new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(5,5,5,5),
            wordWrap = true,
            richText = true
        };

        private const string CNoInfoDefinition = "No Info Definition.";

        internal static string GetInfo(RaycastCore pro)
        {
            var field = pro.GetType().GetField(INFO, BindingFlags.NonPublic | BindingFlags.Static);
            return field != null ? field.GetValue(null).ToString() : CNoInfoDefinition;
        }
        internal static void InfoField(RaycastCore pro)
        {
            GUILayout.BeginHorizontal(BoxStyle);
            
            GUILayout.Box(EditorGUIUtility.ObjectContent(pro, pro.GetType()).image, cleanStyle);
            GUILayout.Label(GetInfo(pro), labelStyle);
            GUILayout.EndHorizontal();
            
            GUILine(Color.white);
            EditorGUILayout.Space(1);
            GUILine(Color.white);
            
            EditorGUILayout.Space(2);
        }
        internal static void TypeField<T>(string label, ref T type) where T : Object
        {
            type = EditorGUILayout.ObjectField(label, type, typeof(T), true) as T;
        }
        internal static void TypeField<T>(GUIContent label, ref T type) where T : Object
        {
            type = EditorGUILayout.ObjectField(label, type, typeof(T), true) as T;
        }
        internal static T TypeField<T>(GUIContent label, T type) where T : Object
        {
            return EditorGUILayout.ObjectField(label, type, typeof(T), true) as T;
        }
        internal static void ButtonSelectionField(string[] labels, ref bool[] button)
        {
            var bColor = GUI.backgroundColor;
            
            GUILayout.BeginHorizontal(BoxStyle);
            for (var i = 0; i < labels.Length; i++)
            {
                var style = new GUIStyle(EditorStyles.toolbarButton);
                if (button[i]) GUI.backgroundColor = Color.white;

                if (GUILayout.Button(labels[i], style)) button[i] = !button[i];
                
                GUI.backgroundColor = bColor;
            }
            GUILayout.EndHorizontal();
        }
        internal static T EnumLabelField<T>(T type, GUIContent content, int column = 4) where T : Enum
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(content);
                
            var n = type.ToString();

            var s = (int) Enum.Parse(type.GetType(), n);

            var enumNames = Enum.GetNames(type.GetType());
            s = GUILayout.SelectionGrid(s, enumNames, enumNames.Length >= column ? column : enumNames.Length, GUILayout.Width(180f));

            EditorGUILayout.EndHorizontal();

            return (T) Enum.ToObject(type.GetType(), s);
        }
        internal static T EnumLabelField<T>(T type, GUIContent content, string[] contents, bool inBox = true, int column = 4) where T : Enum
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(content);
                
            var n = type.ToString();

            var s = (int) Enum.Parse(type.GetType(), n);

            var enumNames = Enum.GetNames(type.GetType()).ToContents(contents);
            s = GUILayout.SelectionGrid(s, enumNames, enumNames.Length >= column ? column : enumNames.Length,GUILayout.Width(200f));
            
            EditorGUILayout.EndHorizontal();

            return (T) Enum.ToObject(type.GetType(), s);
        }
        internal static T EnumHeaderField<T>(T type, bool label = true, int column = 4) where T : Enum
        {
            if (label) HeaderField(typeof(T).Name);

            var n = type.ToString();

            var s = (int) Enum.Parse(type.GetType(), n);

            var enumNames = Enum.GetNames(type.GetType());
            s = GUILayout.SelectionGrid(s, enumNames, enumNames.Length >= column ? column : enumNames.Length);

            return (T) Enum.ToObject(type.GetType(), s);
        }
        internal static T EnumHeaderField<T>(T type,GUIContent content, string[] contents, int column = 4) where T : Enum
        {
            HeaderField(content.text, content.tooltip);

            var n = type.ToString();

            var s = (int) Enum.Parse(type.GetType(), n);

            var enumNames = Enum.GetNames(type.GetType()).ToContents(contents);
            s = GUILayout.SelectionGrid(s, enumNames, enumNames.Length >= column ? column : enumNames.Length);

            return (T) Enum.ToObject(type.GetType(), s);
        }
        internal static void GUILine(Color color, int height = 1, int space = 0)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);

            rect.height = height;
            
            GUILayout.Space(space);
            
            EditorGUI.DrawRect(rect, color);
            
            GUILayout.Space(space);
        }
        internal static void ArrayButtonField<T>(ref T[] type, string label = "", string tooltip = "", bool copyLast = false)
        {
            GUILayout.BeginHorizontal();
            
            if (tooltip == "") GUILayout.Label(("<b>"+label+"</b>"),LabelStyle);
            else GUILayout.Label(("<b>"+label+"</b>").ToContent(tooltip), LabelStyle);

            if (GUILayout.Button("+".ToContent("Add"), GUILayout.Width(30)))
            {
                Array.Resize(ref type, type.Length + 1);

                if (copyLast && type.Length > 1) type[type.Length - 1] = type[type.Length - 2];
            }
            if (GUILayout.Button("-".ToContent("Remove"), GUILayout.Width(30)))
            {
                Array.Resize(ref type, Mathf.Max(0,type.Length - 1));
            }
            if (GUILayout.Button("R".ToContent("Reverse"), GUILayout.Width(30)))
            {
                type = type.Reverse().ToArray();
            }
            if (GUILayout.Button("C".ToContent("Clear"), GUILayout.Width(30)))
            {
                type = new T[] {};
            }
            
            GUILayout.EndHorizontal();
            
            GUILine(Aqua.Alpha(.5f), 1, 1);
        }
        internal static void ListButtonField<T>(ref List<T> type, string label = "", string tooltip = "", bool copyLast = false)
        {
            // setup add and remove button
            GUILayout.BeginHorizontal();

            if (tooltip == "") GUILayout.Label(("<b>"+label+"</b>"),LabelStyle);
            else GUILayout.Label(("<b>"+label+"</b>").ToContent(tooltip), LabelStyle);

            if (GUILayout.Button("+".ToContent("Add"), GUILayout.Width(30)))
            {
                type.Add(default);

                if (copyLast && type.Count > 1) type[type.Count - 1] = type[type.Count - 2];
            }
            if (GUILayout.Button("-".ToContent("Remove"), GUILayout.Width(30)))
            {
                if (type.Count > 0) type.RemoveAt(type.Count-1);
            }
            if (GUILayout.Button("R".ToContent("Reverse"), GUILayout.Width(30)))
            {
                type.Reverse();
            }
            if (GUILayout.Button("C".ToContent("Clear"), GUILayout.Width(30)))
            {
                type = new List<T>();
            }
            GUILayout.EndHorizontal();
            
            GUILine(Aqua.Alpha(.5f), 1, 1);
        }
        internal static void ArrayField<T>(ref T[] type, string label = "", string tooltip = "", Action<T> element = null) where T : Object
        {
            GUILayout.BeginVertical(BoxStyle);
            
            ArrayButtonField(ref type, label == "" ? typeof(T).Name : label, tooltip);
            
            for (var i = 0; i < type?.Length; i++)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label($"{i+1}:".ToContent($"Array Index: {i}"), GUILayout.Width(20));
                
                var x = EditorGUILayout.ObjectField(type[i], typeof(T), true); // array member to dynamic object field

                type[i] = x as T; // safe cast x to Type

                if (type[i]) // add component logo when array member not null
                {
                    GUILayout.Button(EditorGUIUtility.ObjectContent(type[i], type[i].GetType()).image,
                        GUILayout.Width(20), GUILayout.Height(20));
                }
                
                GUI.enabled = i != 0;
                if (GUILayout.Button("↑".ToContent("Move Up"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    type.Swap(i, i-1);  
                }

                GUI.enabled = i != type.Length - 1;
                if (GUILayout.Button("↓".ToContent("Move Down"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    type.Swap(i, i+1);
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                element?.Invoke(type[i]);
            }

            GUILayout.EndVertical();
        }

        internal static void PropertyArrayField(SerializedProperty arrayProp, GUIContent arrayLabel, Func<int, GUIContent> label)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Button(arrayLabel,LabelStyle);

            if (arrayProp.arraySize > 0 && GUILayout.Button("-".ToContent("Remove"), GUILayout.Width(30)))
            {
                arrayProp.DeleteArrayElementAtIndex(arrayProp.arraySize - 1);
            }
            if (GUILayout.Button("+".ToContent("Add"), GUILayout.Width(30)))
            {
                arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            }
            
            GUILayout.EndHorizontal();
            for (var i = 0; i < arrayProp.arraySize; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(arrayProp.GetArrayElementAtIndex(i), label.Invoke(i));
                GUILayout.EndHorizontal();
            }
        }
        internal static IEnumerable<T> EnumerableField<T>(IEnumerable<T> enumerable, string label = "")
        {
            GUILayout.BeginHorizontal();

            var list = enumerable.ToList();

            if (GUILayout.Button("Add "+label))
            {
                list.Add(default);
            }
            if (GUILayout.Button("Remove "+ label) && list.Count > 0)
            {
                list.RemoveAt(list.Count-1);
            }
            GUILayout.EndHorizontal();

            if (list.Count <= 0) return list;
            
            GUILayout.BeginVertical(BoxStyle);
            
            switch (list)
            {
                case List<Vector3> vector3s:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        vector3s[i] = EditorGUILayout.Vector3Field("Point " + i, vector3s[i]);
                    }
                    break;
                }
                case List<Vector2> vector2s:
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        vector2s[i] = EditorGUILayout.Vector2Field("Point " + i, vector2s[i]);
                    }
                    break;
                }
            }
            GUILayout.EndVertical();

            return list;
        }
    }

    [CustomEditor(typeof(Info))]
    public sealed class ExampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            var info = target as Info;
            GUI.color = Color.white;
            GUI.backgroundColor = RCProEditor.Violet;
            GUI.contentColor = RCProEditor.Aqua;
            RCProEditor.HeaderField(info.header);
            EditorGUILayout.LabelField(info.description,RCProEditor.BoxStyle);
        }
    }
}

#endif
