using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; set; }

    public List<GameObject> allUnitList = new List<GameObject>();
    public List<GameObject> selectedUnitList = new List<GameObject>();

    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;

    public bool attackCursorVisible;

    public GameObject groundMaker;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // 0是左键
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // 左键点击了一个clickable对象
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            else // 如果没有点击了一个clickable对象
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }

            }
        }

        if (Input.GetMouseButtonDown(1) && selectedUnitList.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // 如果点击了一个clickable对象
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                groundMaker.transform.position = hit.point;

                groundMaker.SetActive(false);
                groundMaker.SetActive(true);
            }
        }

        if (selectedUnitList.Count > 0 && AtLeaseOneOffensiveUnit(selectedUnitList))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // 如果点击了一个 attackable 对象 (attackable的Box Collider必须打开)
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Debug.Log("点击了一个 attackable 对象");
                attackCursorVisible = true;
                if (Input.GetMouseButtonDown(1))
                {
                    Transform hittedTarget = hit.transform;
                    selectedUnitList
                        .Where(unit => unit.GetComponent<AttackController>() != null)
                        .ToList()
                        .ForEach(unit =>
                    {
                        unit.GetComponent<AttackController>().targetToAttack = hittedTarget;
                    });
                }
            }
            else
            {
                attackCursorVisible = false;
            }
        }
    }

    private bool AtLeaseOneOffensiveUnit(List<GameObject> selectedUnitList)
    {
        return selectedUnitList.Any(unit => unit.GetComponent<AttackController>() != null);
    }

    private void MultiSelect(GameObject unit)
    {
        if (selectedUnitList.Contains(unit))
        {
            selectedUnitList.Remove(unit);
            TriggerSelectedIndicator(unit, false);
            EnableUnitMovement(unit, false);
        }
        else
        {
            selectedUnitList.Add(unit);
            TriggerSelectedIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }
    }

    private void SelectByClicking(GameObject unit)
    {
        DeselectAll();
        selectedUnitList.Add(unit);
        TriggerSelectedIndicator(unit, true);
        EnableUnitMovement(unit, true);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        unit.GetComponent<UnitMovement>().enabled = shouldMove;

    }

    public void DeselectAll()
    {
        selectedUnitList.ForEach(unit =>
        {
            TriggerSelectedIndicator(unit, false);
            EnableUnitMovement(unit, false);
        });
        selectedUnitList.Clear();
        groundMaker.SetActive(false);
    }

    private void TriggerSelectedIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    // TODO awake看起来是一个unity原生方法, 学习一下是咋用的
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // TODO 为啥这里是gameObject, 不是instance
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void DragSelect(GameObject unit)
    {
        if (!selectedUnitList.Contains(unit))
        {
            selectedUnitList.Add(unit);
            TriggerSelectedIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }
    }
}
