using UnityEngine;

public class MyAnimatorController_Script : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 订阅移动事件
        MyUnitMovementScript.MovementEvents.OnSpeedChanged += HandleSpeedChanged;
        MyUnitMovementScript.MovementEvents.OnMovementStateChanged += HandleMovementStateChanged;
        MyUnitMovementScript.MovementEvents.OnAttackingStateChanged += HandleAttack;
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        MyUnitMovementScript.MovementEvents.OnSpeedChanged -= HandleSpeedChanged;
        MyUnitMovementScript.MovementEvents.OnMovementStateChanged -= HandleMovementStateChanged;
        MyUnitMovementScript.MovementEvents.OnAttackingStateChanged -= HandleAttack;
    }

    private void HandleSpeedChanged(float speed)
    {
        // animator.SetFloat("Speed", speed);
    }

    private void HandleMovementStateChanged(bool isMoving)
    {
        animator.SetBool("IsMoving", isMoving);
        Debug.Log("Movement state changed: " + isMoving);
    }

    private void HandleAttack(bool isAttacking)
    {
        animator.SetBool("IsAttacking", isAttacking);
        // 可以在这里添加更多逻辑，比如播放攻击动画或触发攻击事件
    }
}