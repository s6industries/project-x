using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class DefaultCurveStyleDropdownItem : ContextItem
    {
        TMP_Dropdown _dropdown;

        private void SetCurveStyle(int arg0)
        {
            Connection.CurveStyle parsedCurveStyle = (Connection.CurveStyle)System.Enum.Parse(typeof(Connection.CurveStyle), _dropdown.options[_dropdown.value].text);
            ContextMenu.GraphManager.newConnectionTemplate.curveStyle = parsedCurveStyle;
        }

        void SetDropdown()
        {
            _dropdown.value = _dropdown.options.FindIndex(option => option.text == ContextMenu.GraphManager.newConnectionTemplate.curveStyle.ToString());
        }

        protected override void Awake()
        {
            base.Awake();

            _dropdown = GetComponent<TMP_Dropdown>();

            string[] _names = System.Enum.GetNames(typeof(Connection.CurveStyle));
            List<string> _options = new List<string>();
            _options.AddRange(_names);

            _dropdown.options.Clear();
            _dropdown.AddOptions(_options);
        }

        void OnEnable()
        {
            SetDropdown();
            _dropdown.onValueChanged.AddListener(SetCurveStyle);
        }

        void OnDisable()
        {
            _dropdown.onValueChanged.RemoveAllListeners();
        }
    }
}