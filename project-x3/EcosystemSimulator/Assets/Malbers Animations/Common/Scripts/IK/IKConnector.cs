using UnityEngine;

namespace MalbersAnimations.IK
{
    [AddComponentMenu("Malbers/IK/IK Connector")]

    public class IKConnector : MonoBehaviour
    {
        public string IKSet;
        [Tooltip("When a source is found, the IK Set Targets will be assigned too")]
        public bool SetTargetOnSource = true;
        [Tooltip("When a source is Enabled, the IK Set Targets will be assigned too")]
        public bool SetTargetOnEnable = false;
        public Transform[] targets;
        private IIKSource source;

        public virtual void Set_IKSource(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            if (source != null) Targets_Set();
        }

        public virtual void Set_IKSource(Component owner) => Set_IKSource(owner.gameObject);

        public virtual void Targets_Set()
        {
            source?.Target_Set(IKSet, targets);

        }

        public virtual void Targets_Clear() => source?.Target_Clear(IKSet);

        public virtual void Set_Enable(Component owner) => Set_Enable(owner.gameObject);

        public virtual void Set_Enable(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            source?.Set_Enable(IKSet, true);
            source?.Set_Weight(IKSet, true);
            if (SetTargetOnEnable) Targets_Set();
        }

        public virtual void Set_Disable(Component owner) => Set_Disable(owner.gameObject);

        public virtual void Set_Disable(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            source?.Set_Weight(IKSet, false);
        }

        public virtual void Set_Enable() => source?.Set_Enable(IKSet);
        public virtual void Set_Disable() => source?.Set_Disable(IKSet);

    }
}
