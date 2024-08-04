using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static MeadowGames.UINodeConnect4.Port;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    [System.Serializable]
    public class SerializablePort
    {
        /// <summary>
        /// Constructor for the SerializablePort
        /// </summary>
        /// <param name="port">Element to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        public SerializablePort(Port port, bool serializeWithNewSID = true)
        {
            ToSerializable(port, serializeWithNewSID);
        }

        public SerializableRectTransform serializableRectTransform;

        public string id;
        public string sID;

        public PolarityType polarity;
        public int maxConnections;

        public Color iconColorDefault;
        public Color iconColorHover;
        public Color iconColorSelected;
        public Color iconColorConnected;

        public bool enableDrag;
        public bool enableHover;
        public bool disableClick;

        public SerializableRectTransform controlPointSerializableRectTransform;

        public int nodeInstanceID;
        public string nodeSID;

        void ToSerializable(Port port, bool serializeWithNewSID = true)
        {
            serializableRectTransform = new SerializableRectTransform(port.rectTransform);

            id = port.ID;
            sID = serializeWithNewSID ? UICUtility.GenerateSID() : port.SID;

            polarity = port.Polarity;
            maxConnections = port.maxConnections;

            iconColorDefault = port.iconColorDefault;
            iconColorHover = port.iconColorHover;
            iconColorSelected = port.iconColorSelected;
            iconColorConnected = port.iconColorConnected;

            enableDrag = port.EnableDrag;
            enableHover = port.EnableHover;
            disableClick = port.DisableClick;

            controlPointSerializableRectTransform = new SerializableRectTransform(port.controlPoint.transform as RectTransform);

            nodeInstanceID = port.node.GetInstanceID();
            nodeSID = serializeWithNewSID ? UICUtility.GenerateSID() : port.node.SID;
        }

        /// <summary>
        /// deserialize data to the indicated port
        /// </summary>
        /// <param name="port">element that the data will be loaded to</param>
        public void FromSerializable(Port port)
        {
            serializableRectTransform.FromSerializable(port.rectTransform);

            port.ID = id;
            port.SID = sID;

            port.Polarity = polarity;
            port.maxConnections = maxConnections;

            port.iconColorDefault = iconColorDefault;
            port.iconColorHover = iconColorHover;
            port.iconColorSelected = iconColorSelected;
            port.iconColorConnected = iconColorConnected;

            port.EnableDrag = enableDrag;
            port.EnableHover = enableHover;
            port.DisableClick = disableClick;

            controlPointSerializableRectTransform.FromSerializable(port.controlPoint.transform as RectTransform);
        }
    }
}