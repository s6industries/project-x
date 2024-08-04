using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    public class NodesAlignment
    {
        static List<Node> GetNodesFromSelected()
        {
            List<Node> nodes = new List<Node>();
            foreach (ISelectable selectable in UICSystemManager.selectedElements)
            {
                Node node = selectable as Node;
                if (node)
                {
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        public static void AlignVertical()
        {
            AlignVertical(GetNodesFromSelected());
        }
        public static void AlignVertical(List<Node> nodes)
        {
            float centerXPosition = 0;
            int numberOfNodes = 0;
            foreach (Node node in nodes)
            {
                centerXPosition += node.rectTransform.localPosition.x;
                numberOfNodes++;
            }
            if (numberOfNodes > 1)
            {
                centerXPosition = centerXPosition / numberOfNodes;

                foreach (Node node in nodes)
                {
                    node.rectTransform.localPosition = new Vector3(centerXPosition, node.rectTransform.localPosition.y, node.rectTransform.localPosition.z);
                    node.UpdateConnectionsLine();
                }
            }
        }

        public static void AlignHorizontal()
        {
            AlignHorizontal(GetNodesFromSelected());
        }
        public static void AlignHorizontal(List<Node> nodes)
        {
            float centerYPosition = 0;
            int numberOfNodes = 0;
            foreach (ISelectable selectable in nodes)
            {
                Node node = selectable as Node;
                if (node)
                {
                    centerYPosition += node.rectTransform.localPosition.y;
                    numberOfNodes++;
                }
            }
            if (numberOfNodes > 1)
            {
                centerYPosition = centerYPosition / numberOfNodes;

                foreach (ISelectable selectable in nodes)
                {
                    Node node = selectable as Node;
                    if (node)
                    {
                        node.rectTransform.localPosition = new Vector3(node.rectTransform.localPosition.x, centerYPosition, node.rectTransform.localPosition.z);
                        node.UpdateConnectionsLine();
                    }
                }
            }
        }

        public static void DistributeEvenCenterVertical()
        {
            DistributeEvenCenterVertical(GetNodesFromSelected());
        }
        public static void DistributeEvenCenterVertical(List<Node> nodes)
        {
            List<Node> orderedNodes = new List<Node>();
            int numberOfNodes = 0;
            foreach (ISelectable selectable in nodes)
            {
                Node node = selectable as Node;
                if (node)
                {
                    orderedNodes.Add(node);
                    numberOfNodes++;
                }
            }
            orderedNodes.Sort((n1, n2) => n1.rectTransform.localPosition.y.CompareTo(n2.rectTransform.localPosition.y));

            if (numberOfNodes > 1)
            {
                float space = (orderedNodes[orderedNodes.Count - 1].rectTransform.localPosition.y - orderedNodes[0].rectTransform.localPosition.y) /
                                (numberOfNodes - 1);

                float firstPos = orderedNodes[0].rectTransform.localPosition.y;
                int count = 0;
                foreach (Node node in orderedNodes)
                {
                    node.rectTransform.localPosition = new Vector3(node.rectTransform.localPosition.x, firstPos + (space * count), node.rectTransform.localPosition.z);
                    node.UpdateConnectionsLine();
                    count++;
                }
            }
        }

        public static void DistributeEvenCenterHorizontal()
        {
            DistributeEvenCenterHorizontal(GetNodesFromSelected());
        }
        public static void DistributeEvenCenterHorizontal(List<Node> nodes)
        {
            List<Node> orderedNodes = new List<Node>();
            int numberOfNodes = 0;
            foreach (ISelectable selectable in nodes)
            {
                Node node = selectable as Node;
                if (node)
                {
                    orderedNodes.Add(node);
                    numberOfNodes++;
                }
            }
            orderedNodes.Sort((n1, n2) => n1.rectTransform.localPosition.x.CompareTo(n2.rectTransform.localPosition.x));

            if (numberOfNodes > 1)
            {
                float space = (orderedNodes[orderedNodes.Count - 1].rectTransform.localPosition.x - orderedNodes[0].rectTransform.localPosition.x) /
                                (numberOfNodes - 1);

                float firstPos = orderedNodes[0].rectTransform.localPosition.x;
                int count = 0;
                foreach (Node node in orderedNodes)
                {
                    node.rectTransform.localPosition = new Vector3(firstPos + (space * count), node.rectTransform.localPosition.y, node.rectTransform.localPosition.z);
                    node.UpdateConnectionsLine();
                    count++;
                }
            }
        }

        public static void DistributeEvenSpaceVertical()
        {
            DistributeEvenSpaceVertical(GetNodesFromSelected());
        }
        public static void DistributeEvenSpaceVertical(List<Node> nodes)
        {
            List<Node> orderedNodes = new List<Node>();
            int numberOfNodes = 0;
            float nodesSize = 0;
            foreach (ISelectable selectable in nodes)
            {
                Node node = selectable as Node;
                if (node)
                {
                    orderedNodes.Add(node);
                    nodesSize += node.rectTransform.rect.height;
                    numberOfNodes++;
                }
            }
            orderedNodes.Sort((n1, n2) => n1.rectTransform.localPosition.y.CompareTo(n2.rectTransform.localPosition.y));

            if (numberOfNodes > 1)
            {
                float fullDistance = (orderedNodes[orderedNodes.Count - 1].rectTransform.localPosition.y - orderedNodes[0].rectTransform.localPosition.y) +
                                    (orderedNodes[orderedNodes.Count - 1].rectTransform.rect.height / 2) + (orderedNodes[0].rectTransform.rect.height / 2);

                float spaces = (fullDistance - nodesSize) / (numberOfNodes - 1);

                float firstPos = orderedNodes[0].rectTransform.localPosition.y;
                int count = 0;
                float acumulatedPos = firstPos - (orderedNodes[0].rectTransform.rect.height / 2);
                foreach (Node node in orderedNodes)
                {
                    node.rectTransform.localPosition = new Vector3(node.rectTransform.localPosition.x,
                                                                (node.rectTransform.rect.height / 2) + acumulatedPos,
                                                                node.rectTransform.localPosition.z);
                    acumulatedPos += node.rectTransform.rect.height + spaces;
                    node.UpdateConnectionsLine();
                    count++;
                }
            }
        }

        public static void DistributeEvenSpaceHorizontal()
        {
            DistributeEvenSpaceHorizontal(GetNodesFromSelected());
        }
        public static void DistributeEvenSpaceHorizontal(List<Node> nodes)
        {
            List<Node> orderedNodes = new List<Node>();
            int numberOfNodes = 0;
            float nodesSize = 0;
            foreach (ISelectable selectable in nodes)
            {
                Node node = selectable as Node;
                if (node)
                {
                    orderedNodes.Add(node);
                    nodesSize += node.rectTransform.rect.width;
                    numberOfNodes++;
                }
            }
            orderedNodes.Sort((n1, n2) => n1.rectTransform.localPosition.x.CompareTo(n2.rectTransform.localPosition.x));

            if (numberOfNodes > 1)
            {
                float fullDistance = (orderedNodes[orderedNodes.Count - 1].rectTransform.localPosition.x - orderedNodes[0].rectTransform.localPosition.x) +
                                    (orderedNodes[orderedNodes.Count - 1].rectTransform.rect.width / 2) + (orderedNodes[0].rectTransform.rect.width / 2);

                float spaces = (fullDistance - nodesSize) / (numberOfNodes - 1);

                float firstPos = orderedNodes[0].rectTransform.localPosition.x;
                int count = 0;
                float acumulatedPos = firstPos - (orderedNodes[0].rectTransform.rect.width / 2);
                foreach (Node node in orderedNodes)
                {
                    node.rectTransform.localPosition = new Vector3((node.rectTransform.rect.width / 2) + acumulatedPos,
                                                                node.rectTransform.localPosition.y,
                                                                node.rectTransform.localPosition.z);
                    acumulatedPos += node.rectTransform.rect.width + spaces;
                    node.UpdateConnectionsLine();
                    count++;
                }
            }
        }
    }
}