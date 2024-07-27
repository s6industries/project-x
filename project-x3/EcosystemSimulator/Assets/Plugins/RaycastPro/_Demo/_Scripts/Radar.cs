using System.Collections.Generic;
using RaycastPro;
using RaycastPro.Detectors;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class Radar : MonoBehaviour
    {
        [SerializeField] private RadarDetector radarDetector;
    
        private static readonly int EmissiveColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int TColor = Shader.PropertyToID("_Color");
    
        private readonly Dictionary<Collider, MeshRenderer> library = new Dictionary<Collider, MeshRenderer>();

        private static Color Green = new Color(0.12f, 1f, 0.14f);
        private static Color Red = new Color(1f, 0.2f, 0.27f);

        private Color tColor;

        private void Start()
        {
            radarDetector.onNewCollider.AddListener(col =>
            {
                library.Add(col, col.GetComponent<MeshRenderer>());
            });
        
            radarDetector.onLostCollider.AddListener(col =>
            {
                library.Remove(col);
            });
        
            radarDetector.onRadarDetect.AddListener((col, f) =>
            {
                if (library.ContainsKey(col))
                {
                    var cacheTime = f / radarDetector.cacheTime;
                    
                    tColor = Color.Lerp(Red, Green, cacheTime);
                    col.GetComponentInChildren<SpriteRenderer>().color = Color.white.Alpha(cacheTime);
                    library[col].material.SetColor(TColor, tColor);
                    library[col].material.SetColor(EmissiveColor, tColor);
                }
            });
        }
    }
}
