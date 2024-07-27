using RaycastPro.RaySensors;
using RaycastPro.RaySensors2D;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class LinerControl : MonoBehaviour
    {
        [SerializeField] private RaySensor raySensor;
        [SerializeField] private RaySensor2D raySensor2D;
        private float sinus;

        public float speed = 1f;
        public float width = .2f;
        private void Update()
        {
            sinus = (Mathf.Sin(Time.time * speed) + .8f) * .6f;
            if (raySensor)
            {
                raySensor.linerBasePosition = sinus - width;
                raySensor.linerEndPosition = sinus + width;
            }

            if (raySensor2D)
            {
                raySensor2D.linerBasePosition = sinus - width;
                raySensor2D.linerEndPosition = sinus + width;
            }
        }
    }
}
