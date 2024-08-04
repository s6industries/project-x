using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// this is only a simple example code to show the zoomming is possible with the correct hierarchy, please, use a more reliable code in your project 
namespace MeadowGames.UINodeConnect4.SampleScene.ScrollViewGraph
{
    public class Zoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        bool _mouseIsOver;
        public RectTransform contentRectTransform;
        public GraphManager graphManager;

        void Start()
        {
            graphManager = GetComponentInParent<GraphManager>();
        }

        void Update()
        {
            if (_mouseIsOver)
            {
                if (Input.mouseScrollDelta.y != 0)
                {
                    contentRectTransform.localScale += Vector3.one * Input.mouseScrollDelta.y * 0.2f;
                    graphManager.connectionDetectionDistance = 15 * contentRectTransform.localScale.x;
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseIsOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseIsOver = false;
        }
    }
}