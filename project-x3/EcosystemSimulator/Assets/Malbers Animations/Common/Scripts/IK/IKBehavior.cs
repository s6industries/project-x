using UnityEngine;

namespace MalbersAnimations.IK
{
    public class IKBehavior : StateMachineBehaviour
    {
        private IIKSource source;

        public string IKSet;
        public bool OnEnter = true;
        [Hide(nameof(OnEnter))]
        public bool enable = true;

        [Space]
        public bool OnExit = false;
        [Hide(nameof(OnExit))]
        public bool m_enable = true;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            source ??= animator.GetComponent<IIKSource>();

            if (OnEnter)
                source?.Set_Weight(IKSet, enable);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (OnExit)
                source?.Set_Weight(IKSet, m_enable);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}
