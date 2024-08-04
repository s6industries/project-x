using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.GraphicRenderer;

namespace MeadowGames.UINodeConnect4
{
    [System.Serializable]
    public class Connection : IGraphElement, ISelectable, IClickable, IDraggable, IHover
    {
        public enum CurveStyle { Spline, Z_Shape, Soft_Z_Shape, Line }

        [SerializeField] string _id = "Connection";
        public string ID
        {
            get => _id;
            set => _id = value;
        }

        // v4.1 - added SID variable to facilitate serialization
        [HideInInspector] [SerializeField] string _sID = "";
        public string SID
        {
            get
            {
                if (_sID == "")
                {
                    _sID = UICUtility.GenerateSID();
                }
                return _sID;
            }
            set => _sID = value;
        }

        public GraphManager graphManager;

        public Port port0; // start
        public Port port1; // end

        public Color selectedColor = new Color(1, 0.58f, 0.04f);
        public Color hoverColor = new Color(1, 0.81f, 0.3f);
        public Color defaultColor = new Color(0.98f, 0.94f, 0.84f);

        public CurveStyle curveStyle = CurveStyle.Soft_Z_Shape;

        public ConnectionLabel label;

        public Line line;

        // v4.1 - now the Connection's ElementColor (current color) is set independetly from the defaultColor (color the elements comes back when unselect or hover exit)
        public Color ElementColor
        {
            get => line.color;
            set
            {
                line.color = value;
            }
        }

        // v4.1 - pointer selection priority of Node and Connection switched to improve usability (Nodes are now selected before Connections)
        public int Priority => 0;

        [SerializeField] bool _enableDrag = true;
        public bool EnableDrag { get => _enableDrag; set => _enableDrag = value; }
        [SerializeField] bool _enableHover = true;
        public bool EnableHover { get => _enableHover; set => _enableHover = value; }
        [SerializeField] bool _enableSelect = true;
        public bool EnableSelect { get => _enableSelect; set => _enableSelect = value; }
        [SerializeField] bool _disableClick = false;
        public bool DisableClick { get => _disableClick; set => _disableClick = value; }

        // v4.1 - Connection constructor made public to facilitate serialization
        public Connection()
        {
            _sID = UICUtility.GenerateSID();
        }

        public static Connection NewConnection(Port port0, Port port1)
        {
            GraphManager graphManager = port0.graphManager;
            Connection connectionTemplate = graphManager.newConnectionTemplate.Clone();
            return NewConnection(port0, port1, connectionTemplate);
        }

        // v4.1 - adde Connection.NewConnection overload method that can receive a template connection as parameter so the style is copied
        public static Connection NewConnection(Port port0, Port port1, Connection connectionTemplate)
        {
            GraphManager graphManager = port0.graphManager;

            Connection previousConnectionWithSamePort = Connection.GetConnection(port0, port1);
            if (previousConnectionWithSamePort != null)
            {
                if (graphManager.replaceConnectionByReverse)
                {
                    previousConnectionWithSamePort.Remove();
                }
                else
                {
                    return previousConnectionWithSamePort;
                }
            }

            Connection _connection = connectionTemplate.Clone();

            if ((port1.Polarity == Port.PolarityType._out && (port0.Polarity == Port.PolarityType._in || port0.Polarity == Port.PolarityType._all))
                || (port1.Polarity == Port.PolarityType._all && port0.Polarity == Port.PolarityType._in))
            {
                _connection.port0 = port1;
                _connection.port1 = port0;
            }
            else
            {
                _connection.port0 = port0;
                _connection.port1 = port1;
            }
            _connection.graphManager = port0.graphManager;

            UICSystemManager.AddConnectionToList(_connection);

            _connection.UpdateLine(true);

            _connection.ID = string.Format("Connection ({0} - {1})", port0.node ? port0.node.name : "null", port1.node ? port1.node.name : "null");

            UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnConnectionCreated, _connection);

            return _connection;
        }

        public static Connection GetConnection(Port port0, Port port1)
        {
            foreach (Connection connection in UICSystemManager.Connections)
            {
                if ((port0 == connection.port0 && port1 == connection.port1) ||
                        (port0 == connection.port1 && port1 == connection.port0))
                    return connection;
            }
            return null;
        }

        public void OnPointerDown()
        {
            if (!UICSystemManager.selectedElements.Contains(this))
            {
                Select();
            }
            else
            {
                Unselect();
            }
        }

        public void OnPointerUp()
        {
            _dragStart = true;
        }

        public void Remove()
        {
            Unselect();

            UICSystemManager.RemoveConnectionFromList(this);

            port0.UpdateIcon();
            port1.UpdateIcon();

            if (label)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    GameObject.Destroy(label.gameObject);
#if UNITY_EDITOR
                else
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (label) GameObject.DestroyImmediate(label.gameObject);
                    };
                }
#endif
            }

            if (UICSystemManager.clickedElement == this as IElement)
                UICSystemManager.clickedElement = null;

            UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnConnectionRemoved, this);
        }

        public void Select()
        {
            if (EnableSelect)
            {
                line.color = selectedColor;
                if (!UICSystemManager.selectedElements.Contains(this))
                {
                    UICSystemManager.selectedElements.Add(this);
                    UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnElementSelected, this);
                }
            }
        }

        public void Unselect()
        {
            if (EnableSelect)
            {
                line.color = defaultColor;
                if (UICSystemManager.selectedElements.Contains(this))
                {
                    UICSystemManager.selectedElements.Remove(this);
                    UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnElementUnselected, this);
                }
            }
        }

	// v4.1 - added "updatePortsIcons" optional parameter to Connection.UpdateLine
        public void UpdateLine(bool updatePortsIcons = true)
        {
            if (graphManager)
            {
                Vector3[] linePoints = UICUtility.WorldToScreenPointsForRenderMode(graphManager, new Vector3[] {
                    port0.transform.position,
                    port0.controlPoint.Position,
                    port1.controlPoint.Position,
                    port1.transform.position });

                Vector2[] newPoints = LineUtils.ConvertLinePointsToCurve(linePoints, curveStyle);

                line.SetPoints(newPoints);

                if (label)
                {
                    label.UpdateLabel(line);
                }

                // v4.1 - bugfix: icons not being updated when the connection was created from script
                if (updatePortsIcons)
                {
                    port0.UpdateIcon();
                    port1.UpdateIcon();
                }
            }
        }

        public List<Connection> GetCrossedConnections()
        {
            List<Connection> crossedConnections = new List<Connection>();

            foreach (Connection conn in UICSystemManager.Connections)
            {
                if (UICUtility.DoConnectionsIntersect(conn, this))
                    if (!(conn.port0 == port0 || conn.port1 == port1 || conn.port0 == port1 || conn.port1 == port0))
                        crossedConnections.Add(conn);
            }

            return crossedConnections;
        }

        bool _dragStart = true;
        Port otherPort;
        public void OnDrag()
        {
            if (EnableDrag)
            {
                if (_dragStart)
                {
                    _dragStart = false;

                    // check closest port from pointer
                    Port clossestPort = port0;
                    otherPort = port1;
                    float distance = Vector3.Distance(port0.transform.position, InputManager.Instance.GetCanvasPointerPosition(graphManager));
                    if (distance > Vector3.Distance(port1.transform.position, InputManager.Instance.GetCanvasPointerPosition(graphManager)))
                    {
                        clossestPort = port1;
                        otherPort = port0;
                    }

                    UICSystemManager.clickedElement = otherPort;
                    otherPort.OnPointerDown();
                    otherPort.OnDrag();
                    Remove();
                }
            }
        }

        public void OnPointerHoverEnter()
        {
            if (EnableHover)
            {
                line.color = hoverColor;
            }
        }
        public void OnPointerHoverExit()
        {
            if (EnableHover)
            {
                if (UICSystemManager.selectedElements.Contains(this))
                    line.color = selectedColor;
                else
                    line.color = defaultColor;
            }
        }

        public void SetLabel(string value)
        {
            if (label == null)
            {
                ConnectionLabel connectionLabel = GameObject.Instantiate(graphManager.connectionLabelTemplate, graphManager.lineRenderer.transform);
                connectionLabel.SetGraphManager(graphManager);
                label = connectionLabel;
            }

            label.text = value;
        }

        public void RemoveLabel()
        {
            if (label != null)
            {
                GameObject.Destroy(label.gameObject);
                label = null;
            }
        }

        public Connection Clone()
        {
            Connection newConnection = UICUtility.Clone(this);
            // v4.1 - bugfix: label from the original connections being transfered to the cloned connection
            newConnection.label = null;
            newConnection.ElementColor = defaultColor;

            return newConnection;
        }

        // v4.1 - Connection.CopyVariables method made obsolete. Use Clone() instead
        [System.Obsolete("Obsolete. Use Clone(this Connection) instead", true)]
        public Connection CopyVariables()
        {
            Connection newConnection = UICUtility.Clone(this);
            newConnection.label = null;

            return newConnection;
        }
    }
}
