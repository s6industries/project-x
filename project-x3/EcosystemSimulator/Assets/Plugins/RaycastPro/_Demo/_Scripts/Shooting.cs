using RaycastPro.Bullets;
using RaycastPro.Casters;
using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class Shooting : MonoBehaviour
    {
        [SerializeField] private BasicCaster _caster;

        [SerializeField] private ColliderDetector _colliderDetector;
        private void Start()
        {
            _caster = GetComponent<BasicCaster>();
        }

        void Update()
        {
            // Simple Coding
            if (Input.GetMouseButtonDown(0))
            {
                if (_colliderDetector)
                {
                    _colliderDetector.GetNearestCollider(out var target);

                    if (target && _caster.bullets[0] is TrackerBullet)
                    {
                        _caster.trackTarget = target.transform;

                        _caster.Cast(0);
                    }
                }
                else
                {
                    _caster.Cast(0);
                }
            }
        }
    }
}