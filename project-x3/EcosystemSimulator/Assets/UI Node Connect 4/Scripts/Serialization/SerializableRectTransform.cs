using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    [System.Serializable]
    public class SerializableRectTransform
    {
        public SerializableRectTransform(RectTransform rectTransform)
        {
            ToSerializable(rectTransform);
        }

        public Vector3 position;
        public Vector3 localPosition;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 sizeDelta;
        public Vector3 scale;

        void ToSerializable(RectTransform rectTransform)
        {
            anchorMin = rectTransform.anchorMin;
            anchorMax = rectTransform.anchorMax;
            sizeDelta = rectTransform.sizeDelta;
            scale = rectTransform.localScale;
            position = rectTransform.position;
            localPosition = rectTransform.localPosition;
        }

        public void FromSerializable(RectTransform rectTransform)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.localScale = scale;
            rectTransform.position = position;
            rectTransform.localPosition = localPosition;
        }
    }
}