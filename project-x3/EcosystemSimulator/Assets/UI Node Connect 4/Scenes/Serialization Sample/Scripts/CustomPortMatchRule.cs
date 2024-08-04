using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MeadowGames.UINodeConnect4.Extension;

namespace MeadowGames.UINodeConnect4.SampleScene.SerializationSample
{
    public class CustomPortMatchRule : PortMatchRule
    {
        public override bool ExecuteRule(Port draggedPort, Port foundPort)
        {
            return draggedPort.ID == foundPort.ID ? true: false;
        }
    }
}