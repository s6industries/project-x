using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class AddNodeButtonItem : ContextItem
    {
        Button _button;
        public Node node;

        public void AddNode()
        {
            Node newNode = ContextMenu.GraphManager.InstantiateNode(node, node.transform.position);
            ContextMenu.GraphManager.UnselectAllElements();
            newNode.Select();
        }

        protected override void Awake()
        {
            base.Awake();
            _button = GetComponent<Button>();
        }

        void OnEnable()
        {
            _button.onClick.AddListener(AddNode);
        }

        void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}