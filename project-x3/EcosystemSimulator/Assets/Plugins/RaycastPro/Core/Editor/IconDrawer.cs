#if UNITY_EDITOR
namespace RaycastPro.Editor
{
    using UnityEditor;
    using UnityEngine;
    
    [InitializeOnLoad]
    internal class IconDrawer
    {
        private const int iconSize = 18;
        public static int OffsetFromName = 100;
        static IconDrawer ()
        {
            SetEvent(true);
        }
        internal static void SetEvent(bool turn)
        {
            if (turn)
            {
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
            }
            else
            {
                EditorApplication.hierarchyWindowItemOnGUI -= HierarchyItemCB;
            }
        }
        private static void HierarchyItemCB (int instanceID, Rect rect)
        {
            if (!RCProPanel.drawHierarchyIcons) return;
            rect.width = iconSize;
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null)
                return;
            rect.x += EditorStyles.label.CalcSize(new GUIContent(obj.name)).x + RCProPanel.hierarchyIconsOffset;
#if UNITY_2019
            rect.y += 1;
            rect.x += iconSize + 4;
            //rect.x += iconSize + 28;
#elif UNITY_2018_3_OR_NEWER
            rect.y += 2;
            rect.x += iconSize;
#endif
            foreach (var component in obj.GetComponents<Component>())
            {
                if (!(component is RaycastCore)) continue;
                var icon = AssetPreview.GetMiniThumbnail(component);
                GUI.Label(rect, icon);
                rect.x += iconSize;
            }
        }
    }
}

#endif