using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class Node : MonoBehaviour, IGraphElement, ISelectable, IDraggable, IClickable, IHover
    {
        [SerializeField] string _id = "node";
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

        public List<Port> ports = new List<Port>();

        [HideInInspector] public RectTransform rectTransform;

        Outline _outline;
        Image _image;
        public Image Image
        {
            get
            {
                if (!_image)
                    _image = GetComponent<Image>();
                return _image;
            }
            set => _image = value;
        }

        public GraphManager graphManager;
        Pointer _Pointer => graphManager?.pointer;

        // v4.1 - pointer selection priority of Node and Connection switched to improve usability (Nodes are now selected before Connections)
        public int Priority => 1;

        public bool enableSelfConnection = true;
        [SerializeField] bool _enableDrag = true;
        public bool EnableDrag { get => _enableDrag; set => _enableDrag = value; }
        [SerializeField] bool _enableHover = true;
        public bool EnableHover { get => _enableHover; set => _enableHover = value; }
        [SerializeField] bool _enableSelect = true;
        public bool EnableSelect { get => _enableSelect; set => _enableSelect = value; }
        public Color ElementColor { get => Image.color; set => Image.color = value; }
        [SerializeField] bool _disableClick = false;
        public bool DisableClick { get => _disableClick; set => _disableClick = value; }

        public Color defaultColor;
        public Color outlineSelectedColor = new Color(1, 0.58f, 0.04f);
        public Color outlineHoverColor = new Color(1, 0.81f, 0.3f);

        Transform _lastParent;

        void OnValidate()
        {
            graphManager = GetComponentInParent<GraphManager>();
        }

        void Awake()
        {
            _sID = UICUtility.GenerateSID();

            graphManager = GetComponentInParent<GraphManager>();
            _lastParent = transform.parent;

            Initialize();
        }

        void Initialize()
        {
            _outline = _outline ? _outline : gameObject.GetComponent<Outline>() ? gameObject.GetComponent<Outline>() : gameObject.AddComponent<Outline>();
            if (_outline)
            {
                _outline.effectColor = outlineSelectedColor;
                _outline.effectDistance = new Vector2(3, -3);
                _outline.enabled = false;
            }

            if (_id == "")
                _id = name;
        }

        public void UpdateConnectionsLine()
        {
            foreach (Port port in ports)
            {
                List<Connection> connectionsList = port.Connections;
                foreach (Connection conn in connectionsList)
                {
                    conn.UpdateLine();
                }
            }
        }

        public void Select()
        {
            if (EnableSelect)
            {
                _outline.effectColor = outlineSelectedColor;
                _outline.enabled = true;
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
                _outline.enabled = false;
                if (UICSystemManager.selectedElements.Contains(this))
                {
                    UICSystemManager.selectedElements.Remove(this);
                    UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnElementUnselected, this);
                }
            }
        }

        void OnDestroy()
        {
            Unselect();

            UICSystemManager.RemoveNodeFromList(this);

            if (UICSystemManager.clickedElement == this as IElement)
                UICSystemManager.clickedElement = null;
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

        public void OnPointerDown()
        {
            if (!UICSystemManager.selectedElements.Contains(this))
            {
                Select();
                _lastParent = transform.parent;

                transform.SetAsLastSibling();
            }
            else
            {
                Unselect();
            }
        }

        public void OnPointerUp()
        {
            _dragStart = true;

            if (_Pointer?.exclusiveOnDragCanvas)
                transform.SetParent(_lastParent);

            UpdateConnectionsLine();
        }

        bool _dragStart = true;
        Vector3 _distanceFromPointer;
        public void OnDrag()
        {
            if (EnableDrag)
            {
                Select();
                if (_Pointer?.exclusiveOnDragCanvas)
                {
                    if (_dragStart)
                    {
                        transform.SetParent(_Pointer.exclusiveOnDragCanvas.transform);
                    }
                }
                if (!_Pointer.useLegacyDragMethod)
                {
                    if (_dragStart)
                    {
                        _dragStart = false;
                        _distanceFromPointer = transform.position - _Pointer.position;
                    }
                    transform.position = _Pointer.position + _distanceFromPointer;
                }
                UpdateConnectionsLine();

                // drag all selected nodes
                if ((Node)UICSystemManager.clickedElement == this)
                {
                    foreach (ISelectable obj in UICSystemManager.selectedElements)
                    {
                        Node nodeObj = obj as Node;

                        if (nodeObj && nodeObj != this)
                        {
                            (obj as IDraggable).OnDrag();
                        }
                    }
                }
            }
        }

        public void OnPointerHoverEnter()
        {
            if (EnableHover)
            {
                _outline.effectColor = outlineHoverColor;
                _outline.enabled = true;
            }
        }
        public void OnPointerHoverExit()
        {
            if (EnableHover)
            {
                // is selected
                if (UICSystemManager.selectedElements.Contains(this))
                {
                    _outline.effectColor = outlineSelectedColor;
                }
                else // is not selected
                {
                    _outline.enabled = false;
                }
            }
        }

        public List<Node> GetConnectedNodes()
        {
            List<Node> connectedEntities = new List<Node>();
            foreach (Port port in ports)
            {
                List<Connection> connectionsList = port.Connections;
                foreach (Connection conn in connectionsList)
                {
                    Node connEntitiy = conn.port0 != port ? conn.port0.node : conn.port1.node;
                    connectedEntities.Add(connEntitiy);
                }
            }
            return connectedEntities;
        }

        // method to get nodes connected to an specific connection point polarity
        public List<Node> GetNodesConnectedToPolarity(Port.PolarityType polarity)
        {
            List<Node> connectedEntities = new List<Node>();
            foreach (Port port in ports)
            {
                if (port.Polarity == polarity)
                {
                    List<Connection> connectionsList = port.Connections;
                    foreach (Connection conn in connectionsList)
                    {
                        Node connEntitiy = conn.port0 != port ? conn.port0.node : conn.port1.node;
                        connectedEntities.Add(connEntitiy);
                    }
                }
            }
            return connectedEntities;
        }

        //v3.2 - bugfix: drag selection not working after adding node from Hierarchy
        void OnEnable()
        {
            defaultColor = ElementColor;
            rectTransform = transform as RectTransform;

            UICSystemManager.AddNodeToList(this);
        }

        //v3.2 - bugfix: missing reference when removing node from Hierarchy
        void OnDisable()
        {
            Unselect();

            UICSystemManager.RemoveNodeFromList(this);
        }
    }
}
