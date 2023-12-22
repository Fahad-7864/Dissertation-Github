using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryButton : MonoBehaviour
{
    public GameObject inventoryObject;
    // This will enable me to direct the user to the inventory panel
    public void OnInventoryButtonClick()
    {
        bool isActive = inventoryObject.activeSelf;
        inventoryObject.SetActive(!isActive);
    }
}
