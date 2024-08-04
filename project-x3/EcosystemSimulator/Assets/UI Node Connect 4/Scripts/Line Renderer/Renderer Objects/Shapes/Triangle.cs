using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [System.Serializable]
    public class Triangle : Shape
    {
        public static new void Draw(UICLineRenderer lineRenderer, Vector2 position, float angle, float size, Color32 color, params float[] args)
        {
            // Correct line position when LineRenderer is offset for Canvas Overlay
            float cos = Mathf.Cos(angle);   // calc cos
            float sin = Mathf.Sin(angle);   // calc sin
            size /= lineRenderer.rectScaleX;

            float _sSin = (size * sin);
            float _sCos = (size * cos);

            Vector2 po1 = new Vector2((position.x - _sSin), (position.y + _sCos));
            Vector2 po2 = new Vector2((position.x + _sSin) - _sCos, (position.y - _sCos) - _sSin);
            Vector2 po3 = new Vector2((position.x + _sSin) + _sCos, (position.y - _sCos) + _sSin);

            // v4.1 - improved performance of line caps and animation triengle rendering 
            lineRenderer.AddUIVertexTriangle(new[] { po1, po2, po3 }, lineRenderer.UVs, color);
        }
    }
}