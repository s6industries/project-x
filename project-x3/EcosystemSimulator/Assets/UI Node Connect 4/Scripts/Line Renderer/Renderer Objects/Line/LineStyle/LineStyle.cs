using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [System.Serializable]
    public abstract class LineStyle
    {
        public enum Type { Solid, Dashed, Dotted }

        protected static void Draw(UICLineRenderer lineRenderer, Line line)
        {

        }

        public static void DrawStyle(Type lineStyle, UICLineRenderer lineRenderer, Line line)
        {
            if (line.dashDistance > 0 && line.startWidth > 0 && line.endWidth > 0)
            {
                switch (lineStyle)
                {
                    case Type.Solid:
                        Solid.Draw(lineRenderer, line);
                        break;
                    case Type.Dashed:
                        Dashed.Draw(lineRenderer, line);
                        break;
                    case Type.Dotted:
                        Dotted.Draw(lineRenderer, line);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}