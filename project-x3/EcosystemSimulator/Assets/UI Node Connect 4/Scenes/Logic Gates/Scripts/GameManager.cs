using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.SampleScene.LogicGates
{
    public class GameManager : MonoBehaviour
    {
        public GraphManager graphManager;
        public Node pointNode;

        [SerializeField] Toggle toggleMovePoints;
        [SerializeField] Toggle toggleAddPoints;
        [SerializeField] Toggle toggleHelp;

        void OnEnable()
        {
            UICSystemManager.AddToUpdate(OnUpdate);
        }

        void OnDisable()
        {
            UICSystemManager.RemoveFromUpdate(OnUpdate);
        }

        void Solve()
        {
            for (int i = 0; i < graphManager.localNodes.Count; i++)
            {
                SolveRecursive(graphManager.localNodes[i]);
            }

            UpdateConnectionsState();
        }

        // recursive solve allows gate loops and locks 
        void SolveRecursive(Node node)
        {
            Gate gate = node.GetComponent<Gate>();
            foreach (Node otherNode in node.GetNodesConnectedToPolarity(Port.PolarityType._in))
            {
                if (!gate.solved.Contains(otherNode))
                {
                    gate.solved.Add(otherNode);
                    SolveRecursive(otherNode);
                }
            }

            gate.Solve();
        }

        void UpdateConnectionsState()
        {
            foreach (Connection connection in graphManager.localConnections)
            {
                if (connection.port0.node.GetComponent<Gate>().Output)
                {
                    connection.line.animation.isActive = true;
                }
                else
                {
                    connection.line.animation.isActive = false;
                }
            }
        }

        void OnUpdate()
        {
            Solve();

            HandleMovePoints();

            HandleCreateAndDestroyPoints();
        }

        public void SetMovePointsMode(bool value)
        {
            if (value == true)
            {
                foreach (Node node in UICSystemManager.Nodes)
                {
                    foreach (Port port in node.ports)
                    {
                        port.DisableClick = true;
                    }

                    Gate gate = node.GetComponent<Gate>();
                    if (!(gate is PointGate))
                    {
                        node.DisableClick = true;
                        node.ElementColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
                    }
                }
                foreach (Connection connection in UICSystemManager.Connections)
                {
                    connection.DisableClick = true;
                }
            }
            else
            {
                foreach (Node node in UICSystemManager.Nodes)
                {
                    foreach (Port port in node.ports)
                    {
                        port.DisableClick = false;
                    }

                    Gate gate = node.GetComponent<Gate>();
                    if (!(gate is PointGate))
                    {
                        node.DisableClick = false;
                        node.ElementColor = node.defaultColor;
                    }
                }
                foreach (Connection connection in UICSystemManager.Connections)
                {
                    connection.DisableClick = false;
                }
            }
        }

        void HandleMovePoints()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                toggleMovePoints.isOn = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                toggleMovePoints.isOn = false;
            }
        }

        void HandleCreateAndDestroyPoints()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                toggleAddPoints.isOn = true;
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
            {
                toggleAddPoints.isOn = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) || (toggleAddPoints.isOn && Input.GetKeyDown(KeyCode.Mouse0)))
            {
                IElement element = graphManager.pointer.GetElementCloserToPointer();
                if (element != null)
                {
                    Connection connection = element as Connection;
                    if (connection != null)
                    {
                        Node point = Instantiate(pointNode, InputManager.Instance.GetCanvasPointerPosition(graphManager), Quaternion.identity, graphManager.transform) as Node;
                        Port p0 = connection.port0;
                        Port p1 = connection.port1;
                        connection.Remove();
                        Connection.NewConnection(p0, point.ports[0]);
                        Connection.NewConnection(point.ports[0], p1);
                    }

                    Port port = element as Port;
                    if (port != null)
                    {
                        if (port.node.ID == "POINT")
                        {
                            List<Port> outPorts = new List<Port>();
                            List<Port> inPorts = new List<Port>();
                            foreach (Connection conn in port.Connections)
                            {
                                if (conn.port1 == port)
                                    outPorts.Add(conn.port0);
                                else
                                    inPorts.Add(conn.port1);
                            }
                            port.node.Remove();
                            foreach (Port outPort in outPorts)
                            {
                                foreach (Port inPort in inPorts)
                                {
                                    if (outPort.node != inPort.node)
                                        Connection.NewConnection(outPort, inPort);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}