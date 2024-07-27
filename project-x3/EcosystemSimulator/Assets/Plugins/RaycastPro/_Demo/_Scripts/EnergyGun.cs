using System.Collections;
using System.Collections.Generic;
using RaycastPro.Detectors;
using RaycastPro.RaySensors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class EnergyGun : MonoBehaviour
    {
        public WaveRay waveRay;
        public LineDetector lineDetector;

        public List<HoverEnemy> detectedEnemies;
        public float linerSetupTime = 2f;
        public float DPS = 24;
        void Start()
        {
            lineDetector.SyncDetection(detectedEnemies);
        }

        private IEnumerator WaveTween(float to, float duration = 1f)
        {
            var progress = 0f;
            var basePos = waveRay.linerEndPosition;
            while (progress <= 1)
            {
                waveRay.linerEndPosition = Mathf.Lerp(basePos, to, progress);
                progress += Time.deltaTime/duration;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        private void Update()
        {
            if (waveRay.Influence > 0 && Input.GetMouseButton(0))
            {
                
                waveRay.linerEndPosition += Time.deltaTime / linerSetupTime;
                waveRay.Cast();
                
                foreach (var detectedEnemy in detectedEnemies)
                {
                    detectedEnemy.TakeDamage(Time.deltaTime*DPS);
                    detectedEnemy.body.AddForce(-transform.forward * 7);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(WaveTween(1, .4f));
            }

            if (Input.GetMouseButtonUp(0))
            {
                StartCoroutine(WaveTween(0, .4f));
            }
        
            // Optimized Way

        }
    }
}
