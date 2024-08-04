using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MeadowGames.UINodeConnect4.GraphicRenderer;

namespace MeadowGames.UINodeConnect4
{
    [DefaultExecutionOrder(-10)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class GraphManager : MonoBehaviour
    {
        public Camera mainCamera;
        public UICLineRenderer lineRenderer;

        public List<Node> localNodes = new List<Node>();
        public List<Connection> localConnections = new List<Connection>();

        Canvas _canvas;
        public Canvas Canvas
        {
            get
            {
                if (!_canvas)
                    _canvas = GetComponent<Canvas>();

                return _canvas;
            }
            set => _canvas = value;
        }
        RectTransform _canvasRectTransform;
        public RectTransform CanvasRectTransform => _canvasRectTransform;
        public RenderMode CanvasRenderMode => Canvas.renderMode;
        public Pointer pointer = new Pointer();
        public DragSelection dragSelection = new DragSelection();
        public Line ghostConnectionLine;
        public Connection newConnectionTemplate;
        public ConnectionLabel connectionLabelTemplate;

        public bool replaceConnectionByReverse;
        public float connectionDetectionDistance = 15;

        void OnEnable()
        {
            UICSystemManager uicSystemManager = FindObjectOfType<UICSystemManager>();
            if (!uicSystemManager)
            {
                GameObject uicSystemManagerGO = new GameObject("UIC4 System Manager");
                uicSystemManager = uicSystemManagerGO.AddComponent<UICSystemManager>();
            }

            InputManager inputManager = FindObjectOfType<InputManager>();
            if (!inputManager)
            {
                uicSystemManager.gameObject.AddComponent<InputManager_LegacyInputSystem>();
            }

            if (!lineRenderer)
            {
                lineRenderer = GetComponentInChildren<UICLineRenderer>();
                if (!lineRenderer)
                {
                    lineRenderer = new GameObject("UIC4 Line Renderer").AddComponent<UICLineRenderer>();
                    lineRenderer.transform.SetParent(transform);
                }
            }

            dragSelection.OnEnable();
            pointer.OnEnable();

            lineRenderer?.OnPopulateMeshAddListener(DrawConnections);

            if (!UICSystemManager.graphManagers.Contains(this))
            {
                UICSystemManager.graphManagers.Add(this);
            }

            UICSystemManager.UICEvents.StartListening(UICEventType.NodeAdded, AddLocalNode);
            UICSystemManager.UICEvents.StartListening(UICEventType.NodeRemoved, RemoveLocalNode);
            UICSystemManager.UICEvents.StartListening(UICEventType.ConnectionAdded, AddLocalConnection);
            UICSystemManager.UICEvents.StartListening(UICEventType.ConnectionRemoved, RemoveLocalConnection);
        }

        void OnDisable()
        {
            dragSelection.OnDisable();
            pointer.OnDisable();

            if (UICSystemManager.graphManagers.Contains(this))
            {
                UICSystemManager.graphManagers.Remove(this);
            }

            UICSystemManager.UICEvents.StopListening(UICEventType.NodeAdded, AddLocalNode);
            UICSystemManager.UICEvents.StopListening(UICEventType.NodeRemoved, RemoveLocalNode);
            UICSystemManager.UICEvents.StopListening(UICEventType.ConnectionAdded, AddLocalConnection);
            UICSystemManager.UICEvents.StopListening(UICEventType.ConnectionRemoved, RemoveLocalConnection);
        }

        void AddLocalNode(IElement obj)
        {
            Node node = obj as Node;
            if (node && node.graphManager == this)
            {
                if (!localNodes.Contains(node))
                {
                    localNodes.Add(node);
                }
            }
        }
        void RemoveLocalNode(IElement obj)
        {
            Node node = obj as Node;
            if (node && node.graphManager == this)
            {
                if (localNodes.Contains(node))
                {
                    localNodes.Remove(node);
                }
            }
        }
        void UpdateLocalNodes()
        {
            localNodes.Clear();
            if (UICSystemManager.Instance)
            {
                foreach (Node node in UICSystemManager.Nodes)
                {
                    AddLocalNode(node);
                }
            }
        }

        void AddLocalConnection(IElement obj)
        {
            Connection connection = obj as Connection;
            if (connection != null && connection.graphManager == this)
            {
                if (!localConnections.Contains(connection))
                {
                    localConnections.Add(connection);
                }
            }
        }
        void RemoveLocalConnection(IElement obj)
        {
            Connection connection = obj as Connection;
            if (connection != null && connection.graphManager == this)
            {
                if (localConnections.Contains(connection))
                {
                    localConnections.Remove(connection);
                }
            }
        }
        void UpdateLocalConnections()
        {
            localConnections.Clear();
            foreach (Connection connection in UICSystemManager.Connections)
            {
                AddLocalConnection(connection);
            }
        }

        void OnValidate()
        {
            Awake();

            if (newConnectionTemplate.ID != "") newConnectionTemplate.ID = "";
            if (newConnectionTemplate.port0 != null) newConnectionTemplate.port0 = null;
            if (newConnectionTemplate.port1 != null) newConnectionTemplate.port1 = null;
            if (newConnectionTemplate.label != null) newConnectionTemplate.label = null;
            if (newConnectionTemplate.line.points.Count > 0) newConnectionTemplate.line.points.Clear();
            if (ghostConnectionLine.points.Count > 0) ghostConnectionLine.points.Clear();
            if (dragSelection.line.points.Count > 0) dragSelection.line.points.Clear();
            if (newConnectionTemplate.line.color != newConnectionTemplate.defaultColor) newConnectionTemplate.line.color = newConnectionTemplate.defaultColor;

        }

        void Awake()
        {
            if (!mainCamera)
                mainCamera = Camera.main;

            if (!connectionLabelTemplate)
            {
                connectionLabelTemplate = Resources.Load("Connection Label Template", typeof(GameObject)) as ConnectionLabel;
            }

            Canvas = GetComponent<Canvas>();
            _canvasRectTransform = Canvas.transform as RectTransform;

            pointer.Initialize(this);
            dragSelection.Initialize(this);

            UpdateLocalNodes();
            UpdateLocalConnections();
        }

        void Start()
        {
            UpdateLocalNodes();
            UpdateLocalConnections();
        }

        // instantiates Node at position
        public Node InstantiateNode(Node nodeTemplate, Vector3 position)
        {
            Node newNode = Instantiate(nodeTemplate.gameObject, new Vector3(200, 100), Quaternion.identity, Canvas.transform).GetComponent<Node>();

            // instantiate node at world space position 
            if (CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                newNode.transform.position = position + new Vector3(15, 15, 0);
            }
            else if (CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                position.z = 0;
                newNode.transform.localPosition = position + new Vector3(1, 1, 0);
            }
            else if (CanvasRenderMode == RenderMode.WorldSpace)
            {
                position.z = 0;
                newNode.transform.localPosition = position + new Vector3(1, 1, 0);
                newNode.transform.localRotation = Quaternion.identity;
            }

            if (nodeTemplate.transform.parent)
            {
                newNode.transform.SetParent(nodeTemplate.transform.parent);
                newNode.transform.SetAsLastSibling();
            }

            return newNode;
        }

        public void UnselectAllElements()
        {
            if (!InputManager.Instance.Aux0KeyPress)
            {
                for (int i = UICSystemManager.selectedElements.Count - 1; i >= 0; i--)
                {
                    UICSystemManager.selectedElements[i].Unselect();
                }
            }
        }

        public void RemoveUIElement()
        {
            for (int i = UICSystemManager.selectedElements.Count - 1; i >= 0; i--)
            {
                UICSystemManager.selectedElements[i].Remove();
            }
        }

        void DrawConnections()
        {
            foreach (Connection connection in localConnections)
            {
                connection.line.Draw(lineRenderer);

#if UNITY_EDITOR
                if (!Application.isPlaying && connection.line.color != connection.defaultColor)
                    connection.line.color = connection.defaultColor;
#endif
            }
        }

    }

}
