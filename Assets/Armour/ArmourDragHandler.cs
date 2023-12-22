using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
    The ArmourDragHandler script is responsible for handling the drag and drop 
    functionality of armour pieces in the user interface. This script allows the player 
    to equip and unequip armours through a drag and drop interface, updating the character's 
    stats and the display accordingly.
*/

public class ArmourDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ----- Section: Armour Attributes -----

    public Armour armour;
    public CharacterStats characterStats; 
    public DisplayStats displayStats;

    private Vector3 originalPosition;
    private ArmourSlot currentSlot; 

    // ----- Section: Drag Handlers -----

    // This method is called when the dragging starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag called.");
        CharacterStats characterStats = displayStats.GetCurrentStats();

        // If the item is currently on a slot, Unequip it
        if (currentSlot != null)
        {
            armour.Unequip(characterStats);
            characterStats.equippedArmour.Remove(armour);

            currentSlot.equippedArmour = null;
            currentSlot.GetComponent<Image>().sprite = null;
            currentSlot = null;

            // Update the stats display
            Debug.Log("After Unequipping, CharacterStats: " + characterStats.ToString());

            displayStats.UpdateDisplay();
        }

        originalPosition = transform.position;
    }

    // This method is called during the dragging
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag called.");

        CharacterStats characterStats = displayStats.GetCurrentStats();

        transform.position = Input.mousePosition;
    }

    // This method is called when the dragging ends
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag called.");

        CharacterStats characterStats = displayStats.GetCurrentStats();

        // Check for ArmourSlot
        ArmourSlot slot = FindClosestArmourSlot();
        if (slot != null && slot.slotType == armour.type)
        {
            Debug.Log("Armour slot found and matches armour type. Slot Type: " + slot.slotType.ToString());

            slot.GetComponent<Image>().sprite = armour.armourSprite;

            armour.Equip(characterStats);
            characterStats.equippedArmour.Add(armour);

            slot.equippedArmour = armour;

            currentSlot = slot;

            Debug.Log("After Equipping, CharacterStats: " + characterStats.ToString());

            // Update the stats display
            displayStats.UpdateDisplay();
        }
        else
        {
            Debug.Log("No matching armour slot found.");
        }

        transform.position = originalPosition;
    }

    // ----- Section: Utility Methods -----

    // Find the closest ArmourSlot under the mouse cursor
    private ArmourSlot FindClosestArmourSlot()
    {
        ArmourSlot[] slots = FindObjectsOfType<ArmourSlot>();
        foreach (ArmourSlot slot in slots)
        {
            RectTransform slotTransform = slot.GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(slotTransform, Input.mousePosition, null))
            {
                return slot;
            }
        }
        return null;
    }
}
