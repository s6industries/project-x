using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SelectedElementLabelItem : ContextItem
    {
        TMP_Text _text;

        public override void OnChangeSelection()
        {
            if (UICSystemManager.selectedElements.Count <= 0)
            {
                _text.text = "--";
            }
            else if (UICSystemManager.selectedElements.Count == 1)
            {
                _text.text = (UICSystemManager.selectedElements[0] as IGraphElement).ID;
            }
            else
            {
                _text.text = string.Format("Multiple Elements ({0})", UICSystemManager.selectedElements.Count);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _text = transform.GetComponentInChildren<TMP_Text>();
        }
    }
}