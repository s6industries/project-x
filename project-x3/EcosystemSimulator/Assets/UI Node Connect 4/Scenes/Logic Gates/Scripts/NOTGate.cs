using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.LogicGates
{
    public class NOTGate : Gate
    {
        public override void Solve()
        {
            GetInputs();

            if (inputs.Count == 0 || node.GetNodesConnectedToPolarity(Port.PolarityType._in).Count == 0)
            {
                Output = false;
                return;
            }

            Output = !inputs[0];
        }
    }
}