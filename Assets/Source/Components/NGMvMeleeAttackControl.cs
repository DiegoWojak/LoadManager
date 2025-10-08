

using UnityEngine;

public class NGMvMeleeAttackControl : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        // Obtener el valor de AttackSpeed desde el Animator (asignado por el manager)
        float attackSpeed = NGMCombatAnimationManager.Instance.GetAttackSpeed(stateInfo);
        if (attackSpeed > 0f)
        {
            animator.speed = attackSpeed;
        }
        else
        {
            animator.speed = stateInfo.speed;
        }
    }

    public override void OnStateExit(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        // Restaurar la velocidad del animator al salir del estado
        animator.speed = stateInfo.speed;
    }
}