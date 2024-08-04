using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    /// <summary>
    /// Class containing methods to help on the serialization of a Connection to JSON
    /// </summary>
    public static class ConnectionSerializer
    {
        public static string ToJSON(Connection connection)
        {
            SerializableConnection serializableConnection = new SerializableConnection(connection);
            SerializationEvents.OnConnectionSerialize.Invoke(serializableConnection);
            return JsonUtility.ToJson(serializableConnection);
        }

        public static bool FromJSON(Connection connection, string jsonString, GraphManager graphManager, bool findPortByInstanceID = false)
        {
            SerializableConnection serializableConnection = JsonUtility.FromJson<SerializableConnection>(jsonString);

            Port port0 = null;
            Port port1 = null;

            foreach (Node node in UICSystemManager.Nodes)
            {
                foreach (Port port in node.ports)
                {
                    if (!findPortByInstanceID)
                    {
                        if (port.SID == serializableConnection.port0SID)
                        {
                            port0 = port;
                        }

                        if (port.SID == serializableConnection.port1SID)
                        {
                            port1 = port;
                        }
                    }
                    else
                    {
                        if (port.GetInstanceID() == serializableConnection.port0InstanceID)
                        {
                            port0 = port;
                        }

                        if (port.GetInstanceID() == serializableConnection.port1InstanceID)
                        {
                            port1 = port;
                        }
                    }
                }
            }

            if (port0 != null && port1 != null)
            {
                serializableConnection.FromSerializable(connection, graphManager);
                connection.port0 = port0;
                connection.port1 = port1;

                SerializationEvents.OnConnectionDeserialize.Invoke(connection);

                return true;
            }

            return false;
        }

        public static Connection FromJSONInstantiate(string jsonString, GraphManager graphManager, bool findPortByInstanceID = false, bool createLabel = false)
        {
            SerializableConnection serializableConnection = JsonUtility.FromJson<SerializableConnection>(jsonString);

            return FromSerializableInstantiate(serializableConnection, graphManager, findPortByInstanceID, createLabel);
        }

        public static Connection FromSerializableInstantiate(SerializableConnection serializableConnection, GraphManager graphManager, bool findPortByInstanceID = false, bool createLabel = false)
        {
            Port port0 = null;
            Port port1 = null;

            foreach (Node node in UICSystemManager.Nodes)
            {
                foreach (Port port in node.ports)
                {
                    if (!findPortByInstanceID)
                    {
                        if (port.SID == serializableConnection.port0SID)
                        {
                            port0 = port;
                        }

                        if (port.SID == serializableConnection.port1SID)
                        {
                            port1 = port;
                        }
                    }
                    else
                    {
                        if (port.GetInstanceID() == serializableConnection.port0InstanceID)
                        {
                            port0 = port;
                        }

                        if (port.GetInstanceID() == serializableConnection.port1InstanceID)
                        {
                            port1 = port;
                        }
                    }
                }
            }

            Connection connection = new Connection();

            if (port0 != null && port1 != null)
            {
                serializableConnection.FromSerializable(connection, graphManager);

                if (createLabel && serializableConnection.label != "")
                    connection.SetLabel(serializableConnection.label);

                connection.port0 = port0;
                connection.port1 = port1;

                UICSystemManager.AddConnectionToList(connection);

                SerializationEvents.OnConnectionDeserialize.Invoke(connection);

                return connection;
            }

            return connection;
        }
    }
}
