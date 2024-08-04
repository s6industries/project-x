using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [System.Serializable]
    public class Diamond : Shape
    {
        public static new void Draw(UICLineRenderer lineRenderer, Vector2 position, float angle, float size, Color32 color, params float[] args)
        {
            float cos = Mathf.Cos(angle);   // calc cos
            float sin = Mathf.Sin(angle);   // calc sin
            size /= lineRenderer.rectScaleX;

            float _sSin = (size * sin);
            float _sCos = (size * cos);

            Vector2 po0 = new Vector2((position.x - _sSin), (position.y + _sCos));
            Vector2 po1 = new Vector2((position.x) - _sCos, (position.y) - _sSin);
            Vector2 po2 = new Vector2((position.x + _sSin), (position.y - _sCos));
            Vector2 po3 = new Vector2((position.x) + _sCos, (position.y) + _sSin);

            lineRenderer.AddUIVertexQuad(new[] { po0, po1, po2, po3 }, lineRenderer.UVs, color);
        }
    }
}