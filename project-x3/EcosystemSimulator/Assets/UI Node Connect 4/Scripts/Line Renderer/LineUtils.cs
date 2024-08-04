using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    public class LineUtils
    {
        public static Vector2[] GetSoftZShape(Vector2 cp0, Vector2 cp1, Vector2 cp2, Vector2 cp3)
        {
            float curvature = 3;
            List<Vector2> linePoints = new List<Vector2>();

            linePoints.Add(cp0);
            float d01 = Vector2.Distance(cp1, cp0) / curvature;
            if (d01 != 0)
            {
                Vector2 cp01 = GetPointBetweenPoints(cp1, cp0, d01);
                Vector2 cp11 = GetPointBetweenPoints(cp1, cp2, d01);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 point = GetCornerPoint(cp01, cp1, cp11, (float)i / (float)5);
                    linePoints.Add(point);
                }
            }

            float d23 = Vector2.Distance(cp2, cp3) / curvature;
            if (d23 != 0)
            {
                if (d23 > 0)
                {
                    Vector2 cp23 = GetPointBetweenPoints(cp2, cp3, d23);
                    Vector2 cp22 = GetPointBetweenPoints(cp2, cp1, d23);
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 point = GetCornerPoint(cp22, cp2, cp23, (float)i / (float)5);
                        linePoints.Add(point);
                    }
                }
            }

            linePoints.Add(cp3);

            return linePoints.ToArray();
        }

        static Vector3 GetCornerPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * p0 +
                2f * oneMinusT * t * p1 +
                t * t * p2;
        }

        static Vector2 GetPointBetweenPoints(Vector2 p0, Vector2 p1, float d)
        {
            Vector2 point = new Vector2();
            float D = Vector2.Distance(p0, p1);
            point.x = p0.x + ((d / D) * (p1.x - p0.x));
            point.y = p0.y + ((d / D) * (p1.y - p0.y));

            return point;
        }

        static int minPoints = 5;
        static int maxPoints = 15;

        public static Vector2[] GetSPLineCurve(Vector2 cp0, Vector2 cp1, Vector2 cp2, Vector2 cp3)
        {
            int steps = CalculateSPLineNumberOfPointsInLine(cp0, cp1, cp2, cp3);

            Vector2[] linePoints = new Vector2[steps + 1];
            for (int i = 0; i < steps; i++)
            {
                linePoints[i] = GetSPLinePoint(cp0, cp1, cp2, cp3, (float)i / (float)steps);
            }
            linePoints[steps] = cp3;

            return linePoints;
        }

        static int CalculateSPLineNumberOfPointsInLine(Vector2 cp0, Vector2 cp1, Vector2 cp2, Vector2 cp3)
        {
            float a0 = Mathf.Abs(Mathf.Sin(Vector2.Angle(cp0 - cp1, cp3 - cp1) * Mathf.Deg2Rad));
            float a1 = Mathf.Abs(Mathf.Sin(Vector2.Angle(cp3 - cp2, cp0 - cp2) * Mathf.Deg2Rad));
            float r0 = a0 > a1 ? a0 : a1;

            return (int)((r0 * (maxPoints - minPoints)) + minPoints);
        }

        static Vector3 GetSPLinePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return
                oneMinusT * oneMinusT * oneMinusT * p0 +
                3f * oneMinusT * oneMinusT * t * p1 +
                3f * oneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        public static Vector2[] ConvertLinePointsToCurve(Vector3[] linePoints, Connection.CurveStyle curveStyle)
        {
            switch (curveStyle)
            {
                case Connection.CurveStyle.Spline:
                    return LineUtils.GetSPLineCurve(
                        linePoints[0],
                        linePoints[1],
                        linePoints[2],
                        linePoints[3]);
                case Connection.CurveStyle.Z_Shape:
                    return new Vector2[] {
                        linePoints[0],
                        (linePoints[1] + linePoints[0])/2,
                        (linePoints[2] + linePoints[3])/2,
                        linePoints[3] };
                case Connection.CurveStyle.Soft_Z_Shape:
                    return LineUtils.GetSoftZShape(
                        linePoints[0],
                        (linePoints[1] + linePoints[0]) / 2,
                        (linePoints[2] + linePoints[3]) / 2,
                        linePoints[3]);
                case Connection.CurveStyle.Line:
                    return new Vector2[] {
                        linePoints[0],
                        linePoints[3] };
                default:
                    return new Vector2[] { };
            };
        }
    }
}