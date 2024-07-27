using RaycastPro.RaySensors2D;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class PyroCaster2D : MonoBehaviour
    {
        private RaySensor2D raySensor;
        [SerializeField] private MeshRenderer[] meshRenderers;
    
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Color = Shader.PropertyToID("_Color");

        private bool lastPerformed;

        private void Start()
        {
            raySensor = GetComponentInChildren<RaySensor2D>();
        }

        private void Update()
        {
            if (raySensor.Performed != lastPerformed) SetNeonColor(raySensor.Performed);

            lastPerformed = raySensor.Performed;
        }

        public void SetNeonColor(bool turn)
        {
            if (turn)
            {
                meshRenderers[0].materials[0].SetColor(Color, new Color(0.12f, 1f, 0.14f));
                meshRenderers[0].materials[2].SetColor(Color, new Color(0.12f, 1f, 0.14f));
                meshRenderers[1].materials[0].SetColor(Color, new Color(0.12f, 1f, 0.14f));
                meshRenderers[0].materials[0].SetColor(EmissiveColor, new Color(0.12f, 1f, 0.14f));
                meshRenderers[0].materials[2].SetColor(EmissiveColor, new Color(0.12f, 1f, 0.14f));
                meshRenderers[1].materials[0].SetColor(EmissiveColor, new Color(0.12f, 1f, 0.14f));
            }
            else
            {
                meshRenderers[0].materials[0].SetColor(Color, new Color(1f, 0.2f, 0.27f));
                meshRenderers[0].materials[2].SetColor(Color, new Color(1f, 0.2f, 0.27f));
                meshRenderers[1].materials[0].SetColor(Color, new Color(1f, 0.2f, 0.27f));
                meshRenderers[0].materials[0].SetColor(EmissiveColor, new Color(1f, 0.2f, 0.27f));
                meshRenderers[0].materials[2].SetColor(EmissiveColor, new Color(1f, 0.2f, 0.27f));
                meshRenderers[1].materials[0].SetColor(EmissiveColor, new Color(1f, 0.2f, 0.27f));
            }

        }
    }
}
