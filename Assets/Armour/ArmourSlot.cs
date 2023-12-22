using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
    The ArmourSlot script is responsible for managing individual armour slots 
    in the user interface. Each slot corresponds to a particular type of armour 
    (e.g., Helmet, ChestPiece). This script handles the process of dropping an 
    armour piece onto the slot, updating the UI and character's stats accordingly.
*/

public class ArmourSlot : MonoBehaviour, IDropHandler
{
    // ----- Section: Armour Slot Attributes -----

    public ArmourType slotType; // Type of the slot (Helmet, ChestPiece, etc.)
    public CharacterStats characterStats;
    public DisplayStats displayStats;
    public Armour equippedArmour;

    // ----- Section: Drop Handlers -----

    // This method is called when an item is dropped onto the slot
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop called.");

        ArmourDragHandler armour = eventData.pointerDrag.GetComponent<ArmourDragHandler>();
        if (armour != null && armour.armour.type == slotType)
        {
            // Update the UI.
            GetComponent<Image>().sprite = armour.armour.armourSprite;

            // Update the character's stats
            armour.armour.Equip(characterStats);
            displayStats.UpdateDisplay(); 
        }
    }
}
