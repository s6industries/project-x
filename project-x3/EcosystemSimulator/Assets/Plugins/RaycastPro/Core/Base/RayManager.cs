using System;
using System.Collections.Generic;

namespace RaycastPro
{
    using System.Linq;
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [ExecuteInEditMode]
    [AddComponentMenu("RaycastPro/Utility/" + nameof(RayManager))]
    public class RayManager : RaycastCore
    {
        [SerializeField] private RaycastCore[] cores;

        [SerializeField] private bool[] Foldouts = Array.Empty<bool>();

        public override bool Performed
        {
            get => cores.All(r => r.Performed);
            protected set { }
        }

        [ExecuteAlways]
        protected void OnTransformChildrenChanged()
        {
            Refresh();
        }
        

        protected void Refresh()
        {
            cores = GetComponentsInChildren<RaycastCore>(true).Where(c => !(c is RayManager)).ToArray();
            Array.Resize(ref Foldouts, cores.Length);
        }
        protected void Reset()
        {
            Refresh();
            
            styleH = new GUIStyle
            {
                margin = new RectOffset(0, 0, 4, 4),
                padding = new RectOffset(0, 0, 2, 4),
                stretchWidth = false,
                wordWrap = true,
            };

            styleM = new GUIStyle
            {
                margin = new RectOffset(1, 1, 4, 4),
                padding = new RectOffset(5, 5, 4, 4),
                alignment = TextAnchor.MiddleCenter, wordWrap = true
            };
        }

        private GUIStyle styleH, styleM;
        protected override void OnCast()
        {
            
        }

#if UNITY_EDITOR

#pragma warning disable CS0414
        private static string Info = "The ray control and management tool automatically detects children rays."+HUtility+HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        { }
        
        [SerializeField]
        private bool showMain = true;
        [SerializeField]
        private bool showGeneral = false;

        private int index;
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            BeginVerticalBox();
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(showMain)));
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(showGeneral)));
            EndVertical();
            
            for (index = 0; index < cores.Length; index++)
            {
                var core = cores[index];
                
                BeginVerticalBox();
                EditorGUILayout.BeginHorizontal();
                var guiStyle = new GUIStyle(EditorStyles.foldout)
                {
                     margin = new RectOffset(10, 10, 0, 5)
                };
                
                Foldouts[index] = EditorGUILayout.Foldout(Foldouts[index], core.name.ToContent(RCProEditor.GetInfo(core)), guiStyle);

                var _t = EditorGUIUtility.labelWidth;
                InLabelWidth(() =>
                {
                    cores[index].gameObject.SetActive(EditorGUILayout.ToggleLeft("A".ToContent(), cores[index].gameObject.activeInHierarchy, GUILayout.Width(30)));
                    cores[index].enabled = EditorGUILayout.ToggleLeft("E".ToContent(), cores[index].enabled, GUILayout.Width(30));
                }, 15);
                if (cores[index].gameObject.activeInHierarchy)
                {
                    var _cSO = new SerializedObject(cores[index]);
                    _cSO.Update();
                    var prop = _cSO.FindProperty("gizmosUpdate");
                    
                    if (GUILayout.Button(cores[index].gizmosUpdate.ToString(), GUILayout.Width(60f)))
                    {
                        switch (cores[index].gizmosUpdate)
                        {
                            case GizmosMode.Select:
                                prop.enumValueIndex = 0;
                                break;
                            case GizmosMode.Auto:
                                prop.enumValueIndex = 2;
                                break;
                            case GizmosMode.Fix:
                                prop.enumValueIndex = 3;
                                break;
                            case GizmosMode.Off:
                                prop.enumValueIndex = 1;
                                break;
                        }
                        _cSO.ApplyModifiedProperties();
                    }
                }
                else
                {
                    GUILayout.Box("Off", RCProEditor.BoxStyle, GUILayout.Width(60), GUILayout.Height(20));
                }

                GUI.backgroundColor = (core.Performed ? DetectColor : BlockColor).Alpha(.4f);
                if (GUILayout.Button("Cast", GUILayout.Width(60f)))
                {
                    core.TestCast();
                }

                
                //GUILayout.Box(raySensor.Performed ? "<color=#61FF38>✓</color>" : "<color=#FF3822>x</color>", RCProEditor.BoxStyle, GUILayout.Width(40), GUILayout.Height(20));
                EndHorizontal();
                GUI.backgroundColor = RCProEditor.Violet;
                
            if (Foldouts[index])
            {
                var _cSO = new SerializedObject(core);
                _cSO.Update();
                EditorGUI.BeginChangeCheck();
                core.EditorPanel(_cSO, showMain, showGeneral, false, false);
                if (EditorGUI.EndChangeCheck()) _cSO.ApplyModifiedProperties();
            }
            
            EndVertical();
            }
        }
#endif
    }
}