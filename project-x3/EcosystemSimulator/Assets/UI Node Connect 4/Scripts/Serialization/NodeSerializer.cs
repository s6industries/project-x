using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    /// <summary>
    /// Class containing methods to help on the serialization of a Node to JSON
    /// </summary>
    public static class NodeSerializer
    {
        public static string ToJSON(Node node, bool serializeWithNewSID = true, bool serializePorts = true)
        {
            SerializableNode serializableNode = new SerializableNode(node, serializeWithNewSID, serializePorts);
            SerializationEvents.OnNodeSerialize.Invoke(serializableNode);
            return JsonUtility.ToJson(serializableNode);
        }

        public static void FromJSON(Node node, string jsonString)
        {
            SerializableNode serializableNode = JsonUtility.FromJson<SerializableNode>(jsonString);
            SerializationEvents.OnNodeSerialize.Invoke(serializableNode);
            serializableNode.FromSerializable(node);
        }

        public static Node FromJSONInstantiate(string jsonString, Transform parent, DeserializationTemplates deserializationTemplates)
        {
            SerializableNode serializableNode = JsonUtility.FromJson<SerializableNode>(jsonString);

            return FromSerializableInstantiate(serializableNode, parent, deserializationTemplates);
        }

        public static Node FromSerializableInstantiate(SerializableNode serializableNode, Transform parent, DeserializationTemplates deserializationTemplates)
        {
            Node node = deserializationTemplates?.nodeTemplates.Find(serializableNode.id);

            if (node)
            {
                node = MonoBehaviour.Instantiate(node, Vector3.zero, Quaternion.identity, parent);

                serializableNode.FromSerializable(node);

                foreach (SerializablePort serializablePort in serializableNode.serializablePorts)
                {
                    Port port = null;
                    if (serializablePort.polarity == Port.PolarityType._in)
                        port = deserializationTemplates?.portInTemplates.Find(serializablePort.id);
                    if (serializablePort.polarity == Port.PolarityType._out)
                        port = deserializationTemplates?.portOutTemplates.Find(serializablePort.id);
                    if (serializablePort.polarity == Port.PolarityType._all)
                        port = deserializationTemplates?.portAllTemplates.Find(serializablePort.id);

                    if (port)
                    {
                        Transform portParent = node.transform;
                        foreach (Transform child in portParent)
                        {
                            if (child.name == "Ports")
                            {
                                portParent = child;
                                break;
                            }
                        }

                        port = MonoBehaviour.Instantiate(port, Vector3.zero, Quaternion.identity, portParent);
                    }
                    else
                    {
                        Debug.Log("No template available");
                        return null;
                    }

                    serializablePort.FromSerializable(port);
                }
            }
            else
            {
                Debug.Log("No template available");
                return null;
            }

            UICSystemManager.AddNodeToList(node);

            SerializationEvents.OnNodeDeserialize.Invoke(node);

            return node;
        }
    }
}
