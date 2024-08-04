using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.GraphicRenderer;
using TMPro;

namespace MeadowGames.UINodeConnect4
{
    [System.Serializable]
    [RequireComponent(typeof(TMP_Text))]
    public class ConnectionLabel : MonoBehaviour
    {
        [SerializeField] string _labelText;
        public string text
        {
            get => _labelText;
            set
            {
                TMPTextComponent.text = value;
                _labelText = value;
            }
        }
        public TMP_Text TMPTextComponent;
        public enum LabelAngleType { follow, fixed_ }
        public bool adjustScaleOnUpdate = true;
        public LabelAngleType labelAngleType;
        public float angleOffset;

        [SerializeField] GraphManager _graphManager;
        public void SetGraphManager(GraphManager graphManager) => _graphManager = graphManager;

        void OnEnable()
        {
            if (!TMPTextComponent)
            {
                TMPTextComponent = GetComponent<TMP_Text>() ? GetComponent<TMP_Text>() : gameObject.AddComponent<TMP_Text>();
            }
        }

        public void UpdateLabel(Line line)
        {
            System.Tuple<Vector2, float> pointInLine = line.LerpLine(0.5f);

            Vector3 position = transform.position;

            Vector3 pointPos = UICUtility.ScreenToWorldPointsForRenderMode(_graphManager, pointInLine.Item1);

            if (!float.IsNaN(pointPos.x))
                position.x = pointPos.x;
            if (!float.IsNaN(pointPos.y))
                position.y = pointPos.y;
            // position.z = 0;
            transform.position = position;

            Vector3 angle = transform.eulerAngles;

            if (labelAngleType == LabelAngleType.follow)
            {
                float lineAngleDeg = (pointInLine.Item2 * Mathf.Rad2Deg);

                // v4.1 - bugfix: label flipped down when connected ports are horizontally aligned
                // calc text angle
                if ((lineAngleDeg > -90 && lineAngleDeg < 0) || (lineAngleDeg > 180 && lineAngleDeg <= 270))
                {
                    angle.z = lineAngleDeg + 90 + angleOffset;
                }
                else
                {
                    angle.z = lineAngleDeg - 90 + angleOffset;
                }
            }
            else if (labelAngleType == LabelAngleType.fixed_)
            {
                angle.z = angleOffset;
            }

            if (!float.IsNaN(angle.z))
                transform.eulerAngles = angle;

            TMPTextComponent.text = _labelText;

            if (adjustScaleOnUpdate)
            {
                float scale = 1 / _graphManager.lineRenderer.rectScaleX;
                transform.localScale = new Vector3(scale, scale, 1);
            }
        }
    }
}