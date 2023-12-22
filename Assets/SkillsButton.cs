using UnityEngine;

/*
    The SkillsButton class manages the behavior of a UI button dedicated to displaying the skills of the active character.
    When pressed, it toggles the visibility of a skills panel, which lists out each of the character's skills.
    The class ensures that only the skills belonging to the active character are displayed, by cross-referencing with the character's skill list.
    If a skill isn't in the active character's list, its corresponding display in the panel is disabled, maintaining an intuitive and relevant UI for the player.
*/

public class SkillsButton : MonoBehaviour
{
    public GameObject skillsPanel;  
    public TurnManager turnManager; 

    public void OnSkillsButtonClick()
    {
        // If the panel is already visible, hide it
        if (skillsPanel.activeSelf)
        {
            skillsPanel.SetActive(false);
        }
        else
        {
            // Fetch the CharacterStats of the active character
            CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

            // Loop through all children of the skillsPanel
            for (int i = 0; i < skillsPanel.transform.childCount; i++)
            {
                // Get the SkillDisplay component of the child game object
                SkillDisplay skillDisplay = skillsPanel.transform.GetChild(i).GetComponent<SkillDisplay>();

                if (activeCharacterStats.characterSkills.Contains(skillDisplay.skill))
                {
                    // Display the skill if it's in the list
                    skillDisplay.DisplaySkill(skillDisplay.skill);

                    // Enable the game object in case it was disabled
                    if (!skillDisplay.gameObject.activeSelf)
                    {
                        skillDisplay.gameObject.SetActive(true);
                    }
                }
                else
                {
                    // If the skill isn't in the list, disable this game object
                    if (skillDisplay.gameObject.activeSelf)
                    {
                        skillDisplay.gameObject.SetActive(false);
                    }
                }
            }

            // Show the skills panel
            skillsPanel.SetActive(true);
        }
    }

}

