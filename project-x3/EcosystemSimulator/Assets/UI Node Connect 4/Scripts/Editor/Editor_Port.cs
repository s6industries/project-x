using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeadowGames.UINodeConnect4.EditorScript
{
    [CustomEditor(typeof(Port)), CanEditMultipleObjects]
    public class Editor_Port : Editor
    {
        Port port;

        Vector3 controlPointLocalPosition;
        float controlPointDistance;
        float controlPointAngle;

        void OnEnable()
        {
            if (port == null)
                port = (Port)target;

            UpdateControlPointViewValues(port);
        }

        public override void OnInspectorGUI()
        {
            port = (Port)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();
            GUILayout.Label("Control Point", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 100;
                controlPointLocalPosition = EditorGUILayout.Vector3Field("Local Position", controlPointLocalPosition);
                if (GUILayout.Button("Apply", GUILayout.Width(50)))
                {
                    foreach (Port port in targets)
                    {
                        Undo.RecordObject(port.controlPoint.transform, "set control point position");
                        port.SetControlPointLocalPosition(controlPointLocalPosition);

                        UpdateControlPointViewValues(port);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUIUtility.labelWidth = 80;
                controlPointDistance = EditorGUILayout.FloatField("Distance", controlPointDistance);
                EditorGUIUtility.labelWidth = 80;
                controlPointAngle = EditorGUILayout.FloatField("Angle", controlPointAngle);
                if (GUILayout.Button("Apply", GUILayout.Width(50)))
                {
                    foreach (Port port in targets)
                    {
                        Undo.RecordObject(port.controlPoint.transform, "set control point position");
                        port.SetControlPointDistanceAngle(controlPointDistance, controlPointAngle);

                        UpdateControlPointViewValues(port);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnSceneGUI()
        {
            Port port = (Port)target;
            if (port)
            {
                Vector3 portPosition = port.transform.position;
                Vector3 portControlPointPosition = port.controlPoint.Position;

                Handles.DrawDottedLine(portPosition, portControlPointPosition, 2);
                Handles.DrawSolidDisc(port.controlPoint.Position, Vector3.forward, 10 * port.node.graphManager.transform.localScale.x);
                Handles.color = Color.black;
                Vector3 offset = new Vector3(1, 0, 0);
                Handles.DrawDottedLine(portPosition - offset, portControlPointPosition - offset, 2);
                Handles.DrawSolidDisc(port.controlPoint.Position, Vector3.forward, 8 * port.node.graphManager.transform.localScale.x);
                Handles.color = Color.white;
            }
        }

        void UpdateControlPointViewValues(Port port)
        {
            controlPointLocalPosition = port.controlPoint.LocalPosition;
            controlPointDistance = port.controlPoint.LocalPosition.magnitude;
            controlPointAngle = port.GetControlPointAngle();
        }
    }
}