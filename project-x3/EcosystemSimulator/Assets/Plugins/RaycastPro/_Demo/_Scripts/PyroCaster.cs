using RaycastPro.RaySensors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class PyroCaster : MonoBehaviour
    {
        private RaySensor raySensor; // Use "RaySensor" base for define any 3D Ray

        public bool autoRayDetect = true;
        
        [SerializeField] private NeonMaterial[] neon;
        private void Start()
        {
            if (!autoRayDetect) return;

            raySensor = GetComponentInChildren<RaySensor>();
            // Change neon color just by injecting the methods in OnBegin Event..
            raySensor.onBeginDetect.AddListener(_ =>
            {
                if (!clonePerform)
                {
                    if (_.transform.TryGetComponent(out NeonMaterial _neon))
                    {
                        _neon.SetNeonColor(true);
                    }
                    foreach (var neonMaterial in neon) neonMaterial.SetNeonColor(true);
                }
            });
            raySensor.onEndDetect.AddListener(_ =>
            {
                if (!clonePerform)
                {
                    if (_.transform.TryGetComponent(out NeonMaterial _neon))
                    {
                        _neon.SetNeonColor(false);
                    }
                
                    foreach (var neonMaterial in neon) neonMaterial.SetNeonColor(false);
                }
            });
        }

        public void NeonTurn(bool perform)
        {
            foreach (var neonMaterial in neon) neonMaterial.SetNeonColor(perform);
        }
        
        [SerializeField] private bool clonePerform;
    
        private bool lastClonePerformed;
        private void Update()
        {
            if (!clonePerform) return;
            
            if (raySensor.CloneHit.transform != lastClonePerformed)
            {
                foreach (var neonMaterial in neon) neonMaterial.SetNeonColor(raySensor.CloneHit.transform);
            }
            
            lastClonePerformed = raySensor.CloneHit.transform;
        }
    }
}
