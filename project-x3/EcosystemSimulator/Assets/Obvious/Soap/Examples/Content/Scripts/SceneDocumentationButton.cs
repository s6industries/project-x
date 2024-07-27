using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Obvious.Soap.Example
{
    [RequireComponent(typeof(Button))]
    public class SceneDocumentationButton : CacheComponent<Button>
    {
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            var sceneName = SceneManager.GetActiveScene().name.Replace("_Example_Scene", string.Empty);
            var parentFolder = Path.GetDirectoryName(GetSoapUserGuidePath());
            var docPath = parentFolder + $@"\Example Scenes\{sceneName}.pdf";
            _component.onClick.AddListener(() => { Application.OpenURL(docPath); });
#endif
        }
#if UNITY_EDITOR
        private string GetSoapUserGuidePath()
        {
            var guid = AssetDatabase.FindAssets("Soap User Guide").FirstOrDefault();
            return guid != null ? AssetDatabase.GUIDToAssetPath(guid) : null;
        }
#endif
       
    }
}