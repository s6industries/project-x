using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class LineLight : MonoBehaviour
    {
        [SerializeField] private PathDetector pathDetector;
    
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Color = Shader.PropertyToID("_Color");
        void Start()
        {
            // pathDetector.onNewHit.AddListener(_c =>
            // {
            //     Debug.Log(_c.transform.name + "new");
            // });
            // pathDetector.onLostHit.AddListener(_c =>
            // {
            //     Debug.Log(_c.transform.name + "lost");
            // });
            pathDetector.onNewCollider.AddListener(_c =>
            {
                if (_c.transform.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    Debug.Log(_c.transform.name);
                    meshRenderer.material.SetColor(Color, new Color(0.12f, 1f, 0.14f));
                    meshRenderer.material.SetColor(EmissiveColor, new Color(0.12f, 1f, 0.14f));
                }
            });
        
            pathDetector.onLostCollider.AddListener(_c =>
            {
                if (_c.transform.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    meshRenderer.material.SetColor(Color, new Color(1f, 0.2f, 0.27f));
                    meshRenderer.material.SetColor(EmissiveColor, new Color(1f, 0.2f, 0.27f));
                }
            });
        }
    }
}
