using RaycastPro.RaySensors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class RollController : MonoBehaviour
    {
        private Camera mainCamera;
        private Vector3 input;
        [SerializeField] private float speed = 4;
        [SerializeField] private float turnRate = 15;

        private Rigidbody rb;
        [SerializeField] private Transform roll;
        [SerializeField] private RaySensor groundRay;

        private float delta;

        void Start()
        {
            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            input.y = 0;
            input.z = Input.GetAxis("Vertical");
            input.x = Input.GetAxis("Horizontal");


            if (groundRay.Cast())
            {
                input = mainCamera.transform.TransformDirection(input).normalized;
                input = Vector3.ProjectOnPlane(input, groundRay.Normal);

                rb.velocity = input * speed;

                delta = Time.deltaTime;

                if (input.sqrMagnitude > .1f)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(input, groundRay.Normal), 1 - Mathf.Exp(-turnRate * delta));
                }

                roll.Rotate(rb.velocity.magnitude * delta, 0, 0);
            }



        }

    }
}