using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [System.Serializable]
    public class Line
    {
        [System.Serializable]
        public class LineCap
        {
            public bool active = false;
            public Shape.Type shape = Shape.Type.Diamond;
            public float size = 5;
            public Color color = new Color(1, 0.81f, 0.3f);
            public float angleOffset = 0;
        }

        public LineCap capStart;
        public LineCap capEnd;

        public string ID = "line";

        public float startWidth = 3;
        public float endWidth = 3;
        public float dashDistance = 5;

        public Color color = new Color(0.98f, 0.94f, 0.84f);

        [HideInInspector] public List<Vector2> points = new List<Vector2>();

        public LineStyle.Type lineStyle = LineStyle.Type.Solid;

        public Line()
        {
            points = new List<Vector2>();
        }

        public void Draw(UICLineRenderer lineRenderer)
        {
            LineStyle.DrawStyle(lineStyle, lineRenderer, this);

            animation.DrawLineAnimation(lineRenderer, this);
        }

        /// <summary>
        /// Replace all points by the newPoints
        /// </summary>
        /// <param name="newPoints"></param>
        public void SetPoints(Vector2[] newPoints)
        {
            if (newPoints != null)
            {
                points.Clear();
                points.AddRange(newPoints);
            }

            length = GetLength();
        }

        public float length;
        public LineAnimation animation = new LineAnimation();
        float _deg2Rad90 = Mathf.Deg2Rad * 90f;
        public float GetLength()
        {
            float l = 0;
            for (int i = 1; i < points.Count; i++)
            {
                l += Vector2.Distance(points[i - 1], points[i]);
            }
            return l;
        }

        System.Tuple<int, float> GetPreviousPointAndLength(float pos01)
        {
            System.Tuple<int, float> t = new System.Tuple<int, float>(0, 0);
            float l = 0;
            float prevL = 0;
            for (int i = 1; i < points.Count; i++)
            {
                l += Vector2.Distance(points[i - 1], points[i]);
                if (l >= pos01)
                {
                    t = new System.Tuple<int, float>(i - 1, prevL);
                    break;
                }
                prevL = l;
            }
            return t;
        }

        /// <summary>
        /// returns point in the line based on percent from 0 to 1
        /// </summary>
        /// <param name="pos01">percent from 0 to 1</param>
        /// <returns>(Vector2) point and (float) angle at this percent</returns>
        public System.Tuple<Vector2, float> LerpLine(float pos01)
        {
            if (points.Count > 1)
            {
                System.Tuple<int, float> t = GetPreviousPointAndLength(pos01 * length);
                Vector2 p0 = points[t.Item1];
                Vector2 p1 = points[t.Item1 + 1];
                float ppDistance = Vector2.Distance(p0, p1);

                Vector2 position = Vector2.Lerp(p0, p1, ((pos01 * length) - t.Item2) / ppDistance);
                float angle = Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) + _deg2Rad90;

                return new System.Tuple<Vector2, float>(position, angle);
            }
            return new System.Tuple<Vector2, float>(Vector2.zero, 0);
        }

        /// <summary>
        /// returns point in the line based on length
        /// </summary>
        /// <param name="pos01">target legth</param>
        /// <returns>(Vector2) point and (float) angle at this length</returns>
        public System.Tuple<Vector2, float> LerpLineLength(float posLength)
        {
            if (points.Count > 1)
            {
                System.Tuple<int, float> t = GetPreviousPointAndLength(posLength);
                Vector2 p0 = points[t.Item1];
                Vector2 p1 = points[t.Item1 + 1];

                Vector2 position = Vector2.MoveTowards(p0, p1, posLength - t.Item2);
                float angle = Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) + _deg2Rad90;

                return new System.Tuple<Vector2, float>(position, angle);
            }
            return new System.Tuple<Vector2, float>(Vector2.zero, 0);
        }

        public Line Clone()
        {
            return UICUtility.Clone(this);
        }
    }
}