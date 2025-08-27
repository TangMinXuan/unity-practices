using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MonoBehaviour
{
    void Start()
    {
        UnitSelectionManager.Instance.allUnitList.Add(gameObject);

    }

    private void OnDestroy()
    {
        UnitSelectionManager.Instance.allUnitList.Remove(gameObject);
    }
}
