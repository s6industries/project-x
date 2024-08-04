using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.UICSerialization;

namespace MeadowGames.UINodeConnect4.SampleScene.SerializationSample
{
    [ExecuteInEditMode]
    public class ConnectionColorManager : MonoBehaviour
    {
        public GraphManager graphManager;
        public Color flowConnectionColor;
        public Color parameterConnectionColor;

        void OnEnable()
        {
            UICSystemManager.UICEvents.StartListening(UICEventType.OnConnectionCreated, ColorConnection);
            SerializationEvents.OnConnectionDeserialize.AddListener(ColorConnection);

            UpdateConnectionsColor(graphManager);
        }

        void OnDisable()
        {
            UICSystemManager.UICEvents.StopListening(UICEventType.OnConnectionCreated, ColorConnection);
            SerializationEvents.OnConnectionDeserialize.RemoveListener(ColorConnection);
        }

        public void UpdateConnectionsColor(GraphManager graphManager)
        {
            foreach (Connection connection in graphManager.localConnections)
            {
                ColorConnection(connection as IElement);
            }
        }

        public void ColorConnection(IElement element)
        {
            Connection connection = element as Connection;
            ColorConnection(connection);
        }

        public void ColorConnection(Connection connection)
        {
            if (connection.port0.ID == "Flow")
            {
                connection.defaultColor = flowConnectionColor;
                connection.ElementColor = flowConnectionColor;
            }

            if (connection.port0.ID == "Parameter")
            {
                connection.defaultColor = parameterConnectionColor;
                connection.ElementColor = parameterConnectionColor;
            }
        }

    }
}