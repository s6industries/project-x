using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [System.Serializable]
    public class Circle : Shape
    {
        static Vector2 pos0;
        static Vector2 pos1;
        static Vector2 pos2;

        const float pi2 = 2 * Mathf.PI;

        public static new void Draw(UICLineRenderer lineRenderer, Vector2 position, float angle, float size, Color32 color, params float[] args)
        {
            Vector2 p0 = position;

            Vector2 prevX = p0;
            pos1 = p0;
            pos2 = p0;
            int verticeCount = 10;
            if (args.Length > 0)
                verticeCount = (int)args[0];

            size /= lineRenderer.rectScaleX;
            float _circleSlice = pi2 / verticeCount;

            for (int j = 0; j <= verticeCount; j++)
            {
                float internalAngle = (_circleSlice * j);

                float sCos = Mathf.Cos(internalAngle) * size;   // calc cos
                float sSin = Mathf.Sin(internalAngle) * size;   // calc sin

                pos0 = pos1;
                pos1 = new Vector2(p0.x + sCos, p0.y + sSin);

                // v4.1 - improved performance of line caps and animation circle rendering 
                lineRenderer.AddUIVertexTriangle(new[] { pos0, pos1, p0 }, lineRenderer.UVs90, color);
            }
        }
    }
}