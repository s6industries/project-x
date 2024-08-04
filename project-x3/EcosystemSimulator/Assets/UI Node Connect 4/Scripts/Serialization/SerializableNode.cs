using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    [System.Serializable]
    public class SerializableNode
    {
        /// <summary>
        /// Constructor for the SerializableNode
        /// </summary>
        /// <param name="node">Element to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        /// <param name="serializePorts">Serialize all the ports data to a List<SerializablePort> serializablePorts</param>
        public SerializableNode(Node node, bool serializeWithNewSID = true, bool serializePorts = true)
        {
            ToSerializable(node, serializeWithNewSID, serializePorts);
        }

        public SerializableRectTransform serializableRectTransform;

        public string id;
        public string sID;

        public bool enableSelfConnection;
        public bool enableDrag;
        public bool enableHover;
        public bool enableSelect;
        public bool disableClick;

        public Color defaultColor;
        public Color outlineSelectedColor;
        public Color outlineHoverColor;

        public List<SerializablePort> serializablePorts = new List<SerializablePort>();

        void ToSerializable(Node node, bool serializeWithNewSID = true, bool serializePorts = true)
        {
            serializableRectTransform = new SerializableRectTransform(node.rectTransform);

            id = node.ID;
            sID = serializeWithNewSID ? UICUtility.GenerateSID() : node.SID;
            enableSelfConnection = node.enableSelfConnection;
            enableDrag = node.EnableDrag;
            enableHover = node.EnableHover;
            enableSelect = node.EnableSelect;
            disableClick = node.DisableClick;
            defaultColor = node.defaultColor;
            outlineSelectedColor = node.outlineSelectedColor;
            outlineHoverColor = node.outlineHoverColor;

            if (serializePorts)
            {
                serializablePorts = new List<SerializablePort>();

                foreach (Port port in node.ports)
                {
                    SerializablePort serializablePort = new SerializablePort(port, serializeWithNewSID);
                    serializablePort.nodeSID = sID;
                    serializablePorts.Add(serializablePort);
                }
            }
        }

        /// <summary>
        /// deserialize data to the indicated node except ports (data is stored if serializePorts is true when serialized
        /// and can be used from serializableNode.serializablePorts)
        /// </summary>
        /// <param name="node">element that the data will be loaded to</param>
        public void FromSerializable(Node node)
        {
            serializableRectTransform.FromSerializable(node.rectTransform);

            node.ID = id;
            node.SID = sID;
            node.enableSelfConnection = enableSelfConnection;
            node.EnableDrag = enableDrag;
            node.EnableHover = enableHover;
            node.EnableSelect = enableSelect;
            node.DisableClick = disableClick;
            node.defaultColor = defaultColor;
            node.outlineSelectedColor = outlineSelectedColor;
            node.outlineHoverColor = outlineHoverColor;

            node.ElementColor = defaultColor;
        }
    }
}