using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetConnectionAnimationActiveToggleItem : ContextItem
    {
        Toggle _toggle;

        List<Connection> _connList = new List<Connection>();

        public override void OnChangeSelection()
        {
            _connList.Clear();
            bool _show = false;
            foreach (ISelectable item in UICSystemManager.selectedElements)
            {
                if (item is Connection)
                {
                    _connList.Add(item as Connection);
                    _show = true;
                }
            }

            if (_connList.Count == 1)
                _toggle.isOn = _connList[0].line.animation.isActive;

            gameObject.SetActive(_show);
        }

        private void SetIsOn(bool arg0)
        {
            foreach (Connection conn in _connList)
            {
                conn.line.animation.isActive = _toggle.isOn;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _toggle = GetComponentInChildren<Toggle>();
        }

        void OnEnable()
        {
            _toggle.onValueChanged.AddListener(SetIsOn);
        }

        void OnDisable()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }
    }
}