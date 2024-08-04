namespace MeadowGames.UINodeConnect4.UICSerialization
{
    public static class SerializationEvents
    {
        public static UICEvent<SerializableGraph> OnGraphSerialize = new UICEvent<SerializableGraph>();
        public static UICEvent<GraphManager> OnGraphDeserialize = new UICEvent<GraphManager>();

        public static UICEvent<SerializableNode> OnNodeSerialize = new UICEvent<SerializableNode>();
        public static UICEvent<Node> OnNodeDeserialize = new UICEvent<Node>();

        public static UICEvent<SerializablePort> OnPortSerialize = new UICEvent<SerializablePort>();
        public static UICEvent<Port> OnPortDeserialize = new UICEvent<Port>();

        public static UICEvent<SerializableConnection> OnConnectionSerialize = new UICEvent<SerializableConnection>();
        public static UICEvent<Connection> OnConnectionDeserialize = new UICEvent<Connection>();
    }
}
