using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    public class Dashed : LineStyle
    {
        static Vector2 _lastPos2;
        static Vector2 _lastPos3;

        public static new void Draw(UICLineRenderer lineRenderer, Line line)
        {
            int linePointsCount = line.points.Count;
            if (linePointsCount > 0)
            {
                Vector2 p_1 = line.points[0];
                bool drawDash = true;
                float dashSize = (line.dashDistance / lineRenderer.rectScaleX) * 4;
                float angle = 0;
                float lastF = 0;

                float sWidth = line.startWidth / lineRenderer.rectScaleX;
                float eWidth = line.endWidth / lineRenderer.rectScaleX;

                for (float f = dashSize; f <= line.length; f += dashSize)
                {
                    System.Tuple<Vector2, float> t = line.LerpLineLength(f);
                    Vector2 pos = t.Item1;
                    angle = t.Item2;

                    Vector2 p0 = p_1;
                    Vector2 p1 = pos;

                    float w0 = UICUtility.ConvertScale(lastF, 0, line.length, sWidth, eWidth);
                    float w1 = UICUtility.ConvertScale(f, 0, line.length, sWidth, eWidth);

                    float cos = Mathf.Cos(angle);   // calc cos
                    float sin = Mathf.Sin(angle);   // calc sin

                    float _wCos0 = (w0 * cos);
                    float _wSin0 = (w0 * sin);
                    float _wCos1 = (w1 * cos);
                    float _wSin1 = (w1 * sin);

                    // Correct line position when LineRenderer is offset
                    Vector2 pos0 = new Vector2((p0.x) + _wCos0, (p0.y) + _wSin0);
                    Vector2 pos1 = new Vector2((p0.x) - _wCos0, (p0.y) - _wSin0);
                    Vector2 pos2 = new Vector2((p1.x) - _wCos1, (p1.y) - _wSin1);
                    Vector2 pos3 = new Vector2((p1.x) + _wCos1, (p1.y) + _wSin1);

                    if (drawDash)
                    {
                        lineRenderer.AddUIVertexQuad(new[] { pos0, pos1, pos2, pos3 }, lineRenderer.UVs, line.color);
                    }

                    if (line.capStart.active && f == dashSize)
                        Shape.DrawShape(line.capStart.shape, lineRenderer, line.points[0], angle + (line.capStart.angleOffset * Mathf.Deg2Rad), line.capStart.size, line.capStart.color);

                    p_1 = pos;

                    drawDash = !drawDash;

                    _lastPos2 = pos2;
                    _lastPos3 = pos3;
                    lastF = f;
                }

                if (line.capEnd.active)
                    Shape.DrawShape(line.capEnd.shape, lineRenderer, line.points[linePointsCount - 1], angle + (line.capEnd.angleOffset * Mathf.Deg2Rad), line.capEnd.size, line.capEnd.color);
            }
        }
    }
}