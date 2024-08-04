using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetElementsColorItem : ContextItem
    {
        Transform _colorsPanel;
        Button[] _colorButtons;

        public override void OnChangeSelection()
        {
            gameObject.SetActive(UICSystemManager.selectedElements.Count > 0);
        }

        void SetColor(Button _button)
        {
            foreach (IGraphElement obj in UICSystemManager.selectedElements)
            {
                obj.ElementColor = _button.image.color;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _colorsPanel = transform.GetChild(1);
            _colorButtons = new Button[_colorsPanel.childCount];

            for (int i = 0; i < _colorButtons.Length; i++)
            {
                _colorButtons[i] = _colorsPanel.GetChild(i).GetComponent<Button>();
            }
        }

        void OnEnable()
        {
            foreach (Button button in _colorButtons)
            {
                button.onClick.AddListener(delegate { SetColor(button); });
            }
        }

        void OnDisable()
        {
            foreach (Button button in _colorButtons)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }
}