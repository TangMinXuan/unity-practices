using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MyUnitMovementScript : MonoBehaviour
{
    Camera main_cam;
    NavMeshAgent agent;
    LayerMask ground;

    private float lastSpeed = 0f;
    private bool wasMoving = false;
    private bool wasAttacking = false;
    private bool isSelected = false; // 本地选中状态

    void Start()
    {
        main_cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        ground = LayerMask.GetMask("Ground");

        // 监听选中事件
        MyUnitSelectionScript.UnitSelectionEvents.OnUnitSelected += HandleUnitSelected;
        MyUnitSelectionScript.UnitSelectionEvents.OnUnitDeselected += HandleUnitDeselected;
    }

    void Update()
    {
        // 处理移动输入
        HandleMovementInput();

        // 更新动画状态
        UpdateAnimationState();
    }

    void HandleMovementInput()
    {
        if (Input.GetMouseButtonDown(1) && isSelected)
        {
            Ray ray = main_cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, ground))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    void UpdateAnimationState()
    {
        float currentSpeed = agent.velocity.magnitude;
        bool isMoving = currentSpeed > 0.1f;

        // 只在状态改变时触发事件
        if (Mathf.Abs(currentSpeed - lastSpeed) > 0.05f)
        {
            MovementEvents.TriggerSpeedChanged(currentSpeed);
            lastSpeed = currentSpeed;
        }

        if (isMoving != wasMoving)
        {
            MovementEvents.TriggerMovementStateChanged(isMoving);
            wasMoving = isMoving;
        }
    }

    private void HandleUnitSelected(GameObject unit)
    {
        if (unit == gameObject)
            isSelected = true;
    }

    private void HandleUnitDeselected(GameObject unit)
    {
        if (unit == gameObject)
            isSelected = false;
    }

    /**
     * Unit被销毁时取消事件监听(例如: 这个unit被消灭了)
     */
    void OnDestroy()
    {
        MyUnitSelectionScript.UnitSelectionEvents.OnUnitSelected -= HandleUnitSelected;
        MyUnitSelectionScript.UnitSelectionEvents.OnUnitDeselected -= HandleUnitDeselected;
    }

    public static class MovementEvents
    {
        public static event Action<float> OnSpeedChanged;
        public static event Action<bool> OnMovementStateChanged;
        public static event Action<bool> OnAttackingStateChanged;

        public static void TriggerSpeedChanged(float speed) => OnSpeedChanged?.Invoke(speed);
        public static void TriggerMovementStateChanged(bool isMoving) => OnMovementStateChanged?.Invoke(isMoving);
        public static void TriggerAttackingStateChanged(bool isAttacking) => OnAttackingStateChanged?.Invoke(isAttacking);
        // ?. 是空条件操作符（null-conditional operator），确保在 OnAttackingStateChanged 不为null时才调用 Invoke
    }
}
