using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionBox : MonoBehaviour
{
    Camera myCam;

    // 视觉层面的选择框
    [SerializeField]
    RectTransform boxVisual;

    // 逻辑层面的选择框
    Rect selectionBox;

    Vector2 startPosition;
    Vector2 endPosition;

    private void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    private void Update()
    {
        // When Clicked
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;

            // For selection the Units
            selectionBox = new Rect();
        }

        // When Dragging
        if (Input.GetMouseButton(0))
        {
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();

            if (boxVisual.rect.width > 0 && boxVisual.rect.height > 0)
            {
                UnitSelectionManager.Instance.DeselectAll();
                SelectUnits();
            }
        }

        // When Releasing
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    void DrawVisual()
    {
        // 画视觉选择框: 通过确定矩形的 中心点 和 长宽 来画
        // TODO: 为啥 视觉选择框 不能像 逻辑选择框 一样, 用4个顶点来画
        // Calculate the starting and ending positions of the selection box.
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        // Calculate the center of the selection box.
        Vector2 boxCenter = (boxStart + boxEnd) / 2;

        // Set the position of the visual selection box based on its center.
        boxVisual.position = boxCenter;

        // Calculate the size of the selection box in both width and height.
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        // Set the size of the visual selection box based on its calculated size.
        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        // 画逻辑选择框: 通过确定矩形的 4个顶点 来画
        selectionBox.xMin = Math.Min(Input.mousePosition.x, startPosition.x);
        selectionBox.xMax = Math.Max(Input.mousePosition.x, startPosition.x);

        selectionBox.yMin = Math.Min(Input.mousePosition.y, startPosition.y);
        selectionBox.yMax = Math.Max(Input.mousePosition.y, startPosition.y);
    }

    void SelectUnits()
    {
        foreach (var unit in UnitSelectionManager.Instance.allUnitList)
        {

            if (selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelectionManager.Instance.DragSelect(unit);
            }
        }
    }
}