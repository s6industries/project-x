using RaycastPro.RaySensors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class BasicCast : MonoBehaviour
    {
        [SerializeField] private RaySensor raySensor;
        private void Start()
        {
            // Add manually Event actions
            raySensor.onBeginDetect.AddListener(_hit =>
            {
                Debug.Log($"On Begin Detect <color=#7AFF4D>{_hit.transform.name}</color>");
            });
        
            // Add manually Event actions
            raySensor.onEndDetect.AddListener(_hit =>
            {
                Debug.Log($"On End Detect <color=#7AFF4D>{_hit.transform.name}</color>");
            });
        }
        private void Update()
        {
            // Use Cast if your raySensor is Disable (Auto update off) for updating core results.
            raySensor.Cast(); // simple code for casting any ray

            // Use Performed just if raySensor AutoUpdate Enabled and you don't need to cast more.
            if (raySensor.Performed) // simple performing check
            {
                // Do actions..
            }
        }
    }
}

