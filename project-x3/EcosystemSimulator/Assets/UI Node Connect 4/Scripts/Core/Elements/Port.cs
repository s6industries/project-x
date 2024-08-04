using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MeadowGames.UINodeConnect4.GraphicRenderer;

namespace MeadowGames.UINodeConnect4
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    // v4.1 - Port inheritance changed from Graphc to MaskableGraphic
    public class Port : MaskableGraphic, IGraphElement, IDraggable, IClickable, IHover
    {
        [SerializeField] string _id = "Port";
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

        [HideInInspector] public new RectTransform rectTransform;

        public GraphManager graphManager;
        [HideInInspector]
        public Node node;

        public enum PolarityType { _in, _out, _all };
        [SerializeField] PolarityType _polarity = PolarityType._all;
        public PolarityType Polarity
        {
            get
            {
                return _polarity;
            }
            set
            {
                _polarity = value;
            }
        }

        public int maxConnections = 0; // 0 - no limit

        public Sprite iconUnconnected;
        public Sprite iconConnected;

        public Color iconColorDefault = new Color(0.98f, 0.94f, 0.84f);
        public Color iconColorHover = new Color(1, 0.81f, 0.3f);
        public Color iconColorSelected = new Color(1, 0.58f, 0.04f);
        public Color iconColorConnected = new Color(0.98f, 0.94f, 0.84f);

        public Image image;

        [HideInInspector] public PortControlPoint controlPoint;

        // v4.1 - bugfix: ElementColor of Port not changing image color
        public Color ElementColor { get => image.color; set => image.color = value; }

        public int Priority => 2;
        [SerializeField] bool _enableDrag = true;
        public bool EnableDrag { get => _enableDrag; set => _enableDrag = value; }
        [SerializeField] bool _enableHover = true;
        public bool EnableHover { get => _enableHover; set => _enableHover = value; }
        [SerializeField] bool _disableClick = false;
        public bool DisableClick { get => _disableClick; set => _disableClick = value; }

        [HideInInspector] public Port lastFoundPort;
        Port closestFoundPort;

        public bool HasSpots
        {
            get
            {
                return maxConnections == 0 || ConnectionsCount < maxConnections;
            }
        }

        public List<Connection> Connections
        {
            get
            {
                List<Connection> connections = new List<Connection>();
                foreach (Connection connection in UICSystemManager.Connections)
                {
                    if (connection.port0 == this || connection.port1 == this)
                    {
                        connections.Add(connection);
                    }
                }
                return connections;
            }
        }

        public int ConnectionsCount
        {
            get
            {
                int count = 0;
                foreach (Connection connection in UICSystemManager.Connections)
                {
                    if (connection.port0 == this || connection.port1 == this)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
        protected override void OnPopulateMesh(VertexHelper vh) { }

        protected override void OnEnable()
        {
            base.OnEnable();

            rectTransform = transform as RectTransform;

            node = GetComponentInParent<Node>();
            if (node && !node.ports.Contains(this))
                node.ports.Add(this);

            graphManager = node?.graphManager;

            image = transform.GetComponentInChildren<Image>();
            if (!image)
            {
                image = new GameObject("Image", typeof(RectTransform)).AddComponent<Image>();
                image.transform.SetParent(transform);
                image.transform.localPosition = Vector3.zero;
                (image.transform as RectTransform).sizeDelta = new Vector2(20, 20);
                image.raycastTarget = false;
            }

            controlPoint = GetComponentInChildren<PortControlPoint>();
            if (!controlPoint)
            {
                controlPoint = new GameObject("Control Point", typeof(RectTransform)).AddComponent<PortControlPoint>();
                controlPoint.transform.SetParent(transform);
                (controlPoint.transform as RectTransform).sizeDelta = Vector2.zero;
                SetControlPointDistanceAngle(50, 180);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            graphManager?.lineRenderer?.OnPopulateMeshRemoveListener(DrawOnDragConnectionLine);

            node = GetComponentInParent<Node>();
            if (node && node.ports.Contains(this))
            {
                node.ports.Remove(this);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _sID = UICUtility.GenerateSID();
        }

        protected override void Start()
        {
            base.Start();

            if (_id == "")
                _id = name;

            UpdateIcon();

            StartCoroutine(C_UpdateLineOnStart());
        }

        IEnumerator C_UpdateLineOnStart()
        {
            yield return new WaitForEndOfFrame();
            foreach (Connection connection in Connections)
            {
                connection.UpdateLine(true);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!Application.isPlaying)
            {
                if (image)
                    image.color = iconColorDefault;

                UpdateIcon();
            }

            if (Polarity != _polarity)
                Polarity = _polarity;
        }
#endif

        public void UpdateIcon()
        {
            if (image)
            {
                if (ConnectionsCount > 0)
                {
                    image.sprite = iconConnected;
                    image.color = iconColorConnected;
                }
                else
                {
                    image.sprite = iconUnconnected;
                    image.color = iconColorDefault;
                }
            }
        }

        public Connection ConnectTo(Port otherPort)
        {
            return Connection.NewConnection(this, otherPort);
        }
        // v4.1 - adde Port.ConnectTo overload method that can receive a template connection as parameter so the style is copied
        public Connection ConnectTo(Port otherPort, Connection connectionTemplate)
        {
            return Connection.NewConnection(this, otherPort, connectionTemplate);
        }

        public void Remove()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                Destroy(gameObject);
#if UNITY_EDITOR
            }
            else
            {
                DestroyImmediate(gameObject);
            }
#endif
        }

        protected override void OnDestroy()
        {
            RemoveAllConnections();

            if (UICSystemManager.clickedElement == this as IElement)
                UICSystemManager.clickedElement = null;

            base.OnDestroy();
        }

        public void RemoveAllConnections()
        {
            List<Connection> connectionsList = Connections;
            for (int i = connectionsList.Count - 1; i >= 0; i--)
            {
                connectionsList[i].Remove();
            }

            UpdateIcon();
        }

        void DrawOnDragConnectionLine()
        {
            graphManager.ghostConnectionLine.Draw(graphManager.lineRenderer);
        }

        public void OnPointerDown()
        {
            if (image)
                image.color = iconColorSelected;
            graphManager.lineRenderer.OnPopulateMeshAddListener(DrawOnDragConnectionLine);
        }

        public void OnPointerUp()
        {
            if (image)
                image.color = iconColorDefault;
            graphManager.ghostConnectionLine.points.Clear();
            graphManager.lineRenderer.OnPopulateMeshRemoveListener(DrawOnDragConnectionLine);

            if (closestFoundPort && EnableDrag)
            {
                closestFoundPort.image.color = closestFoundPort.iconColorDefault;

                ConnectTo(closestFoundPort);
            }

            UpdateIcon();

            lastFoundPort = null;
            closestFoundPort = null;
        }

        public void OnDrag()
        {
            if (EnableDrag)
            {
                if (lastFoundPort)
                    lastFoundPort.UpdateIcon();

                closestFoundPort = graphManager.pointer.RaycastPortOfOppositPolarity(this);

                if (closestFoundPort)
                {
                    if (closestFoundPort.image)
                        closestFoundPort.image.color = closestFoundPort.iconColorSelected;
                    lastFoundPort = closestFoundPort;
                }

                Vector3 pointerPosition = InputManager.Instance.GetCanvasPointerPosition(graphManager);
                Vector3[] linePoints = UICUtility.WorldToScreenPointsForRenderMode(graphManager, new Vector3[] {
                    transform.position,
                    controlPoint.Position,
                    closestFoundPort ? closestFoundPort.controlPoint.Position : pointerPosition,
                    closestFoundPort ? closestFoundPort.transform.position : pointerPosition });

                Vector2[] newPoints = LineUtils.ConvertLinePointsToCurve(linePoints, graphManager.newConnectionTemplate.curveStyle);

                graphManager.ghostConnectionLine.SetPoints(newPoints);
            }
        }

        public void OnPointerHoverEnter()
        {
            if (EnableHover)
            {
                if (image)
                    image.color = iconColorHover;
            }
        }
        public void OnPointerHoverExit()
        {
            if (EnableHover)
            {
                if (image)
                    image.color = iconColorDefault;
            }
        }

        public List<Node> GetConnectedNodes()
        {
            List<Node> connectedEntities = new List<Node>();
            List<Connection> connectionsList = Connections;
            foreach (Connection conn in connectionsList)
            {
                Node connEntitiy = conn.port0 != this ? conn.port0.node : conn.port1.node;
                connectedEntities.Add(connEntitiy);
            }
            return connectedEntities;
        }

        public List<Connection> GetOutConnections()
        {
            List<Connection> outConnections = new List<Connection>();
            List<Connection> connectionsList = Connections;
            foreach (Connection conn in connectionsList)
            {
                if (conn.port0 == this)
                    outConnections.Add(conn);
            }
            return outConnections;
        }

        public List<Connection> GetInConnections()
        {
            List<Connection> outConnections = new List<Connection>();
            List<Connection> connectionsList = Connections;
            foreach (Connection conn in connectionsList)
            {
                if (conn.port1 == this)
                    outConnections.Add(conn);
            }
            return outConnections;
        }

        public Port GetOppositePort(Connection connection)
        {
            return connection.port0 == this ? connection.port1 : connection.port0;
        }

        public List<Port> GetConnectedPorts()
        {
            List<Port> otherPorts = new List<Port>();
            List<Connection> connectionsList = Connections;
            foreach (Connection connection in connectionsList)
            {
                Port otherPort = GetOppositePort(connection);

                if (otherPort)
                    otherPorts.Add(otherPort);
            }

            return otherPorts;
        }

        public void SetControlPointLocalPosition(Vector3 position)
        {
            controlPoint.LocalPosition = position;

            node?.UpdateConnectionsLine();
        }
        public void SetControlPointDistanceAngle(float distance, float angle)
        {
            var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
            var newPosition = transform.localPosition;
            newPosition.x = x;
            newPosition.y = y;
            controlPoint.LocalPosition = new Vector3(newPosition.x, newPosition.y, 0);

            node?.UpdateConnectionsLine();
        }

        public float GetControlPointAngle()
        {
            Vector2 Point_1 = transform.position;
            Vector2 Point_2 = controlPoint.Position;
            return Mathf.Atan2(Point_2.y - Point_1.y, Point_2.x - Point_1.x) * 180 / Mathf.PI;
        }

        /// <summary>
        /// Set the X position of the Port in relation to the node
        /// </summary>
        /// <param name="position">position or percent (if nodeSizeProportion = true)</param>
        /// <param name="nodeSizeProportion">flag to indicate if the given position is atual position value or percent based on width of the node size</param>
        public void SetLocalPositionX(float position, bool nodeSizeProportion = false)
        {
            float nodeWidth = node.rectTransform.rect.width;
            Vector3 pos = transform.localPosition;

            if (!nodeSizeProportion)
                pos.x = position;
            else // position value represents proportion of node width from 0 to 1
                pos.x = ((nodeWidth) * position) - (nodeWidth / 2);

            transform.localPosition = pos;

            node.UpdateConnectionsLine();
        }

        /// <summary>
        /// Set the Y position of the Port in relation to the node
        /// </summary>
        /// <param name="position">position or percent (if nodeSizeProportion = true)</param>
        /// <param name="nodeSizeProportion">flag to indicate if the given position is atual position value or percent based on width of the node size</param>
        public void SetLocalPositionY(float position, bool nodeSizeProportion = false)
        {
            float nodeHeight = node.rectTransform.rect.height;
            Vector3 pos = transform.localPosition;

            if (!nodeSizeProportion)
                pos.y = position;
            else // position value represents proportion of node height from 0 to 1
                pos.y = ((nodeHeight) * position) - (nodeHeight / 2);

            transform.localPosition = pos;

            node.UpdateConnectionsLine();
        }


    }
}
