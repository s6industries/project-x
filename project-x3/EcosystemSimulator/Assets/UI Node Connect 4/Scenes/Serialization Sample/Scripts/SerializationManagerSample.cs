using System.Collections;
using System.Collections.Generic;
using System.IO;
using MeadowGames.UINodeConnect4.UICSerialization;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.SerializationSample
{
    [ExecuteInEditMode]
    public class SerializationManagerSample : MonoBehaviour
    {
        public GraphManager graphManager;
        public DeserializationTemplates deserializationTemplates;
        public string saveFileName = "SavedFile";

        public string GetPath(string fileName)
        {
            return Application.dataPath + "/UI Node Connect 4/Scenes/Serialization Sample/Saves/" + fileName + ".txt";
        }

        public void SaveGraph()
        {
            string jsonString = GraphSerializer.ToJSON(graphManager);
            SaveJSONToFile(jsonString, GetPath(saveFileName));
        }

        public void LoadGraph()
        {
            string jsonString = LoadJSONFromFile(GetPath(saveFileName));
            GraphSerializer.FromJSONInstantiate(graphManager, jsonString, graphManager.transform, deserializationTemplates);
        }

        public void SaveSelected()
        {
            string jsonString = GraphSerializer.ToJSON(UICSystemManager.selectedElements);
            SaveJSONToFile(jsonString, GetPath(saveFileName));
        }

        public void SaveJSONToFile(string jsonString, string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(jsonString);
            writer.Close();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public string LoadJSONFromFile(string path)
        {
            StreamReader reader = new StreamReader(path);
            string jsonString = reader.ReadToEnd();
            reader.Close();
            return jsonString;
        }
    }
}