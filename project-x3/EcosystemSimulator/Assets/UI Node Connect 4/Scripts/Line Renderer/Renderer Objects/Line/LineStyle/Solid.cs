using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    public class Solid : LineStyle
    {
        static float _deg2Rad90 = Mathf.Deg2Rad * 90f;
        public static new void Draw(UICLineRenderer lineRenderer, Line line)
        {
            int linePointsCount = line.points.Count;
            if (linePointsCount > 0)
            {
                float lastAngle = 0;
                float angle = 0;
                Vector2 _lastPos2 = Vector2.zero;
                Vector2 _lastPos3 = Vector2.zero;

                float currentLenght = 0;

                float sWidth = line.startWidth / lineRenderer.rectScaleX ;
                float eWidth = line.endWidth / lineRenderer.rectScaleX ;

                for (int i = 1; i < linePointsCount; i++)
                {
                    Vector2 p0 = line.points[i - 1];
                    Vector2 p1 = line.points[i];

                    angle = Mathf.Atan2(p0.y - p1.y, p0.x - p1.x) + _deg2Rad90;

                    float cos = Mathf.Cos(angle);   // calc cos
                    float sin = Mathf.Sin(angle);   // calc sin

                    float lengthToAdd = Vector2.Distance(p0, p1);
                    currentLenght += lengthToAdd;
                    float w0 = UICUtility.ConvertScale(currentLenght - lengthToAdd, 0, line.length, sWidth, eWidth);
                    float w1 = UICUtility.ConvertScale(currentLenght, 0, line.length, sWidth, eWidth);

                    float _wCos0 = (w0 * cos);
                    float _wSin0 = (w0 * sin);
                    float _wCos1 = (w1 * cos);
                    float _wSin1 = (w1 * sin);

                    // Correct line position when LineRenderer is offset
                    Vector2 pos0 = new Vector2((p0.x) + _wCos0, (p0.y) + _wSin0);
                    Vector2 pos1 = new Vector2((p0.x) - _wCos0, (p0.y) - _wSin0);
                    Vector2 pos2 = new Vector2((p1.x) - _wCos1, (p1.y) - _wSin1);
                    Vector2 pos3 = new Vector2((p1.x) + _wCos1, (p1.y) + _wSin1);

                    lineRenderer.AddUIVertexQuad(new[] { pos0, pos1, pos2, pos3 }, lineRenderer.UVs, line.color);

                    if (lastAngle != angle && i > 1 && i < linePointsCount)
                    {
                        Vector2 pos0_ = _lastPos3;
                        Vector2 pos1_ = _lastPos2;
                        Vector2 pos2_ = pos1;
                        Vector2 pos3_ = pos0;

                        lineRenderer.AddUIVertexQuad(new[] { pos0_, pos1_, pos2_, pos3_ }, lineRenderer.UVs, line.color);
                    }

                    if (line.capStart.active && i == 1)
                        Shape.DrawShape(line.capStart.shape, lineRenderer, line.points[0], angle + (line.capStart.angleOffset * Mathf.Deg2Rad), line.capStart.size, line.capStart.color);

                    _lastPos2 = pos2;
                    _lastPos3 = pos3;

                    lastAngle = angle;
                }

                if (line.capEnd.active)
                    Shape.DrawShape(line.capEnd.shape, lineRenderer, line.points[linePointsCount - 1], angle + (line.capEnd.angleOffset * Mathf.Deg2Rad), line.capEnd.size, line.capEnd.color);
            }
        }
    }
}