using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    public abstract class Shape
    {
        public enum Type { Square, Circle, Triangle, Diamond };

        protected static void Draw(UICLineRenderer lineRenderer, Vector2 position, float angle, float size, Color32 color, params float[] args)
        {

        }

        public static void DrawShape(Type shape, UICLineRenderer lineRenderer, Vector2 position, float angle, float size, Color32 color, params float[] args)
        {
            switch (shape)
            {
                case Type.Square:
                    Square.Draw(lineRenderer, position, angle, size, color, args);
                    break;
                case Type.Circle:
                    Circle.Draw(lineRenderer, position, 0, size, color, args);
                    break;
                case Type.Triangle:
                    Triangle.Draw(lineRenderer, position, angle, size, color, args);
                    break;
                case Type.Diamond:
                    Diamond.Draw(lineRenderer, position, angle, size, color, args);
                    break;
                default:
                    break;
            }
        }
    }
}