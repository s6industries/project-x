
namespace Plugins.RaycastPro.Demo.Scripts
{
    using UnityEngine;

    public class CameraShake : MonoBehaviour
    {
        private Vector3 originalPosition;
        public float duration = .4f;
        public float amplification = .7f;

        private float shakeDuration = 0f;
        private float shakeAmplification = 0.7f;
        private float shakeDecreaseFactor = 1.0f;

        void Start()
        {
            originalPosition = transform.localPosition;
        }

        void Update()
        {
            if (shakeDuration > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeAmplification;
                shakeDuration -= Time.deltaTime * shakeDecreaseFactor;
            }
            else
            {
                shakeDuration = 0f;
                transform.localPosition = originalPosition;
            }
        }

        public void Shake()
        {
            shakeDuration = duration;
            shakeAmplification = amplification;
        }
    }
}