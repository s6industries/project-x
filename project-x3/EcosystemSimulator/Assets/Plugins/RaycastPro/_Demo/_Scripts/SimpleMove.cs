using RaycastPro.RaySensors;
using UnityEngine;
using UnityEngine.AI;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class SimpleMove : MonoBehaviour
    {
        public RaySensor raySensor;
        public NavMeshAgent agent;
        void Update()
        {
            // Left Click mouse
            if (Input.GetMouseButtonDown(0))
            {
                agent.destination = raySensor.HitPoint;
            }
        }
    }
}
