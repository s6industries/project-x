using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4
{
    public static class DragSelectionUtility
    {
        public static bool ConnectionIsInsideSelection(this Connection connection, Vector4 v4rect)
        {
            List<Vector2> points = connection.line.points;
            bool isInside = false;
            foreach (Vector2 point in points)
            {
                isInside = DragSelectionUtility.PointIsInsideRect(point, v4rect);
                if (isInside)
                {
                    return isInside;
                }
            }

            int pointCount = points.Count;
            for (int i = 1; i < pointCount; i++)
            {
                isInside = UICUtility.DoLinesIntersect(new Vector2(v4rect.x, v4rect.y), new Vector2(v4rect.z, v4rect.y), points[i - 1], points[i]) ||
                           UICUtility.DoLinesIntersect(new Vector2(v4rect.z, v4rect.y), new Vector2(v4rect.z, v4rect.w), points[i - 1], points[i]) ||
                           UICUtility.DoLinesIntersect(new Vector2(v4rect.z, v4rect.w), new Vector2(v4rect.x, v4rect.w), points[i - 1], points[i]) ||
                           UICUtility.DoLinesIntersect(new Vector2(v4rect.x, v4rect.w), new Vector2(v4rect.x, v4rect.y), points[i - 1], points[i]);

                if (isInside)
                {
                    return isInside;
                }
            }

            return isInside;
        }

        public static bool RectIsInsideSelection(this RectTransform rect, Vector4 v4rect)
        {
            Vector3[] v = new Vector3[4];
            rect.GetWorldCorners(v);
            bool isInside = false;
            foreach (Vector2 point in v)
            {
                isInside = point.PointIsInsideRect(v4rect);
                if (isInside)
                {
                    return isInside;
                }
            }

            isInside = UICUtility.DoLinesIntersect(v[0], v[1], new Vector2(v4rect.x, v4rect.y), new Vector2(v4rect.z, v4rect.y)) ||
                       UICUtility.DoLinesIntersect(v[2], v[3], new Vector2(v4rect.x, v4rect.y), new Vector2(v4rect.z, v4rect.y)) ||
                       UICUtility.DoLinesIntersect(v[1], v[2], new Vector2(v4rect.x, v4rect.y), new Vector2(v4rect.x, v4rect.w)) ||
                       UICUtility.DoLinesIntersect(v[3], v[0], new Vector2(v4rect.x, v4rect.y), new Vector2(v4rect.x, v4rect.w));

            return isInside;
        }

        public static bool PointIsInsideRect(this Vector2 point, Vector4 rect)
        {
            if (point.x > rect.x && point.x < rect.z && point.y < rect.y && point.y > rect.w)
                return true;

            return false;
        }
    }
}