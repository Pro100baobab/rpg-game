using UnityEngine;

public class ResetHitTrigger : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("GetHit");
    }
}
