using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetConnectionWidthInputItem : ContextItem
    {
        TMP_InputField _inputFieldStart;
        TMP_InputField _inputFieldEnd;

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

            if (_connList.Count != 1)
            {
                _inputFieldStart.text = "-";
                _inputFieldEnd.text = "-";
            }
            else
            {
                _inputFieldStart.text = _connList[0].line.startWidth.ToString();
                _inputFieldEnd.text = _connList[0].line.endWidth.ToString();
            }

            gameObject.SetActive(_show);
        }

        private void SetWidth(string arg0)
        {
            foreach (Connection conn in _connList)
            {
                if (_inputFieldStart.text != "-")
                {
                    float w = conn.line.startWidth;
                    if (float.TryParse(_inputFieldStart.text, NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                        conn.line.startWidth = w;
                }
                if (_inputFieldEnd.text != "-")
                {
                    float w = conn.line.endWidth;
                    if (float.TryParse(_inputFieldEnd.text, NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                        conn.line.endWidth = w;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _inputFieldStart = transform.GetChild(1).GetComponent<TMP_InputField>();
            _inputFieldEnd = transform.GetChild(2).GetComponent<TMP_InputField>();
        }

        void OnEnable()
        {
            // v4.0.1 - bugfix: SetWidth being called OnElementSelected and changing the width of the selected Connections 
            _inputFieldStart.onEndEdit.AddListener(SetWidth);
            _inputFieldEnd.onEndEdit.AddListener(SetWidth);
        }

        void OnDisable()
        {
            // v4.0.1 - bugfix: SetWidth being called OnElementSelected and changing the width of the selected Connections 
            _inputFieldStart.onEndEdit.RemoveAllListeners();
            _inputFieldEnd.onEndEdit.RemoveAllListeners();
        }
    }
}