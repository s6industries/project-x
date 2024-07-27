using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace Obvious.Soap.Editor
{
    public class SoapWizardWindow : EditorWindow
    {
        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private Dictionary<ScriptableBase, Object> _subAssetsLookup;
        private ScriptableType _currentType = ScriptableType.All;
        private readonly float _tabWidth = 45f;
        private Texture[] _icons;
        private string _searchText = "";
        private UnityEditor.Editor _editor;
        private List<(GameObject, Type, string, string)> _sceneReferences;
        private Dictionary<string, int> _assetReferences;

        [SerializeField] private string _currentFolderPath = "Assets";
        [SerializeField] private int _selectedScriptableIndex;
        [SerializeField] private int _typeTabIndex = -1;
        [SerializeField] private int _tagMask;
        [SerializeField] private bool _isInitialized;
        [SerializeField] private ScriptableBase _scriptableBase;
        [SerializeField] private ScriptableBase _previousScriptableBase;
        [SerializeField] private FavoriteData _favoriteData;

        private List<ScriptableBase> Favorites => _favoriteData.Favorites;
        internal const string PathKey = "SoapWizard_Path";
        internal const string FavoriteKey = "SoapWizard_Favorites";
        internal const string TagsKey = "SoapWizard_Tags";
        private SoapSettings _soapSettings;

        [Serializable]
        private class FavoriteData
        {
            public List<ScriptableBase> Favorites = new List<ScriptableBase>();
        }

        private enum ScriptableType
        {
            All,
            Variable,
            Event,
            List,
            Enum,
            Favorite
        }

        [MenuItem("Window/Obvious Game/Soap/Soap Wizard")]
        public new static void Show()
        {
            var window = GetWindow<SoapWizardWindow>(typeof(SceneView));
            window.titleContent = new GUIContent("Soap Wizard", Resources.Load<Texture>("Icons/icon_soapLogo"));
        }

        [MenuItem("Tools/Obvious Game/Soap/Soap Wizard")]
        private static void OpenSoapWizard() => Show();

        private void OnEnable()
        {
            _soapSettings = SoapEditorUtils.GetOrCreateSoapSettings();
            this.wantsMouseMove = true;
            LoadIcons();
            LoadSavedData();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (_soapSettings.ReferencesRefreshMode == EReferencesRefreshMode.Auto)
            {
                EditorApplication.hierarchyChanged += OnHierarchyChanged;
                EditorApplication.projectChanged += OnProjectChanged;
            }

            if (_isInitialized)
            {
                SelectTab(_typeTabIndex);
                return;
            }

            SelectTab((int)_currentType, true); //default is 0
            _isInitialized = true;
        }

        private void OnDisable()
        {
            var favoriteData = JsonUtility.ToJson(_favoriteData, false);
            EditorPrefs.SetString(FavoriteKey, favoriteData);
            EditorPrefs.SetInt(TagsKey, _tagMask);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void LoadIcons()
        {
            _icons = new Texture[16];
            _icons[0] = EditorGUIUtility.IconContent("Favorite On Icon").image;
            _icons[1] = Resources.Load<Texture>("Icons/icon_scriptableVariable");
            _icons[2] = Resources.Load<Texture>("Icons/icon_scriptableEvent");
            _icons[3] = Resources.Load<Texture>("Icons/icon_scriptableList");
            _icons[4] = Resources.Load<Texture>("Icons/icon_scriptableEnum");
            _icons[5] = Resources.Load<Texture>("Icons/icon_ping");
            _icons[6] = Resources.Load<Texture>("Icons/icon_edit");
            _icons[7] = Resources.Load<Texture>("Icons/icon_duplicate");
            _icons[8] = Resources.Load<Texture>("Icons/icon_delete");
            _icons[9] = null;
            _icons[10] = EditorGUIUtility.IconContent("Folder Icon").image;
            _icons[11] = Resources.Load<Texture>("Icons/icon_eventListener");
            _icons[12] = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            _icons[13] = EditorGUIUtility.IconContent("Button Icon").image;
            _icons[14] = EditorGUIUtility.IconContent("cs Script Icon").image;
            _icons[15] = EditorGUIUtility.IconContent("Favorite Icon").image;
        }

        private void LoadSavedData()
        {
            _currentFolderPath = EditorPrefs.GetString(PathKey, "Assets");
            _favoriteData = new FavoriteData();
            var favoriteDataJson = JsonUtility.ToJson(_favoriteData, false);
            var favoriteData = EditorPrefs.GetString(FavoriteKey, favoriteDataJson);
            JsonUtility.FromJsonOverwrite(favoriteData, _favoriteData);
            _tagMask = EditorPrefs.GetInt(TagsKey, 1);
        }

        private void OnGUI()
        {
            if (_soapSettings == null)
                return;

            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(2);
            var leftPanelWidth = 6 * _tabWidth;
            DrawLeftPanel(leftPanelWidth);
            SoapInspectorUtils.DrawVerticalColoredLine(2, Color.black.Lighten(0.15f));
            var rightPanelWidth = position.width - (leftPanelWidth) - 5 - 2f;
            DrawRightPanel(rightPanelWidth);
            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        private void DrawLeftPanel(float width)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            DrawFolder();
            GUILayout.Space(2);
            DrawTags();
            GUILayout.Space(2);
            DrawSearchBar();
            SoapInspectorUtils.DrawColoredLine(1, Color.black.Lighten(0.137f));
            DrawTabs();
            DrawScriptableBases(_scriptableObjects);
            EditorGUILayout.EndVertical();
        }

        private void DrawRightPanel(float width)
        {
            if (_scriptableBase == null)
                return;

            EditorGUILayout.BeginVertical(GUILayout.Width(width));
            _itemScrollPosition = EditorGUILayout.BeginScrollView(_itemScrollPosition, GUILayout.ExpandHeight(true));

            DrawUtilityButtons(width);

            if (_scriptableBase == null) //can be deleted by button!
                return;

            //Draw Selected Scriptable
            if (_editor == null || _scriptableBase != _previousScriptableBase)
            {
                if (_previousScriptableBase != null)
                    _previousScriptableBase.RepaintRequest -= OnRepaintRequested;
                //reset references
                _sceneReferences = null;
                _assetReferences = null;
                UnityEditor.Editor.CreateCachedEditor(_scriptableBase, null, ref _editor);
                _previousScriptableBase = _scriptableBase;
                _scriptableBase.RepaintRequest += OnRepaintRequested;
            }

            _editor.DrawHeader();
            _editor.OnInspectorGUI();
            SoapInspectorUtils.DrawLine();
            GUILayout.Space(5f);
            if (!EditorApplication.isPlaying)
            {
                DrawReferences(width * 0.97f);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }


        private void DrawFolder()
        {
            EditorGUILayout.BeginHorizontal();
            var buttonContent = new GUIContent(_icons[10], "Change Selected Folder");
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(0, 2, 0, 0);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Height(20f), GUILayout.MaxWidth(40)))
            {
                var path = EditorUtility.OpenFolderPanel("Select folder to set path.", _currentFolderPath, "");

                //remove Application.dataPath from path & replace \ with / for cross-platform compatibility
                path = path.Replace(Application.dataPath, "Assets").Replace("\\", "/");

                if (!AssetDatabase.IsValidFolder(path))
                    EditorUtility.DisplayDialog("Error: File Path Invalid",
                        "Make sure the path is a valid folder in the project.", "Ok");
                else
                {
                    _currentFolderPath = path;
                    EditorPrefs.SetString(PathKey, _currentFolderPath);
                    OnTabSelected(_currentType, true);
                }
            }

            var displayedPath = $"{_currentFolderPath}/";
            EditorGUILayout.LabelField(displayedPath);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTags()
        {
            var height = 20f;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(height));
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(2, 2, 0, 0);
            buttonStyle.padding = new RectOffset(4, 4, 4, 4);
            var buttonContent = new GUIContent(_icons[6], "Edit Tags");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxWidth(25), GUILayout.MaxHeight(20)))
                PopupWindow.Show(new Rect(), new TagPopUpWindow(position));
            EditorGUILayout.LabelField("Tags", GUILayout.MaxWidth(70));
            var tags = _soapSettings.Tags.ToArray();
            _tagMask = EditorGUILayout.MaskField(_tagMask, tags);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();

            var tabNames = Enum.GetNames(typeof(ScriptableType));

            var defaultStyle = SoapInspectorUtils.Styles.ToolbarButton;
            var selectedStyle = new GUIStyle(defaultStyle);
            selectedStyle.normal.textColor = Color.white;

            for (int i = 0; i < tabNames.Length; i++)
            {
                var isSelected = i == _typeTabIndex;

                var style = isSelected ? selectedStyle : defaultStyle;

                if (GUILayout.Button(tabNames[i], style, GUILayout.Width(_tabWidth)))
                {
                    _typeTabIndex = i;
                    OnTabSelected((ScriptableType)_typeTabIndex, true);
                }
            }

            EditorGUILayout.EndHorizontal();

            // Draw the bottom line
            var lastRect = GUILayoutUtility.GetLastRect();
            var width = lastRect.width / tabNames.Length;
            var x = lastRect.x + _typeTabIndex * width;
            EditorGUI.DrawRect(new Rect(x, lastRect.yMax - 2, width, 2), Color.white);
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            _searchText = GUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("", GUI.skin.FindStyle("SearchCancelButton")))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawScriptableBases(List<ScriptableBase> scriptables)
        {
            if (scriptables is null)
                return;

            EditorGUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var count = scriptables.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var scriptable = scriptables[i];
                if (scriptable == null)
                    continue;

                //filter tags
                if ((_tagMask & (1 << scriptable.TagIndex)) == 0)
                    continue;

                var entryName = GetNameFor(scriptable);
                //filter search
                if (entryName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var entryStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = Color.white }
                };
                var rect = GUILayoutUtility.GetRect(new GUIContent(entryName), entryStyle);
                var selected = _selectedScriptableIndex == i;

                //Draw Background
                var backgroundRect = new Rect(rect);
                backgroundRect.height += 2f;
                backgroundRect.width *= 1.2f;
                backgroundRect.x -= 10f;
                if (selected)
                    EditorGUI.DrawRect(backgroundRect, new Color(0.172f, 0.365f, 0.529f));
                else if (rect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(backgroundRect, new Color(0.3f, 0.3f, 0.3f));

                //Draw icon
                var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite
                    ? GetIconFor(scriptable)
                    : _icons[(int)_currentType];
                var iconStyle = new GUIStyle(GUIStyle.none);
                var guiContent = new GUIContent();
                guiContent.image = icon;
                var iconRect = new Rect(rect);
                var iconWidth = 18f;
                iconRect.height = iconWidth;
                GUI.Box(iconRect, guiContent, iconStyle);

                //Draw Label or button
                rect.x += iconWidth + 1f;
                if (selected)
                {
                    GUI.Label(rect, entryName, entryStyle);
                }
                else if (GUI.Button(rect, entryName, EditorStyles.label)) //Select
                {
                    _selectedScriptableIndex = i;
                    _scriptableBase = scriptable;
                    GUIUtility.keyboardControl = 0; //remove focus (for TextArea description)
                }
            }

            //Handles deselection
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    var scriptable = scriptables[i];
                    if (scriptable == null)
                        continue;
                    var rect = GUILayoutUtility.GetRect(new GUIContent(scriptable.name), EditorStyles.label);
                    if (!rect.Contains(Event.current.mousePosition))
                    {
                        Deselect();
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private string GetNameFor(ScriptableBase scriptableBase)
        {
            if (_subAssetsLookup != null && _subAssetsLookup.TryGetValue(scriptableBase, out var mainAsset))
            {
                var prefix = $"[{mainAsset.name}] ";
                var subAssetName = prefix + scriptableBase.name;
                return subAssetName;
            }

            return scriptableBase.name;
        }
        
        private void DrawUtilityButtons(float width)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.padding = new RectOffset(0, 0, 3, 3);
            var lessPaddingStyle = new GUIStyle(buttonStyle);
            lessPaddingStyle.padding = new RectOffset(0, 0, 1, 1);
            
            var buttonHeight = 20;
            
            var icon = Favorites.Contains(_scriptableBase) ? _icons[15] : _icons[0];
            var tooltip = Favorites.Contains(_scriptableBase) ? "Remove from favorite" : "Add to favorite";
            var buttonContent = new GUIContent("Favorite",icon, tooltip);
            if (GUILayout.Button(buttonContent, lessPaddingStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                if (Favorites.Contains(_scriptableBase))
                    Favorites.Remove(_scriptableBase);
                else
                    Favorites.Add(_scriptableBase);
            }

            buttonContent = new GUIContent("Ping", _icons[5], "Pings the asset in the project");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                Selection.activeObject = _scriptableBase;
                EditorGUIUtility.PingObject(_scriptableBase);
            }

            buttonContent = new GUIContent("Rename", _icons[6]);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
                PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, _scriptableBase));

            GUI.enabled = AssetDatabase.IsMainAsset(_scriptableBase);
            buttonContent = new GUIContent("Duplicate", _icons[7], "Create Copy");
            if (GUILayout.Button(buttonContent, lessPaddingStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                SoapEditorUtils.CreateCopy(_scriptableBase);
                Refresh(_currentType);
            }
            GUI.enabled = true;

            buttonContent = new GUIContent("Delete", _icons[8]);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(buttonHeight)))
            {
                var isDeleted = SoapEditorUtils.DeleteObjectWithConfirmation(_scriptableBase);
                if (isDeleted)
                {
                    _scriptableBase = null;
                    OnTabSelected(_currentType, true);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawReferences(float width)
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(width));
            EditorGUILayout.LabelField("Find References (Used by)", EditorStyles.miniBoldLabel);

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.fontStyle = FontStyle.Bold;

            //Draw Scene Foldout Button
            {
                var suffix = _sceneReferences == null ? "" : $"({_sceneReferences.Count})";
                var buttonText = $"Scene {suffix}";
                if (GUILayout.Button(new GUIContent(buttonText, _icons[12]), buttonStyle, GUILayout.Height(18),
                        GUILayout.Width(width)))
                {
                    _sceneReferences = _sceneReferences == null
                        ? SoapEditorUtils.FindReferencesInScene(_scriptableBase)
                        : null;
                }
            }

            //Draw Scene Reference entries
            if (_sceneReferences != null)
            {
                foreach (var sceneReference in _sceneReferences)
                {
                    if (sceneReference.Item1 == null) //object could have been deleted (for manual refresh mode)
                        continue;
                    Texture icon = GetComponentIcon(sceneReference.Item2);
                    var data = new ReferenceEntryData
                    {
                        Obj = sceneReference.Item1,
                        Icon = icon,
                        Content = $"{sceneReference.Item1.name} ({sceneReference.Item2.Name})",
                        Argument = sceneReference.Item3
                    };
                    DrawReferenceEntry(data, 0.7f);
                }
            }

            //Draw Asset Foldout Button
            {
                var suffix = _assetReferences == null ? "" : $"({_assetReferences.Count})";
                var buttonText = $"Assets {suffix}";
                if (GUILayout.Button(new GUIContent(buttonText, _icons[10]), buttonStyle, GUILayout.Height(18),
                        GUILayout.Width(width)))
                {
                    _assetReferences = _assetReferences == null
                        ? SoapEditorUtils.FindReferencesInAssets(_scriptableBase)
                        : null;
                }
            }

            //Draw Asset Reference entries
            if (_assetReferences != null)
            {
                foreach (var assetReference in _assetReferences)
                {
                    var assetPath = assetReference.Key;
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (mainAsset == null) //object could have been deleted (for manual refresh mode)
                        continue;
                    var objectContent = EditorGUIUtility.ObjectContent(mainAsset, mainAsset.GetType());
                    Texture2D icon = objectContent.image as Texture2D;
                    var path = assetPath.Remove(0, 7);
                    var data = new ReferenceEntryData
                    {
                        Obj = mainAsset,
                        Icon = icon,
                        Content = path,
                        Argument = assetReference.Value.ToString()
                    };
                    DrawReferenceEntry(data, 0.95f);
                }
            }

            EditorGUILayout.EndVertical();
            return;

            void DrawReferenceEntry(ReferenceEntryData data, float ratio)
            {
                EditorGUILayout.BeginHorizontal();
                var style = new GUIStyle(EditorStyles.objectField)
                {
                    margin = new RectOffset(8, 0, 2, 2),
                    padding = new RectOffset(2, 2, 2, 2),
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 10
                };
                if (GUILayout.Button(new GUIContent(data.Content, data.Icon), style,
                        GUILayout.Height(18), GUILayout.Width(width * ratio - 8))) //remove offset
                {
                    EditorGUIUtility.PingObject(data.Obj);
                }

                style = new GUIStyle(EditorStyles.helpBox)
                {
                    margin = new RectOffset(0, 0, 0, 0),
                    stretchWidth = false,
                    wordWrap = false,
                    alignment = TextAnchor.MiddleLeft
                };
                EditorGUILayout.LabelField(data.Argument, style, GUILayout.Width(width * (1 - ratio)));
                EditorGUILayout.EndHorizontal();
            }

            Texture GetComponentIcon(Type componentType)
            {
                if (typeof(EventListenerBase).IsAssignableFrom(componentType))
                {
                    return _icons[11];
                }

                if (typeof(Button).IsAssignableFrom(componentType))
                {
                    return _icons[13];
                }

                return _icons[14];
            }
        }

        private void OnHierarchyChanged()
        {
            if (_soapSettings.ReferencesRefreshMode != EReferencesRefreshMode.Auto)
                return;

            if (_sceneReferences != null)
                _sceneReferences = SoapEditorUtils.FindReferencesInScene(_scriptableBase);
        }

        private void OnProjectChanged()
        {
            Refresh(_currentType);
            if (_soapSettings.ReferencesRefreshMode != EReferencesRefreshMode.Auto)
                return;

            if (_assetReferences != null)
                _assetReferences = SoapEditorUtils.FindReferencesInAssets(_scriptableBase);
        }

        private struct ReferenceEntryData
        {
            public Texture Icon;
            public Object Obj;
            public string Content;
            public string Argument;
        }

        private void OnTabSelected(ScriptableType type, bool deselectCurrent = false)
        {
            Refresh(type);
            _currentType = type;
            if (deselectCurrent)
            {
                Deselect();
            }
        }

        private void Deselect()
        {
            _selectedScriptableIndex = -1;
            _scriptableBase = null;
            GUIUtility.keyboardControl = 0; //remove focus
        }

        private void Refresh(ScriptableType type)
        {
            switch (type)
            {
                case ScriptableType.All:
                    _scriptableObjects =
                        SoapEditorUtils.FindAll<ScriptableBase>(_currentFolderPath, out _subAssetsLookup);
                    break;
                case ScriptableType.Variable:
                    var variables =
                        SoapEditorUtils.FindAll<ScriptableVariableBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = variables.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Event:
                    var events = SoapEditorUtils.FindAll<ScriptableEventBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = events.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.List:
                    var lists = SoapEditorUtils.FindAll<ScriptableListBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = lists.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Enum:
                    var enums = SoapEditorUtils.FindAll<ScriptableEnumBase>(_currentFolderPath, out _subAssetsLookup);
                    _scriptableObjects = enums.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Favorite:
                    _scriptableObjects = Favorites;
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            _typeTabIndex = index;
            OnTabSelected((ScriptableType)_typeTabIndex, deselect);
        }

        private Texture GetIconFor(ScriptableBase scriptableBase)
        {
            var iconIndex = 0;
            switch (scriptableBase)
            {
                case ScriptableVariableBase _:
                    iconIndex = 1;
                    break;
                case ScriptableEventBase _:
                    iconIndex = 2;
                    break;
                case ScriptableListBase _:
                    iconIndex = 3;
                    break;
                case ScriptableEnumBase _:
                    iconIndex = 4;
                    break;
            }

            return _icons[iconIndex];
        }

        #region Repaint

        private void OnPlayModeStateChanged(PlayModeStateChange pm)
        {
            if (_scriptableBase == null)
                return;
            if (pm == PlayModeStateChange.EnteredPlayMode)
                _scriptableBase.RepaintRequest += OnRepaintRequested;
            else if (pm == PlayModeStateChange.EnteredEditMode)
                _scriptableBase.RepaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested()
        {
            //Debug.Log("Repaint Wizard " + _scriptableBase.name);
            Repaint();
        }

        #endregion
    }
}