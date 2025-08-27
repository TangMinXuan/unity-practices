using System.Collections.Generic;
using UnityEngine;

public class MyUnitSelectionScript : MonoBehaviour
{
    public List<GameObject> selectedUnit; // 选中的单位
    Camera _mainCam;

    Rect _selectionBox; // 选择框
    Vector2 _startPosition;
    Vector2 _endPosition;
    bool _isDragging; // 添加拖拽状态标志

    // 记得把这个脚本挂载到一个空的GameObject上(比如我这里的MyUnitSelection), 不然整个脚本都不会生效
    void Start()
    {
        selectedUnit = new List<GameObject>();
        _mainCam = Camera.main;
    }

    void Update()
    {
        // 点击选中逻辑
        if (Input.GetMouseButtonDown(0))
        {
            _startPosition = Input.mousePosition;
            _isDragging = true;

            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 一条射线从main camera出发, 方向是Input.mousePosition:
            // 返回true: 射线与第一个Collider物体发生碰撞, 同时将碰撞信息存储在hit中
            // 返回false: 射线没有与任何Collider物体发生碰撞
            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                if (clickedObject.CompareTag("MyUnit")) // 射线碰撞到了一个MyUnit对象
                {
                    if (Input.GetKey(KeyCode.LeftShift)) // 按住Shift键多选
                    {
                        if (IsUnitSelected(clickedObject))
                        {
                            RemoveUnitFromSelection(clickedObject); // 取消选择
                        }
                        else
                        {
                            AddUnitToSelection(clickedObject); // 添加到选择列表
                        }
                    }
                    else // 单选
                    {
                        ClearSelection(); // 清除之前的选择
                        AddUnitToSelection(clickedObject); // 添加新的选择
                    }
                    _isDragging = false; // 点击到单位时不启动拖拽选择
                }
                else // 点击非单位对象，清除选择
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        ClearSelection();
                    }
                }
            }
        }

        // 拖拽选择逻辑
        if (Input.GetMouseButton(0) && _isDragging) // 左键按下并拖动
        {
            _endPosition = Input.mousePosition;
            _selectionBox = CalculateSelectionBox(_startPosition, _endPosition);
        }

        if (Input.GetMouseButtonUp(0) && _isDragging) // 左键释放
        {
            if (_selectionBox.width > 10 && _selectionBox.height > 10) // 只有当选择框足够大时才选择
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    ClearSelection(); // 如果没按Shift，先清除之前的选择
                }
                SelectUnitsInBox();
            }

            _isDragging = false;
            _selectionBox = new Rect(); // 重置选择框
        }
    }

    /**
     * 和start(), update()类似, 也是一个unity暴露出来的方法
     * 它会在GUI渲染事件、鼠标事件、键盘事件等发生时被调用 (暂时理解为, 像update一样的每帧调用)
     */
    void OnGUI()
    {
        if (_isDragging && _selectionBox.width > 0 && _selectionBox.height > 0)
        {
            DrawSelectionBox(_selectionBox);
        }
    }

    private Rect CalculateSelectionBox(Vector2 startPos, Vector2 endPos)
    {
        // Unity的屏幕坐标和GUI坐标Y轴是相反的，需要转换
        // 1. 屏幕坐标系（Input.mousePosition）: 原点(0,0)在屏幕左下角，Y轴向上增长
        // 2. GUI坐标系（OnGUI()中使用）: 原点(0,0)在屏幕左上角，Y轴向下增长
        // 为了跨平台和兼容, 都是历史包袱!!
        startPos.y = Screen.height - startPos.y;
        endPos.y = Screen.height - endPos.y;

        Vector2 topLeft = Vector2.Min(startPos, endPos);
        Vector2 bottomRight = Vector2.Max(startPos, endPos);

        return new Rect(topLeft.x, topLeft.y, bottomRight.x - topLeft.x, bottomRight.y - topLeft.y);
    }

    private void DrawSelectionBox(Rect selectionRect)
    {
        // 创建半透明的选择框样式
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.95f, 0.25f)); // 淡蓝色半透明
        texture.Apply();

        GUIStyle style = new GUIStyle();
        style.normal.background = texture;

        // 先绘制选择框的内部填充区域(是一个半透明的填充矩形)
        GUI.Box(selectionRect, "", style);

        // 再绘制选择框的四条边, 每天边都是粗2像素, 蓝色的线条
        GUI.color = new Color(0.8f, 0.8f, 0.95f, 1f);
        // 上边框
        GUI.DrawTexture(new Rect(selectionRect.x, selectionRect.y, selectionRect.width, 2), texture);
        // 下边框
        GUI.DrawTexture(new Rect(selectionRect.x, selectionRect.y + selectionRect.height - 2, selectionRect.width, 2), texture);
        // 左边框
        GUI.DrawTexture(new Rect(selectionRect.x, selectionRect.y, 2, selectionRect.height), texture);
        // 右边框
        GUI.DrawTexture(new Rect(selectionRect.x + selectionRect.width - 2, selectionRect.y, 2, selectionRect.height), texture);

        GUI.color = Color.white; // 重置GUI颜色
    }

    private void SelectUnitsInBox()
    {
        // 获取所有带有"MyUnit"标签的物体
        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("MyUnit");

        foreach (GameObject unit in allUnits)
        {
            // 将世界坐标转换为屏幕坐标
            Vector3 screenPos = _mainCam.WorldToScreenPoint(unit.transform.position);

            // 检查单位是否在选择框内
            if (IsPointInSelectionBox(screenPos))
            {
                AddUnitToSelection(unit);
            }
        }
    }

    private bool IsPointInSelectionBox(Vector3 screenPos)
    {
        // 转换屏幕坐标以匹配选择框坐标系
        Vector2 point = new Vector2(screenPos.x, Screen.height - screenPos.y);

        return _selectionBox.Contains(point) && screenPos.z > 0; // z > 0 确保物体在摄像机前方
    }

    #region 单位选择管理方法

    /// <summary>
    /// 添加单位到选择列表
    /// </summary>
    /// <param name="unit">要添加的单位</param>
    public void AddUnitToSelection(GameObject unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("尝试添加空的单位到选择列表");
            return;
        }

        if (!selectedUnit.Contains(unit))
        {
            selectedUnit.Add(unit);
            CreateSelectionIndicator(unit);
            UnitSelectionEvents.TriggerUnitSelected(unit);
        }
    }

    /// <summary>
    /// 从选择列表中移除单位
    /// </summary>
    /// <param name="unit">要移除的单位</param>
    public void RemoveUnitFromSelection(GameObject unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("尝试移除空的单位从选择列表");
            return;
        }

        if (selectedUnit.Contains(unit))
        {
            selectedUnit.Remove(unit);
            DestroySelectionIndicator(unit);
            UnitSelectionEvents.TriggerUnitDeselected(unit);
        }
    }

    /// <summary>
    /// 清除所有选择
    /// </summary>
    public void ClearSelection()
    {
        if (selectedUnit.Count == 0) return;

        // 使用倒序遍历避免修改列表时的索引问题
        for (int i = selectedUnit.Count - 1; i >= 0; i--)
        {
            GameObject unit = selectedUnit[i];
            RemoveUnitFromSelection(unit);
        }
    }

    /// <summary>
    /// 检查单位是否已被选中
    /// </summary>
    /// <param name="unit">要检查的单位</param>
    /// <returns>如果单位已被选中返回true，否则返回false</returns>
    public bool IsUnitSelected(GameObject unit)
    {
        return unit != null && selectedUnit.Contains(unit);
    }

    /// <summary>
    /// 获取当前选中的单位数量
    /// </summary>
    /// <returns>选中单位的数量</returns>
    public int GetSelectedUnitCount()
    {
        return selectedUnit.Count;
    }

    /// <summary>
    /// 获取选中单位的只读列表
    /// </summary>
    /// <returns>选中单位的只读列表</returns>
    public List<GameObject> GetSelectedUnits()
    {
        return new List<GameObject>(selectedUnit); // 返回副本，防止外部修改
    }

    /// <summary>
    /// 切换单位的选择状态
    /// </summary>
    /// <param name="unit">要切换状态的单位</param>
    public void ToggleUnitSelection(GameObject unit)
    {
        if (IsUnitSelected(unit))
        {
            RemoveUnitFromSelection(unit);
        }
        else
        {
            AddUnitToSelection(unit);
        }
    }

    #endregion

    #region 选择指示器管理

    /// <summary>
    /// 为单位创建选择指示器
    /// </summary>
    /// <param name="unit">目标单位</param>
    private void CreateSelectionIndicator(GameObject unit)
    {
        // 检查是否已经有指示器
        Transform existingIndicator = unit.transform.Find("SelectionIndicator");
        if (existingIndicator != null)
        {
            return; // 已经有指示器了
        }

        // 生成绿色圆盘
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(indicator.GetComponent<Collider>()); // 移除碰撞体，避免干扰射线检测

        indicator.transform.SetParent(unit.transform); // SetParent之后, 生成的indicator会跟随unit移动

        // 计算指示器位置 - 放在单位底部
        Renderer unitRenderer = unit.GetComponent<Renderer>();
        float unitHeight = unitRenderer != null ? unitRenderer.bounds.size.y : 1f;

        indicator.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        indicator.transform.localPosition = new Vector3(0, -unitHeight * 0.5f - 0.05f, 0);
        indicator.GetComponent<Renderer>().material.color = Color.green;
        indicator.name = "SelectionIndicator";
    }

    /// <summary>
    /// 销毁单位的选择指示器
    /// </summary>
    /// <param name="unit">目标单位</param>
    private void DestroySelectionIndicator(GameObject unit)
    {
        Transform indicator = unit.transform.Find("SelectionIndicator");
        if (indicator != null)
        {
            Destroy(indicator.gameObject);
        }
    }

    #endregion

    public static class UnitSelectionEvents
    {
        public static event System.Action<GameObject> OnUnitSelected;
        public static event System.Action<GameObject> OnUnitDeselected;

        public static void TriggerUnitSelected(GameObject unit) => OnUnitSelected?.Invoke(unit);
        public static void TriggerUnitDeselected(GameObject unit) => OnUnitDeselected?.Invoke(unit);
    }
}
