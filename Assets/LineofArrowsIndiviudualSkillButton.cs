using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineofArrowsIndiviudualSkillButton : MonoBehaviour
{
    public Skill skill;
    public TurnManager turnManager;  

    private Button button;

    public GameObject skillsPanel;
    [SerializeField]
    private ChatboxController chatbox;


    public void OnButtonClick()
    {
        if (skill != null)
        {
            Character activeCharacter = turnManager.GetActiveCharacter();
            GameObject activeCharacterGameObject = activeCharacter.stats.characterGameObject;
            var lineOfArrowsScript = activeCharacterGameObject.GetComponent<LineOfArrows>();

            // Check if the skill is on cooldown
            if (lineOfArrowsScript && lineOfArrowsScript.cooldownCounter > 0)
            {
                Debug.Log("Skill is on cooldown");
                return;
            }
            // Get the active character from the TurnManager
            if (activeCharacter == null)
            {
                Debug.LogError("No active character found.");
                return;
            }
            // Get the GameObject of the active character

            if (activeCharacterGameObject == null)
            {
                Debug.LogError("Active character's GameObject is null.");
                return;
            }

            // Get the LineOfArrowsScript component from the GameObject of the active character

            if (lineOfArrowsScript == null)
            {
                Debug.LogError("No LineOfArrowsScript found on the active character.");
                return;
            }

            // Call the update line function
            lineOfArrowsScript.isUpdatingLine = true;
            skillsPanel.SetActive(false);

        }
    }
}
