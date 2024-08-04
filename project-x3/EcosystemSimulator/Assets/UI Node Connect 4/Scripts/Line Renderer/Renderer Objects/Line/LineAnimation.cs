using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    // animation class for the lines 
    [System.Serializable]
    public class LineAnimation
    {
        // public MotionStyleType motionStyle = MotionStyleType.Lerp;

        public bool isActive = false;
        public float pointsDistance = 35;
        public float size = 10;
        public Color color = new Color(1, 0.81f, 0.3f);
        public Shape.Type shape = Shape.Type.Diamond;
        public float speed = 20;

        float _time = 0;

        // method to draw line animations
        public void DrawLineAnimation(UICLineRenderer lineRenderer, Line line)
        {
            if (isActive)
            {
                float pointsDistanceScale = pointsDistance / lineRenderer.rectScaleX;
                if (pointsDistanceScale < 1)
                    pointsDistanceScale = 1;

                float lengthPlusOne = (line.length + pointsDistanceScale);
                float length = (lengthPlusOne - (lengthPlusOne % pointsDistanceScale));
                _time += Mathf.Abs(Time.deltaTime * speed);

                if (_time > length)
                    _time = 0;

                for (float f = 0; f <= length; f += pointsDistanceScale)
                {
                    System.Tuple<Vector2, float> pointInLine = new System.Tuple<Vector2, float>(Vector2.zero, 0);

                    float timePerPoint = _time + f;

                    if (speed < 0)
                        timePerPoint = f - _time;

                    if (timePerPoint > length)
                        timePerPoint = timePerPoint - length;

                    if (timePerPoint < 0)
                        timePerPoint = timePerPoint + length;

                    pointInLine = line.LerpLineLength(timePerPoint);

                    if (timePerPoint <= line.length)
                        Shape.DrawShape(shape, lineRenderer, pointInLine.Item1, pointInLine.Item2, size , color);
                }
            }
        }

        public LineAnimation Clone()
        {
            return UICUtility.Clone(this);
        }
    }
}