using RaycastPro;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class NeonMaterial : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private int[] index;
    
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int Color = Shader.PropertyToID("_Color");

        public float intensity = 2f;
        private static Color Green = new Color(0.12f, 1f, 0.14f);
        private static Color Red = new Color(1f, 0.2f, 0.27f);

        public void SetNeonColor(bool turn)
        {
            if (turn)
            {
                foreach (var i in index)
                {
                    meshRenderer.materials[i].SetColor(Color, Green*intensity);
                    meshRenderer.materials[i].SetColor(EmissiveColor, Green*intensity);
                }
            }
            else
            {
                foreach (var i in index)
                {
                    meshRenderer.materials[i].SetColor(Color, Red*intensity);
                    meshRenderer.materials[i].SetColor(EmissiveColor, Red*intensity);
                }
            }
        }
    }
}
