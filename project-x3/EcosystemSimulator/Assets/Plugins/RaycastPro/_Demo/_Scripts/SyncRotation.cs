using System;
using RaycastPro.RaySensors;
using RaycastPro.RaySensors2D;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class SyncRotation : MonoBehaviour
    {
        public Transform target;
        public RaySensor raySensor;
        public RaySensor2D raySensor2D;

        public bool autoUpdate = true;

        public enum SyncCondition
        {
            Always,
            OnPerformed,
            NoPerformed
        }

        public SyncCondition syncCondition = SyncCondition.Always;

        private Transform currentTarget;

        public void Sync()
        {
            currentTarget = target ? target : transform;
            if (raySensor)
            {
                if (!raySensor.enabled) raySensor.Cast();
                
                switch (syncCondition)
                {
                    case SyncCondition.Always:
                        currentTarget.forward = raySensor.TipDirection;
                        break;
                    case SyncCondition.OnPerformed:
                        if (raySensor.Performed)
                        {
                            currentTarget.forward = raySensor.TipDirection;
                        }
                        break;
                    case SyncCondition.NoPerformed:
                        if (!raySensor.Performed)
                        {
                            currentTarget.forward = raySensor.TipDirection;
                        }
                        break;
                }
            }
            else if (raySensor2D)
            {
                if (!raySensor2D.enabled) raySensor2D.Cast();
                
                switch (syncCondition)
                {
                    case SyncCondition.Always:
                        currentTarget.forward = raySensor2D.TipDirection;
                        break;
                    case SyncCondition.OnPerformed:
                        if (raySensor2D.Performed)
                        {
                            currentTarget.forward = raySensor2D.TipDirection;
                        }
                        break;
                    case SyncCondition.NoPerformed:
                        if (!raySensor2D.Performed)
                        {
                            currentTarget.forward = raySensor2D.TipDirection;
                        }
                        break;
                }
            }
        }
        void Update()
        {
            if (autoUpdate) Sync();
        }
    }
}
