using System.Collections;
using System.Collections.Generic;
using MeadowGames.UINodeConnect4;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    // v4.1 - PortMatchRule extension added to facilitate the implementation of custom rules for connect ports by dragging
    public abstract class PortMatchRule : MonoBehaviour
    {
        [SerializeField] static PortMatchRule _instance;
        public static PortMatchRule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PortMatchRule>();
                }
                return _instance;
            }
            set => _instance = value;
        }

        public virtual bool ExecuteRule(Port draggedPort, Port foundPort)
        {
            return true;
        }
    }
}