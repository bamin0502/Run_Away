using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoadTest : MonoBehaviour
{
    void Start()
    {
        ItemTable itemTable = new ItemTable();
        itemTable.Load("RunAway_Item");
        Debug.Log("Loaded item table.");
    }
}
