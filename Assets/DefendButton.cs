using UnityEngine;

public class DefendButton : MonoBehaviour
{
    public TurnManager turnManager;

    public void OnDefendButtonClick()
    {
        CharacterStats activeCharacterStats = turnManager.GetActiveCharacterStats();

        // Check if there is an active character and if it still has energy points, and if it's a Friendly type
        if (activeCharacterStats != null && activeCharacterStats.isCharacterTurn && activeCharacterStats.energy > 0 && activeCharacterStats.type == CharacterType.Friendly)
        {
            // increasing the defense attribute temporarily
            activeCharacterStats.defence *= 1.5f;

            activeCharacterStats.energy -= 100;

            // Log the defend action
            Debug.Log(activeCharacterStats.characterName + " is now defending!");

        }
    }
}
