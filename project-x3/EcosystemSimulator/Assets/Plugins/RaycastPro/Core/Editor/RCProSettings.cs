#if UNITY_EDITOR

namespace RaycastPro.Editor
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "RCProSettings", menuName = "RCProSettings", order = 0)]
    public class RCProSettings : ScriptableObject
    {
        internal RCProSettings profile;

        private void OnEnable()
        {
            profile = this;
        }

        public bool realtimeEditor = true;
        public bool rcProInspector = false;
        
        public Color DefaultColor = RCProEditor.Aqua;
        public Color DetectColor = new Color(.3f, 1, .3f, 1f);
        public Color HelperColor = new Color(1f, .7f, .0f, 1f);
        public Color BlockColor = new Color(1f, .2f, .2f, 1f);
        
        public float normalDiscRadius = .2f;
        public float elementDotSize = .05f;
        public float alphaAmount = .2f;
        public float gizmosOffTime = 4f;
        
        public float raysStepSize = 4f;
        public float normalFilterRadius = 1f;
        public float linerMaxWidth = 1f;

        public bool DrawBlockLine = true;
        public bool DrawDetectLine = true;
        public bool DrawGuide = true;
        public bool ShowLabels = true;

        public int maxSubdivideTime = 6;

        public bool drawHierarchyIcons = true;
        public int hierarchyIconsOffset = 100;
    }
}
#endif