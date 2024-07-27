using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class SinusMove : MonoBehaviour
    {
        [SerializeField] private Vector3 finalOffset = Vector3.up;

        [SerializeField] private float sinusOffset;
        [SerializeField] private float speed = 1;

        private Vector3 basePosition;

        void Start()
        {
            basePosition = transform.position;
        }

        void Update()
        {
            transform.position = basePosition + finalOffset * Mathf.Sin(sinusOffset + Time.time * speed);
        }
    }
}