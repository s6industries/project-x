#if UNITY_EDITOR
namespace RaycastPro
{
    using UnityEngine;
    
    public class Info : MonoBehaviour
    {
        public string header;
        [TextArea] public string description;
    }
}
#endif