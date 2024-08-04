using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    /// <summary>
    /// Class containing methods to help on the serialization of all or selected elements of a specific graph to JSON
    /// </summary>
    public static class GraphSerializer
    {
        public static string ToJSON(GraphManager graphManager)
        {
            SerializableGraph serializableGraph = new SerializableGraph(graphManager.localNodes, graphManager.localConnections);
            SerializationEvents.OnGraphSerialize.Invoke(serializableGraph);
            return JsonUtility.ToJson(serializableGraph);
        }

        public static string ToJSON(List<Node> nodes, List<Connection> connections)
        {
            SerializableGraph serializableGraph = new SerializableGraph(nodes, connections);
            SerializationEvents.OnGraphSerialize.Invoke(serializableGraph);
            return JsonUtility.ToJson(serializableGraph);
        }

        public static string ToJSON(List<IGraphElement> elements)
        {
            SerializableGraph serializableGraph = new SerializableGraph(elements);
            SerializationEvents.OnGraphSerialize.Invoke(serializableGraph);
            return JsonUtility.ToJson(serializableGraph);
        }

        public static string ToJSON(List<ISelectable> elements)
        {
            SerializableGraph serializableGraph = new SerializableGraph(elements);
            SerializationEvents.OnGraphSerialize.Invoke(serializableGraph);
            return JsonUtility.ToJson(serializableGraph);
        }

        public static void FromJSONInstantiate(GraphManager graphManager, string jsonString, Transform parent, DeserializationTemplates deserializationTemplates)//, bool handleDuplicateSID = true)
        {
            SerializableGraph serializableGraph = JsonUtility.FromJson<SerializableGraph>(jsonString);
            FromSerializableInstantiate(serializableGraph, graphManager, parent, deserializationTemplates);
        }

        public static void FromSerializableInstantiate(SerializableGraph serializableGraph, GraphManager graphManager, Transform parent, DeserializationTemplates deserializationTemplates)//, bool handleDuplicateSID = true)
        {
            foreach (SerializableNode serializableNode in serializableGraph.serializableNodes)
            {
                NodeSerializer.FromSerializableInstantiate(serializableNode, parent, deserializationTemplates);
            }

            foreach (SerializableConnection serializableConnection in serializableGraph.serializableConnections)
            {
                ConnectionSerializer.FromSerializableInstantiate(serializableConnection, graphManager, false);
            }
            SerializationEvents.OnGraphDeserialize.Invoke(graphManager);
        }
    }
}
