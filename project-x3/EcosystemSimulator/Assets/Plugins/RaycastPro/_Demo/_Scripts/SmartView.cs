using System.Collections.Generic;
using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class SmartView : MonoBehaviour
    {
        public SightDetector _sightDetector;
        public List<TargetDetector> TDs;
        void Start()
        {
            _sightDetector.SyncDetection(TDs);
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var td in TDs)
            {
                td.CastFrom(transform.position);
                if (td.DirectValue > .5f)
                {
                    Debug.DrawRay(td.transform.position, Vector3.up, Color.blue);
                }
            }
        }
    }
}
