using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    [CustomEditor(typeof(ConnectionLabelRule), true), CanEditMultipleObjects]
    public class Editor_ConnectionLabelRule : Editor
    {
        public override void OnInspectorGUI()
        {
            ConnectionLabelRule labelRule = (ConnectionLabelRule)target;

            ConnectionLabelRule.warnMultipleRulesInScene = EditorGUILayout.Toggle("Warn Multiple Rules In Scene",ConnectionLabelRule.warnMultipleRulesInScene);

            DrawDefaultInspector();
        }
    }
}