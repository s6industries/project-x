using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    /// <summary>
    /// A data class with lists for SerializableNode and SerializableConnection.
    /// </summary>
    [System.Serializable]
    public class SerializableGraph
    {
        /// <summary>
        /// Constructor for the SerializableGraph
        /// </summary>
        /// <param name="nodes">Elements to serialize</param>
        /// <param name="connections">Element to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        public SerializableGraph(List<Node> nodes, List<Connection> connections, bool serializeWithNewSID = true)
        {
            ToSerializable(nodes, connections, serializeWithNewSID);
        }

        /// <summary>
        /// Constructor for the SerializableGraph
        /// </summary>
        /// <param name="elements">Elements to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        public SerializableGraph(List<IGraphElement> elements, bool serializeWithNewSID = true)
        {
            ToSerializable(elements, serializeWithNewSID);
        }

        /// <summary>
        /// Constructor for the SerializableGraph
        /// </summary>
        /// <param name="elements">Elements to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        public SerializableGraph(List<ISelectable> elements, bool serializeWithNewSID = true)
        {
            ToSerializable(elements, serializeWithNewSID);
        }

        public List<SerializableNode> serializableNodes = new List<SerializableNode>();
        public List<SerializableConnection> serializableConnections = new List<SerializableConnection>();

        void ToSerializable(List<Node> nodes, List<Connection> connections, bool serializeWithNewSID = true)
        {
            List<IGraphElement> elements = new List<IGraphElement>();
            elements.AddRange(nodes.Cast<IGraphElement>().ToArray());
            elements.AddRange(connections.Cast<IGraphElement>().ToArray());
            ToSerializable<IGraphElement>(elements, serializeWithNewSID);
        }

        void ToSerializable(List<IGraphElement> elements, bool serializeWithNewSID = true)
        {
            ToSerializable<IGraphElement>(elements, serializeWithNewSID);
        }

        void ToSerializable(List<ISelectable> elements, bool serializeWithNewSID = true)
        {
            ToSerializable<ISelectable>(elements, serializeWithNewSID);
        }

        void ToSerializable<T>(List<T> elements, bool serializeWithNewSID = true)
        {
            Dictionary<string, string> sIDPortMap = new Dictionary<string, string>();

            serializableNodes = new List<SerializableNode>();
            List<Connection> connections = new List<Connection>();
            foreach (T element in elements)
            {
                Node node = element as Node;
                if (node)
                {
                    SerializableNode serializableNode = new SerializableNode(node, serializeWithNewSID, false);
                    serializableNodes.Add(serializableNode);

                    serializableNode.serializablePorts = new List<SerializablePort>();
                    foreach (Port port in node.ports)
                    {
                        SerializablePort serializablePort = new SerializablePort(port, serializeWithNewSID);
                        serializablePort.nodeSID = serializableNode.sID;
                        serializableNode.serializablePorts.Add(serializablePort);

                        sIDPortMap.Add(port.SID, serializablePort.sID);
                    }
                }
                else
                {
                    Connection connection = element as Connection;
                    if (connection != null)
                    {
                        connections.Add(connection);
                    }
                }
            }

            serializableConnections = new List<SerializableConnection>();
            foreach (Connection connection in connections)
            {
                string p0 = sIDPortMap.TryGetValue(connection.port0.SID);
                string p1 = sIDPortMap.TryGetValue(connection.port1.SID);
                if (p0 != null && p1 != null)
                {
                    SerializableConnection serializableConnection = new SerializableConnection(connection, serializeWithNewSID);
                    serializableConnection.port0SID = sIDPortMap[connection.port0.SID];
                    serializableConnection.port1SID = sIDPortMap[connection.port1.SID];
                    serializableConnections.Add(serializableConnection);
                }
            }
        }
    }
}