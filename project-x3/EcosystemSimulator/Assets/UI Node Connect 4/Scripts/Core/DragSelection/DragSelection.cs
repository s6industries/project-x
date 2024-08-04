using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.GraphicRenderer;

namespace MeadowGames.UINodeConnect4
{
    [System.Serializable]
    public class DragSelection
    {
        public GraphicRenderer.UICLineRenderer customRenderer;
        public Line line;

        GraphManager _graphManager;
        GraphicRenderer.UICLineRenderer _LineRenderer
        {
            get
            {
                return customRenderer ? customRenderer : _graphManager.lineRenderer;
            }
        }

        public void Initialize(GraphManager graphManager)
        {
            _graphManager = graphManager;
        }

        public void OnEnable()
        {
            UICSystemManager.UICEvents.StartListening(UICEventType.OnPointerDown, OnPointerDown);
            UICSystemManager.UICEvents.StartListening(UICEventType.OnDrag, OnDrag);
            UICSystemManager.UICEvents.StartListening(UICEventType.OnPointerUp, OnPointerUp);
        }

        public void OnDisable()
        {
            UICSystemManager.UICEvents.StopListening(UICEventType.OnPointerDown, OnPointerDown);
            UICSystemManager.UICEvents.StopListening(UICEventType.OnDrag, OnDrag);
            UICSystemManager.UICEvents.StopListening(UICEventType.OnPointerUp, OnPointerUp);
        }

        void DrawSelectionLine()
        {
            line.Draw(_LineRenderer);
        }

        Vector3 firstPosition;
        Vector3 pointerPos;
        Vector3 firstPositionCanvas;
        Vector3 pointerPosCanvas;
        public void OnPointerDown(IElement element)
        {
            if (element == null)
            {
                _LineRenderer.OnPopulateMeshAddListener(DrawSelectionLine);

                firstPositionCanvas = InputManager.Instance.GetCanvasPointerPosition(_graphManager);

                firstPosition = UICUtility.ConvertPointsToRenderMode(_graphManager, firstPositionCanvas);

                canSelect = true;
            }
        }

        public void OnPointerUp(IElement element)
        {
            if (canSelect && didDrag && line.points.Count > 0)
            {
                Vector4 selectionV4Rect = GetSelectionV4Rect(firstPosition, pointerPos);
                Vector4 selectionV4RectCanvas = GetSelectionV4Rect(firstPositionCanvas, pointerPosCanvas);

                foreach (Node node in UICSystemManager.Nodes)
                {
                    RectTransform rt = node.rectTransform;

                    bool isInside = rt.RectIsInsideSelection(selectionV4RectCanvas);
                    if (isInside)
                    {
                        (node as ISelectable).Select();
                    }
                }

                foreach (Connection connection in UICSystemManager.Connections)
                {
                    bool isInside = connection.ConnectionIsInsideSelection(selectionV4Rect);
                    if (isInside)
                    {
                        (connection as ISelectable).Select();
                    }
                }
            }

            _LineRenderer.OnPopulateMeshRemoveListener(DrawSelectionLine);

            line.points.Clear();
            didDrag = false;
            canSelect = false;
        }

        bool canSelect;
        bool didDrag;
        public void OnDrag(IElement element)
        {
            didDrag = true;

            pointerPosCanvas = InputManager.Instance.GetCanvasPointerPosition(_graphManager);

            pointerPos = UICUtility.ConvertPointsToRenderMode(_graphManager, pointerPosCanvas);

            line.SetPoints(new Vector2[]{
                firstPosition,
                new Vector2(pointerPos.x, firstPosition.y),
                pointerPos,
                new Vector2(firstPosition.x, pointerPos.y),
                firstPosition
            });
        }

        Vector4 GetSelectionV4Rect(Vector2 firstPosition, Vector2 pointerPos)
        {
            Vector4 rectPoints = Vector4.zero;
            if (firstPosition.x < pointerPos.x)
            {
                rectPoints.x = firstPosition.x;
                rectPoints.z = pointerPos.x;
            }
            else
            {
                rectPoints.z = firstPosition.x;
                rectPoints.x = pointerPos.x;
            }

            if (firstPosition.y > pointerPos.y)
            {
                rectPoints.y = firstPosition.y;
                rectPoints.w = pointerPos.y;
            }
            else
            {
                rectPoints.w = firstPosition.y;
                rectPoints.y = pointerPos.y;
            }

            return rectPoints;
        }
    }
}