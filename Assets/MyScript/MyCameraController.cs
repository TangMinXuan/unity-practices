using System;
using UnityEngine;

public class MyCameraController : MonoBehaviour
{
    [SerializeField]float movementSensitivity = 1f;

    Camera main_cam;
    private Vector3 start_drag_position;
    private Vector3 camera_start_position;
    private Vector3 current_drag_position;
    private bool isDragging = false;
    private Vector3 new_position;

    private void Start()
    {
        main_cam = Camera.main;
        new_position = transform.position; // 初始化相机位置, 不然开始游戏的时候, 摄像头会摔到地上, 到(0,0,0)
    }

    private void Update()
    {
        Ray ray = main_cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        LayerMask groundLayerMask = LayerMask.GetMask("Ground");

        // 开始拖动
        if (Input.GetMouseButtonDown(2) && Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            start_drag_position = hit.point; // 鼠标按下时的初始位置
            camera_start_position = transform.position; // 相机初始位置
            isDragging = true;
        }

        // 拖动过程中实时更新
        if (isDragging && Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            current_drag_position = hit.point;
            Vector3 drag_offset = start_drag_position - current_drag_position;
            new_position = camera_start_position + drag_offset;
        }

        // 结束拖动
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        // 平滑移动相机到新位置, 如果放在if (isDragging) 中, 相机不会在拖动结束后"慢慢停下来"
        transform.position = Vector3.Lerp(transform.position, new_position, Time.deltaTime * movementSensitivity);
    }
}