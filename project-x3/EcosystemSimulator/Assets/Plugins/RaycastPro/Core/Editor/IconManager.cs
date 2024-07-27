#if UNITY_EDITOR
namespace RaycastPro.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RaycastPro;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Icon manager.
    /// </summary>
    public static class IconManager
    {
        /// <summary>
        /// Set the icon for this object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="texture">The icon.</param>
        public static void SetIcon(this Object obj, Texture2D texture)
        {
            try
            {
#if UNITY_2021_2_OR_NEWER && !UNITY_2021_1 && !UNITY_2021_2
                EditorGUIUtility.SetIconForObject(obj, texture);
#else
                var ty = typeof(EditorGUIUtility);
                var method = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);

                method.Invoke(null, new object[] {obj, texture});
#endif

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Get the icon for this object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The icon for this object.</returns>
        public static Texture2D GetIcon(this Object obj)
        {
#if UNITY_2021_2_OR_NEWER && !UNITY_2021_1 && !UNITY_2021_2
            return EditorGUIUtility.GetIconForObject(obj);
#else
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            return mi.Invoke(null, new object[] { obj }) as Texture2D;
#endif
        }

        /// <summary>
        /// Remove this icon's object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public static void RemoveIcon(this Object obj) => SetIcon(obj, (Texture2D) null);

        private static IEnumerable<Type> GetCores => typeof(RaycastCore).GetInheritedTypes();

        public static Dictionary<Type, Texture2D> GetIcons()
        {
            var dict = new Dictionary<Type, Texture2D>();
            var icons = GetResources();
            var lookup = icons.ToLookup(I => I.name);
            const string prefix = "Icon_";
            foreach (var type in GetCores)
            {
                var icon = GetIconFromType(type, lookup, prefix);
                if (icon) dict.Add(type, icon);
            }

            return dict;
        }

        public static Texture2D GetIconFromType(Type type, ILookup<string, Texture2D> lookup, string prefix)
        {
            var textureArray = lookup[$"{prefix}{type.Name}"].ToArray();
            var texture = textureArray.Length > 0 ? textureArray[0] : null;
            return texture;
        }

        public static Texture2D GetIconFromName(string name)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(RCProPanel.ResourcePath + $"/{name}.png");
            return texture;
        }

        //private static string Resource_Path = "Assets/Plugins/RaycastPro/Resources";
        public static Texture2D GetHeader()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(RCProPanel.ResourcePath + "/RaycastPro_Header.png");
        }

        public static IEnumerable<Texture2D> GetResources()
        {
            var textureGUIDs = AssetDatabase.FindAssets("t:Texture", new[] {RCProPanel.ResourcePath});
            var textures = new List<Texture2D>();
            foreach (var guid in textureGUIDs)
            {
                var texturePath = AssetDatabase.GUIDToAssetPath(guid);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                textures.Add(texture);
            }

            return textures;
        }
    }
}
#endif