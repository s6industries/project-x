using RaycastPro;
using RaycastPro.Detectors;

namespace Plugins.RaycastPro.Demo.Scripts
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SoundLight : MonoBehaviour
    {
        [SerializeField] private SoundDetector soundDetector;
        private MeshRenderer _meshRenderer;
        public Color red = Color.red;
        public Color green = Color.green;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Speed = Shader.PropertyToID("_Speed");
        
        public Image left, right, up, down;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private Color col;


        void Update()
        {
            left.color = right.color = up.color = down.color = Color.grey;
            if (soundDetector.Performed)
            {
                
                if (soundDetector.IsHearingBack)
                {
                    down.color = Color.cyan;
                }
                else if (soundDetector.IsHearingForward)
                {
                    up.color = Color.green;
                }
                else if (soundDetector.IsHearingLeft)
                {
                    left.color = Color.yellow;
                }
                else if (soundDetector.IsHearingRight)
                {
                    right.color = Color.white;
                }
                
                
                _meshRenderer.materials[1].SetColor(EmissionColor, col);
                _meshRenderer.materials[2].SetColor(EmissionColor, col);
            }
            
        }
    }
}
