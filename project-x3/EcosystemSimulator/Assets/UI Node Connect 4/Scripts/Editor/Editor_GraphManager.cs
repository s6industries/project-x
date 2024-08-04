using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.EditorScript
{
    [CustomEditor(typeof(GraphManager))]
    public class Editor_GraphManager : Editor
    {
        GraphManager graphManager;

        public override void OnInspectorGUI()
        {
            graphManager = (GraphManager)target;
            DrawDefaultInspector();
        }

        Port _portClicked = null;
        bool editConnections = true;
        private void OnSceneGUI()
        {
            GraphManager graphManager = (GraphManager)target;

            if (!UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(graphManager.gameObject.scene))
            {
                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(0, 0, 150, 150));
                {
                    editConnections = GUILayout.Toggle(editConnections, "Edit Connections");
                    if (editConnections)
                    {
                        if (_portClicked)
                            GUILayout.Box("Current " + _portClicked.name);
                    }
                }
                GUILayout.EndArea();
                Handles.EndGUI();

                if (editConnections)
                {
                    UICSystemManager.UpdateNodeList();
                    foreach (Node node in UICSystemManager.Nodes)
                    {
                        foreach (Port port in node.ports)
                        {
                            Vector3 position = port.transform.position + Vector3.up * 2f;
                            float size = 20 * graphManager.transform.localScale.x;

                            Handles.color = Color.green;

                            if (Handles.Button(position, Quaternion.identity, size, size, Handles.RectangleHandleCap))
                            {
                                if (_portClicked == port)
                                {
                                    _portClicked = null;
                                }
                                else if (_portClicked)
                                {
                                    Connection.NewConnection(_portClicked, port);

                                    _portClicked = null;
                                    EditorApplication.QueuePlayerLoopUpdate();
                                }
                                else
                                {
                                    _portClicked = port;
                                }
                            }
                        }

                        if (_portClicked)
                        {
                            Vector3 mousePosition = Event.current.mousePosition;
                            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                            mousePosition = ray.origin;
                            Vector3 position = _portClicked.transform.position + Vector3.up * 2f;
                            Handles.DrawBezier(position, mousePosition, _portClicked.controlPoint.Position + Vector3.up * 2f, mousePosition, Color.grey, null, 2);
                        }


                        for (int i = 0; i < UICSystemManager.Connections.Count; i++)
                        {
                            Connection connection = UICSystemManager.Connections[i];
                            Vector3 position = (Vector3)connection.line.LerpLine(0.5f).Item1 + Vector3.up * 2f;

                            position = UICUtility.ScreenToWorldPointsForRenderMode(graphManager, position);

                            float size = 20 * graphManager.transform.localScale.x;

                            Handles.color = Color.red;

                            if (Handles.Button(position, Quaternion.identity, size, size, Handles.RectangleHandleCap))
                            {
                                connection.Remove();
                                EditorApplication.QueuePlayerLoopUpdate();
                            }
                        }
                    }
                }
            }
        }
    }
}