using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    /// <summary>
    /// label port0.node : port1.node
    /// </summary>
    public class StartPlusEndNodeIDsRule : ConnectionLabelRule
    {
        public override void ExecuteRule(Connection connection)
        {
            connection.SetLabel(connection.port0.node.ID + " : " + connection.port1.node.ID);
        }
    }
}