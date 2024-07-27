#if UNITY_EDITOR
namespace RaycastPro.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Bullets;
    using Bullets2D;
    using Casters;
    using Casters2D;
    using Detectors;
    using Detectors2D;
    using Planers;
    using Planers2D;
    using RaySensors;
    using RaySensors2D;
    using UnityEditor;
    using UnityEngine;
    using Sensor;
    public sealed class RCProPanel : EditorWindow
    {
        internal const string KEY = "RaycastPro_Key : ";
        internal const string CResourcePath = "Resource_Path";
        internal const string CShowOnStart = "Show On Startup";
        private const int width = 450;
        private const int height = 600;

        internal static Mode mode = Mode.TwoD;
        internal static CoreMode coreMode = CoreMode.RaySensors;

        internal static bool showOnStart;

        public RCProSettings settingProfile;
        internal static RCProSettings SettingProfile;
        
        [SavePreference] internal string settingProfilePath;
        internal static bool realtimeEditor => SettingProfile ? SettingProfile.realtimeEditor : true;
        internal static bool rcProInspector => SettingProfile ? SettingProfile.rcProInspector : true;

        internal static Color DefaultColor => SettingProfile ? SettingProfile.DefaultColor : RCProEditor.Aqua;
        internal static Color DetectColor => SettingProfile ? SettingProfile.DetectColor : new Color(.3f, 1, .3f, 1f);
        internal static Color HelperColor => SettingProfile ? SettingProfile.HelperColor : new Color(1f, .7f, .0f, 1f);
        internal static Color BlockColor => SettingProfile ? SettingProfile.BlockColor : new Color(1f, .2f, .2f, 1f);
        
        internal static float normalDiscRadius => SettingProfile ? SettingProfile.normalDiscRadius : .2f;
        internal static float elementDotSize => SettingProfile ? SettingProfile.elementDotSize :.05f;
        internal static float alphaAmount => SettingProfile ? SettingProfile.alphaAmount :.2f;
        internal static float gizmosOffTime => SettingProfile ? SettingProfile.gizmosOffTime :4f;

        internal static float raysStepSize => SettingProfile ? SettingProfile.raysStepSize : 4f;
        internal static float normalFilterRadius => SettingProfile ? SettingProfile.normalFilterRadius : 1f;
        internal static float linerMaxWidth => SettingProfile ? SettingProfile.linerMaxWidth : 1f;
        
        internal static bool DrawBlockLine =>  !SettingProfile || SettingProfile.DrawBlockLine;
        internal static bool DrawDetectLine =>  !SettingProfile || SettingProfile.DrawDetectLine;
        internal static bool DrawGuide =>  !SettingProfile || SettingProfile.DrawGuide;
        internal static bool ShowLabels => !SettingProfile || SettingProfile.ShowLabels;
        
        internal static int maxSubdivideTime => SettingProfile ? SettingProfile.maxSubdivideTime : 6;
        internal static bool drawHierarchyIcons => !SettingProfile || SettingProfile.drawHierarchyIcons; 
        internal static int hierarchyIconsOffset => SettingProfile ? SettingProfile.hierarchyIconsOffset : 100;
        

        internal static Dictionary<Type, Texture2D> ICON_DICTIONARY = new Dictionary<Type, Texture2D>();
        internal static EditorWindow window;
        internal static bool LoadWhenOpen = false;
        private Texture2D headerTexture;
        private Vector2 scrollPos;
        internal static string ResourcePath => GetFolderPath("Resource", "RaycastPro");
   
        public static string GetFolderPath(string currentFolderName, string parentFolderName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Folder {currentFolderName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string[] pathParts = path.Split('/');

                if (pathParts.Length >= 2 && pathParts[pathParts.Length - 2] == parentFolderName)
                {
                    return path;
                }

            }
            return string.Empty;
        }

        private float timer;

        private SerializedObject _so;
        
        private void OnEnable()
        {
            LoadPreferences();
            showOnStart = EditorPrefs.GetBool(KEY + CShowOnStart, true);
            headerTexture = IconManager.GetHeader();
            RefreshIcons();

            modes = new[] {"Utility", "2D", "3D"};

            _so = new SerializedObject(this);
            
            // Load Settings
            settingProfilePath = EditorPrefs.GetString(KEY + nameof(settingProfilePath));
            var file = AssetDatabase.LoadAssetAtPath<RCProSettings>(settingProfilePath);
            if (file)
            {
                settingProfile = file;
                // Singleton
                SettingProfile = settingProfile;
                settingProfSO = new SerializedObject(settingProfile);
            }
        }
        private void OnDisable()
        {
            EditorPrefs.SetBool(KEY + CShowOnStart, showOnStart);

            _so = null;
        }

        private void OnFocus()
        {
            Repaint();
        }

        private List<Type> cores = new List<Type>();

        private Color lineColor;

        private static string[] modes;

        private SerializedObject settingProfSO;
        
        private float time;
        private string ColorHash;
        private Color randomColor;
        private void OnInspectorUpdate()
        {
            time += .1f;
            if (time > 1)
            {
                time %= 1f;
            }

            randomColor = Color.HSVToRGB(time % 1f, 1f, 1f);
            ColorHash = ColorUtility.ToHtmlStringRGB(randomColor);
            
            Repaint();
        }

        private const string mScript = "m_Script";
        private void OnGUI()
        {
            lineColor = RCProEditor.Aqua;
            GUI.color = Color.white;
            var boxStyle = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.UpperCenter
            };
            if (headerTexture)
            {
                GUILayout.Box(headerTexture, boxStyle, GUILayout.Width(width), GUILayout.Height(153));
            }
            RCProEditor.GUILine(lineColor);
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperCenter,
                richText = true
            };
            GUILayout.Label($"<b>RAYCAST_PRO 1.1.1</b> developed by <color=#2BC6D2>KIYNL</color>", labelStyle);
            RCProEditor.GUILine(lineColor);
            #region Content Buttons
            GUI.contentColor = RCProEditor.Aqua;
            GUI.backgroundColor = RCProEditor.Violet;
            var enumInt = GUILayout.SelectionGrid((int) mode, modes, 3);
            mode = (Mode) Enum.ToObject(typeof(Mode), enumInt);

            cores.Clear();
            
            if (mode == Mode.Utility)
            {
                cores = new List<Type>
                {
                    typeof(RayManager), typeof(PointSensor), typeof(RayStamp), typeof(RayLiner)
                };
            }
            else
            {
                var coreInt = GUILayout.SelectionGrid((int) coreMode, Enum.GetNames(typeof(CoreMode)), 5);
                coreMode = (CoreMode) Enum.ToObject(typeof(CoreMode), coreInt);
                
                if (mode == Mode.ThreeD)
                switch (coreMode)
                {
                    case CoreMode.RaySensors:
                        cores = new List<Type>
                        {
                            typeof(BasicRay), typeof(PipeRay), typeof(BoxRay), typeof(RadialRay), typeof(ChainRay),
                            typeof(ReflectRay),
                            typeof(TargetRay), typeof(PointerRay), typeof(WaveRay), typeof(NoiseRay), typeof(CurveRay), typeof(ArcRay), typeof(HybridRay)
                        };
                        break;
                    case CoreMode.Detectors:
                        cores = new List<Type>
                        {
                            typeof(LineDetector), typeof(RangeDetector), typeof(BoxDetector), typeof(PolyDetector), typeof(MeshDetector), typeof(TargetDetector),
                            typeof(RadarDetector), typeof(SightDetector), typeof(SteeringDetector),
                            typeof(SoundDetector), typeof(LightDetector), typeof(PathDetector)
                        };
                        break;
                    case CoreMode.Planers:
                        cores = new List<Type>
                        {
                            typeof(BlockPlanar), typeof(ReflectPlanar), typeof(RefractPlanar), typeof(PortalPlanar), typeof(DividePlanar)
                        };
                        break;
                    case CoreMode.Casters:
                        cores = new List<Type>
                            {typeof(BasicCaster), typeof(AdvanceCaster)};
                        break;
                    case CoreMode.Bullets:
                        cores = new List<Type>
                            {typeof(BasicBullet), typeof(InstantBullet), typeof(PhysicalBullet), typeof(TrackerBullet), typeof(PathBullet)};
                        break;
                }
            else if (mode == Mode.TwoD)
                switch (coreMode)
                {
                    case CoreMode.RaySensors:
                        cores = new List<Type>
                        {
                            typeof(BasicRay2D), typeof(PipeRay2D), typeof(BoxRay2D), typeof(RadialRay2D),
                            typeof(ChainRay2D), typeof(ReflectRay2D),
                            typeof(TargetRay2D), typeof(PointerRay2D), typeof(WaveRay2D), typeof(NoiseRay2D), typeof(CurveRay2D), typeof(ArcRay2D), typeof(HybridRay2D)
                        };
                        break;
                    case CoreMode.Detectors:
                        cores = new List<Type>
                        {
                            typeof(LineDetector2D), typeof(RangeDetector2D), typeof(BoxDetector2D), typeof(PolyDetector2D), typeof(SurfaceDetector2D),
                            typeof(TargetDetector2D),
                            typeof(SteeringDetector2D), typeof(RadarDetector2D), typeof(PathDetector2D)
                        };
                        break;
                    case CoreMode.Planers:
                        cores = new List<Type>
                        {
                            typeof(BlockPlanar2D), typeof(ReflectPlanar2D), typeof(RefractPlanar2D),
                            typeof(PortalPlanar2D), typeof(DividePlanar2D)
                        };
                        break;
                    case CoreMode.Casters:
                        cores = new List<Type>
                        {
                            typeof(BasicCaster2D), typeof(AdvanceCaster2D)
                        };
                        break;
                    case CoreMode.Bullets:
                        cores = new List<Type>
                        {
                            typeof(BasicBullet2D), typeof(InstantBullet2D), typeof(PhysicalBullet2D), typeof(TrackerBullet2D),
                            typeof(PathBullet2D)
                        };
                        break;
                }
            }
            

            #endregion

            IconLayout(cores, 7);
            
            GUILayout.Space(4);
            RCProEditor.GUILine(lineColor);
            
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            // UNDO SYSTEM

            EditorGUILayout.BeginVertical(RCProEditor.BoxStyle);
            
            _so.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(settingProfile)), true);
            _so.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                settingProfilePath = AssetDatabase.GetAssetPath(settingProfile);
                SettingProfile = settingProfile;
                if (settingProfile)
                {
                    settingProfSO = new SerializedObject(settingProfile);
                    EditorPrefs.SetString(KEY + nameof(settingProfilePath), settingProfilePath);
                    SceneView.RepaintAll();
                }
            }

            if (settingProfile)
            {
                settingProfSO.Update();
                var property = settingProfSO.GetIterator();
                if (property.NextVisible(true)) {
                    do
                    {
                        var prop = settingProfSO.FindProperty(property.name);

                        if (prop.name != mScript)
                        {
                            EditorGUILayout.PropertyField(prop, true);
                        }
                    }
                    while (property.NextVisible(false));
                }
                settingProfSO.ApplyModifiedProperties();
            }
            

            EditorGUILayout.EndVertical();
            
            GUILayout.Space(4);
            RCProEditor.GUILine(lineColor);
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                richText = true
            };
            var heart = GetColorHash('♥');
            var play = GetColorHash('▶');
            string GetColorHash(char str) => $"<color=#{ColorHash}>{str}</color>";
            if (GUILayout.Button($"{heart} Thanks for Submit a Review {heart}", buttonStyle)) Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/raycastpro-214714#reviews");
            if (GUILayout.Button($"Follow tutorials on  {play} YouTube", buttonStyle)) Application.OpenURL("https://www.youtube.com/@KiynL");
            
            GUILayout.EndScrollView();
            GUILayout.Space(2);
            RCProEditor.GUILine(Color.white);
            GUILayout.Space(2);
            GUILayout.Label("Copyright all rights reserved", labelStyle);
            GUILayout.Space(2);
            RCProEditor.GUILine(Color.white);
            showOnStart = EditorGUILayout.Toggle("Show Panel on Start", showOnStart);
            RCProEditor.GUILine(Color.white);
        }

        [MenuItem("Tools/RaycastPro", priority = -10000)]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = GetWindow(typeof(RCProPanel), true, "RaycastPro Panel", true);

            window.maxSize = new Vector2(width, height);
            window.minSize = new Vector2(width, height);

            window.Show();
            
            mode = SceneView.lastActiveSceneView.in2DMode ? Mode.TwoD : Mode.ThreeD;
        }

        public static void SavePreferences()
        {
            RCProEditor.Log("<color=#00FF00>Preferences Saves.</color>");
            var type = typeof(RCProPanel);
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.GetCustomAttribute(typeof(SavePreference)) == null) continue;

                if (fieldInfo.FieldType == typeof(bool))
                    EditorPrefs.SetBool(KEY + fieldInfo.Name, (bool) fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(float))
                    EditorPrefs.SetFloat(KEY + fieldInfo.Name, (float) fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(int))
                    EditorPrefs.SetInt(KEY + fieldInfo.Name, (int) fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(string))
                    EditorPrefs.SetString(KEY + fieldInfo.Name, (string) fieldInfo.GetValue(null));
                else if (fieldInfo.FieldType == typeof(Color))
                    SaveColor(KEY + fieldInfo.Name, (Color) fieldInfo.GetValue(null));
            }
        }
        public static void LoadPreferences(bool message = true)
        {
            if (message) RCProEditor.Log("<color=#00FF00>Preferences Loaded.</color>");

            var type = typeof(RCProPanel);
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.GetCustomAttribute(typeof(SavePreference)) == null) continue;
                if (!EditorPrefs.HasKey(KEY + fieldInfo.Name)) continue;
                if (fieldInfo.FieldType == typeof(bool))
                {
                    fieldInfo.SetValue(null, EditorPrefs.GetBool(KEY + fieldInfo.Name));
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    fieldInfo.SetValue(null, EditorPrefs.GetFloat(KEY + fieldInfo.Name));
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    fieldInfo.SetValue(null, EditorPrefs.GetInt(KEY + fieldInfo.Name));
                }
                else if (fieldInfo.FieldType == typeof(string))
                {
                    fieldInfo.SetValue(null, EditorPrefs.GetString(KEY + fieldInfo.Name));
                }
                else if (fieldInfo.FieldType == typeof(Color))
                {
                    fieldInfo.SetValue(null, LoadColor(KEY + fieldInfo.Name));
                }
            }
        }
        private static void SaveColor(string key, Color color)
        {
            EditorPrefs.SetBool(key, true);
            EditorPrefs.SetFloat(key + "R", color.r);
            EditorPrefs.SetFloat(key + "G", color.g);
            EditorPrefs.SetFloat(key + "B", color.b);
            EditorPrefs.SetFloat(key + "A", color.a);
        }
        private static Color LoadColor(string key)
        {
            var col = new Color
            {
                r = EditorPrefs.GetFloat(key + "R"),
                g = EditorPrefs.GetFloat(key + "G"),
                b = EditorPrefs.GetFloat(key + "B"),
                a = EditorPrefs.GetFloat(key + "A")
            };
            return col;
        }
        
        /// <summary>
        /// Create every component once for catching their icons
        /// </summary>
        public static void RefreshIcons()
        {
            ICON_DICTIONARY = IconManager.GetIcons();
            
            foreach (var type in ICON_DICTIONARY.Keys.ToList())
            {
                if (type.IsAbstract) continue;
                try
                {
                    var obj = new GameObject();
                    if (obj)
                    {
                        var component = obj.AddComponent(type);
                        component?.SetIcon(ICON_DICTIONARY[type]);
                        DestroyImmediate(obj);
                    }
                }
                catch (Exception e)
                {
                    RCProEditor.Log(e.Message);
                    throw;
                }
            }

        }
        
        
        public static void IconLayout(List<Type> types, int columnWidth)
        {
            var rows = types.Count / columnWidth;
            var guiStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter
            };

            EditorGUILayout.BeginVertical(guiStyle);

            for (var i = 0; i <= rows; i++)
            {
                EditorGUILayout.BeginHorizontal(guiStyle);
                for (var j = 0; j < columnWidth; j++)
                {
                    var index = i * columnWidth + j;
                    if (index > types.Count - 1)
                    {
                        if (j == 0) break;
                        
                        GUILayout.Box("", GUILayout.Width(60), GUILayout.Height(60));
                    }
                    else
                    {
                        Button(types[index]);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
        public static bool Button(Type type)
        {
            var infoFiled = type.GetField("Info", BindingFlags.Static | BindingFlags.NonPublic);

            var info = infoFiled != null ? infoFiled.GetValue(null).ToString() : "No information";
            var content = ICON_DICTIONARY.Keys.Contains(type)
                ? new GUIContent(ICON_DICTIONARY[type], info)
                : new GUIContent(ICON_DICTIONARY[typeof(PointSensor)], info);

            var style = new GUIStyle(GUI.skin.button)
            {
                stretchWidth = false,
                border = new RectOffset(0, 0, 0,0),
                margin = new RectOffset(6, 6, 6, 6),
                padding = new RectOffset(4, 4, 4, 4),
                wordWrap = false,
            };
            
            EditorGUILayout.BeginVertical();
            GUI.contentColor = Color.white;
            GUI.backgroundColor = Color.white;
            
            var click = GUILayout.Button(content, style, GUILayout.Width(56), GUILayout.Height(56));

            var name = type.Name;

            if (type.IsSubclassOf(typeof(RaySensor))) name = name.Replace("Ray", "");
            else if (type.IsSubclassOf(typeof(RaySensor2D))) name = name.Replace("Ray2D", "");
            else if (type.IsSubclassOf(typeof(Detector))) name = name.Replace("Detector", "");
            else if (type.IsSubclassOf(typeof(Detector2D))) name = name.Replace("Detector2D", "");
            else if (type.IsSubclassOf(typeof(Planar))) name = name.Replace("Planar", "");
            else if (type.IsSubclassOf(typeof(Planar2D))) name = name.Replace("Planar2D", "");
            else if (type.IsSubclassOf(typeof(BaseCaster)))
            {
                name = name.Replace("Caster", "");
                name = name.Replace("2D", "");
            }
            else if (type.IsSubclassOf(typeof(Bullet)))
            {
                name = name.Replace("Bullet", "");
            }
            else if (type.IsSubclassOf(typeof(Bullet2D)))
            {
                name = name.Replace("Bullet2D", "");
            }

            style = new GUIStyle(RCProEditor.BoxStyle)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(4, 4, 0, 2)
            };

            GUILayout.Box($"<color=#2BC6D2>{name.ToRegex()}</color>", style, GUILayout.Width(60), GUILayout.Height(30));
            EditorGUILayout.EndVertical();
            GUI.contentColor = RCProEditor.Aqua;
            GUI.backgroundColor = RCProEditor.Violet;

            if (click) CreateCore(type);
            return click;
        }
        private static void CreateCore(Type type)
        {
            var obj = new GameObject();

            if (type.IsSubclassOf(typeof(Planar)))
            {
                DestroyImmediate(obj);
                obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                obj.transform.localScale = new Vector3(10f, 10f, 1f);
            }
            else if (type.IsSubclassOf(typeof(Planar2D)))
            {
                var spriteRenderer = obj.AddComponent<SpriteRenderer>();

                var tex = new Texture2D(20, 200);
                var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                    100.0f);

                sprite.name = type.Name;

                spriteRenderer.sprite = sprite;
                spriteRenderer.color = DefaultColor;

                obj.transform.localScale = new Vector3(1, 1f, 1f);

                var boxCollider = obj.AddComponent<BoxCollider2D>();

                boxCollider.size = new Vector2(.2f, 2);
            }
            else if (type.IsSubclassOf(typeof(Bullet)))
            {
                DestroyImmediate(obj);

                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                obj.transform.localScale = new Vector3(.4f, .4f, 1f);
            }
            else if (type.IsSubclassOf(typeof(Bullet2D)))
            {
                var spriteRenderer = obj.AddComponent<SpriteRenderer>();

                var tex = new Texture2D(100, 60);
                var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                    100.0f);

                sprite.name = type.Name;

                spriteRenderer.sprite = sprite;

                spriteRenderer.color = DefaultColor;
                obj.transform.localScale = new Vector3(1, 1f, 1f);

                var boxCollider = obj.AddComponent<BoxCollider2D>();

                boxCollider.size = new Vector2(1, .6f);
            }

            obj.name = type.Name.ToRegex();

            Undo.RegisterCreatedObjectUndo(obj, "create_core, ID: " + obj.GetInstanceID());

            var camera = SceneView.lastActiveSceneView.camera.transform;

            obj.transform.position = camera.position + camera.forward * 10f;

            obj.AddComponent(type);

            var activeSelection = Selection.activeTransform;

            if (activeSelection)
            {
                obj.transform.parent = activeSelection;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }

            Selection.activeTransform = obj.transform;
        }

        [AttributeUsage(AttributeTargets.Field)]
        internal class SavePreference : Attribute { }
        
        [AttributeUsage(AttributeTargets.Class)]
        internal class RawEditor : Attribute { }

        internal enum CoreMode
        {
            RaySensors,
            Detectors,
            Planers,
            Casters,
            Bullets
        }

        internal enum Mode
        {
            Utility,
            TwoD,
            ThreeD,
        }
    }
}
#endif