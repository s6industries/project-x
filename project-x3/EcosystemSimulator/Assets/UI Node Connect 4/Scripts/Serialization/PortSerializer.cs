using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    /// <summary>
    /// Class containing methods to help on the serialization of a Port to JSON
    /// </summary>
    public static class PortSerializer
    {
        public static string ToJSON(Port port)
        {
            SerializablePort serializablePort = new SerializablePort(port);
            SerializationEvents.OnPortSerialize.Invoke(serializablePort);
            return JsonUtility.ToJson(serializablePort);
        }

        public static void FromJSON(Port port, string jsonString)
        {
            SerializablePort serializablePort = JsonUtility.FromJson<SerializablePort>(jsonString);
            SerializationEvents.OnPortSerialize.Invoke(serializablePort);
            serializablePort.FromSerializable(port);
        }

        public static Port FromJSONInstantiate(string jsonString, Transform parent, DeserializationTemplates deserializationTemplates)
        {
            SerializablePort serializablePort = JsonUtility.FromJson<SerializablePort>(jsonString);

            return FromSerializableInstantiate(serializablePort, parent, deserializationTemplates);
        }

        public static Port FromSerializableInstantiate(SerializablePort serializablePort, Transform parent, DeserializationTemplates deserializationTemplates)
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
                port = MonoBehaviour.Instantiate(port, Vector3.zero, Quaternion.identity, parent);
            }
            else
            {
                Debug.Log("No template available");
                return null;
            }

            serializablePort.FromSerializable(port);

            SerializationEvents.OnPortDeserialize.Invoke(port);

            return port;
        }
    }
}
