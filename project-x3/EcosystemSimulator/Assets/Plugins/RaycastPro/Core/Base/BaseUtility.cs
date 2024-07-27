namespace RaycastPro
{
    #if UNITY_EDITOR
        using Editor;
        using UnityEditor;
    #endif

    public abstract class BaseUtility : RaycastCore
    {

        protected void Update() { if (autoUpdate == UpdateMode.Normal) OnCast(); }
        protected void LateUpdate() { if (autoUpdate == UpdateMode.Late) OnCast(); }
        protected void FixedUpdate() { if (autoUpdate == UpdateMode.Fixed) OnCast(); }
        
#if UNITY_EDITOR
        
#endif
    }
}