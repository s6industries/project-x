using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    /// <summary>
    /// get label p0 : p1
    /// </summary>
    [ExecuteInEditMode]
    public class FixedLabelRule : ConnectionLabelRule
    {
        public string label = "Connection";

        public override void ExecuteRule(Connection connection)
        {
            connection.SetLabel(label);
        }
    }
}