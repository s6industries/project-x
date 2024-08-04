using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    [ExecuteInEditMode]
    public class StartNodeIDRule : ConnectionLabelRule
    {
        public override void ExecuteRule(Connection connection)
        {
            string label = "";

            if (connection.port0.Polarity == Port.PolarityType._out)
            {
                label = connection.port0.node.ID;
            }
            else if (connection.port1.Polarity == Port.PolarityType._out)
            {
                label = connection.port1.node.ID;
            }
            else if (connection.port0.Polarity == Port.PolarityType._all)
            {
                label = connection.port0.node.ID;
            }
            else if (connection.port1.Polarity == Port.PolarityType._all)
            {
                label = connection.port1.node.ID;
            }
            else
            {
                label = connection.port0.node.ID;
            }

            connection.SetLabel(label);
        }
    }
}