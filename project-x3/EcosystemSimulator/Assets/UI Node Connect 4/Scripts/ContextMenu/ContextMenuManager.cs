using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class ContextMenuManager : MonoBehaviour
    {
        [SerializeField] GraphManager _graphManager;
        public GraphManager GraphManager
        {
            get
            {
                if (!_graphManager)
                    _graphManager = GetComponentInParent<GraphManager>();
                return _graphManager;
            }
            set => _graphManager = value;
        }
        List<IContextItem> contextItemList;

        void Awake()
        {
            if (!GraphManager)
                GraphManager = GetComponentInParent<GraphManager>();

            contextItemList = new List<IContextItem>();
            contextItemList.AddRange(GetComponentsInChildren<IContextItem>());
        }

        void OnEnable()
        {
            UICSystemManager.UICEvents.StartListening(UICEventType.OnElementSelected, UpdateContextMenu);
            UICSystemManager.UICEvents.StartListening(UICEventType.OnElementUnselected, UpdateContextMenu);
        }
        void OnDisable()
        {
            UICSystemManager.UICEvents.StopListening(UICEventType.OnElementSelected, UpdateContextMenu);
            UICSystemManager.UICEvents.StopListening(UICEventType.OnElementUnselected, UpdateContextMenu);
        }

        void Start()
        {
            UpdateContextMenu();
        }

        public void UpdateContextMenu(IElement element = null)
        {
            StartCoroutine(C_UpdateContextMenu());
        }

        IEnumerator C_UpdateContextMenu()
        {
            yield return new WaitForEndOfFrame();

            if (contextItemList != null)
            {
                foreach (IContextItem item in contextItemList)
                {
                    item.OnChangeSelection();
                }
            }
        }
    }
}
