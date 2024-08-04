using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4
{
    public class PortControlPoint : MonoBehaviour
    {
        Transform _transform;
        public Transform Transform
        {
            get
            {
                if (!_transform)
                    _transform = transform;
                return _transform;
            }
        }
        public Vector3 Position
        {
            get => Transform.position;
            set => Transform.position = value;
        }
        public Vector3 LocalPosition
        {
            get => Transform.localPosition;
            set => Transform.localPosition = value;
        }
    }
}