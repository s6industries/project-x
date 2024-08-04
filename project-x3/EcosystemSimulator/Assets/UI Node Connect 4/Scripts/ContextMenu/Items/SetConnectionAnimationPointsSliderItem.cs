using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public class SetConnectionAnimationPointsSliderItem : ContextItem
    {
        Slider _slider;

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

            // v4.0.2 - bugfix: line.animation.pointsDistance was changed OnSelection if it was outside of the min and max slider range  
            if (_connList.Count == 1)
                _slider.value = _connList[0].line.animation.pointsDistance;

            gameObject.SetActive(_show);
        }

        private void SetPointCount(float arg0)
        {
            foreach (Connection conn in _connList)
            {
                conn.line.animation.pointsDistance = (int)_slider.value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _slider = GetComponentInChildren<Slider>();
        }

        void OnEnable()
        {
            _slider.onValueChanged.AddListener(SetPointCount);
        }

        void OnDisable()
        {
            _slider.onValueChanged.RemoveAllListeners();
        }
    }
}