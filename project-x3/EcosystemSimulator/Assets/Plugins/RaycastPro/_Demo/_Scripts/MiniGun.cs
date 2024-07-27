using RaycastPro.Casters;
using UnityEngine;

namespace Plugins.RaycastPro.Demo.Scripts
{
    public class MiniGun : MonoBehaviour
    {
        public AdvanceCaster advanceCaster;
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                advanceCaster.enabled = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                advanceCaster.enabled = false;
            }
        }
    }
}
