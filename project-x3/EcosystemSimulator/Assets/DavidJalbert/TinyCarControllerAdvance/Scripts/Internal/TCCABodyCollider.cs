using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCABodyCollider : MonoBehaviour
    {
        private TCCABody body = null;

        public void initialize(TCCABody b)
        {
            body = b;
        }

        private void OnCollisionEnter(Collision collision)
        {
            body.onCollisionEnter(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            body.onCollisionExit(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            body.onCollisionStay(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            body.onTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            body.onTriggerExit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            body.onTriggerStay(other);
        }
    }
}