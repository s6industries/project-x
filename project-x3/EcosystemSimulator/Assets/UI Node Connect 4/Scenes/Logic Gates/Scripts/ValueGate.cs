using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.LogicGates
{
    public class ValueGate : Gate
    {
        public bool value;

        public override void Solve()
        {
            Output = value;
        }
    }
}