using RaycastPro;
using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class DetectorControl : MonoBehaviour
    {
        [SerializeField] private ColliderDetector detector;
        private void Start()
        {
            detector.onNewCollider.AddListener(OnNewCollider);
            detector.onLostCollider.AddListener(OnLostCollider);
        }
        private void OnNewCollider(Collider col)
        {
            Debug.Log($"<color=#5AFFDA>{col.name}</color> is Detected.");
            if (col.TryGetComponent(out NeonMaterial _neonMaterial))
            {
                _neonMaterial.SetNeonColor(true);
            }
        }
        private void OnLostCollider(Collider col)
        {
            Debug.Log($"<color=#609EFF>{col.name}</color> is Lost Detect.");
            if (col.TryGetComponent(out NeonMaterial _neonMaterial))
            {
                _neonMaterial.SetNeonColor(false);
            }
        }
        
    }
}
