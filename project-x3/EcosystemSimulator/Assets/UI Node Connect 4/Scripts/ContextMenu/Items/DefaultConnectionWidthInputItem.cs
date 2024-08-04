using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using System.Globalization;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class DefaultConnectionWidthInputItem : ContextItem
    {
        TMP_InputField _inputFieldStart;
        TMP_InputField _inputFieldEnd;


        public override void OnChangeSelection()
        {
            _inputFieldStart.text = ContextMenu.GraphManager.newConnectionTemplate.line.startWidth.ToString();
            _inputFieldEnd.text = ContextMenu.GraphManager.newConnectionTemplate.line.endWidth.ToString();
        }

        private void SetWidth(string arg0)
        {
            float w = ContextMenu.GraphManager.newConnectionTemplate.line.startWidth;
            if (float.TryParse(_inputFieldStart.text, NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                ContextMenu.GraphManager.newConnectionTemplate.line.startWidth = w;

            w = ContextMenu.GraphManager.newConnectionTemplate.line.endWidth;
            if (float.TryParse(_inputFieldEnd.text, NumberStyles.Float, CultureInfo.InvariantCulture, out w))
                ContextMenu.GraphManager.newConnectionTemplate.line.endWidth = w;
        }

        protected override void Awake()
        {
            base.Awake();

            _inputFieldStart = transform.GetChild(1).GetComponent<TMP_InputField>();
            _inputFieldEnd = transform.GetChild(2).GetComponent<TMP_InputField>();
        }

        void OnEnable()
        {
            _inputFieldStart.onEndEdit.AddListener(SetWidth);
            _inputFieldEnd.onEndEdit.AddListener(SetWidth);

            _inputFieldStart.text = ContextMenu.GraphManager.newConnectionTemplate.line.startWidth.ToString();
            _inputFieldEnd.text = ContextMenu.GraphManager.newConnectionTemplate.line.endWidth.ToString();
        }

        void OnDisable()
        {
            _inputFieldStart.onEndEdit.RemoveAllListeners();
            _inputFieldEnd.onEndEdit.RemoveAllListeners();
        }
    }
}