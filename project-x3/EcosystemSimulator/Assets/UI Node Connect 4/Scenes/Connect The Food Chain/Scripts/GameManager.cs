using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.ConnectTheFoodChain
{
    public class GameManager : MonoBehaviour
    {
        public List<string> allowedConnections;

        void OnEnable()
        {
            UICSystemManager.UICEvents.StartListening(UICEventType.ConnectionAdded, ValidateConnection);
        }

        void OnDisable()
        {
            UICSystemManager.UICEvents.StopListening(UICEventType.ConnectionAdded, ValidateConnection);
        }

        void ValidateConnection(IElement element)
        {
            Connection connection = element as Connection;

            foreach (string allowed in allowedConnections)
            {
                string start = allowed.Split('-')[0];
                string end = allowed.Split('-')[1];

                if (start == connection.port0.node.ID && end == connection.port1.node.ID)
                {
                    connection.defaultColor = new Color(0.48f, 0.75f, 0.44f);
                    connection.ElementColor = connection.defaultColor;
                    connection.line.capEnd.color = connection.defaultColor;
                }
            }
        }
    }
}