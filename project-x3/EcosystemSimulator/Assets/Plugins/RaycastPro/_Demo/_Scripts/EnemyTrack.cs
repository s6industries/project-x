using System.Collections;
using System.Collections.Generic;
using Plugins.RaycastPro.Demo.Scripts;
using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class EnemyTrack : MonoBehaviour
    {
        public SyncRotation syncRotation;
        public LightDetector lightDetector;

        void Update()
        {
            if (lightDetector.Performed)
            {
                syncRotation.Sync();
            }
        }
    }
}
