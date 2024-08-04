using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class RemoveElementsButtonItem : ContextItem
    {
        Button _button;

        public void RemoveSelected()
        {
            for (int i = UICSystemManager.selectedElements.Count - 1; i >= 0; i--)
            {
                UICSystemManager.selectedElements[i].Remove();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _button = GetComponent<Button>();
        }

        void OnEnable()
        {
            _button.onClick.AddListener(RemoveSelected);
        }

        void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        public override void OnChangeSelection()
        {
            gameObject.SetActive(UICSystemManager.selectedElements.Count > 0);
        }
    }
}