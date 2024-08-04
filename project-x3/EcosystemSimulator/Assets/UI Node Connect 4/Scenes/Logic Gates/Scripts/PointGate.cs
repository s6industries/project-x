using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.LogicGates
{
    public class PointGate : Gate
    {
        [SerializeField] bool _output;
        public override bool Output
        {
            get
            {
                SolveRecursive(node);
                return _output;
            }
            set
            {
                _output = value;
            }
        }

        public override void Solve()
        {
            List<Port> connectedPorts = node.ports[0].GetConnectedPorts();
            if (connectedPorts.Count == 0 && !InputManager.Instance.PointerPress)
                Destroy(gameObject);

            foreach (Port otherPort in connectedPorts)
            {
                if (otherPort.Polarity != Port.PolarityType._in)
                {
                    if (otherPort.node.GetComponent<Gate>().Output == true)
                    {
                        Output = true;
                        return;
                    }
                }
            }

            Output = false;
        }

        public void SolveRecursive(Node node)
        {
            if (!solved.Contains(node))
            {
                Output = false;
                solved.Add(node);
                foreach (Port otherPort in node.ports[0].GetConnectedPorts())
                {
                    if (otherPort.Polarity != Port.PolarityType._in)
                    {
                        if (otherPort.node.GetComponent<Gate>().Output == true)
                        {
                            Output = true;
                        }
                    }

                    if (otherPort.Polarity == Port.PolarityType._all)
                    {
                        SolveRecursive(otherPort.node);
                    }
                }
            }
        }
    }
}