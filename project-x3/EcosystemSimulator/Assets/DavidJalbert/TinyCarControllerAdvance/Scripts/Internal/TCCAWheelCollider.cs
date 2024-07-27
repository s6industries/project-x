using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCAWheelCollider : MonoBehaviour
    {
        private float highDotSide = 0;
        private TCCAWheel wheel = null;

        public void initialize(TCCAWheel w)
        {
            wheel = w;
        }

        public void resetCollision()
        {
            highDotSide = 0;
        }

        public bool isTouchingGround()
        {
            return highDotSide >= 0.5f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            wheel.onCollisionEnter(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            wheel.onCollisionExit(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            wheel.onCollisionStay(collision);
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                float dotSide = 1f - Mathf.Abs(Vector3.Dot(contact.normal, transform.right));
                highDotSide = Mathf.Max(highDotSide, dotSide);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            wheel.onTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            wheel.onTriggerExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            wheel.onTriggerStay(other);
        }
    }
}