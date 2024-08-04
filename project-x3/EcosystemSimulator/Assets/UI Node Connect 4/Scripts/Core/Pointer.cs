using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MeadowGames.UINodeConnect4.UICContextMenu;

namespace MeadowGames.UINodeConnect4
{
    [System.Serializable]
    public class Pointer
    {
        GraphManager _graphManager;

        [Header("Optional Settings")]
        public Image customImage;

        public Sprite iconDefault;
        public Sprite iconOnDrag;

        public Canvas exclusiveOnDragCanvas;

        [Header("Legacy (requires Exclusive OnDrag Canvas)")]
        public bool useLegacyDragMethod = false;

        public bool ImageIsActive => customImage && customImage.IsActive();

        [HideInInspector] public Vector3 position;

        public void Initialize(GraphManager graphManager)
        {
            _graphManager = graphManager;

            if (ImageIsActive)
            {
                Cursor.visible = false;
                customImage.raycastTarget = false;

                Canvas _pointerCanvas = customImage.GetComponent<Canvas>() ? customImage.GetComponent<Canvas>() : customImage.gameObject.AddComponent<Canvas>();
                _pointerCanvas.overrideSorting = true;
                _pointerCanvas.sortingOrder = 999;
            }
        }

        public void OnEnable()
        {
            UICSystemManager.AddToUpdate(OnUpdate);

            InputManager.Instance?.e_OnPointerDown.AddListener(OnPointerDown);
            InputManager.Instance?.e_OnDrag.AddListener(OnDrag);
            InputManager.Instance?.e_OnPointerUp.AddListener(OnPointerUp);
            InputManager.Instance?.e_OnDelete.AddListener(OnDeleteKeyPressed);
            InputManager.Instance?.e_OnPointerHover.AddListener(OnPointerHover);
        }

        public void OnDisable()
        {
            UICSystemManager.RemoveFromUpdate(OnUpdate);

            InputManager.Instance?.e_OnPointerDown.RemoveListener(OnPointerDown);
            InputManager.Instance?.e_OnDrag.RemoveListener(OnDrag);
            InputManager.Instance?.e_OnPointerUp.RemoveListener(OnPointerUp);
            InputManager.Instance?.e_OnDelete.RemoveListener(OnDeleteKeyPressed);
            InputManager.Instance?.e_OnPointerHover.RemoveListener(OnPointerHover);
        }

        void OnUpdate()
        {
            position = InputManager.Instance.GetCanvasPointerPosition(_graphManager);

            if (ImageIsActive)
            {
                customImage.transform.position = position;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    customImage.sprite = iconDefault;
#endif
            }

            if (useLegacyDragMethod && exclusiveOnDragCanvas)
                exclusiveOnDragCanvas.transform.position = position;
        }

        public void OnPointerDown()
        {
            if (ImageIsActive)
                customImage.sprite = iconOnDrag;

            IElement clickedElement = GetElementCloserToPointer();
            if (clickedElement != null)
                UICSystemManager.clickedElement = clickedElement;

            if (!(UICSystemManager.clickedElement is IContextItem))
            {
                _graphManager.UnselectAllElements();

                if (UICSystemManager.clickedElement is IClickable)
                {
                    (UICSystemManager.clickedElement as IClickable).OnPointerDown();
                }
                UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnPointerDown, UICSystemManager.clickedElement);
            }
        }

        public void OnDrag()
        {
            IDraggable draggable = UICSystemManager.clickedElement as IDraggable;
            draggable?.OnDrag();

            UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnDrag, UICSystemManager.clickedElement);
        }

        public void OnPointerUp()
        {
            if (ImageIsActive)
                customImage.sprite = iconDefault;

            if (UICSystemManager.clickedElement is IClickable)
                (UICSystemManager.clickedElement as IClickable).OnPointerUp();

            foreach (IElement element in UICSystemManager.selectedElements)
            {
                if (element is IClickable)
                    (element as IClickable).OnPointerUp();
            }

            UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnPointerUp, UICSystemManager.clickedElement);

            UICSystemManager.clickedElement = null;
        }

        public void OnDeleteKeyPressed()
        {
            for (int i = UICSystemManager.selectedElements.Count - 1; i >= 0; i--)
            {
                UICSystemManager.selectedElements[i].Remove();
            }

            UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnDeleteKeyPressed, UICSystemManager.clickedElement);
        }

        IElement _lastHoverElement;
        public void OnPointerHover()
        {
            IElement hoverElement = GetElementCloserToPointer();
            UICSystemManager.hoverElement = hoverElement;
            if (hoverElement != _lastHoverElement)
            {
                if (hoverElement is IHover)
                {
                    (hoverElement as IHover).OnPointerHoverEnter();
                    UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnPointerHoverEnter, hoverElement);
                }

                if (_lastHoverElement is IHover)
                {
                    (_lastHoverElement as IHover).OnPointerHoverExit();
                    UICSystemManager.UICEvents.TriggerEvent(UICEventType.OnPointerHoverExit, _lastHoverElement);
                }
            }

            _lastHoverElement = hoverElement;
        }

        public IElement GetElementCloserToPointer()
        {
            InputManager inputManager = InputManager.Instance;

            List<IElement> orderedElementsList = Raycaster.OrderedElementsAtPosition(_graphManager, inputManager.ScreenPointerPosition, inputManager.GetCanvasPointerPosition(_graphManager));

            if (orderedElementsList.Count > 0)
            {
                return orderedElementsList[0];
            }

            return null;
        }

        public Port RaycastPortOfOppositPolarity(Port draggedPort)
        {
            Port closestPort = null;

            List<RaycastResult> results = Raycaster.RaycastUIAll(InputManager.Instance.ScreenPointerPosition);
            IElement element = null;
            foreach (RaycastResult result in results)
            {
                element = result.gameObject.GetComponent<IElement>();

                if (element != null)
                {
                    if (!(element is IClickable) || !(element as IClickable).DisableClick)
                        if (element is Port)
                        {
                            Port port = (element as Port);
                            if (draggedPort != port && port.HasSpots && draggedPort.HasSpots)
                                if ((port.node == draggedPort.node && port.node.enableSelfConnection) || port.node != draggedPort.node)
                                    if (port.Polarity != draggedPort.Polarity || port.Polarity == Port.PolarityType._all)
                                    {
                                        // v4.1 - RaycastPortOfOppositPolarity updated to use PortMatchRule if exists in scene
                                        if (Extension.PortMatchRule.Instance?.ExecuteRule(draggedPort, port) == false)
                                        {
                                            return null;
                                        }

                                        return port;
                                    }
                        }
                }
            }
            return closestPort;
        }

    }
}