using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        public Transform target;
        [SerializeField] private float speed;
        void Update()
        {
            _rigidbody.velocity = (target.position - transform.position).normalized * speed;
        }
    }
}
