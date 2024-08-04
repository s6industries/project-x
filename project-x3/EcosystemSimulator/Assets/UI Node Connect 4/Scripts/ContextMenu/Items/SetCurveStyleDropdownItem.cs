using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetCurveStyleDropdownItem : ContextItem
    {
        TMP_Dropdown _dropdown;

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

            SetDropdown();

            gameObject.SetActive(_show);
        }

        [SerializeField] List<Connection> _connList = new List<Connection>();

        private void SetCurveStyle(int arg0)
        {
            Connection.CurveStyle parsedCurveType = (Connection.CurveStyle)System.Enum.Parse(typeof(Connection.CurveStyle), _dropdown.options[_dropdown.value].text);

            foreach (Connection conn in _connList)
            {
                conn.curveStyle = parsedCurveType;
                conn.UpdateLine();
            }
        }

        void SetDropdown()
        {
            if (_connList.Count == 1)
            {
                _dropdown.value = _dropdown.options.FindIndex(option => option.text == _connList[0].curveStyle.ToString());
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _connList.Clear();
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
            _connList.Clear();
            _dropdown.onValueChanged.RemoveAllListeners();
        }
    }
}