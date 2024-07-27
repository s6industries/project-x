using RaycastPro.Detectors;

namespace Plugins.RaycastPro.Demo.Scripts
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class SteeringController : MonoBehaviour
    {
        private Rigidbody rb;
        public SteeringDetector detector;

        public float accelerationSpeed = 4;
        public float arriveDistance = 2f;
        public float turnRate = 15;

        [SerializeField] private bool movable = true;
        [SerializeField] private bool rotatable = true;
        [SerializeField] private bool fixRotate = true;

        [Range(2f, 50)] public float speed = 10;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Update()
        {
            if (movable)
            {
                var inArriveDistance = Vector3.Distance(transform.position, detector.destination.position) < arriveDistance;
                
                rb.velocity = Vector3.Lerp(rb.velocity, inArriveDistance ? Vector3.zero : detector.SteeringDirection * speed, 
                    1 - Mathf.Exp(-accelerationSpeed * Time.deltaTime));
            }

            if (rotatable)
            {
                if (fixRotate)
                {
                    transform.forward = Vector3.ProjectOnPlane(detector.SteeringDirection, Vector3.up);
                }
                else
                {
                    rb.rotation = Quaternion.Lerp(rb.rotation,
                        Quaternion.LookRotation(detector.SteeringDirection == Vector3.zero ? transform.forward : detector.SteeringDirection, Vector3.up), 1 - Mathf.Exp(-turnRate * Time.deltaTime));
                }
            }
        }
    }
}