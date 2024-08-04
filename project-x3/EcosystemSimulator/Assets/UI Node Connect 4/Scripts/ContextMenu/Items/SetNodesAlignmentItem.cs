using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using MeadowGames.UINodeConnect4.Extension;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetNodesAlignmentItem : ContextItem
    {
        List<Node> _nodesList = new List<Node>();

        public Button alignVertical;
        public Button distributeEvenCenterVertical;
        public Button distributeEvenSpaceVertical;

        public Button alignHorizontal;
        public Button distributeEvenCenterHorizontal;
        public Button distributeEvenSpaceHorizontal;

        protected override void Awake()
        {
            base.Awake();
            _nodesList.Clear();
        }

        void OnEnable()
        {
            alignVertical.onClick.AddListener(NodesAlignment.AlignVertical);
            distributeEvenCenterVertical.onClick.AddListener(NodesAlignment.DistributeEvenCenterVertical);
            distributeEvenSpaceVertical.onClick.AddListener(NodesAlignment.DistributeEvenSpaceVertical);

            alignHorizontal.onClick.AddListener(NodesAlignment.AlignHorizontal);
            distributeEvenCenterHorizontal.onClick.AddListener(NodesAlignment.DistributeEvenCenterHorizontal);
            distributeEvenSpaceHorizontal.onClick.AddListener(NodesAlignment.DistributeEvenSpaceHorizontal);
        }

        void OnDisable()
        {
            alignVertical.onClick.RemoveAllListeners();
            distributeEvenCenterVertical.onClick.RemoveAllListeners();
            distributeEvenSpaceVertical.onClick.RemoveAllListeners();

            alignHorizontal.onClick.RemoveAllListeners();
            distributeEvenCenterHorizontal.onClick.RemoveAllListeners();
            distributeEvenSpaceHorizontal.onClick.RemoveAllListeners();
        }

        public override void OnChangeSelection()
        {
            _nodesList.Clear();
            bool _show = false;
            foreach (ISelectable item in UICSystemManager.selectedElements)
            {
                if (item is Node)
                {
                    _nodesList.Add(item as Node);
                }
            }
            if (_nodesList.Count > 1)
                _show = true;

            gameObject.SetActive(_show);
        }
    }
}