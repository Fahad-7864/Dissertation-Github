using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


/*
    The DisplayStats class is responsible for displaying the statistics
    of characters in the game. This class fetches data from the CharacterStats 
    class and updates the UI elements to reflect the current status and attributes 
    of each character. It provides mechanisms to iterate through characters and 
    visually represent their stats using text and sliders.
*/

public class DisplayStats : MonoBehaviour
{
    // ----- Section: References to UI Components -----
    // These are references to the various UI elements in the game where the stats will be displayed.
    public EveryonesStats everyonesStats;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI magicAttackText;
    public TextMeshProUGUI magicDefenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI EnergyText;
    public TextMeshProUGUI MovementpointsText;
    public Slider hpSlider;
    public Slider mpSlider;
    public Canvas statsCanvas;
    private int currentIndex = 0;

    private void Start()
    {
        UpdateDisplay();
    }


    // ----- Section: Navigation Controls -----
    // These methods allow the user to navigate through different characters' stats.

    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % everyonesStats.GetCharacterCount();
        UpdateDisplay();
        UpdateArmourDisplay(everyonesStats.allCharacterStats[currentIndex]);
    }

    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = everyonesStats.GetCharacterCount() - 1;
        UpdateDisplay();
        UpdateArmourDisplay(everyonesStats.allCharacterStats[currentIndex]);
    }

    // ----- Section: Display Updation -----
    // Contains methods responsible for updating the UI components with character stats.
    public void UpdateDisplay()
    {
        // Check if everyonesStats is not null
        if (everyonesStats != null)
        {
            // Check if the list is not null and the index is within the range of the list
            if (everyonesStats.allCharacterStats != null && currentIndex < everyonesStats.allCharacterStats.Count)
            {
                CharacterStats currentStats = everyonesStats.allCharacterStats[currentIndex];

                // Check if currentStats is not null before trying to access its properties
                if (currentStats != null)
                {
                //    Debug.Log("Updating display for " + currentStats.characterName); // New debug log

                    nameText.text = currentStats.characterName;
                    hpText.text = "Health: " + currentStats.hp.ToString();
                    mpText.text = "Mana: " + currentStats.mp.ToString();
                    attackText.text = "Attack: " + currentStats.attack.ToString();
                    defenseText.text = "Defence: " + currentStats.defence.ToString();
                    magicAttackText.text = "Magic Attack: " + currentStats.magicAttack.ToString();
                    magicDefenseText.text = "Magic Defence: " + currentStats.magicDefence.ToString();
                    speedText.text = "Speed: " + currentStats.speed.ToString();
                    accuracyText.text = "Accuracy: " + currentStats.accuracy.ToString();
                    EnergyText.text = "Energy: " + currentStats.energy.ToString();
                    MovementpointsText.text = "Movement Points: " + currentStats.actionPoints.ToString();

                    float maxHp = 30f;
                    float maxMp = 30f;
                    hpSlider.value = currentStats.hp;
                    mpSlider.value = currentStats.mp;
                }
                else
                {
                    Debug.LogError("currentStats is null");
                }
            }
            else
            {
                Debug.LogError("allCharacterStats is null or currentIndex is out of range");
            }
        }
        else
        {
            Debug.LogError("everyonesStats is null");
        }
    }


    private void UpdateArmourDisplay(CharacterStats character)
    {
        ArmourSlot[] slots = FindObjectsOfType<ArmourSlot>();

        foreach (ArmourSlot slot in slots)
        {
            slot.GetComponent<Image>().sprite = null;
        }

        foreach (Armour armour in character.equippedArmour)
        {
            foreach (ArmourSlot slot in slots)
            {
                if (slot.slotType == armour.type)
                {
                    slot.GetComponent<Image>().sprite = armour.armourSprite;
                    break;
                }
            }
        }
    }


    // These methods control the visibility of the stats canvas.
    public void Show()
    {
        statsCanvas.enabled = true;
    }

    public void Hide()
    {
        statsCanvas.enabled = false;
    }


    // ----- Section: Utility Methods -----
    // Additional utility functions to assist in displaying character stats based on various parameters.
    public CharacterStats GetCurrentStats()
    {
        return everyonesStats.allCharacterStats[currentIndex];
    }

    public void ShowStatsByName(string name)
    {
        int index = 0;
        foreach (CharacterStats character in everyonesStats.allCharacterStats)
        {
            if (character.characterName == name)
            {
                currentIndex = index;
                UpdateDisplay();
                break;
            }
            index++;
        }
    }

    public void ShowStatsByGameObject(GameObject characterGameObject)
    {
        Debug.Log("ShowStatsByGameObject called with: " + characterGameObject.name); 
        int index = 0;
        foreach (CharacterStats character in everyonesStats.allCharacterStats)
        {
            if (character.characterGameObject != null)
            {
                Debug.Log("Comparing: " + character.characterGameObject.name + " with " + characterGameObject.name);

                if (character.characterGameObject == characterGameObject)  
                {
                    Debug.Log("Found matching character stats for " + characterGameObject.name); 
                    currentIndex = index;
                    UpdateDisplay();
                    break;
                }
            }
            else
            {
                Debug.Log("character.characterGameObject is null for character: " + character.characterName);
            }

            index++;
        }
    }





}
