using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.EditorScript
{
    [CustomEditor(typeof(Node)), CanEditMultipleObjects]
    public class Editor_Node : Editor
    {
        public override void OnInspectorGUI()
        {
            Node node = (Node)target;

            DrawDefaultInspector();
        }

        void OnSceneGUI()
        {
            Node node = (Node)target;

            foreach (Port port in node.ports)
            {
                if (port && node.graphManager)
                {
                    Vector3 portPosition = port.transform.position;
                    Vector3 portControlPointPosition = port.controlPoint.Position;

                    Handles.DrawDottedLine(portPosition, portControlPointPosition, 2);
                    Handles.DrawSolidDisc(port.controlPoint.Position, Vector3.forward, 10 * node.graphManager.transform.localScale.x);
                    Handles.color = Color.black;
                    Vector3 offset = new Vector3(1, 0, 0);
                    Handles.DrawDottedLine(portPosition - offset, portControlPointPosition - offset, 2);
                    Handles.DrawSolidDisc(port.controlPoint.Position, Vector3.forward, 8 * node.graphManager.transform.localScale.x);
                    Handles.color = Color.white;
                }
            }
        }
    }
}