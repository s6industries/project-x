using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    public class Dotted : LineStyle
    {
        public static new void Draw(UICLineRenderer lineRenderer, Line line)
        {
            int linePointsCount = line.points.Count;
            if (linePointsCount > 0)
            {
                bool drawDash = true;

                float dashSize = (line.dashDistance / lineRenderer.rectScaleX) * 2;
                float angle = 0;
                float w0 = 0;

                float sWidth = line.startWidth;
                float eWidth = line.endWidth;

                for (float f = dashSize; f <= line.length; f += dashSize * w0 / 5)
                {
                    System.Tuple<Vector2, float> t = line.LerpLineLength(f);
                    Vector2 pos = t.Item1;
                    angle = t.Item2;

                    w0 = UICUtility.ConvertScale(f, 0, line.length, sWidth, eWidth);

                    if (drawDash)
                    {
                        Shape.DrawShape(Shape.Type.Diamond, lineRenderer, pos, angle, w0, line.color, 5);
                    }

                    if (line.capStart.active && f == dashSize)
                        Shape.DrawShape(line.capStart.shape, lineRenderer, line.points[0], angle + (line.capStart.angleOffset * Mathf.Deg2Rad), line.capStart.size, line.capStart.color);

                    drawDash = !drawDash;
                }

                if (line.capEnd.active)
                    Shape.DrawShape(line.capEnd.shape, lineRenderer, line.points[linePointsCount - 1], angle + (line.capEnd.angleOffset * Mathf.Deg2Rad), line.capEnd.size, line.capEnd.color);
            }
        }
    }
}