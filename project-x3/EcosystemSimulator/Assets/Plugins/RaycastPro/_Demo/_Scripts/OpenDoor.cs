using RaycastPro.RaySensors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class OpenDoor : MonoBehaviour
    {
        public RaySensor _pipeRay;
        void Start()
        {
            _pipeRay.onDetect.AddListener(hit =>
                { hit.transform.Translate(Vector3.down * Time.deltaTime); });
        }
    }
}