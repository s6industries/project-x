using RaycastPro.Detectors2D;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class DetectorControl2D : MonoBehaviour
    {
        [SerializeField] private ColliderDetector2D detector;

        private void Start()
        {
            detector.onNewCollider.AddListener(OnNewCollider);
            detector.onLostCollider.AddListener(OnLostCollider);
        }

        private void OnNewCollider(Collider2D col)
        {
            Debug.Log($"<color=#5AFFDA>{col.name}</color> is Detected.");
            if (col.TryGetComponent(out NeonMaterial _neonMaterial))
            {
                _neonMaterial.SetNeonColor(true);
            }
        }
        private void OnLostCollider(Collider2D col)
        {
            Debug.Log($"<color=#609EFF>{col.name}</color> is Lost Detect.");
            if (col.TryGetComponent(out NeonMaterial _neonMaterial))
            {
                _neonMaterial.SetNeonColor(false);
            }
        }
    
    }
}
