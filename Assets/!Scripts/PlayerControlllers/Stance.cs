using UnityEngine;

public class Stance : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Attack");
        animator.SetBool("IsAttacking", false);
        
        animator.ResetTrigger("Jump");
    }
}
