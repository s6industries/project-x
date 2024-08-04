using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4
{
    public class Raycaster
    {
        static PointerEventData _pointerEventData;

        // v4.1 - removed unecessary GraphManager parameter from Raycaster.RaycastUIAll method
        public static List<RaycastResult> RaycastUIAll(Vector3 position)
        {
            if (_pointerEventData == null)
                _pointerEventData = new PointerEventData(null);

            _pointerEventData.position = position;
            List<RaycastResult> resultsLocal = new List<RaycastResult>();
            List<RaycastResult> results = new List<RaycastResult>();

            List<GraphicRaycaster> _raycasterList = new List<GraphicRaycaster>();

            if (UICSystemManager.Instance.CacheRaycasters)
            {
                _raycasterList = UICSystemManager.raycasterList;
            }
            else
            {
                _raycasterList.Clear();
                _raycasterList.AddRange(GameObject.FindObjectsOfType<GraphicRaycaster>());
            }

            foreach (GraphicRaycaster gr in _raycasterList)
            {
                gr.Raycast(_pointerEventData, resultsLocal);
                results.AddRange(resultsLocal);
            }

            return results;
        }

        public static List<IElement> OrderedElementsAtPosition(GraphManager graphManager, Vector3 screenPosition, Vector3 canvasPosition)
        {
            IElement element = null;
            List<IElement> orderedElements = new List<IElement>();

            List<RaycastResult> results = RaycastUIAll(screenPosition);
            foreach (RaycastResult result in results)
            {
                element = result.gameObject.GetComponent<IElement>();

                if (element != null)
                {
                    if (!(element is IClickable) || !(element as IClickable).DisableClick)
                        orderedElements.Add(element);
                }
            }

            Vector3 convertedPosition = UICUtility.ConvertPointsToRenderMode(graphManager, canvasPosition);

            element = FindClosestConnectionToPosition(convertedPosition, graphManager.connectionDetectionDistance);

            if (element != null)
                if (!(element as IClickable).DisableClick)
                    orderedElements.Add(element);

            orderedElements.Sort(UICUtility.SortByPriority);

            return orderedElements;
        }

        public static Connection FindClosestConnectionToPosition(Vector3 position, float maxDistance)
        {
            float minDist = Mathf.Infinity;
            Connection closestConnection = null;
            foreach (GraphManager graphManager in UICSystemManager.graphManagers)
            {
                foreach (Connection connection in UICSystemManager.Connections)
                {
                    float distance = UICUtility.DistanceToConnection(connection, position, maxDistance);
                    if (distance < minDist)
                    {
                        closestConnection = connection;
                        minDist = distance;
                    }
                }
            }

            return closestConnection;
        }

    }
}
