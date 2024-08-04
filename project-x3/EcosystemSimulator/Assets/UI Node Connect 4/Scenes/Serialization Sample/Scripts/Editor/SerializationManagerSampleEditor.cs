using System.Collections;
using System.Collections.Generic;
using MeadowGames.UINodeConnect4.UICSerialization;
using UnityEditor;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.SerializationSample
{
    [CustomEditor(typeof(SerializationManagerSample)), CanEditMultipleObjects]
    public class SerializationManagerSampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializationManagerSample serializationManager = (SerializationManagerSample)target;

            serializationManager.graphManager = (GraphManager)EditorGUILayout.ObjectField("Graph Manager", serializationManager.graphManager, typeof(GraphManager), true);
            serializationManager.deserializationTemplates = (DeserializationTemplates)EditorGUILayout.ObjectField("Templates", serializationManager.deserializationTemplates, typeof(DeserializationTemplates), true);

            serializationManager.saveFileName = EditorGUILayout.TextField("Save File Name", serializationManager.saveFileName);

            if (GUILayout.Button("Save All"))
            {
                serializationManager.SaveGraph();
            }

            if (GUILayout.Button("Save Selected"))
            {
                serializationManager.SaveSelected();
            }

            if (GUILayout.Button("Load"))
            {
                serializationManager.LoadGraph();
            }
        }
    }
}