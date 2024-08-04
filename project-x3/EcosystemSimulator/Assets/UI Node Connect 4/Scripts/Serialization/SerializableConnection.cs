using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.GraphicRenderer;
using static MeadowGames.UINodeConnect4.Connection;

namespace MeadowGames.UINodeConnect4.UICSerialization
{
    [System.Serializable]
    public class SerializableConnection
    {
        /// <summary>
        /// Constructor for the SerializablePort
        /// </summary>
        /// <param name="connection">Element to serialize</param>
        /// <param name="serializeWithNewSID">Generate new unique SID for this SerializableNode</param>
        public SerializableConnection(Connection connection, bool serializeWithNewSID = false)
        {
            ToSerializable(connection, serializeWithNewSID);
        }

        public string id;
        public string sID;

        public int port0InstanceID; // start
        public int port1InstanceID; // end
        public string port0SID;
        public string port1SID;

        public Color selectedColor;
        public Color hoverColor;
        public Color defaultColor;

        public CurveStyle curveStyle;
        public string label;
        public Line line;

        public bool enableDrag;
        public bool enableHover;
        public bool enableSelect;
        public bool disableClick;

        void ToSerializable(Connection connection, bool serializeWithNewSID = false)
        {
            id = connection.ID;
            sID = serializeWithNewSID ? UICUtility.GenerateSID() : connection.SID;

            port0InstanceID = connection.port0.GetInstanceID();
            port1InstanceID = connection.port1.GetInstanceID();
            port0SID = serializeWithNewSID ? UICUtility.GenerateSID() : connection.port0.SID;
            port1SID = serializeWithNewSID ? UICUtility.GenerateSID() : connection.port1.SID;

            selectedColor = connection.selectedColor;
            hoverColor = connection.hoverColor;
            defaultColor = connection.defaultColor;

            curveStyle = connection.curveStyle;
            label = connection.label.text;
            line = connection.line;

            enableDrag = connection.EnableDrag;
            enableHover = connection.EnableHover;
            enableSelect = connection.EnableSelect;
            disableClick = connection.DisableClick;
        }

        /// <summary>
        /// deserialize data to the indicated connection except label (data is stored and can be used from serializableConnection.lable)
        /// </summary>
        /// <param name="connection">element that the data will be loaded to</param>
        /// <param name="graphManager">graph that indicates which UIC line renderer will render the connection</param>
        public void FromSerializable(Connection connection, GraphManager graphManager)
        {
            connection.graphManager = graphManager;

            connection.ID = id;
            connection.SID = sID;

            connection.selectedColor = selectedColor;
            connection.hoverColor = hoverColor;
            connection.defaultColor = defaultColor;

            connection.curveStyle = curveStyle;

            connection.line = line;

            connection.EnableDrag = enableDrag;
            connection.EnableHover = enableHover;
            connection.EnableSelect = enableSelect;
            connection.DisableClick = disableClick;

            connection.ElementColor = defaultColor;
        }
    }
}